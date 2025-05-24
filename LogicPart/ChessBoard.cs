
namespace Chess.LogicPart
{
    public class ChessBoard
    {
        private readonly Square[,] _board = new Square[8, 8];
        private readonly Stack<ChessPiece> _removedPieces = new();

        public GamePosition GameStartPosition { get; private set; }

        public Move LastMove { get; private set; }

        public PieceColor MoveTurn { get; private set; }

        public King WhiteKing { get; private set; }

        public King BlackKing { get; private set; }

        public int MovesAfterCaptureOrPawnMoveCount { get; private set; }

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

        public IEnumerable<ChessPiece> GetMaterial()
        {
            ulong gamesCount;
            ulong modCount;

            lock (Locker)
            {
                gamesCount = GamesCount;
                modCount = ModCount;
            }

            ChessPiece previous = null;

            foreach (var square in GetSquares())
            {
                var current = square.Contained;

                if (current == null)
                {
                    continue;
                }

                if (previous == null)
                {
                    previous = current;
                    continue;
                }

                if (ModCount != modCount || GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Позиция была изменена во время перечисления материала.");
                }

                yield return previous;
                previous = current;
            }

            if (ModCount != modCount || GamesCount != gamesCount)
            {
                throw new InvalidOperationException("Позиция была изменена во время перечисления материала.");
            }

            if (previous != null)
            {
                yield return previous;
            }
        }

        public IEnumerable<ChessPiece> GetMaterial(PieceColor color) => GetMaterial().Where(piece => piece.Color == color);

        public IEnumerable<Move> GetLegalMoves()
        {
            ulong gamesCount;
            ulong modCount;

            lock (Locker)
            {
                if (Status != BoardStatus.GameIncomplete)
                {
                    yield break;
                }

                gamesCount = GamesCount;
                modCount = ModCount;
            }

            Move current;
            Move previous = null;

            foreach (var piece in GetMaterial(MoveTurn))
            {
                foreach (var square in piece.GetAccessibleSquares())
                {
                    if (piece.Name != PieceName.Pawn || (square.Y != 0 && square.Y != 7))
                    {
                        try
                        {
                            current = new(piece, square);
                        }

                        catch (ArgumentException)
                        {
                            throw new InvalidOperationException("Изменение позиции во время перечисления.");
                        }

                        if (previous == null)
                        {
                            previous = current;
                            continue;
                        }

                        if (ModCount != modCount || GamesCount != gamesCount)
                        {
                            throw new InvalidOperationException("Изменение позиции во время перечисления.");
                        }

                        yield return previous;
                        previous = current;
                        continue;
                    }

                    var newPieceNames = new PieceName[] { PieceName.Queen, PieceName.Rook, PieceName.Knight, PieceName.Bishop };

                    foreach (var name in newPieceNames)
                    {
                        try
                        {
                            current = new((Pawn)piece, square, name);
                        }

                        catch (ArgumentException)
                        {
                            throw new InvalidOperationException("Изменение позиции во время перечисления.");
                        }

                        if (previous == null)
                        {
                            previous = current;
                            continue;
                        }

                        if (ModCount != modCount || GamesCount != gamesCount)
                        {
                            throw new InvalidOperationException("Изменение позиции во время перечисления.");
                        }

                        yield return previous;
                        previous = current;
                    }
                }
            }

            if (ModCount != modCount || GamesCount != gamesCount)
            {
                throw new InvalidOperationException("Изменение позиции во время перечисления.");
            }

            yield return previous;
        }

        public void SetPosition(GamePosition position)
        {
            if (position == null)
            {
                throw new ArgumentNullException();
            }

            if (position.IsClear())
            {
                Clear();
                return;
            }

            if (!position.IsLegal())
            {
                throw new ArgumentException("Невозможная по правилам позиция.");
            }

            lock (Locker)
            {
                if (GamesCount == ulong.MaxValue)
                {
                    throw new OverflowException("Переполнение числа партий.");
                }

                InvokeSettingPositonEvents();
                ++GamesCount;
                ModCount = 0;

                foreach (var square in GetSquares())
                {
                    square.RemoveAllMenaces();

                    if (!square.IsClear)
                    {
                        square.Contained.Square = null;
                    }

                    if (!position.HasPieceAt(square.Location))
                    {
                        square.Contained = null;
                        continue;
                    }

                    var newPieceName = (PieceName)position.GetPieceName(square.Location);
                    var newPieceColor = (PieceColor)position.GetPieceColor(square.Location);
                    var newPiece = ChessPiece.GetNewPiece(newPieceName, newPieceColor);
                    newPiece.Square = square;
                    square.Contained = newPiece;

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

                foreach (var piece in GetMaterial())
                {
                    piece.AddMenaces();
                }

                _removedPieces.Clear();
                GameStartPosition = position;
                LastMove = null;
                MoveTurn = position.MoveTurn;
                MovesAfterCaptureOrPawnMoveCount = 0;

                if (!GetMaterial(MoveTurn).Any(piece => piece.CanMove()))
                {
                    if (MoveTurn == PieceColor.White)
                    {
                        if (WhiteKing.Square.IsMenacedBy(PieceColor.Black))
                        {
                            Status = BoardStatus.BlackWon;
                            DrawReason = DrawReason.None;
                        }
                        else
                        {
                            Status = BoardStatus.Draw;
                            DrawReason = DrawReason.Stalemate;
                        }
                    }
                    else
                    {
                        if (BlackKing.Square.IsMenacedBy(PieceColor.White))
                        {
                            Status = BoardStatus.WhiteWon;
                            DrawReason = DrawReason.None;
                        }
                        else
                        {
                            Status = BoardStatus.Draw;
                            DrawReason = DrawReason.Stalemate;
                        }
                    }
                }
                else if (LacksMaterial())
                {
                    Status = BoardStatus.Draw;
                    DrawReason = DrawReason.MaterialLack;
                }
                else
                {
                    Status = BoardStatus.GameIncomplete;
                    DrawReason = DrawReason.None;
                }
            }

            PositionChanged?.Invoke();
            PositionSet?.Invoke();
        }

        public void SetPosition(IEnumerable<PieceName> whiteMaterial, IEnumerable<string> whitePositions,
        IEnumerable<PieceName> blackMaterial, IEnumerable<string> blackPositions, PieceColor moveTurn)
        {
            var position = new GamePosition(whiteMaterial, whitePositions, blackMaterial, blackPositions, moveTurn);
            SetPosition(position);
        }

        public void Clear()
        {
            lock (Locker)
            {
                if (Status == BoardStatus.Clear)
                {
                    return;
                }

                if (ModCount == ulong.MaxValue)
                {
                    throw new OverflowException("Переполнение ModCount.");
                }

                InvokeSettingPositonEvents();
                ++ModCount;

                foreach (var square in GetSquares())
                {
                    if (!square.IsClear)
                    {
                        square.Contained.Square = null;
                        square.Contained = null;
                    }

                    square.RemoveAllMenaces();
                }

                _removedPieces.Clear();
                GameStartPosition = null;
                LastMove = null;
                MoveTurn = default;
                WhiteKing = null;
                BlackKing = null;
                MovesAfterCaptureOrPawnMoveCount = 0;
                Status = BoardStatus.Clear;
                DrawReason = DrawReason.None;
            }

            PositionChanged?.Invoke();
            PositionSet?.Invoke();
        }

        private void InvokeSettingPositonEvents()
        {
            var modCount = ModCount;
            var gamesCount = GamesCount;
            ChangingPosition?.Invoke();

            if (ModCount != modCount || GamesCount != gamesCount)
            {
                throw new InvalidOperationException("Обработчики события ChangingPosition не могут менять позицию на доске.");
            }

            SettingPosition?.Invoke();

            if (ModCount != modCount || GamesCount != gamesCount)
            {
                throw new InvalidOperationException("Обработчики события SettingPosition не могут менять позицию на доске.");
            }
        }

        private bool LacksMaterial()
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

                            if (piece.X % 2 == piece.Y % 2)
                            {
                                darkSquaredBishopsPresent = true;
                            }
                            else
                            {
                                lightSquaredBishopsPresent = true;
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
                }
                ;
            }

            return true;
        }

        public void CopyGame(ChessBoard sourceBoard)
        {
            if (sourceBoard == null)
            {
                throw new ArgumentNullException();
            }

            lock (sourceBoard.Locker)
            {
                lock (Locker)
                {
                    if (sourceBoard.Status == BoardStatus.Clear)
                    {
                        Clear();
                        return;
                    }

                    if (GamesCount == ulong.MaxValue)
                    {
                        throw new OverflowException("Переполнение числа партий.");
                    }

                    InvokeSettingPositonEvents();
                    ++GamesCount;
                    ModCount = 0;

                    foreach (var square in GetSquares())
                    {
                        square.RemoveAllMenaces();

                        if (!square.IsClear)
                        {
                            square.Contained.Square = null;
                        }

                        var sourcePiece = sourceBoard.GetPiece(square.Location);

                        if (sourcePiece == null)
                        {
                            square.Contained = null;
                            continue;
                        }

                        var copyPiece = sourcePiece.Copy();
                        copyPiece.Square = square;
                        square.Contained = copyPiece;
                    }

                    foreach (var piece in GetMaterial())
                    {
                        piece.AddMenaces();
                    }

                    _removedPieces.Clear();

                    foreach (var piece in sourceBoard._removedPieces.Reverse())
                    {
                        _removedPieces.Push(piece.Copy());
                    }

                    GameStartPosition = sourceBoard.GameStartPosition;
                    LastMove = sourceBoard.LastMove;
                    MoveTurn = sourceBoard.MoveTurn;
                    WhiteKing = (King)GetPiece(sourceBoard.WhiteKing.Location);
                    BlackKing = (King)GetPiece(sourceBoard.BlackKing.Location);
                    MovesAfterCaptureOrPawnMoveCount = sourceBoard.MovesAfterCaptureOrPawnMoveCount;
                    Status = sourceBoard.Status;
                    DrawReason = sourceBoard.DrawReason;
                }
            }

            PositionChanged?.Invoke();
            PositionSet?.Invoke();
        }

        public void MakeMove(Move move)
        {
            lock (Locker)
            {
                if (move == null)
                {
                    throw new ArgumentNullException();
                }

                if (ModCount == ulong.MaxValue)
                {
                    throw new OverflowException("Переполнение ModCount.");
                }

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

                if (!CheckLegacy(move, out var exceptionType))
                {
                    var constructor = exceptionType.GetConstructor(Array.Empty<Type>());
                    throw (IllegalMoveException)constructor.Invoke(null);
                }

                var modCount = ModCount;
                var gamesCount = GamesCount;
                ChangingPosition?.Invoke();

                if (ModCount != modCount || GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Обработчики события ChangingPosition не могут менять позицию на доске.");
                }

                MakingMove?.Invoke(move);

                if (ModCount != modCount || GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Обработчики события MakingMove не могут менять позицию на доске.");
                }

                ++ModCount;
                LastMove = move;
                MoveTurn = MoveTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;

                if (move.IsCastling)
                {
                    ++MovesAfterCaptureOrPawnMoveCount;
                    var king = move.MovingPieceColor == PieceColor.White ? WhiteKing : BlackKing;
                    king.FirstMoveDepth = move.Depth;

                    if (move.Destination.X == 6)
                    {
                        king.CastleKingside();
                    }
                    else
                    {
                        king.CastleQueenside();
                    }
                }
                else //Не рокировка.
                {
                    var movingPiece = GetPiece(move.Start);
                    var destinationSquare = this[move.Destination];

                    if (!movingPiece.HasMoved)
                    {
                        movingPiece.FirstMoveDepth = move.Depth;
                    }

                    if (move.IsCapture) //Взятие.
                    {
                        MovesAfterCaptureOrPawnMoveCount = 0;

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
                                movingPiece.PromoteAt(destinationSquare, (PieceName)move.NewPieceName);
                            }
                            else  //Простое взятие: не на проходе и без превращ..
                            {
                                movingPiece.CaptureAt(destinationSquare);
                            }
                        }
                    }
                    else if (move.IsPawnPromotion) //Простое превращ-е: без взятия.
                    {
                        MovesAfterCaptureOrPawnMoveCount = 0;
                        _removedPieces.Push(movingPiece);
                        movingPiece.PromoteAt(destinationSquare, (PieceName)move.NewPieceName);
                    }
                    else  //Простой ход: не взятие, не превращ. и не рокировка.
                    {
                        MovesAfterCaptureOrPawnMoveCount = move.IsPawnMove ? 0 : MovesAfterCaptureOrPawnMoveCount + 1;
                        movingPiece.MoveTo(destinationSquare);
                    }
                }

                CheckGameResult();
            }

            PositionChanged?.Invoke();
            MoveMade?.Invoke();
        }

        public void MakeMove(int startX, int startY, int destinationX, int destinationY)
        {
            lock (Locker)
            {
                var piece = GetPiece(startX, startY);
                var square = _board[destinationX, destinationY];
                var move = new Move(piece, square);
                MakeMove(move);
            }
        }

        public void MakeMove(int startX, int startY, int destinationX, int destinationY, PieceName newPieceName)
        {
            lock (Locker)
            {
                var piece = GetPiece(startX, startY);

                if (piece == null)
                {
                    throw new ArgumentException("Поле с координатами [" + nameof(startX) + ", " + nameof(startY) + "] пусто.");
                }

                if (piece.Name != PieceName.Pawn)
                {
                    throw new ArgumentException("Превращаться в фигуру может только пешка.");
                }

                var square = _board[destinationX, destinationY];
                var move = new Move((Pawn)piece, square, newPieceName);
                MakeMove(move);
            }
        }

        private bool CheckLegacy(Move move, out Type exceptionType)
        {
            if (move.IsCastling)
            {
                var king = move.MovingPieceColor == PieceColor.White ? WhiteKing : BlackKing;

                if (move.Destination.X == 6)
                {
                    return king.CheckKingsideCastlingLegacy(out exceptionType);
                }

                return king.CheckQueensideCastlingLegacy(out exceptionType);
            }

            if (!GetPiece(move.Start).CanMoveTo(this[move.Destination], out var t))
            {
                exceptionType = t;
                return false;
            }

            if (move.IsPawnMove && (move.Destination.Y == 0 || move.Destination.Y == 7) && !move.IsPawnPromotion)
            {
                exceptionType = typeof(NewPieceNotSelectedException);
                return false;
            }

            exceptionType = null;
            return true;
        }

        private void CheckGameResult()
        {
            if (!GetMaterial(MoveTurn).Any(piece => piece.CanMove()))
            {
                if (MoveTurn == PieceColor.White)
                {
                    if (WhiteKing.Square.IsMenacedBy(PieceColor.Black))
                    {
                        Status = BoardStatus.BlackWon;
                        return;
                    }
                }
                else
                {
                    if (BlackKing.Square.IsMenacedBy(PieceColor.White))
                    {
                        Status = BoardStatus.WhiteWon;
                        return;
                    }
                }

                Status = BoardStatus.Draw;
                DrawReason = DrawReason.Stalemate;
                return;
            }

            if (LacksMaterial())
            {
                Status = BoardStatus.Draw;
                DrawReason = DrawReason.MaterialLack;
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

        private bool HasThreePositionRepeats()
        {
            if (MovesAfterCaptureOrPawnMoveCount < 8)
            {
                return false;
            }

            var position = new GamePosition(this);
            var positionRepeatsCount = 1;
            var skipsCount = 3;
            var movesLeftCount = MovesAfterCaptureOrPawnMoveCount;

            foreach (var move in LastMove.GetPrecedingMoves().Prepend(LastMove).Take(MovesAfterCaptureOrPawnMoveCount))
            {
                position.ToPreceding(move);

                if (skipsCount > 0)
                {
                    --skipsCount;
                }
                else if (HasEqualPosition(position))
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

        public bool HasEqualPosition(GamePosition position)
        {
            lock (Locker)
            {
                if (MoveTurn != position.MoveTurn)
                {
                    return false;
                }

                foreach (var square in GetSquares())
                {
                    var piece = square.Contained;

                    if (piece == null)
                    {
                        if (position.HasPieceAt(square.Location))
                        {
                            return false;
                        }

                        continue;
                    }

                    if (piece.Name != position.GetPieceName(square.Location) ||
                        piece.Color != position.GetPieceColor(square.Location))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public void CancelMove()
        {
            Move cancelledMove;

            lock (Locker)
            {
                if (ModCount == ulong.MaxValue)
                {
                    throw new OverflowException("Переполнение ModCount.");
                }

                if (LastMove == null)
                {
                    throw new InvalidOperationException("Невозможно взять ход обратно: на доске начальная позиция партии.");
                }

                var modCount = ModCount;
                var gamesCount = GamesCount;
                ChangingPosition?.Invoke();

                if (ModCount != modCount || GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Обработчики события ChangingPosition не могут менять позицию на доске.");
                }

                CancellingMove?.Invoke();

                if (ModCount != modCount || GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Обработчики события CancellingMove не могут менять позицию на доске.");
                }

                ++ModCount;
                cancelledMove = LastMove;
                LastMove = LastMove.PrecedingMove;
                MoveTurn = MoveTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;

                if (cancelledMove.IsCastling)
                {
                    --MovesAfterCaptureOrPawnMoveCount;
                    var king = cancelledMove.MovingPieceColor == PieceColor.White ? WhiteKing : BlackKing;
                    king.FirstMoveDepth = 0;

                    if (cancelledMove.Destination.X == 6)
                    {
                        king.CancelKingsideCastling();
                    }
                    else
                    {
                        king.CancelQueensideCastling();
                    }
                }
                else //Не рокировка.
                {
                    var startSquare = this[cancelledMove.Start];
                    var destinationSquare = this[cancelledMove.Destination];

                    if (cancelledMove.IsPawnPromotion) //Превращение.
                    {
                        MovesAfterCaptureOrPawnMoveCount = cancelledMove.GetPrecedingMoves().
                        TakeWhile(move => !move.IsCapture && !move.IsPawnMove).Count();

                        var pawn = _removedPieces.Pop();

                        if (pawn.FirstMoveDepth == cancelledMove.Depth)
                        {
                            pawn.FirstMoveDepth = 0;
                        }

                        if (cancelledMove.IsCapture)  //Превращ. со взятием.
                        {
                            var capturedPiece = _removedPieces.Pop();
                            var newPiece = destinationSquare.Contained;
                            pawn.MoveTo(startSquare);
                            newPiece.RemoveMenaces();
                            newPiece.Square = null;
                            capturedPiece.Square = destinationSquare;
                            destinationSquare.Contained = capturedPiece;
                            capturedPiece.AddMenaces();
                        }
                        else //Простое превращ-е: без взятия.
                        {
                            destinationSquare.Clear();
                            pawn.MoveTo(startSquare);
                        }
                    }
                    else  //Не превращение и не рокировка.
                    {
                        var movingPiece = destinationSquare.Contained;

                        if (movingPiece.FirstMoveDepth == cancelledMove.Depth)
                        {
                            movingPiece.FirstMoveDepth = 0;
                        }

                        if (cancelledMove.IsCapture) //Взятие без превращения.
                        {
                            MovesAfterCaptureOrPawnMoveCount = cancelledMove.GetPrecedingMoves().
                            TakeWhile(move => !move.IsCapture && !move.IsPawnMove).Count();

                            var capturedPiece = _removedPieces.Pop();

                            if (cancelledMove.IsEnPassantCapture) //Взятие на проходе.
                            {
                                var capturedPawnPosition = _board[destinationSquare.X, startSquare.Y];
                                movingPiece.MoveTo(startSquare);
                                capturedPiece.MoveTo(capturedPawnPosition);
                            }
                            else //Простое взятие: не на проходе и без превращения.
                            {
                                movingPiece.RemoveUnactualMenaces(startSquare);
                                movingPiece.Square = startSquare;
                                startSquare.Contained = movingPiece;
                                movingPiece.AddMissingMenaces(destinationSquare);
                                capturedPiece.Square = destinationSquare;
                                destinationSquare.Contained = capturedPiece;

                                if (movingPiece.IsLongRanged)
                                {
                                    movingPiece.BlockLine(destinationSquare);
                                }

                                startSquare.BlockLines();
                                capturedPiece.AddMenaces();
                            }
                        }
                        else //Простой ход: не взятие, не превращ-е и не рокировка.
                        {
                            MovesAfterCaptureOrPawnMoveCount = MovesAfterCaptureOrPawnMoveCount > 0 ?
                            MovesAfterCaptureOrPawnMoveCount - 1 :
                            cancelledMove.GetPrecedingMoves().TakeWhile(move => !move.IsCapture && !move.IsPawnMove).Count();

                            movingPiece.MoveTo(startSquare);
                        }
                    }
                }

                Status = BoardStatus.GameIncomplete;
                DrawReason = DrawReason.None;
            }

            PositionChanged?.Invoke();
            MoveCancelled?.Invoke(cancelledMove);
        }

        public void BreakGame(BoardStatus gameResult)
        {
            lock (Locker)
            {
                if (ModCount == ulong.MaxValue)
                {
                    throw new OverflowException("Переполнение ModCount.");
                }

                if (Status != BoardStatus.GameIncomplete)
                {
                    throw new InvalidOperationException("На доске не идет партия.");
                }

                if (gameResult != BoardStatus.WhiteWon && gameResult != BoardStatus.BlackWon && gameResult != BoardStatus.Draw)
                {
                    throw new ArgumentException();
                }

                var modCount = ModCount;
                var gamesCount = GamesCount;
                BreakingGame?.Invoke(gameResult);

                if (ModCount != modCount || GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Обработчики события BreakingGame не могут менять позицию на доске.");
                }

                ++ModCount;
                Status = gameResult;
                GameBroken?.Invoke();
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

        public event Action ChangingPosition;
        public event Action PositionChanged;
        public event Action SettingPosition;
        public event Action PositionSet;
        public event Action<Move> MakingMove;
        public event Action MoveMade;
        public event Action CancellingMove;
        public event Action<Move> MoveCancelled;
        public event Action<BoardStatus> BreakingGame;
        public event Action GameBroken;
    }
}