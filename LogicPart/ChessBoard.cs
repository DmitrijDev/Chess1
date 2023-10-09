
namespace Chess.LogicPart
{
    public class ChessBoard
    {
        private readonly Square[,] _board = new Square[8, 8];
        private readonly Stack<GamePosition> _gamePositions = new();
        private Stack<Move> _moves = new();
        private bool _menacesListsAreActual = true;
        private int _locksCount;
        private readonly object _locker = new();

        public BoardStatus Status { get; private set; } = BoardStatus.ClearBoard;

        public King WhiteKing { get; private set; }

        public King BlackKing { get; private set; }

        public ChessPieceColor MovingSideColor { get; private set; } = ChessPieceColor.White;

        public int MovesAfterCaptureOrPawnMoveCount { get; private set; }

        public Square PassedByPawnSquare { get; private set; }

        public DrawReason DrawReason { get; private set; } = DrawReason.None;

        public ulong ModCount { get; private set; }

        public ChessBoard()
        {
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
            lock (_locker)
            {
                WaitForBoardUnlock();
                sourceBoard.Lock();

                if (Status != BoardStatus.ClearBoard)
                {
                    SetDefaultValues();
                }

                MovingSideColor = sourceBoard.MovingSideColor;

                if (sourceBoard.Status == BoardStatus.ClearBoard)
                {
                    ModCount = sourceBoard.ModCount;
                    sourceBoard.Unlock();
                    return;
                }

                CopyMaterialAndMoves(sourceBoard);

                _gamePositions = new(_moves.Reverse().Select(move => move.PrecedingPosition));
                _gamePositions.Push(new(this));

                _menacesListsAreActual = false;
                Status = sourceBoard.Status;
                MovesAfterCaptureOrPawnMoveCount = sourceBoard.MovesAfterCaptureOrPawnMoveCount;

                if (sourceBoard.PassedByPawnSquare != null)
                {
                    PassedByPawnSquare = _board[sourceBoard.PassedByPawnSquare.Vertical, sourceBoard.PassedByPawnSquare.Horizontal];
                }

                DrawReason = sourceBoard.DrawReason;
                ModCount = sourceBoard.ModCount;
                sourceBoard.Unlock();
            }
        }

        public Square this[int vertical, int horizontal] => _board[vertical, horizontal];

        private void CopyMaterialAndMoves(ChessBoard sourceBoard)
        {
            var capturedSourcePieces = sourceBoard._moves.Where(move => move.IsCapture).Select(move => move.CapturedPiece);

            var sourcePieces = sourceBoard._moves.Where(move => move.IsPawnPromotion).Select(move => move.MovingPiece).
            Concat(capturedSourcePieces).Concat(sourceBoard.EnumerateMaterial()).ToArray();

            var copyPieces = sourcePieces.Select(oldPiece => ChessPiece.GetNewPiece(oldPiece.Name, oldPiece.Color)).ToArray();

            for (var i = 0; i < copyPieces.Length; ++i)
            {
                var oldPiece = sourcePieces[i];
                var newPiece = copyPieces[i];

                newPiece.FirstMoveMoment = oldPiece.FirstMoveMoment;

                if (oldPiece.IsOnBoard)
                {
                    var square = _board[oldPiece.Vertical, oldPiece.Horizontal];
                    newPiece.PutTo(square);
                }
            }

            if (sourceBoard.WhiteKing != null)
            {
                WhiteKing = (King)_board[sourceBoard.WhiteKing.Vertical, sourceBoard.WhiteKing.Horizontal].ContainedPiece;
            }

            if (sourceBoard.BlackKing != null)
            {
                BlackKing = (King)_board[sourceBoard.BlackKing.Vertical, sourceBoard.BlackKing.Horizontal].ContainedPiece;
            }

            if (sourceBoard._moves.Count == 0)
            {
                return;
            }

            var sourceMoves = sourceBoard._moves.ToArray();

            var copyMoves = sourceMoves.Select(oldMove => new Move()
            {
                PrecedingPosition = new(oldMove.PrecedingPosition),
                StartSquare = _board[oldMove.StartSquare.Vertical, oldMove.StartSquare.Horizontal],
                MoveSquare = _board[oldMove.MoveSquare.Vertical, oldMove.MoveSquare.Horizontal],
                IsEnPassantCapture = oldMove.IsEnPassantCapture,
                IsCastleKingside = oldMove.IsCastleKingside,
                IsCastleQueenside = oldMove.IsCastleQueenside
            }).ToArray();

            for (var i = 0; i < copyMoves.Length; ++i)
            {
                var oldMove = sourceMoves[i];
                var newMove = copyMoves[i];

                for (var j = 0; ; ++j)
                {
                    var oldPiece = sourcePieces[j];
                    var newPiece = copyPieces[j];

                    if (oldMove.MovingPiece == oldPiece)
                    {
                        newMove.MovingPiece = newPiece;
                    }

                    if (oldMove.CapturedPiece == oldPiece)
                    {
                        newMove.CapturedPiece = newPiece;
                    }

                    if (oldMove.NewPiece == oldPiece)
                    {
                        newMove.NewPiece = newPiece;
                    }

                    if (newMove.MovingPiece != null && (!oldMove.IsCapture || newMove.CapturedPiece != null) &&
                        (!oldMove.IsPawnPromotion || newMove.NewPiece != null))
                    {
                        break;
                    }
                }
            }

            _moves = new(copyMoves.Reverse());
        }

        internal void Lock()
        {
            while (ModCount == 0)
            { }

            lock (_locker)
            {
                while (_locksCount == int.MaxValue)
                { }

                ++_locksCount;
            }
        }

        internal void Unlock() => --_locksCount;

        private void WaitForBoardUnlock()
        {
            while (ModCount == 0 || _locksCount > 0)
            { }
        }

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
                    _board[i, j].ClearMenaces();
                }
            }

            _gamePositions.Clear();
            _moves.Clear();
            _menacesListsAreActual = true;
            WhiteKing = null;
            BlackKing = null;
            MovesAfterCaptureOrPawnMoveCount = 0;
            PassedByPawnSquare = null;
            Status = BoardStatus.ClearBoard;
            DrawReason = DrawReason.None;
        }

        public void Clear()
        {
            lock (_locker)
            {
                WaitForBoardUnlock();
                SetDefaultValues();
                IncreaseModCount();
            }
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
            lock (_locker)
            {
                WaitForBoardUnlock();

                if (piecePositons.Distinct().Count() != piecePositons.Count)
                {
                    throw new ArgumentException("Для двух фигур указана одна и та же позиция.");
                }

                if (Status != BoardStatus.ClearBoard)
                {
                    SetDefaultValues();
                }

                MovingSideColor = movingSideColor;

                if (material.Count == 0)
                {
                    IncreaseModCount();
                    return;
                }

                for (var i = 0; i < material.Count; ++i)
                {
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

                _menacesListsAreActual = false;
                _gamePositions.Push(new(this));

                if (!CheckPositionLegacy())
                {
                    Status = BoardStatus.IllegalPosition;
                    IncreaseModCount();
                    return;
                }

                if (IsDrawByMaterial())
                {
                    Status = BoardStatus.Draw;
                    DrawReason = DrawReason.NotEnoughMaterial;
                    IncreaseModCount();
                    return;
                }

                if (EnumerateMaterial(MovingSideColor).Any(piece => piece.GetAccessibleSquares().Any()))
                {
                    Status = BoardStatus.GameIsIncomplete;
                }
                else
                {
                    if (WhiteKing.IsChecked())
                    {
                        Status = BoardStatus.BlackWin;
                        IncreaseModCount();
                        return;
                    }

                    if (BlackKing.IsChecked())
                    {
                        Status = BoardStatus.WhiteWin;
                        IncreaseModCount();
                        return;
                    }

                    Status = BoardStatus.Draw;
                    DrawReason = DrawReason.Stalemate;
                }

                IncreaseModCount();
            }
        }

        public Square GetSquare(string squareName)
        {
            var coordinates = StringsUsing.GetChessSquareCoordinates(squareName);
            return _board[coordinates[0], coordinates[1]];
        }

        internal void RenewMenaces()
        {
            if (_menacesListsAreActual)
            {
                return;
            }

            if (Status != BoardStatus.ClearBoard)
            {
                for (var i = 0; i < 8; ++i)
                {
                    for (var j = 0; j < 8; ++j)
                    {
                        _board[i, j].ClearMenaces();
                    }
                }
            }

            foreach (var piece in EnumerateMaterial())
            {
                foreach (var square in piece.GetAttackedSquares())
                {
                    square.AddMenace(piece);
                }
            }

            _menacesListsAreActual = true;
        }

        private bool CheckPositionLegacy()
        {
            if (WhiteKing == null || BlackKing == null)
            {
                return false;
            }

            var king = MovingSideColor == ChessPieceColor.White ? BlackKing : WhiteKing;

            if (king.IsChecked())
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
            lock (_locker)
            {
                WaitForBoardUnlock();

                if (Status != BoardStatus.GameIsIncomplete)
                {
                    throw new InvalidOperationException("На доске не идет партия.");
                }

                if (move.Board != this)
                {
                    throw new ArgumentException("Указан ход на другой доске.");
                }

                if (move.PrecedingPosition != _gamePositions.Peek())
                {
                    throw new ArgumentException("Ход был создан для другой позиции.");
                }

                var exception = move.MovingPiece.CheckMoveLegacy(move);

                if (exception != null)
                {
                    throw exception;
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

                _menacesListsAreActual = false;
                _moves.Push(move);
                MovingSideColor = MovingSideColor == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
                PassedByPawnSquare = GetPassedByPawnSquare();
                MovesAfterCaptureOrPawnMoveCount = move.IsCapture || move.IsPawnMove ? 0 : MovesAfterCaptureOrPawnMoveCount + 1;

                if (move.MovingPiece.FirstMoveMoment == 0)
                {
                    move.MovingPiece.FirstMoveMoment = _moves.Count;
                }

                _gamePositions.Push(new(this));
                CheckGameResult();
                IncreaseModCount();
            }
        }

        private void CheckGameResult()
        {
            if (!EnumerateMaterial(MovingSideColor).Any(piece => piece.GetAccessibleSquares().Any()))
            {
                if (WhiteKing.IsChecked())
                {
                    Status = BoardStatus.BlackWin;
                    return;
                }

                if (BlackKing.IsChecked())
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
            lock (_locker)
            {
                WaitForBoardUnlock();

                if (_moves.Count == 0)
                {
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

                _menacesListsAreActual = false;
                MovingSideColor = MovingSideColor == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
                PassedByPawnSquare = GetPassedByPawnSquare();

                MovesAfterCaptureOrPawnMoveCount = lastMove.IsCapture || lastMove.IsPawnMove ?
                    _moves.TakeWhile(move => !move.IsCapture && !move.IsPawnMove).Count() : MovesAfterCaptureOrPawnMoveCount - 1;

                if (lastMove.MovingPiece.FirstMoveMoment > _moves.Count)
                {
                    lastMove.MovingPiece.FirstMoveMoment = 0;
                }

                _gamePositions.Pop();
                Status = BoardStatus.GameIsIncomplete;
                DrawReason = DrawReason.None;
                IncreaseModCount();
            }
        }

        private void IncreaseModCount() => ModCount = ModCount == ulong.MaxValue ? 1 : ModCount + 1;

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

        public bool HasSingleLegalMove()
        {
            Lock();

            if (Status != BoardStatus.GameIsIncomplete)
            {
                Unlock();
                return false;
            }

            var hasLegalMoves = false;

            foreach (var piece in EnumerateMaterial(MovingSideColor))
            {
                foreach (var square in piece.GetAccessibleSquares())
                {
                    if (hasLegalMoves)
                    {
                        Unlock();
                        return false;
                    }

                    hasLegalMoves = true;
                }
            }

            Unlock();
            return hasLegalMoves;
        }

        public Move GetLastMove() => _moves.Peek();

        public GamePosition GetCurrentPosition() => _gamePositions.Peek();

        public void SetStatus(BoardStatus status)
        {
            lock (_locker)
            {
                WaitForBoardUnlock();

                if (Status != BoardStatus.GameIsIncomplete || (status != BoardStatus.WhiteWin && status != BoardStatus.BlackWin && status != BoardStatus.Draw))
                {
                    throw new InvalidOperationException("Невозможное присвоение.");
                }

                Status = status;
                IncreaseModCount();
            }
        }

        public int MovesCount => _moves.Count;
    }
}