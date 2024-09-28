
namespace Chess.LogicPart
{
    public class ChessBoard
    {
        private readonly Square[,] _board = new Square[8, 8];
        private Stack<ChessPiece> _removedPieces = new();

        internal bool IsSettingPosition { get; private set; }

        public GamePosition GameStartPosition { get; private set; }

        public Move LastMove { get; private set; }

        public PieceColor MoveTurn { get; private set; }

        public int WhitePiecesCount { get; private set; }

        public int BlackPiecesCount { get; private set; }

        public King WhiteKing { get; private set; }

        public King BlackKing { get; private set; }

        public int MovesAfterCaptureOrPawnMoveCount { get; private set; }

        public Square PawnPassedSquare { get; private set; }

        public BoardStatus Status { get; private set; } = BoardStatus.Clear;

        public DrawReason DrawReason { get; private set; } = DrawReason.None;

        public ulong GamesCount { get; private set; }

        public ulong ModCount { get; private set; }

        internal object Locker { get; } = new();

        public ChessBoard()
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _board[i, j] = new(this, i, j);
                }
            }
        }

        public Square this[int x, int y] => _board[x, y];

        public Square this[SquareLocation location] => _board[location.X, location.Y];

        public ChessPiece GetPiece(int x, int y) => _board[x, y].Contained;

        public ChessPiece GetPiece(SquareLocation location) => _board[location.X, location.Y].Contained;

        public void CopyGame(ChessBoard source)
        {
            lock (source.Locker)
            {
                lock (Locker)
                {
                    if (source.Status == BoardStatus.Clear)
                    {
                        Clear();
                        MoveTurn = source.MoveTurn;
                        return;
                    }

                    IncreaseGamesCount();
                    CopyMaterial(source);
                    GameStartPosition = source.GameStartPosition;
                    LastMove = source.LastMove;
                    MoveTurn = source.MoveTurn;
                    WhitePiecesCount = source.WhitePiecesCount;
                    BlackPiecesCount = source.BlackPiecesCount;
                    MovesAfterCaptureOrPawnMoveCount = source.MovesAfterCaptureOrPawnMoveCount;
                    Status = source.Status;
                    DrawReason = source.DrawReason;

                    PawnPassedSquare = source.PawnPassedSquare == null ? null :
                    this[source.PawnPassedSquare.Location];

                    foreach (var square in GetSquares())
                    {
                        square.WhiteMenaces.Clear();
                        square.BlackMenaces.Clear();
                    }

                    foreach (var piece in GetMaterial())
                    {
                        piece.AddMenaces();
                    }
                }
            }

            PositionSet?.Invoke();
        }

        private void CopyMaterial(ChessBoard sourceBoard)
        {
            foreach (var sourceSquare in sourceBoard.GetSquares())
            {
                var copySquare = this[sourceSquare.Location];

                if (!copySquare.IsClear)
                {
                    copySquare.Contained.Square = null;
                }

                if (sourceSquare.IsClear)
                {
                    copySquare.Contained = null;
                    continue;
                }

                var sourcePiece = sourceSquare.Contained;
                var copyPiece = ChessPiece.GetNewPiece(sourcePiece.Name, sourcePiece.Color);
                copyPiece.FirstMoveMoment = sourcePiece.FirstMoveMoment;
                copyPiece.Square = copySquare;
                copySquare.Contained = copyPiece;

                if (copyPiece.Name == PieceName.King)
                {
                    if (copyPiece.Color == PieceColor.White)
                    {
                        WhiteKing = (King)copyPiece;
                    }
                    else
                    {
                        BlackKing = (King)copyPiece;
                    }
                }
            }

            _removedPieces = new(sourceBoard._removedPieces.Reverse().Select(sourcePiece =>
            {
                var copyPiece = ChessPiece.GetNewPiece(sourcePiece.Name, sourcePiece.Color);
                copyPiece.FirstMoveMoment = sourcePiece.FirstMoveMoment;
                return copyPiece;
            }));
        }

        private void IncreaseModCount()
        {
            if (ModCount == ulong.MaxValue)
            {
                throw new OverflowException("Переполнение ModCount.");
            }

            ++ModCount;
        }

        private void IncreaseGamesCount()
        {
            if (GamesCount == ulong.MaxValue)
            {
                throw new OverflowException("Переполнение числа партий.");
            }

            ++GamesCount;
            ModCount = 0;
        }

        private void IncreasePiecesCount(PieceColor color)
        {
            if (color == PieceColor.White)
            {
                ++WhitePiecesCount;
                return;
            }

            ++BlackPiecesCount;
        }

        private void DecreasePiecesCount(PieceColor color)
        {
            if (color == PieceColor.White)
            {
                --WhitePiecesCount;
                return;
            }

            --BlackPiecesCount;
        }

        public IEnumerable<Square> GetSquares()
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    yield return _board[i, j];
                }
            }
        }

        public IEnumerable<ChessPiece> GetMaterial(PieceColor color)
        {
            int totalPiecesCount;
            ulong gamesCount;
            ulong modCount;

            lock (Locker)
            {
                totalPiecesCount = color == PieceColor.White ? WhitePiecesCount : BlackPiecesCount;

                if (totalPiecesCount == 0)
                {
                    yield break;
                }

                gamesCount = GamesCount;
                modCount = ModCount;
            }

            var count = 0;

            foreach (var square in GetSquares())
            {
                var piece = square.Contained;

                if (piece?.Color != color)
                {
                    continue;
                }

                if (ModCount != modCount || GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Позиция была изменена во время перечисления материала.");
                }

                yield return piece;
                ++count;

                if (count == totalPiecesCount)
                {
                    yield break;
                }
            }

            throw new InvalidOperationException("Позиция была изменена во время перечисления материала.");
        }

        public IEnumerable<ChessPiece> GetMaterial()
        {
            int totalPiecesCount;
            ulong gamesCount;
            ulong modCount;

            lock (Locker)
            {
                totalPiecesCount = WhitePiecesCount + BlackPiecesCount;

                if (totalPiecesCount == 0)
                {
                    yield break;
                }

                gamesCount = GamesCount;
                modCount = ModCount;
            }

            var count = 0;

            foreach (var square in GetSquares())
            {
                var piece = square.Contained;

                if (piece == null)
                {
                    continue;
                }

                if (ModCount != modCount || GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Позиция была изменена во время перечисления материала.");
                }

                yield return piece;
                ++count;

                if (count == totalPiecesCount)
                {
                    yield break;
                }
            }

            throw new InvalidOperationException("Позиция была изменена во время перечисления материала.");
        }

        public void Clear()
        {
            lock (Locker)
            {
                IncreaseGamesCount();

                foreach (var square in GetSquares())
                {
                    if (!square.IsClear)
                    {
                        square.Contained.Square = null;
                        square.Contained = null;
                    }

                    square.WhiteMenaces.Clear();
                    square.BlackMenaces.Clear();
                }

                _removedPieces.Clear();
                GameStartPosition = null;
                LastMove = null;
                WhitePiecesCount = 0;
                BlackPiecesCount = 0;
                WhiteKing = null;
                BlackKing = null;
                MovesAfterCaptureOrPawnMoveCount = 0;
                PawnPassedSquare = null;
                Status = BoardStatus.Clear;
                DrawReason = DrawReason.None;
            }

            PositionSet?.Invoke();
        }

        public void SetPosition(GamePosition position)
        {
            if (!position.IsLegal())
            {
                if (position.IsClear())
                {
                    Clear();
                    return;
                }

                throw new ArgumentException("Невозможная по правилам позиция.");
            }

            lock (Locker)
            {
                IsSettingPosition = true;
                IncreaseGamesCount();
                WhitePiecesCount = 0;
                BlackPiecesCount = 0;

                foreach (var square in GetSquares())
                {
                    square.WhiteMenaces.Clear();
                    square.BlackMenaces.Clear();

                    if (!square.IsClear)
                    {
                        square.Contained.Square = null;
                    }

                    if (position.PieceNames[square.X, square.Y] == null)
                    {
                        square.Contained = null;
                        continue;
                    }

                    var newPieceName = (PieceName)position.PieceNames[square.X, square.Y];
                    var newPieceColor = (PieceColor)position.PieceColors[square.X, square.Y];
                    var newPiece = ChessPiece.GetNewPiece(newPieceName, newPieceColor);
                    newPiece.Square = square;
                    square.Contained = newPiece;
                    IncreasePiecesCount(newPieceColor);

                    if (newPieceName == PieceName.King)
                    {
                        if (newPieceColor == PieceColor.White)
                        {
                            WhiteKing = (King)newPiece;
                        }
                        else
                        {
                            BlackKing = (King)newPiece;
                        }
                    }
                }

                _removedPieces.Clear();
                GameStartPosition = position;
                LastMove = null;
                MoveTurn = position.MoveTurn;
                MovesAfterCaptureOrPawnMoveCount = 0;
                PawnPassedSquare = null;

                foreach (var piece in GetMaterial())
                {
                    piece.AddMenaces();
                }

                if (HasInsufficientMaterial())
                {
                    Status = BoardStatus.Draw;
                    DrawReason = DrawReason.InsufficientMaterial;
                }
                else if (GetMaterial(MoveTurn).Any(piece => piece.CanMove()))
                {
                    Status = BoardStatus.GameIncomplete;
                    DrawReason = DrawReason.None;
                }
                else if (WhiteKing.IsChecked)
                {
                    Status = BoardStatus.BlackWon;
                    DrawReason = DrawReason.None;
                }
                else if (BlackKing.IsChecked)
                {
                    Status = BoardStatus.WhiteWon;
                    DrawReason = DrawReason.None;
                }
                else
                {
                    Status = BoardStatus.Draw;
                    DrawReason = DrawReason.Stalemate;
                }

                IsSettingPosition = false;
            }

            PositionSet?.Invoke();
        }

        public void SetPosition(IEnumerable<PieceName> whiteMaterial, IEnumerable<string> whitePositions,
        IEnumerable<PieceName> blackMaterial, IEnumerable<string> blackPositions, PieceColor moveTurn) =>
        SetPosition(new GamePosition(whiteMaterial, whitePositions, blackMaterial, blackPositions, moveTurn));

        private bool HasInsufficientMaterial()
        {
            if (LastMove != null && !LastMove.IsCapture && !LastMove.IsPawnPromotion)
            {
                return false;
            }

            var knightsPresent = false;
            var lightSquaredBishopsPresent = false;
            var darkSquaredBishopsPresent = false;

            foreach (var piece in GetMaterial())
            {
                switch (piece.Name)
                {
                    case PieceName.King:
                        {
                            break;
                        }

                    case PieceName.Knight:
                        {
                            if (knightsPresent || lightSquaredBishopsPresent || darkSquaredBishopsPresent)
                            {
                                return false;
                            }

                            knightsPresent = true;
                            break;
                        }

                    case PieceName.Bishop:
                        {
                            if (knightsPresent)
                            {
                                return false;
                            }

                            if (piece.X % 2 != piece.Y % 2)
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

                    default:
                        {
                            return false;
                        }
                };
            }

            return true;
        }

        private bool HasThreePositionRepeats()
        {
            if (MovesAfterCaptureOrPawnMoveCount < 8)
            {
                return false;
            }

            var current = new GamePosition(this);
            var other = new GamePosition(current);

            var positionRepeatsCount = 1;
            var skipsCount = 3;
            var movesLeftCount = MovesAfterCaptureOrPawnMoveCount;

            foreach (var move in LastMove.GetPrecedingMoves().Prepend(LastMove).Take(MovesAfterCaptureOrPawnMoveCount))
            {
                other.ToPreceding(move);

                if (skipsCount > 0)
                {
                    --skipsCount;
                }
                else if (other.EqualsInProperties(current))
                {
                    ++positionRepeatsCount;

                    if (positionRepeatsCount == 3)
                    {
                        return true;
                    }

                    skipsCount = 3;
                }

                --movesLeftCount;

                if (movesLeftCount < 5 && positionRepeatsCount == 1)
                {
                    return false;
                }
            }

            return false;
        }

        public void MakeMove(Move move)
        {
            lock (Locker)
            {
                if (Status != BoardStatus.GameIncomplete)
                {
                    throw new InvalidOperationException("На доске не идет партия.");
                }

                if (LastMove == null)
                {
                    if (GameStartPosition != move.Precedent as GamePosition)
                    {
                        throw new ArgumentException("Ход был создан для другой позиции.");
                    }
                }
                else
                {
                    if (LastMove != move.PrecedingMove)
                    {
                        throw new ArgumentException("Ход был создан для другой позиции.");
                    }
                }

                if (!IsLegal(move, out var exception))
                {
                    throw exception;
                }

                var modCount = ModCount;
                var gamesCount = GamesCount;
                MakingMove?.Invoke(move);

                if (ModCount != modCount || GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Обработчики события MakingMove не могут менять позицию на доске.");
                }

                IncreaseModCount();
                LastMove = move;
                MoveTurn = MoveTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
                var movingPiece = GetPiece(move.Start);
                var destinationSquare = this[move.Destination];

                if (movingPiece.FirstMoveMoment == 0)
                {
                    movingPiece.FirstMoveMoment = move.Depth;
                }

                if (move.IsCapture)
                {
                    DecreasePiecesCount(MoveTurn);
                    MovesAfterCaptureOrPawnMoveCount = 0;
                    PawnPassedSquare = null;

                    if (move.IsEnPassantCapture)
                    {
                        var capturedPawn = GetPiece(move.Destination.X, move.Start.Y);
                        _removedPieces.Push(capturedPawn);
                        movingPiece.MoveTo(destinationSquare);
                        capturedPawn.Remove();
                    }
                    else  //Взятие не на проходе.
                    {
                        _removedPieces.Push(destinationSquare.Contained);

                        if (move.IsPawnPromotion) //Превращ. со взятием.
                        {
                            _removedPieces.Push(movingPiece);
                            movingPiece.Remove();

                            var capturedPiece = destinationSquare.Contained;
                            var newPiece = ChessPiece.GetNewPiece((PieceName)move.NewPieceName, movingPiece.Color);

                            capturedPiece.RemoveMenaces();
                            capturedPiece.Square = null;
                            newPiece.Square = destinationSquare;
                            destinationSquare.Contained = newPiece;
                            newPiece.AddMenaces();
                        }
                        else  //Простое взятие: не на проходе и без превращ..
                        {
                            movingPiece.CaptureAt(destinationSquare);
                        }
                    }
                }
                else  //Не взятие.
                {
                    if (move.IsPawnPromotion) //Простое превращ-е: без взятия.
                    {
                        MovesAfterCaptureOrPawnMoveCount = 0;
                        PawnPassedSquare = null;
                        _removedPieces.Push(movingPiece);
                        movingPiece.Remove();
                        var newPiece = ChessPiece.GetNewPiece((PieceName)move.NewPieceName, movingPiece.Color);
                        newPiece.MoveTo(destinationSquare);
                    }
                    else if (move.IsKingsideCastling)
                    {
                        ++MovesAfterCaptureOrPawnMoveCount;
                        PawnPassedSquare = null;
                        var king = MoveTurn == PieceColor.White ? BlackKing : WhiteKing;
                        king.CastleKingside();
                    }
                    else if (move.IsQueensideCastling)
                    {
                        ++MovesAfterCaptureOrPawnMoveCount;
                        PawnPassedSquare = null;
                        var king = MoveTurn == PieceColor.White ? BlackKing : WhiteKing;
                        king.CastleQueenside();
                    }
                    else  //Простой ход: не взятие, не превращ. и не рокировка.
                    {
                        MovesAfterCaptureOrPawnMoveCount = move.IsPawnMove ? 0 : MovesAfterCaptureOrPawnMoveCount + 1;
                        PawnPassedSquare = GetPawnPassedSquare();
                        movingPiece.MoveTo(destinationSquare);
                    }
                }

                CheckGameResult();
            }

            MoveMade?.Invoke();
        }

        public void MakeMove(int startX, int startY, int destinationX, int destinationY) =>
        MakeMove(new(GetPiece(startX, startY), _board[destinationX, destinationY]));

        public void MakeMove(int startX, int startY, int destinationX, int destinationY, PieceName newPieceName) =>
        MakeMove(new(GetPiece(startX, startY), _board[destinationX, destinationY], newPieceName));

        private bool IsLegal(Move move, out IllegalMoveException exception)
        {
            if (move.IsKingsideCastling)
            {
                var king = MoveTurn == PieceColor.White ? WhiteKing : BlackKing;
                return king.CanCastleKingside(out exception);
            }

            if (move.IsQueensideCastling)
            {
                var king = MoveTurn == PieceColor.White ? WhiteKing : BlackKing;
                return king.CanCastleQueenside(out exception);
            }

            if (!GetPiece(move.Start).CanMoveTo(this[move.Destination], out exception))
            {
                return false;
            }

            if (move.IsPawnMove)
            {
                if (move.Destination.Y == 0 || move.Destination.Y == 7)
                {
                    if (!move.IsPawnPromotion)
                    {
                        exception = new NewPieceNotSelectedException();
                        return false;
                    }
                }
                else
                {
                    if (move.IsPawnPromotion)
                    {
                        exception = new();
                        return false;
                    }
                }
            }

            return true;
        }

        private Square GetPawnPassedSquare()
        {
            if (LastMove == null || !LastMove.IsPawnJump)
            {
                return null;
            }

            var x = LastMove.Start.X;
            var y = LastMove.MovingPieceColor == PieceColor.White ? 2 : 5;
            return _board[x, y];
        }

        private void CheckGameResult()
        {
            if (!GetMaterial(MoveTurn).Any(piece => piece.CanMove()))
            {
                if (MoveTurn == PieceColor.White)
                {
                    if (WhiteKing.IsChecked)
                    {
                        Status = BoardStatus.BlackWon;
                        return;
                    }
                }
                else
                {
                    if (BlackKing.IsChecked)
                    {
                        Status = BoardStatus.WhiteWon;
                        return;
                    }
                }

                Status = BoardStatus.Draw;
                DrawReason = DrawReason.Stalemate;
                return;
            }

            if (HasInsufficientMaterial())
            {
                Status = BoardStatus.Draw;
                DrawReason = DrawReason.InsufficientMaterial;
                return;
            }

            if (HasThreePositionRepeats())
            {
                Status = BoardStatus.Draw;
                DrawReason = DrawReason.ThreeRepeatsRule;
                return;
            }

            if (MovesAfterCaptureOrPawnMoveCount == 100)
            {
                Status = BoardStatus.Draw;
                DrawReason = DrawReason.FiftyMovesRule;
            }
        }

        public void CancelMove()
        {
            Move cancelledMove;

            lock (Locker)
            {
                if (LastMove == null)
                {
                    throw new InvalidOperationException("Невозможно взять ход обратно: на доске начальная позиция партии.");
                }

                var modCount = ModCount;
                var gamesCount = GamesCount;
                CancellingMove?.Invoke();

                if (ModCount != modCount || GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Обработчики события TakingBackMove не могут менять позицию на доске.");
                }

                IncreaseModCount();
                cancelledMove = LastMove;
                LastMove = LastMove.PrecedingMove;
                MoveTurn = MoveTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
                var startSquare = this[cancelledMove.Start];
                var destinationSquare = this[cancelledMove.Destination];

                if (cancelledMove.IsPawnPromotion)
                {
                    MovesAfterCaptureOrPawnMoveCount = cancelledMove.GetPrecedingMoves().
                    TakeWhile(move => !move.IsCapture && !move.IsPawnMove).Count();

                    var pawn = _removedPieces.Pop();

                    if (LastMove == null || pawn.FirstMoveMoment > LastMove.Depth)
                    {
                        pawn.FirstMoveMoment = 0;
                    }

                    pawn.MoveTo(startSquare);

                    if (cancelledMove.IsCapture)  //Превращ. со взятием.
                    {
                        var capturedPiece = _removedPieces.Pop();
                        IncreasePiecesCount(capturedPiece.Color);
                        var newPiece = destinationSquare.Contained;

                        newPiece.RemoveMenaces();
                        newPiece.Square = null;
                        capturedPiece.Square = destinationSquare;
                        destinationSquare.Contained = capturedPiece;
                        capturedPiece.AddMenaces();
                    }
                    else //Простое превращ-е: без взятия.
                    {
                        destinationSquare.Clear();
                    }
                }
                else  //Не превращение.
                {
                    var movingPiece = destinationSquare.Contained;

                    if (LastMove == null || movingPiece.FirstMoveMoment > LastMove.Depth)
                    {
                        movingPiece.FirstMoveMoment = 0;
                    }

                    if (cancelledMove.IsCapture) //Взятие без превращения.
                    {
                        MovesAfterCaptureOrPawnMoveCount = cancelledMove.GetPrecedingMoves().
                        TakeWhile(move => !move.IsCapture && !move.IsPawnMove).Count();

                        var capturedPiece = _removedPieces.Pop();
                        IncreasePiecesCount(capturedPiece.Color);

                        if (cancelledMove.IsEnPassantCapture)
                        {
                            var capturedPawnPosition = _board[destinationSquare.X, startSquare.Y];
                            movingPiece.MoveTo(startSquare);
                            capturedPiece.MoveTo(capturedPawnPosition);
                        }
                        else //Простое взятие: не на проходе и без превращения.
                        {
                            movingPiece.RemoveExcessMenaces(startSquare);
                            movingPiece.Square = startSquare;
                            startSquare.Contained = movingPiece;
                            capturedPiece.Square = destinationSquare;
                            destinationSquare.Contained = capturedPiece;
                            startSquare.BlockLines();
                            movingPiece.AddMissingMenaces(destinationSquare);

                            if (movingPiece.IsLongRanged)
                            {
                                movingPiece.BlockLine(destinationSquare);
                            }

                            capturedPiece.AddMenaces();
                        }
                    }
                    else if (cancelledMove.IsKingsideCastling)
                    {
                        --MovesAfterCaptureOrPawnMoveCount;
                        var king = MoveTurn == PieceColor.White ? WhiteKing : BlackKing;
                        king.CancelKingsideCastling();
                    }
                    else if (cancelledMove.IsQueensideCastling)
                    {
                        --MovesAfterCaptureOrPawnMoveCount;
                        var king = MoveTurn == PieceColor.White ? WhiteKing : BlackKing;
                        king.CancelQueensideCastling();
                    }
                    else //Простой ход: не взятие, не превращ-е и не рокировка.
                    {
                        MovesAfterCaptureOrPawnMoveCount = MovesAfterCaptureOrPawnMoveCount > 0 ?
                        MovesAfterCaptureOrPawnMoveCount - 1 :
                        cancelledMove.GetPrecedingMoves().TakeWhile(move => !move.IsCapture && !move.IsPawnMove).Count();

                        movingPiece.MoveTo(startSquare);
                    }
                }

                PawnPassedSquare = GetPawnPassedSquare();
                Status = BoardStatus.GameIncomplete;
                DrawReason = DrawReason.None;
            }

            MoveCancelled?.Invoke(cancelledMove);
        }

        public IEnumerable<Move> GetLegalMoves()
        {
            PieceColor moveTurn;
            ulong gamesCount;
            ulong modCount;
            int totalPiecesCount;

            lock (Locker)
            {
                if (Status != BoardStatus.GameIncomplete)
                {
                    yield break;
                }

                moveTurn = MoveTurn;
                gamesCount = GamesCount;
                modCount = ModCount;
                totalPiecesCount = moveTurn == PieceColor.White ? WhitePiecesCount : BlackPiecesCount;
            }

            var newPieceNames = new PieceName[] { PieceName.Queen, PieceName.Rook, PieceName.Knight, PieceName.Bishop };
            var count = 0;

            foreach (var piece in GetMaterial(moveTurn))
            {
                ++count;
                var squares = piece.GetAccessibleSquares();

                if (ModCount != modCount || GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                foreach (var sq in squares)
                {
                    if (piece.Name != PieceName.Pawn || !(sq.Y == 0 || sq.Y == 7))
                    {
                        Move move;

                        try
                        {
                            move = new(piece, sq);
                        }

                        catch
                        {
                            throw new InvalidOperationException("Изменение позиции во время перечисления.");
                        }

                        if (ModCount != modCount || GamesCount != gamesCount)
                        {
                            throw new InvalidOperationException("Изменение позиции во время перечисления.");
                        }

                        yield return move;
                        continue;
                    }

                    foreach (var name in newPieceNames)
                    {
                        Move move;

                        try
                        {
                            move = new(piece, sq, name);
                        }

                        catch
                        {
                            throw new InvalidOperationException("Изменение позиции во время перечисления.");
                        }

                        if (ModCount != modCount || GamesCount != gamesCount)
                        {
                            throw new InvalidOperationException("Изменение позиции во время перечисления.");
                        }

                        yield return move;
                    }
                }
            }

            if (count < totalPiecesCount)
            {
                throw new InvalidOperationException("Изменение позиции во время перечисления.");
            }
        }

        public void BreakGame(BoardStatus gameResult)
        {
            lock (Locker)
            {
                if (Status != BoardStatus.GameIncomplete)
                {
                    throw new InvalidOperationException("На доске не идет партия.");
                }

                if (gameResult != BoardStatus.WhiteWon && gameResult != BoardStatus.BlackWon && gameResult != BoardStatus.Draw)
                {
                    throw new ArgumentException();
                }

                IncreaseModCount();
                Status = gameResult;
            }
        }

        public int MovesCount
        {
            get
            {
                var lastMove = LastMove;
                return lastMove == null ? 0 : lastMove.Depth;
            }
        }

        public event Action PositionSet;
        public event Action<Move> MakingMove;
        public event Action MoveMade;
        public event Action CancellingMove;
        public event Action<Move> MoveCancelled;
    }
}