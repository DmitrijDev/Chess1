
namespace Chess.LogicPart
{
    public sealed class Pawn : ChessPiece
    {
        internal Pawn(PieceColor color) : base(color) { }

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

            var nextHorizontal = Color == PieceColor.White ? square.Y + 1 : square.Y - 1;

            if (square.X > 0)
            {
                yield return board[square.X - 1, nextHorizontal];
            }

            if (square.X < 7)
            {
                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[square.X + 1, nextHorizontal];
            }
        }

        internal override bool CanMove()
        {
            var isKingSafe = GetKingSafetyFunc();

            if (Color == PieceColor.White)
            {
                if (Board[X, Y + 1].IsClear)
                {
                    if (isKingSafe(Board[X, Y + 1]))
                    {
                        return true;
                    }

                    if (Y == 1 && Board[X, 3].IsClear && isKingSafe(Board[X, 3]))
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (Board[X, Y - 1].IsClear)
                {
                    if (isKingSafe(Board[X, Y - 1]))
                    {
                        return true;
                    }

                    if (Y == 6 && Board[X, 4].IsClear && isKingSafe(Board[X, 4]))
                    {
                        return true;
                    }
                }
            }

            return GetAttackedSquares().Any(sq =>
            ((!sq.IsClear && sq.Contained.Color != Color) || sq.IsPawnPassed) && isKingSafe(sq));
        }

        internal override bool CanMoveTo(Square moveSquare, out Type exceptionType)
        {
            if (Attacks(moveSquare))
            {
                if (moveSquare.IsClear && !moveSquare.IsPawnPassed)
                {
                    exceptionType = typeof(IllegalMoveException);
                    return false;
                }
            }
            else
            {
                if (!moveSquare.IsClear || moveSquare.X != X)
                {
                    exceptionType = typeof(IllegalMoveException);
                    return false;
                }

                if (Color == PieceColor.White)
                {
                    if (moveSquare.Y < Y || moveSquare.Y > Y + 2)
                    {
                        exceptionType = typeof(IllegalMoveException);
                        return false;
                    }

                    if (moveSquare.Y == Y + 2 && !(Y == 1 && Board[X, 2].IsClear))
                    {
                        exceptionType = typeof(IllegalMoveException);
                        return false;
                    }
                }
                else
                {
                    if (moveSquare.Y > Y || moveSquare.Y < Y - 2)
                    {
                        exceptionType = typeof(IllegalMoveException);
                        return false;
                    }

                    if (moveSquare.Y == Y - 2 && !(Y == 6 && Board[X, 5].IsClear))
                    {
                        exceptionType = typeof(IllegalMoveException);
                        return false;
                    }
                }
            }

            var king = Color == PieceColor.White ? Board.WhiteKing : Board.BlackKing;

            if (moveSquare.X != X && ShieldsByVertical(king.Square))
            {
                exceptionType = typeof(PawnPinnedException);
                return false;
            }

            if (ShieldsByHorizontal(king.Square))
            {
                exceptionType = typeof(PawnPinnedException);
                return false;
            }

            if (!(moveSquare.IsOnSameDiagonal(Square) && moveSquare.IsOnSameDiagonal(king)) &&
                ShieldsByDiagonal(king.Square))
            {
                exceptionType = typeof(PawnPinnedException);
                return false;
            }

            if (king.Square.IsMenacedBy(EnemyColor) && !ProtectsKingWithMoveTo(moveSquare))
            {
                exceptionType = typeof(KingCheckedException);
                return false;
            }

            exceptionType = null;
            return true;
        }

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

                collection = GetAttackedSquares().
                Where(square =>
                {
                    var piece = square.Contained;
                    return (piece != null && piece.Color != Color) || square.IsPawnPassed;
                });

                if (Color == PieceColor.White)
                {
                    if (board[X, Y + 1].IsClear)
                    {
                        collection = collection.Append(board[X, Y + 1]);

                        if (Y == 1 && board[X, 3].IsClear)
                        {
                            collection = collection.Append(board[X, 3]);
                        }
                    }
                }
                else
                {
                    if (board[X, Y - 1].IsClear)
                    {
                        collection = collection.Append(board[X, Y - 1]);

                        if (Y == 6 && board[X, 4].IsClear)
                        {
                            collection = collection.Append(board[X, 4]);
                        }
                    }
                }

                var isKingSafe = GetKingSafetyFunc();
                collection = collection.Where(isKingSafe);
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

        internal override void RemoveUnactualMenaces(Square newSquare) => RemoveMenaces();

        internal override void AddMissingMenaces(Square oldSquare) => AddMenaces();

        public override PieceName Name => PieceName.Pawn;

        public override bool IsLongRanged => false;
    }
}
