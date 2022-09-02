using Chess.StringsUsing;

namespace Chess.LogicPart
{
    public class ChessBoard
    {
        private readonly Square[,] _board = new Square[8, 8];
        private readonly List<Move> _legalMoves = new();
        private int _legalMovesLastRenewMoment = -1;
        private readonly List<GamePosition> _positions = new();

        internal GameSide White { get; private set; }

        internal GameSide Black { get; private set; }

        internal GameSide MovingSide { get; private set; }

        public int MovesAfterCaptureOrPawnMoveCount { get; private set; }

        public int MovesCount { get; private set; }

        public GameStatus Status { get; private set; }

        public ChessBoard()
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _board[i, j] = new Square(this, i, j);
                }
            }

            White = new GameSide(PieceColor.White, this);
            Black = new GameSide(PieceColor.Black, this);
            MovingSide = White;
            Status = GameStatus.ClearBoard;
        }

        public ChessBoard(IEnumerable<string> whiteMaterial, IEnumerable<string> whitePositions, IEnumerable<string> blackMaterial, IEnumerable<string> blackPositions,
            PieceColor movingSide) : this()
        {
            SetPosition(whiteMaterial, whitePositions, blackMaterial, blackPositions, movingSide);
        }

        public ChessBoard(ChessBoard sourceBoard) : this()
        {
            var whiteMaterial = sourceBoard.White.Material.Select(piece => piece.Copy());
            var whitePositions = sourceBoard.White.Material.Select(piece => piece.Position.Name);
            var blackMaterial = sourceBoard.Black.Material.Select(piece => piece.Copy());
            var blackPositions = sourceBoard.Black.Material.Select(piece => piece.Position.Name);
            SetPosition(whiteMaterial.Concat(blackMaterial).ToArray(), whitePositions.Concat(blackPositions).ToArray(), sourceBoard.MovingSideColor);

            for (var i = 0; i < 8; ++i)
            {
                _board[i, 2].IsLegalForEnPassantCapture = sourceBoard._board[i, 2].IsLegalForEnPassantCapture;
                _board[i, 5].IsLegalForEnPassantCapture = sourceBoard._board[i, 5].IsLegalForEnPassantCapture;
            }

            _positions.Clear();
            Array.ForEach(sourceBoard._positions.ToArray(), position => _positions.Add(new GamePosition(position)));

            MovesCount = sourceBoard.MovesCount;
            MovesAfterCaptureOrPawnMoveCount = sourceBoard.MovesAfterCaptureOrPawnMoveCount;
        }

        internal Square this[int vertical, int horizontal]
        {
            get
            {
                if (vertical < 0 || horizontal < 0 || vertical >= 8 || horizontal >= 8)
                {
                    throw new IndexOutOfRangeException("Поля с указанными координатами не существует.");
                }

                return _board[vertical, horizontal];
            }
        }

        public void SetPosition(IEnumerable<string> whiteMaterial, IEnumerable<string> whitePositions, IEnumerable<string> blackMaterial, IEnumerable<string> blackPositions,
            PieceColor movingSide)
        {
            var whiteMaterialArray = whiteMaterial.Select(name => GetNewPiece(name, PieceColor.White)).ToArray();
            var whitePositionsArray = whitePositions.ToArray();

            var blackMaterialArray = blackMaterial.Select(name => GetNewPiece(name, PieceColor.Black)).ToArray();
            var blackPositionsArray = blackPositions.ToArray();

            if (whiteMaterialArray == null || whitePositionsArray == null || blackMaterialArray == null || blackPositionsArray == null)
            {
                throw new ArgumentException("Некорректные аргументы");
            }

            if (whiteMaterialArray.Length == 0 || whiteMaterialArray.Length != whitePositionsArray.Length)
            {
                throw new ArgumentException("Для белых должно быть указано равное положительное количество фигур и полей");
            }

            if (blackMaterialArray.Length == 0 || blackMaterialArray.Length != blackPositionsArray.Length)
            {
                throw new ArgumentException("Для черных должно быть указано равное положительное количество фигур и полей");
            }

            SetPosition(whiteMaterialArray.Concat(blackMaterialArray).ToArray(), whitePositionsArray.Concat(blackPositionsArray).ToArray(), movingSide);
        }

        private void SetPosition(ChessPiece[] material, string[] squareNames, PieceColor movingSideColor)
        {
            if (Status != GameStatus.ClearBoard)
            {
                Clear();
            }

            MovingSide = movingSideColor == PieceColor.White ? White : Black;

            for (var i = 0; i < material.Length; ++i)
            {
                var square = GetSquare(squareNames[i]);

                if (!square.IsEmpty)
                {
                    throw new ArgumentException("Для двух фигур указана одна и та же позиция");
                }

                material[i].Position = square;

                if (material[i].Color == PieceColor.White)
                {
                    White.Material.Add(material[i]);

                    if (material[i] is King)
                    {
                        White.King = (King)material[i];
                    }
                }
                else
                {
                    Black.Material.Add(material[i]);

                    if (material[i] is King)
                    {
                        Black.King = (King)material[i];
                    }
                }
            }

            _positions.Add(new GamePosition(this));

            if (!CheckPositionLegacy())
            {
                Status = GameStatus.IllegalPosition;
                return;
            }

            if (IsDrawByMaterial())
            {
                Status = GameStatus.Draw;
                return;
            }

            if (RenewLegalMoves().Count == 0)
            {
                if (White.King.IsMenaced())
                {
                    Status = GameStatus.BlackWin;
                    return;
                }

                if (Black.King.IsMenaced())
                {
                    Status = GameStatus.WhiteWin;
                    return;
                }

                Status = GameStatus.Draw;
                return;
            }

            Status = GameStatus.GameCanContinue;
        }

        private static ChessPiece GetNewPiece(string name, PieceColor color)
        {
            if (name == null)
            {
                throw new ArgumentException("Не указано имя фигуры");
            }

            var pieces = new ChessPiece[5] { new King(color), new Queen(color), new Rook(color), new Bishop(color), new Pawn(color) }; // Других фигур пока нет.
            var trimmedName = SharedItems.RemoveSpacesAndToLower(name);

            foreach (var piece in pieces)
            {
                if (SharedItems.RemoveSpacesAndToLower(piece.EnglishName) == trimmedName || SharedItems.RemoveSpacesAndToLower(piece.RussianName) == trimmedName)
                {
                    return piece;
                }
            }

            throw new ArgumentException("Фигуры с указанным полным именем не существует");
        }

        public void Clear()
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _board[i, j].SetDefaultValues();
                }
            }

            _legalMoves.Clear();
            _legalMovesLastRenewMoment = -1;
            _positions.Clear();

            White = new GameSide(PieceColor.White, this);
            Black = new GameSide(PieceColor.Black, this);

            MovesAfterCaptureOrPawnMoveCount = 0;
            MovesCount = 0;
            Status = GameStatus.ClearBoard;
        }

        private Square GetSquare(string squareName)
        {
            var coordinates = SharedItems.GetChessSquareCoordinates(squareName);
            return _board[coordinates[0], coordinates[1]];
        }

        public bool CheckPositionLegacy()
        {
            if (White.King == null || Black.King == null)
            {
                return false;
            }

            foreach (var piece in White.Material)
            {
                if (piece is King && piece != White.King)
                {
                    return false;
                }

                if (piece is Pawn && (piece.Horizontal == 0 || piece.Horizontal == 7))
                {
                    return false;
                }

                if (MovingSide == White && piece.GetAttackedSquares().Contains(Black.King.Position))
                {
                    return false;
                }
            }

            foreach (var piece in Black.Material)
            {
                if (piece is King && piece != Black.King)
                {
                    return false;
                }

                if (piece is Pawn && (piece.Horizontal == 0 || piece.Horizontal == 7))
                {
                    return false;
                }

                if (MovingSide == Black && piece.GetAttackedSquares().Contains(White.King.Position))
                {
                    return false;
                }
            }

            return true;
        }

        private List<Move> RenewLegalMoves()
        {
            if (Status == GameStatus.IllegalPosition || _legalMovesLastRenewMoment == MovesCount)
            {
                return _legalMoves;
            }

            _legalMoves.Clear();

            foreach (var piece in MovingSide.Material)
            {
                foreach (var square in piece.GetLegalMoveSquares())
                {
                    if (!(piece is Pawn && (square.Horizontal == 0 || square.Horizontal == 7)))
                    {
                        _legalMoves.Add(new Move(piece, square));
                    }
                    else
                    {
                        _legalMoves.Add(new Move(piece, square, new Queen(piece.Color)));
                        _legalMoves.Add(new Move(piece, square, new Rook(piece.Color)));
                        _legalMoves.Add(new Move(piece, square, new Bishop(piece.Color)));
                    }
                }
            }

            _legalMovesLastRenewMoment = MovesCount;
            return _legalMoves;
        }

        public bool IsDrawByMaterial()
        {
            if (White.Material.Count + Black.Material.Count == 2)
            {
                return true;
            }

            if (White.Material.Count + Black.Material.Count == 3)
            {
                return White.Material.All(piece => piece is King || piece is Bishop) && Black.Material.All(piece => piece is King || piece is Bishop);
            }

            var lightSquaredBishopsPresent = false;
            var darkSquaredBishopsPresent = false;

            foreach (var piece in White.Material)
            {
                if (piece is Bishop)
                {
                    var bishop = (Bishop)piece;

                    if (bishop.IsLightSquared)
                    {
                        lightSquaredBishopsPresent = true;
                    }
                    else
                    {
                        darkSquaredBishopsPresent = true;
                    }
                }
                else if (piece is not King)
                {
                    return false;
                }
            }

            foreach (var piece in Black.Material)
            {
                if (piece is Bishop)
                {
                    var bishop = (Bishop)piece;

                    if (bishop.IsLightSquared)
                    {
                        lightSquaredBishopsPresent = true;
                    }
                    else
                    {
                        darkSquaredBishopsPresent = true;
                    }
                }
                else if (piece is not King)
                {
                    return false;
                }
            }

            return !lightSquaredBishopsPresent || !darkSquaredBishopsPresent;
        }

        public bool IsDrawByThreeRepeats()
        {
            if (_positions.Count < 5)
            {
                return false;
            }

            var lastPosition = _positions[_positions.Count - 1];
            var repeatsCount = 1;

            for (var i = 0; i < _positions.Count - 1; ++i)
            {
                if (_positions[i].Equals(lastPosition))
                {
                    ++repeatsCount;

                    if (repeatsCount == 3)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void MakeMove(int[] moveParams)
        {
            if (moveParams.Length < 4)
            {
                throw new ArgumentException("Некорректный аргумент: неподходящий по длине массив");
            }

            MakeMove(moveParams[0], moveParams[1], moveParams[2], moveParams[3], moveParams.Length < 5 ? 0 : moveParams[4]);
        }

        public void MakeMove(int startVertical, int startHorizontal, int destinationVertical, int destinationHorizontal, int newPieceIndex) =>
            MakeMove(_board[startVertical, startHorizontal].ContainedPiece, _board[destinationVertical, destinationHorizontal], newPieceIndex);

        private void MakeMove(ChessPiece movingPiece, Square moveSquare, int newPieceIndex)
        {
            if (movingPiece == null)
            {
                throw new ArgumentException("В качестве начального поля хода указано пустое поле.");
            }

            if (movingPiece.Color != MovingSideColor)
            {
                throw new IllegalMoveException("Указанный ход невозможен т.к. очередь хода за другой стороной.");
            }

            foreach (var move in RenewLegalMoves())
            {
                if (move.MovingPiece == movingPiece && move.MoveSquare == moveSquare)
                {
                    if ((!move.IsPawnPromotion && newPieceIndex == 0) || (move.IsPawnPromotion && newPieceIndex == move.NewPiece.NumeralIndex))
                    {
                        MakeMove(move);
                        return;
                    }

                    if (move.IsPawnPromotion && newPieceIndex == 0)
                    {
                        throw new NewPieceNotSelectedException();
                    }
                }
            }

            throw new IllegalMoveException("Невозможный ход.");
        }

        private void MakeMove(Move move)
        {
            if (move.IsCapture)
            {
                move.CapturedPiece = !move.IsEnPassantCapture ? move.MoveSquare.ContainedPiece :
                    MovingSide == White ? _board[move.MoveSquare.Vertical, move.MoveSquare.Horizontal - 1].ContainedPiece :
                    _board[move.MoveSquare.Vertical, move.MoveSquare.Horizontal + 1].ContainedPiece;

                move.CapturedPiece.Position = null;
                MovingSide.Enemy.Material.Remove(move.CapturedPiece);
            }

            if (!move.IsPawnPromotion)
            {
                move.MovingPiece.Position = move.MoveSquare;
                move.MovingPiece.HasMoved = true;
            }
            else
            {
                move.MovingPiece.Position = null;
                MovingSide.Material.Remove(move.MovingPiece);
                move.NewPiece.Position = move.MoveSquare;
                MovingSide.Material.Add(move.NewPiece);
            }

            if (move.IsCastleKingside)
            {
                var rook = _board[7, move.MovingPiece.Horizontal].ContainedPiece;
                rook.Position = _board[5, rook.Horizontal];
                rook.HasMoved = true;
            }

            if (move.IsCastleQueenside)
            {
                var rook = _board[0, move.MovingPiece.Horizontal].ContainedPiece;
                rook.Position = _board[3, rook.Horizontal];
                rook.HasMoved = true;
            }

            for (var i = 0; i < 8; ++i)
            {
                _board[i, 2].IsLegalForEnPassantCapture = false;
                _board[i, 5].IsLegalForEnPassantCapture = false;
            }

            if (move.IsPawnJump)
            {
                if (MovingSide == White)
                {
                    _board[move.MoveSquare.Vertical, move.MoveSquare.Horizontal - 1].IsLegalForEnPassantCapture = true;
                }
                else
                {
                    _board[move.MoveSquare.Vertical, move.MoveSquare.Horizontal + 1].IsLegalForEnPassantCapture = true;
                }
            }

            MovingSide = MovingSide.Enemy;

            if (!move.IsCapture && !move.IsPawnMove)
            {
                ++MovesAfterCaptureOrPawnMoveCount;
            }
            else
            {
                MovesAfterCaptureOrPawnMoveCount = 0;
                _positions.Clear();
            }

            ++MovesCount;
            _positions.Add(new GamePosition(this));

            if (RenewLegalMoves().Count == 0)
            {
                if (White.King.IsMenaced())
                {
                    Status = GameStatus.BlackWin;
                    return;
                }

                if (Black.King.IsMenaced())
                {
                    Status = GameStatus.WhiteWin;
                    return;
                }

                Status = GameStatus.Draw;
                return;
            }

            if (IsDrawByMaterial() || IsDrawByThreeRepeats() || MovesAfterCaptureOrPawnMoveCount == 100)
            {
                Status = GameStatus.Draw;
                _legalMoves.Clear();
            }
        }

        public List<int[]> GetLegalMoves() => RenewLegalMoves().Select(move => new int[5] { move.MovingPiece.Vertical, move.MovingPiece.Horizontal, move.MoveSquare.Vertical,
            move.MoveSquare.Horizontal, move.IsPawnPromotion ? move.NewPiece.NumeralIndex : 0}).ToList();

        public GamePosition CurrentPosition => _positions.Count > 0 ? _positions[_positions.Count - 1] : new GamePosition(this);

        public PieceColor MovingSideColor => MovingSide.Color;
    }
}