
namespace Chess.LogicPart
{
    public sealed class King : ChessPiece
    {
        internal King(PieceColor color) : base(color) { }

        public override IEnumerable<Square> GetAttackedSquares()
        {
            var board = Board;

            if (board == null)
            {
                yield break;
            }

            Square square;
            ulong gamesCount;
            ulong modCount;

            lock (board.Locker)
            {
                if (Board != board)
                {
                    yield break;
                }

                square = Square;
                gamesCount = board.GamesCount;
                modCount = board.ModCount;
            }

            for (var i = square.X > 0 ? square.X - 1 : 0; i <= square.X + 1 && i < 8; ++i)
            {
                for (var j = square.Y > 0 ? square.Y - 1 : 0; j <= square.Y + 1 && j < 8; ++j)
                {
                    if (i == square.X && j == square.Y)
                    {
                        continue;
                    }

                    if (board.ModCount != modCount || board.GamesCount != gamesCount)
                    {
                        throw new InvalidOperationException("Изменение позиции во время перечисления.");
                    }

                    yield return board[i, j];
                }
            }
        }

        internal override bool CanMove() => GetAttackedSquares().
        Any(sq => sq.Contained?.Color != Color && WillBeSafeAt(sq));

        private bool WillBeSafeAt(Square moveSquare)
        {
            var board = moveSquare.Board;

            lock (board.Locker)
            {
                if (moveSquare.IsMenacedBy(EnemyColor))
                {
                    return false;
                }

                if (Board != board || moveSquare == Square)
                {
                    return true;
                }

                foreach (var menace in Square.GetMenaces(EnemyColor).Where(m => m.IsLongRanged))
                {
                    if (moveSquare == menace.Square)
                    {
                        continue;
                    }

                    if (menace.X == X)
                    {
                        if (moveSquare.X != X)
                        {
                            continue;
                        }

                        var movesToUpper = moveSquare.Y > Y;
                        var moveSquareIsShielded = false;

                        for (var i = movesToUpper ? Y + 1 : Y - 1; i != moveSquare.Y; i = movesToUpper ? i + 1 : i - 1)
                        {
                            if (!board[X, i].IsClear)
                            {
                                moveSquareIsShielded = true;
                                break;
                            }
                        }

                        if (!moveSquareIsShielded)
                        {
                            return false;
                        }
                    }
                    else if (menace.Y == Y)
                    {
                        if (moveSquare.Y != Y)
                        {
                            continue;
                        }

                        var movesToRight = moveSquare.X > X;
                        var moveSquareIsShielded = false;

                        for (var i = movesToRight ? X + 1 : X - 1; i != moveSquare.X; i = movesToRight ? i + 1 : i - 1)
                        {
                            if (!board[i, Y].IsClear)
                            {
                                moveSquareIsShielded = true;
                                break;
                            }
                        }

                        if (!moveSquareIsShielded)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!moveSquare.IsOnSameDiagonal(Square) || !moveSquare.IsOnSameDiagonal(menace))
                        {
                            continue;
                        }

                        var movesToRight = moveSquare.X > X;
                        var movesToUpper = moveSquare.Y > Y;
                        var moveSquareIsShielded = false;

                        for (int i = movesToRight ? X + 1 : X - 1, j = movesToUpper ? Y + 1 : Y - 1;
                             i != moveSquare.X; i = movesToRight ? i + 1 : i - 1, j = movesToUpper ? j + 1 : j - 1)
                        {
                            if (!board[i, j].IsClear)
                            {
                                moveSquareIsShielded = true;
                                break;
                            }
                        }

                        if (!moveSquareIsShielded)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        internal override bool CanMoveTo(Square moveSquare, out Type exceptionType)
        {
            if (!Attacks(moveSquare))
            {
                if (!Move.IsCastlingMove(this, moveSquare))
                {
                    exceptionType = typeof(IllegalMoveException);
                    return false;
                }

                if (moveSquare.X == 6)
                {
                    return CheckKingsideCastlingLegacy(out exceptionType);
                }

                return CheckQueensideCastlingLegacy(out exceptionType);
            }

            if (!WillBeSafeAt(moveSquare))
            {
                exceptionType = typeof(KingMovesIntoCheckException);
                return false;
            }

            exceptionType = null;
            return true;
        }

        internal bool CheckKingsideCastlingLegacy(out Type exceptionType)
        {
            if (HasMoved)
            {
                exceptionType = typeof(KingHasMovedException);
                return false;
            }

            if (Board.GetPiece(7, Y).HasMoved)
            {
                exceptionType = typeof(RookHasMovedException);
                return false;
            }

            if (Square.IsMenacedBy(EnemyColor))
            {
                exceptionType = typeof(CastlingKingCheckedException);
                return false;
            }

            if (Board[5, Y].IsMenacedBy(EnemyColor))
            {
                exceptionType = typeof(KingPassesUnsafeSquareException);
                return false;
            }

            if (Board[6, Y].IsMenacedBy(EnemyColor))
            {
                exceptionType = typeof(KingMovesIntoCheckException);
                return false;
            }

            exceptionType = null;
            return true;
        }

        internal bool CheckKingsideCastlingLegacy() => CheckKingsideCastlingLegacy(out var t);

        internal bool CheckQueensideCastlingLegacy(out Type exceptionType)
        {
            if (HasMoved)
            {
                exceptionType = typeof(KingHasMovedException);
                return false;
            }

            if (Board.GetPiece(0, Y).HasMoved)
            {
                exceptionType = typeof(RookHasMovedException);
                return false;
            }

            if (Square.IsMenacedBy(EnemyColor))
            {
                exceptionType = typeof(CastlingKingCheckedException);
                return false;
            }

            if (Board[3, Y].IsMenacedBy(EnemyColor))
            {
                exceptionType = typeof(KingPassesUnsafeSquareException);
                return false;
            }

            if (Board[2, Y].IsMenacedBy(EnemyColor))
            {
                exceptionType = typeof(KingMovesIntoCheckException);
                return false;
            }

            exceptionType = null;
            return true;
        }

        internal bool CheckQueensideCastlingLegacy() => CheckQueensideCastlingLegacy(out var t);

        public override IEnumerable<Square> GetAccessibleSquares()
        {
            IEnumerable<Square> collection;
            ulong modCount;
            ulong gamesCount;
            var board = Board;

            if (board == null)
            {
                yield break;
            }

            lock (board.Locker)
            {
                if (!IsOnBoard)
                {
                    yield break;
                }

                if (board.Status != BoardStatus.GameIncomplete || board.MoveTurn != Color)
                {
                    yield break;
                }

                collection = GetAttackedSquares().Where(sq => sq.Contained?.Color != Color && WillBeSafeAt(sq));
                var startHorizontal = Color == PieceColor.White ? 0 : 7;
                var castlingSquare = board[6, startHorizontal];

                if (Move.IsCastlingMove(this, castlingSquare) && CheckKingsideCastlingLegacy())
                {
                    collection = collection.Append(castlingSquare);
                }

                castlingSquare = board[2, startHorizontal];

                if (Move.IsCastlingMove(this, castlingSquare) && CheckQueensideCastlingLegacy())
                {
                    collection = collection.Append(castlingSquare);
                }

                modCount = board.ModCount;
                gamesCount = board.GamesCount;
            }

            Square previous = null;

            foreach (var current in collection)
            {
                if (previous == null)
                {
                    previous = current;
                    continue;
                }

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return previous;
                previous = current;
            }

            if (board.ModCount != modCount || board.GamesCount != gamesCount)
            {
                throw new InvalidOperationException("Изменение позиции во время перечисления.");
            }

            if (previous != null)
            {
                yield return previous;
            }
        }

        internal override void RemoveUnactualMenaces(Square newSquare)
        {
            newSquare.RemoveMenace(this);

            foreach (var square in GetAttackedSquares().Where(sq => sq.X < newSquare.X - 1 ||
                     sq.X > newSquare.X + 1 || sq.Y < newSquare.Y - 1 || sq.Y > newSquare.Y + 1))
            {
                square.RemoveMenace(this);
            }
        }

        internal override void AddMissingMenaces(Square oldSquare)
        {
            oldSquare.AddMenace(this);

            foreach (var square in GetAttackedSquares().Where(sq => sq.X < oldSquare.X - 1 ||
                     sq.X > oldSquare.X + 1 || sq.Y < oldSquare.Y - 1 || sq.Y > oldSquare.Y + 1))
            {
                square.AddMenace(this);
            }
        }

        internal void CastleKingside()
        {
            var rook = Board.GetPiece(7, Y);
            var nextHorizontal = Color == PieceColor.White ? 1 : 6;

            Board[3, Y].RemoveMenace(this);
            Board[3, nextHorizontal].RemoveMenace(this);
            Board[4, nextHorizontal].RemoveMenace(this);
            rook.RemoveVerticalMenaces();
            Board[5, Y].RemoveMenace(rook);

            Square.Contained = null;
            Square = Board[6, Y];
            Square.Contained = this;

            rook.Square.Contained = null;
            rook.Square = Board[5, Y];
            rook.Square.Contained = rook;

            var collection = Color == PieceColor.White ? Board[4, 0].GetMenaces(PieceColor.White) : Board[4, 7].GetMenaces(PieceColor.Black);
            var piece = collection.FirstOrDefault(p => p.Y == Y && p.X < 4);

            if (piece != null)
            {
                Board[5, Y].AddMenace(piece);
            }

            Board[6, nextHorizontal].AddMenace(this);
            Board[7, nextHorizontal].AddMenace(this);
            Board[7, Y].AddMenace(this);
            rook.AddVerticalMenaces();

            for (var i = 3; i >= 0; --i)
            {
                var square = Board[i, Y];
                square.AddMenace(rook);

                if (!square.IsClear)
                {
                    break;
                }
            }
        }

        internal void CastleQueenside()
        {
            var rook = Board.GetPiece(0, Y);
            var nextHorizontal = Color == PieceColor.White ? 1 : 6;

            Board[5, Y].RemoveMenace(this);
            Board[5, nextHorizontal].RemoveMenace(this);
            Board[4, nextHorizontal].RemoveMenace(this);
            rook.RemoveVerticalMenaces();
            Board[1, Y].RemoveMenace(rook);
            Board[3, Y].RemoveMenace(rook);

            Square.Contained = null;
            Square = Board[2, Y];
            Square.Contained = this;

            rook.Square.Contained = null;
            rook.Square = Board[3, Y];
            rook.Square.Contained = rook;

            var collection = Color == PieceColor.White ? Board[4, 0].GetMenaces(PieceColor.White) : Board[4, 7].GetMenaces(PieceColor.Black);
            var piece = collection.FirstOrDefault(p => p.Y == Y && p.X > 4);

            if (piece != null)
            {
                Board[3, Y].AddMenace(piece);
            }

            Board[2, nextHorizontal].AddMenace(this);
            Board[1, nextHorizontal].AddMenace(this);
            Board[1, Y].AddMenace(this);
            rook.AddVerticalMenaces();

            for (var i = 5; i < 8; ++i)
            {
                var square = Board[i, Y];
                square.AddMenace(rook);

                if (!square.IsClear)
                {
                    break;
                }
            }
        }

        internal void CancelKingsideCastling()
        {
            var rook = Board.GetPiece(5, Y);
            var nextHorizontal = Color == PieceColor.White ? 1 : 6;

            Board[7, Y].RemoveMenace(this);
            Board[7, nextHorizontal].RemoveMenace(this);
            Board[6, nextHorizontal].RemoveMenace(this);
            rook.RemoveVerticalMenaces();

            for (var i = 3; i >= 0; --i)
            {
                var square = Board[i, Y];
                square.RemoveMenace(rook);

                if (!square.IsClear)
                {
                    break;
                }
            }

            Square.Contained = null;
            Square = Board[4, Y];
            Square.Contained = this;

            rook.Square.Contained = null;
            rook.Square = Board[7, Y];
            rook.Square.Contained = rook;

            var collection = Color == PieceColor.White ? Board[5, 0].GetMenaces(PieceColor.White) : Board[5, 7].GetMenaces(PieceColor.Black);
            var piece = collection.FirstOrDefault(p => p.Y == Y && p.X < 4);

            if (piece != null)
            {
                Board[5, Y].RemoveMenace(piece);
            }

            Board[3, Y].AddMenace(this);
            Board[3, nextHorizontal].AddMenace(this);
            Board[4, nextHorizontal].AddMenace(this);
            rook.AddVerticalMenaces();
            Board[5, Y].AddMenace(rook);
        }

        internal void CancelQueensideCastling()
        {
            var rook = Board.GetPiece(3, Y);
            var nextHorizontal = Color == PieceColor.White ? 1 : 6;

            Board[1, Y].RemoveMenace(this);
            Board[1, nextHorizontal].RemoveMenace(this);
            Board[2, nextHorizontal].RemoveMenace(this);
            rook.RemoveVerticalMenaces();

            for (var i = 5; i < 8; ++i)
            {
                var square = Board[i, Y];
                square.RemoveMenace(rook);

                if (!square.IsClear)
                {
                    break;
                }
            }

            Square.Contained = null;
            Square = Board[4, Y];
            Square.Contained = this;

            rook.Square.Contained = null;
            rook.Square = Board[0, Y];
            rook.Square.Contained = rook;

            var collection = Color == PieceColor.White ? Board[3, 0].GetMenaces(PieceColor.White) : Board[3, 7].GetMenaces(PieceColor.Black);
            var piece = collection.FirstOrDefault(p => p.Y == Y && p.X > 4);

            if (piece != null)
            {
                Board[3, Y].RemoveMenace(piece);
            }

            Board[4, nextHorizontal].AddMenace(this);
            Board[5, nextHorizontal].AddMenace(this);
            Board[5, Y].AddMenace(this);
            rook.AddVerticalMenaces();
            Board[1, Y].AddMenace(rook);
            Board[3, Y].AddMenace(rook);
        }

        public override PieceName Name => PieceName.King;

        public override bool IsLongRanged => false;
    }
}