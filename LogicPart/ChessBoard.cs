
namespace Chess.LogicPart
{
    public class ChessBoard
    {
        private readonly Square[,] _board = new Square[8, 8];
        private readonly Stack<GamePosition> _gamePositions = new();
        private readonly Stack<Move> _moves = new();
        private long _modStartsCount;
        private int _locksCount;
        private readonly object _locker = new();

        public BoardStatus Status { get; private set; } = BoardStatus.ClearBoard;

        public King WhiteKing { get; private set; }

        public King BlackKing { get; private set; }

        public ChessPieceColor MovingSideColor { get; private set; } = ChessPieceColor.White;

        public int MovesAfterCaptureOrPawnMoveCount { get; private set; }

        public Square PassedByPawnSquare { get; private set; }

        public DrawReason DrawReason { get; private set; } = DrawReason.None;

        public long ModCount { get; private set; }

        public Comparison<ChessPiece> ComparePieceValues { get; set; }

        public ChessBoard()
        {
            _modStartsCount = 1;

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _board[i, j] = new Square(this, i, j);
                }
            }

            ModCount = 1;
        }

        public ChessBoard(ChessBoard sourceBoard) : this()
        {
            sourceBoard.Lock();

            if (sourceBoard.Status == BoardStatus.ClearBoard)
            {
                _modStartsCount = sourceBoard._modStartsCount;
                ModCount = sourceBoard.ModCount;
                MovingSideColor = sourceBoard.MovingSideColor;
                ComparePieceValues = sourceBoard.ComparePieceValues;
                sourceBoard.Unlock();
                return;
            }

            if (sourceBoard.Status == BoardStatus.IllegalPosition)
            {
                SetPosition(sourceBoard._gamePositions.Single());
                _modStartsCount = sourceBoard._modStartsCount;
                ModCount = sourceBoard.ModCount;
                ComparePieceValues = sourceBoard.ComparePieceValues;
                sourceBoard.Unlock();
                return;
            }

            SetPosition(sourceBoard._gamePositions.Last());
            _modStartsCount = sourceBoard._modStartsCount - sourceBoard._moves.Count;
            ModCount = _modStartsCount;

            foreach (var move in sourceBoard._moves.Reverse())
            {
                var piece = _board[move.StartSquare.Vertical, move.StartSquare.Horizontal].ContainedPiece;
                var square = _board[move.MoveSquare.Vertical, move.MoveSquare.Horizontal];
                MakeMove(!move.IsPawnPromotion ? new Move(piece, square) : new Move(piece, square, move.NewPiece.Name));
            }

            ComparePieceValues = sourceBoard.ComparePieceValues;
            sourceBoard.Unlock();
        }

        public Square this[int vertical, int horizontal] => _board[vertical, horizontal];

        private void CheckThreadSafety()
        {
            while (ModCount == 0)
            { }

            lock (_locker)
            {
                while (_modStartsCount != ModCount || _locksCount > 0)
                { }

                ++_modStartsCount;
            }
        }

        internal void Lock()
        {
            while (ModCount == 0)
            { }

            lock (_locker)
            {
                while (_modStartsCount != ModCount)
                { }

                ++_locksCount;
            }
        }

        internal void Unlock() => --_locksCount;

        private IEnumerable<ChessPiece> EnumerateMaterial()
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

        private IEnumerable<ChessPiece> EnumerateMaterial(ChessPieceColor color) => EnumerateMaterial().Where(piece => piece.Color == color);

        public List<ChessPiece> GetMaterial()
        {
            Lock();
            var result = EnumerateMaterial().ToList();
            Unlock();
            return result;
        }

        private void SetDefaultValues()
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _board[i, j].Clear();
                    _board[i, j].Menaces = null;
                }
            }

            _gamePositions.Clear();
            _moves.Clear();
            MovesAfterCaptureOrPawnMoveCount = 0;
            PassedByPawnSquare = null;
            WhiteKing = null;
            BlackKing = null;
            Status = BoardStatus.ClearBoard;
            DrawReason = DrawReason.None;
        }

        public void Clear()
        {
            CheckThreadSafety();
            SetDefaultValues();
            ++ModCount;
        }

        public void SetPosition(ChessPieceName[] whitePieceNames, string[] whiteSquareNames,
            ChessPieceName[] blackPieceNames, string[] blackSquareNames, ChessPieceColor movingSideColor)
        {
            if (whitePieceNames.Length == 0 || whitePieceNames.Length != whiteSquareNames.Length)
            {
                throw new ArgumentException("Для белых должно быть указано равное положительное количество фигур и полей.");
            }

            if (blackPieceNames.Length == 0 || blackPieceNames.Length != blackSquareNames.Length)
            {
                throw new ArgumentException("Для черных должно быть указано равное положительное количество фигур и полей.");
            }

            var whiteMaterial = whitePieceNames.Select(name => ChessPiece.GetNewPiece(name, ChessPieceColor.White));
            var blackMaterial = blackPieceNames.Select(name => ChessPiece.GetNewPiece(name, ChessPieceColor.Black));

            var whitePiecePositions = whiteSquareNames.Select(name => GetSquare(name));
            var blackPiecePositions = blackSquareNames.Select(name => GetSquare(name));

            SetPosition(whiteMaterial.Concat(blackMaterial).ToList(), whitePiecePositions.Concat(blackPiecePositions).ToList(), movingSideColor);
        }

        public void SetPosition(GamePosition position)
        {
            var material = new List<ChessPiece>();
            var squares = new List<Square>();

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (position.GetPieceName(i, j) == null)
                    {
                        continue;
                    }

                    var pieceName = (ChessPieceName)position.GetPieceName(i, j);
                    var pieceColor = (ChessPieceColor)position.GetPieceColor(i, j);
                    var piece = ChessPiece.GetNewPiece(pieceName, pieceColor);

                    material.Add(piece);
                    squares.Add(_board[i, j]);
                }
            }

            SetPosition(material, squares, position.MovingSideColor);
        }

        private void SetPosition(List<ChessPiece> material, List<Square> piecePositons, ChessPieceColor movingSideColor)
        {
            CheckThreadSafety();

            if (Status != BoardStatus.ClearBoard)
            {
                SetDefaultValues();
            }

            MovingSideColor = movingSideColor;

            if (material.Count == 0)
            {
                ++ModCount;
                return;
            }

            for (var i = 0; i < material.Count; ++i)
            {
                if (!piecePositons[i].IsEmpty)
                {
                    ++ModCount;
                    throw new ArgumentException("Для двух фигур указана одна и та же позиция.");
                }

                material[i].PutTo(piecePositons[i]);

                if (material[i].Name == ChessPieceName.King)
                {
                    if (material[i].Color == ChessPieceColor.White)
                    {
                        WhiteKing = (King)material[i];
                    }
                    else
                    {
                        BlackKing = (King)material[i];
                    }
                }
            }

            _gamePositions.Push(new GamePosition(this));
            RenewMenaces();

            if (!CheckPositionLegacy())
            {
                Status = BoardStatus.IllegalPosition;
                ++ModCount;
                return;
            }

            if (IsDrawByMaterial())
            {
                Status = BoardStatus.Draw;
                DrawReason = DrawReason.NotEnoughMaterial;
                ++ModCount;
                return;
            }

            if (EnumerateMaterial(MovingSideColor).Any(piece => piece.GetAccessibleSquares().Any()))
            {
                Status = BoardStatus.GameIsIncomplete;
            }
            else
            {
                var menaces = WhiteKing.Position.Menaces;

                if (menaces != null && menaces.Any(piece => piece.Color == ChessPieceColor.Black))
                {
                    Status = BoardStatus.BlackWin;
                    ++ModCount;
                    return;
                }

                menaces = BlackKing.Position.Menaces;

                if (menaces != null && menaces.Any(piece => piece.Color == ChessPieceColor.White))
                {
                    Status = BoardStatus.WhiteWin;
                    ++ModCount;
                    return;
                }

                Status = BoardStatus.Draw;
                DrawReason = DrawReason.Stalemate;
            }

            ++ModCount;
        }

        public Square GetSquare(string squareName)
        {
            var coordinates = StringsUsing.GetChessSquareCoordinates(squareName);
            return _board[coordinates[0], coordinates[1]];
        }

        private void RenewMenaces()
        {
            if (Status != BoardStatus.ClearBoard)
            {
                for (var i = 0; i < 8; ++i)
                {
                    for (var j = 0; j < 8; ++j)
                    {
                        _board[i, j].Menaces = null;
                    }
                }
            }

            foreach (var piece in EnumerateMaterial())
            {
                foreach (var square in piece.GetAttackedSquares())
                {
                    square.Menaces ??= new List<ChessPiece>();
                    square.Menaces.Add(piece);
                }
            }
        }

        private bool CheckPositionLegacy()
        {
            if (WhiteKing == null || BlackKing == null)
            {
                return false;
            }

            var king = MovingSideColor == ChessPieceColor.White ? BlackKing : WhiteKing;
            var menaces = king.Position.Menaces;

            if (menaces != null && menaces.Any(piece => piece.Color != king.Color))
            {
                return false;
            }

            foreach (var piece in EnumerateMaterial())
            {
                if (piece.Name == ChessPieceName.King && piece != WhiteKing && piece != BlackKing)
                {
                    return false;
                }

                if (piece.Name == ChessPieceName.Pawn && (piece.Horizontal == 0 || piece.Horizontal == 7))
                {
                    return false;
                }
            }

            return true;
        }

        private Square GetPassedByPawnSquare()
        {
            if (_moves.Count == 0)
            {
                return null;
            }

            var lastMove = _moves.Peek();

            if (!lastMove.IsPawnDoubleVerticalMove)
            {
                return null;
            }

            var vertical = lastMove.MoveSquare.Vertical;
            var horizontal = lastMove.MovingPiece.Color == ChessPieceColor.White ? 2 : 5;
            return _board[vertical, horizontal];
        }        

        private bool IsDrawByMaterial()
        {
            if (_moves.Count > 0 && !_moves.Peek().IsCapture && !_moves.Peek().IsPawnPromotion)
            {
                return false;
            }

            var knightsPresent = false;
            var lightSquaredBishopsPresent = false;
            var darkSquaredBishopsPresent = false;

            foreach (var piece in EnumerateMaterial())
            {
                switch (piece.Name)
                {
                    case ChessPieceName.Queen:
                    case ChessPieceName.Rook:
                    case ChessPieceName.Pawn:
                        {
                            return false;
                        }

                    case ChessPieceName.Knight:
                        {
                            if (knightsPresent || lightSquaredBishopsPresent || darkSquaredBishopsPresent)
                            {
                                return false;
                            }

                            knightsPresent = true;
                            break;
                        }

                    case ChessPieceName.Bishop:
                        {
                            if (knightsPresent)
                            {
                                return false;
                            }

                            if (piece.Vertical % 2 != piece.Horizontal % 2)
                            {
                                lightSquaredBishopsPresent = true;
                            }
                            else
                            {
                                darkSquaredBishopsPresent = true;
                            }

                            if (lightSquaredBishopsPresent && darkSquaredBishopsPresent)
                            {
                                return false;
                            }

                            break;
                        }
                };
            }

            return true;
        }

        private bool IsDrawByThreeRepeats()
        {
            if (MovesAfterCaptureOrPawnMoveCount < 8)
            {
                return false;
            }

            var positionRepeatsCount = 1;
            var skipsCount = 0;
            var positionsLeftCount = MovesAfterCaptureOrPawnMoveCount - 3;

            foreach (var position in _gamePositions.Take(MovesAfterCaptureOrPawnMoveCount + 1).Skip(4))
            {
                if (positionsLeftCount < 5 && positionRepeatsCount == 1)
                {
                    return false;
                }

                --positionsLeftCount;

                if (skipsCount > 0)
                {
                    --skipsCount;
                    continue;
                }

                if (_gamePositions.Peek().IsEqualTo(position))
                {
                    ++positionRepeatsCount;

                    if (positionRepeatsCount == 3)
                    {
                        return true;
                    }

                    skipsCount = 3;
                }
            }

            return false;
        }

        public void MakeMove(Move move)
        {
            CheckThreadSafety();

            if (move.Board != this)
            {
                ++ModCount;
                throw new ArgumentException("Указан ход на другой доске.");
            }

            if (move.CreationMoment != ModCount)
            {
                ++ModCount;
                throw new ArgumentException("Состояние доски изменилось с момента создания хода.");
            }

            var illegalMoveMessage = move.MovingPiece.CheckMoveLegacy(move);

            if (illegalMoveMessage != null)
            {
                ++ModCount;
                throw new IllegalMoveException(illegalMoveMessage);
            }

            if (move.IsPawnPromotion && !move.NewPieceSelected)
            {
                ++ModCount;
                throw new NewPieceNotSelectedException();
            }

            if (!move.IsPawnPromotion)
            {
                move.MovingPiece.PutTo(move.MoveSquare);
            }
            else
            {
                move.MovingPiece.Remove();
                move.NewPiece.PutTo(move.MoveSquare);
            }

            if (move.IsEnPassantCapture)
            {
                move.CapturedPiece.Remove();
            }

            if (move.IsCastleKingside)
            {
                var rook = _board[7, move.MovingPiece.Horizontal].ContainedPiece;
                rook.PutTo(_board[5, rook.Horizontal]);
                rook.FirstMoveMoment = _moves.Count + 1;
            }

            if (move.IsCastleQueenside)
            {
                var rook = _board[0, move.MovingPiece.Horizontal].ContainedPiece;
                rook.PutTo(_board[3, rook.Horizontal]);
                rook.FirstMoveMoment = _moves.Count + 1;
            }

            _moves.Push(move);
            MovingSideColor = MovingSideColor == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
            PassedByPawnSquare = GetPassedByPawnSquare();
            MovesAfterCaptureOrPawnMoveCount = move.IsCapture || move.IsPawnMove ? 0 : MovesAfterCaptureOrPawnMoveCount + 1;            

            if (move.MovingPiece.FirstMoveMoment == 0)
            {
                move.MovingPiece.FirstMoveMoment = _moves.Count;
            }

            _gamePositions.Push(new GamePosition(this));
            RenewMenaces();
            CheckGameResult();
            ++ModCount;
        }

        private void CheckGameResult()
        {
            if (!EnumerateMaterial(MovingSideColor).Any(piece => piece.GetAccessibleSquares().Any()))
            {
                var menaces = WhiteKing.Position.Menaces;

                if (menaces != null && menaces.Any(piece => piece.Color == ChessPieceColor.Black))
                {
                    Status = BoardStatus.BlackWin;
                    return;
                }

                menaces = BlackKing.Position.Menaces;

                if (menaces != null && menaces.Any(piece => piece.Color == ChessPieceColor.White))
                {
                    Status = BoardStatus.WhiteWin;
                    return;
                }

                Status = BoardStatus.Draw;
                DrawReason = DrawReason.Stalemate;
                return;
            }

            if (IsDrawByMaterial())
            {
                Status = BoardStatus.Draw;
                DrawReason = DrawReason.NotEnoughMaterial;
                return;
            }

            if (IsDrawByThreeRepeats())
            {
                Status = BoardStatus.Draw;
                DrawReason = DrawReason.ThreeRepeatsRule;
                return;
            }

            if (MovesAfterCaptureOrPawnMoveCount >= 100)
            {
                Status = BoardStatus.Draw;
                DrawReason = DrawReason.FiftyMovesRule;
            }
        }

        public void TakebackMove()
        {
            CheckThreadSafety();

            if (_moves.Count == 0)
            {
                ++ModCount;
                throw new InvalidOperationException("Невозможно взять ход обратно: на доске начальная позиция партии.");
            }

            var lastMove = _moves.Pop();

            lastMove.MovingPiece.PutTo(lastMove.StartSquare);

            if (lastMove.IsCapture)
            {
                var capturedPiecePosition = !lastMove.IsEnPassantCapture ? lastMove.MoveSquare :
                _board[lastMove.MoveSquare.Vertical, lastMove.CapturedPiece.Color == ChessPieceColor.White ? 3 : 4];

                lastMove.CapturedPiece.PutTo(capturedPiecePosition);
            }

            if (lastMove.IsPawnPromotion)
            {
                lastMove.NewPiece.Remove();
            }

            if (lastMove.IsCastleKingside)
            {
                var rook = _board[5, lastMove.MovingPiece.Horizontal].ContainedPiece;
                rook.PutTo(_board[7, rook.Horizontal]);
                rook.FirstMoveMoment = 0;
            }

            if (lastMove.IsCastleQueenside)
            {
                var rook = _board[3, lastMove.MovingPiece.Horizontal].ContainedPiece;
                rook.PutTo(_board[0, rook.Horizontal]);
                rook.FirstMoveMoment = 0;
            }

            MovingSideColor = MovingSideColor == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
            PassedByPawnSquare = GetPassedByPawnSquare();

            MovesAfterCaptureOrPawnMoveCount = lastMove.IsCapture || lastMove.IsPawnMove ?
                _moves.TakeWhile(move => !move.IsCapture && !move.IsPawnMove).Count() : MovesAfterCaptureOrPawnMoveCount - 1;            

            if (lastMove.MovingPiece.FirstMoveMoment > _moves.Count)
            {
                lastMove.MovingPiece.FirstMoveMoment = 0;
            }

            _gamePositions.Pop();
            RenewMenaces();
            Status = BoardStatus.GameIsIncomplete;
            DrawReason = DrawReason.None;
            ++ModCount;
        }

        public List<Move> GetLegalMoves()
        {
            Lock();

            var result = new List<Move>();

            if (Status != BoardStatus.GameIsIncomplete)
            {
                Unlock();
                return result;
            }

            foreach (var piece in EnumerateMaterial(MovingSideColor))
            {
                foreach (var square in piece.GetAccessibleSquares())
                {
                    if (piece.Name != ChessPieceName.Pawn || !(square.Horizontal == 0 || square.Horizontal == 7))
                    {
                        result.Add(Move.CreateMove(piece, square));
                        continue;
                    }

                    var pieceNames = new ChessPieceName[] { ChessPieceName.Queen, ChessPieceName.Rook, ChessPieceName.Knight, ChessPieceName.Bishop };

                    foreach (var newPieceName in pieceNames)
                    {
                        result.Add(Move.CreateMove(piece, square, newPieceName));
                    }
                }
            }

            Unlock();
            return result;
        }

        public Move GetLastMove() => _moves.Peek();

        public GamePosition GetCurrentPosition() => _gamePositions.Peek();

        public void SetStatus(BoardStatus status)
        {
            CheckThreadSafety();

            if (Status != BoardStatus.GameIsIncomplete || (status != BoardStatus.WhiteWin && status != BoardStatus.BlackWin && status != BoardStatus.Draw))
            {
                ++ModCount;
                throw new InvalidOperationException("Невозможное присвоение.");
            }

            Status = status;
            ++ModCount;
        }        

        public int MovesCount => _moves.Count;
    }
}