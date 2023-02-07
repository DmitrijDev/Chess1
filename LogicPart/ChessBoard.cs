using Chess;

namespace Chess.LogicPart
{
    public class ChessBoard
    {
        private readonly Square[,] _board = new Square[8, 8];
        private readonly Stack<GamePosition> _positions = new();
        private GameStatus _status;

        internal GameSide White { get; private set; }

        internal GameSide Black { get; private set; }

        internal GameSide MovingSide { get; private set; }

        public int MovesCount { get; private set; }

        public int LastMenacesRenewMoment { get; internal set; } = -1;

        internal Square PassedByPawnSquare { get; private set; }

        public DrawReason DrawReason { get; private set; }

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
            _status = GameStatus.ClearBoard;
        }

        public ChessBoard(IEnumerable<PieceName> whiteMaterial, IEnumerable<string> whitePositions, IEnumerable<PieceName> blackMaterial, IEnumerable<string> blackPositions,
            PieceColor movingSide) : this()
        {
            SetPosition(whiteMaterial, whitePositions, blackMaterial, blackPositions, movingSide);
        }

        public ChessBoard(ChessBoard sourceBoard) : this()
        {
            var material = sourceBoard.GetMaterial().Select(piece => piece.Copy());
            var positions = sourceBoard.GetMaterial().Select(piece => piece.Position.Name);
            SetPosition(material.ToArray(), positions.ToArray(), sourceBoard.MovingSideColor);

            _positions = new Stack<GamePosition>(sourceBoard._positions.Reverse().Select(position => new GamePosition(position)));

            if (sourceBoard.PassedByPawnSquare != null)
            {
                PassedByPawnSquare = _board[sourceBoard.PassedByPawnSquare.Vertical, sourceBoard.PassedByPawnSquare.Horizontal];
            }

            MovesCount = sourceBoard.MovesCount;
            _status = sourceBoard._status;
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

        internal IEnumerable<ChessPiece> GetMaterial()
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (!_board[i, j].IsEmpty)
                    {
                        yield return _board[i, j].ContainedPiece;
                    }
                }
            }
        }

        public void SetPosition(IEnumerable<PieceName> whitePiecesNames, IEnumerable<string> whitePositions,
            IEnumerable<PieceName> blackPiecesNames, IEnumerable<string> blackPositions, PieceColor movingSideColor)
        {            
            if (blackPiecesNames.Count() == 0 || blackPiecesNames.Count() != blackPositions.Count())
            {
                throw new ArgumentException("Для черных должно быть указано равное положительное количество фигур и полей.");
            }

            var whiteMaterial = whitePiecesNames.Select(name => ChessPiece.GetNewPiece(name, PieceColor.White));
            var blackMaterial = blackPiecesNames.Select(name => ChessPiece.GetNewPiece(name, PieceColor.Black));

            SetPosition(whiteMaterial.Concat(blackMaterial).ToArray(), whitePositions.Concat(blackPositions).ToArray(), movingSideColor);
        }

        private void SetPosition(ChessPiece[] material, string[] squareNames, PieceColor movingSideColor)
        {
            if (_status != GameStatus.ClearBoard)
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

                if (material[i] is King)
                {
                    if (material[i].Color == PieceColor.White)
                    {
                        White.King = (King)material[i];
                    }
                    else
                    {
                        Black.King = (King)material[i];
                    }
                }
            }

            _positions.Push(new GamePosition(this));

            if (!CheckPositionLegacy())
            {
                _status = GameStatus.IllegalPosition;
                DrawReason = DrawReason.None;
                return;
            }

            if (IsDrawByMaterial())
            {
                _status = GameStatus.Draw;
                DrawReason = DrawReason.NotEnoughMaterial;
                return;
            }

            _status = GameStatus.GameCanContinue;
            DrawReason = DrawReason.None;

            if (!HasLegalMoves())
            {
                if (White.King.IsMenaced())
                {
                    _status = GameStatus.BlackWin;
                    return;
                }

                if (Black.King.IsMenaced())
                {
                    _status = GameStatus.WhiteWin;
                    return;
                }

                _status = GameStatus.Draw;
                DrawReason = DrawReason.Stalemate;
            }
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

            _positions.Clear();

            White = new GameSide(PieceColor.White, this);
            Black = new GameSide(PieceColor.Black, this);

            MovesCount = 0;
            LastMenacesRenewMoment = -1;
            _status = GameStatus.ClearBoard;
        }

        private Square GetSquare(string squareName)
        {
            var coordinates = Chess.StringsUsing.GetChessSquareCoordinates(squareName);
            return _board[coordinates[0], coordinates[1]];
        }

        public bool CheckPositionLegacy()
        {
            if (White.King == null || Black.King == null)
            {
                return false;
            }

            foreach (var piece in GetMaterial())
            {
                if (piece is King && piece != White.King && piece != Black.King)
                {
                    return false;
                }

                if (piece is Pawn && (piece.Horizontal == 0 || piece.Horizontal == 7))
                {
                    return false;
                }

                if (MovingSide.Color == piece.Color && piece.Attacks(piece.Enemy.King))
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasLegalMoves() => MovingSide.GetMaterial().Any(piece => piece.CanMove());

        private IEnumerable<Move> GetLegalMoves()
        {
            var result = Enumerable.Empty<Move>();

            if (_status != GameStatus.GameCanContinue)
            {
                return result;
            }

            foreach (var piece in MovingSide.GetMaterial())
            {
                result = result.Concat(piece.GetLegalMoves());
            }

            return result;
        }

        public bool IsDrawByMaterial()
        {
            var material = GetMaterial().ToArray();

            if (material.Length < 4)
            {
                return material.All(piece => piece is King || piece is Knight || piece is Bishop);
            }

            var lightSquaredBishopsPresent = false;
            var darkSquaredBishopsPresent = false;

            foreach (var piece in material)
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
            if (_positions.Count < 9)
            {
                return false;
            }

            var repeatsCount = 0;

            foreach (var position in _positions)
            {
                if (position.Equals(_positions.Peek()))
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

        public void MakeMove(string startSquareName, string destinationSquareName, PieceName? newPieceName)
        {
            var startSquareCoordinates = StringsUsing.GetChessSquareCoordinates(startSquareName);
            var movingPiece = _board[startSquareCoordinates[0], startSquareCoordinates[1]].ContainedPiece;

            var moveSquareCoordinates = StringsUsing.GetChessSquareCoordinates(destinationSquareName);
            var moveSquare = _board[moveSquareCoordinates[0], moveSquareCoordinates[1]];

            MakeMove(movingPiece, moveSquare, newPieceName);
        }

        private void MakeMove(ChessPiece movingPiece, Square moveSquare, PieceName? newPieceName)
        {
            foreach (var move in movingPiece.GetLegalMoves())
            {
                if (move.MoveSquare == moveSquare)
                {
                    if (!move.IsPawnPromotion || newPieceName == move.NewPiece.Name)
                    {
                        MakeMove(move);
                        return;
                    }

                    if (move.IsPawnPromotion && newPieceName == null)
                    {
                        throw new NewPieceNotSelectedException();
                    }
                }
            }

            throw new IllegalMoveException(movingPiece.GetIllegalMoveMessage(moveSquare));
        }

        private void MakeMove(Move move)
        {
            ++MovesCount;

            if (!move.IsPawnPromotion)
            {
                move.MovingPiece.Position = move.MoveSquare;

                if (move.MovingPiece.FirstMoveMoment == 0)
                {
                    move.MovingPiece.FirstMoveMoment = MovesCount;
                }
            }
            else
            {
                move.MovingPiece.Position = null;
                move.NewPiece.Position = move.MoveSquare;
            }

            if (move.IsEnPassantCapture)
            {
                move.CapturedPiece.Position = null;
            }

            if (move.IsCastleKingside)
            {
                var rook = _board[7, move.MovingPiece.Horizontal].ContainedPiece;
                rook.Position = _board[5, rook.Horizontal];
                rook.FirstMoveMoment = MovesCount;
            }

            if (move.IsCastleQueenside)
            {
                var rook = _board[0, move.MovingPiece.Horizontal].ContainedPiece;
                rook.Position = _board[3, rook.Horizontal];
                rook.FirstMoveMoment = MovesCount;
            }

            PassedByPawnSquare = !move.IsPawnDoubleMove ? null : _board[move.MoveSquare.Vertical, MovingSide == White ? 2 : 5];
            MovingSide = MovingSide.Enemy;

            if (move.IsCapture || move.IsPawnMove)
            {
                _positions.Clear();
            }

            _positions.Push(new GamePosition(this));
            CheckGameResult();
        }

        private void CheckGameResult()
        {
            if (!HasLegalMoves())
            {
                if (White.King.IsMenaced())
                {
                    _status = GameStatus.BlackWin;
                    return;
                }

                if (Black.King.IsMenaced())
                {
                    _status = GameStatus.WhiteWin;
                    return;
                }

                _status = GameStatus.Draw;
                DrawReason = DrawReason.Stalemate;
                return;
            }

            if (IsDrawByMaterial())
            {
                _status = GameStatus.Draw;
                DrawReason = DrawReason.NotEnoughMaterial;
                return;
            }

            if (IsDrawByThreeRepeats())
            {
                _status = GameStatus.Draw;
                DrawReason = DrawReason.ThreeRepeatsRule;
                return;
            }

            if (_positions.Count > 100)
            {
                _status = GameStatus.Draw;
                DrawReason = DrawReason.FiftyMovesRule;
            }
        }

        public IEnumerable<string[]> GetLegalMovesAsStrings() => GetLegalMoves().Select(move => new string[3]
        { move.MovingPiece.Position.Name, move.MoveSquare.Name, move.IsPawnPromotion ? move.NewPiece.Name.ToString() : null});

        public GameStatus Status
        {
            get => _status;

            set
            {
                if (_status != GameStatus.GameCanContinue || !(value == GameStatus.WhiteWin || value == GameStatus.BlackWin))
                {
                    throw new InvalidOperationException();
                }

                _status = value;
            }
        }

        public GamePosition GetCurrentPosition() => new(this);

        public PieceColor MovingSideColor => MovingSide.Color;
    }
}