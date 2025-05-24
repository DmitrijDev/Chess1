
namespace Chess.LogicPart
{
    public sealed class Queen : ChessPiece
    {
        internal Queen(PieceColor color) : base(color) { }

        public override IEnumerable<Square> GetAttackedSquares()
        {
            var board = Board;

            if (board == null)
            {
                yield break;
            }

            Square queenPosition;
            ulong gamesCount;
            ulong modCount;

            lock (board.Locker)
            {
                if (Board != board)
                {
                    yield break;
                }

                queenPosition = Square;
                gamesCount = board.GamesCount;
                modCount = board.ModCount;
            }

            for (var i = queenPosition.Y + 1; i < 8; ++i)
            {
                var square = board[queenPosition.X, i];
                var squareClear = square.IsClear;

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return square;

                if (!squareClear)
                {
                    break;
                }
            }

            for (var i = queenPosition.Y - 1; i >= 0; --i)
            {
                var square = board[queenPosition.X, i];
                var squareClear = square.IsClear;

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return square;

                if (!squareClear)
                {
                    break;
                }
            }

            for (var i = queenPosition.X + 1; i < 8; ++i)
            {
                var square = board[i, queenPosition.Y];
                var squareClear = square.IsClear;

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return square;

                if (!squareClear)
                {
                    break;
                }
            }

            for (var i = queenPosition.X - 1; i >= 0; --i)
            {
                var square = board[i, queenPosition.Y];
                var squareClear = square.IsClear;

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return square;

                if (!squareClear)
                {
                    break;
                }
            }

            for (int i = queenPosition.X + 1, j = queenPosition.Y + 1; i < 8 && j < 8; ++i, ++j)
            {
                var square = board[i, j];
                var squareClear = square.IsClear;

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return square;

                if (!squareClear)
                {
                    break;
                }
            }

            for (int i = queenPosition.X - 1, j = queenPosition.Y - 1; i >= 0 && j >= 0; --i, --j)
            {
                var square = board[i, j];
                var squareClear = square.IsClear;

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return square;

                if (!squareClear)
                {
                    break;
                }
            }

            for (int i = queenPosition.X + 1, j = queenPosition.Y - 1; i < 8 && j >= 0; ++i, --j)
            {
                var square = board[i, j];
                var squareClear = square.IsClear;

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return square;

                if (!squareClear)
                {
                    break;
                }
            }

            for (int i = queenPosition.X - 1, j = queenPosition.Y + 1; i >= 0 && j < 8; --i, ++j)
            {
                var square = board[i, j];
                var squareClear = square.IsClear;

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return square;

                if (!squareClear)
                {
                    break;
                }
            }
        }

        internal override void RemoveUnactualMenaces(Square newSquare)
        {
            newSquare.RemoveMenace(this);

            if (newSquare.X == X)
            {
                RemoveHorizontalMenaces();
                RemoveDiagonalMenaces();
                return;
            }

            if (newSquare.Y == Y)
            {
                RemoveVerticalMenaces();
                RemoveDiagonalMenaces();
                return;
            }

            RemoveVerticalMenaces();
            RemoveHorizontalMenaces();

            if ((newSquare.X > X && newSquare.Y > Y) ||
                (newSquare.X < X && newSquare.Y < Y))
            {
                RemoveMenacesToLowerRight();
                RemoveMenacesToUpperLeft();
                return;
            }

            RemoveMenacesToUpperRight();
            RemoveMenacesToLowerLeft();
        }

        internal override void AddMissingMenaces(Square oldSquare)
        {
            if (oldSquare == null)
            {
                AddMenaces();
                return;
            }

            oldSquare.AddMenace(this);

            if (oldSquare.X == X)
            {
                AddHorizontalMenaces();
                AddDiagonalMenaces();
                return;
            }

            if (oldSquare.Y == Y)
            {
                AddVerticalMenaces();
                AddDiagonalMenaces();
                return;
            }

            AddVerticalMenaces();
            AddHorizontalMenaces();

            if ((X > oldSquare.X && Y > oldSquare.Y) ||
                (X < oldSquare.X && Y < oldSquare.Y))
            {
                AddMenacesToLowerRight();
                AddMenacesToUpperLeft();
                return;
            }

            AddMenacesToUpperRight();
            AddMenacesToLowerLeft();
        }

        public override PieceName Name => PieceName.Queen;

        public override bool IsLongRanged => true;
    }
}
