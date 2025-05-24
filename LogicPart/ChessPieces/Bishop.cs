
namespace Chess.LogicPart
{
    public sealed class Bishop : ChessPiece
    {
        internal Bishop(PieceColor color) : base(color) { }

        public override IEnumerable<Square> GetAttackedSquares()
        {
            var board = Board;

            if (board == null)
            {
                yield break;
            }

            Square bishopPosition;
            ulong gamesCount;
            ulong modCount;

            lock (board.Locker)
            {
                if (Board != board)
                {
                    yield break;
                }

                bishopPosition = Square;
                gamesCount = board.GamesCount;
                modCount = board.ModCount;
            }

            for (int i = bishopPosition.X + 1, j = bishopPosition.Y + 1; i < 8 && j < 8; ++i, ++j)
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

            for (int i = bishopPosition.X - 1, j = bishopPosition.Y - 1; i >= 0 && j >= 0; --i, --j)
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

            for (int i = bishopPosition.X + 1, j = bishopPosition.Y - 1; i < 8 && j >= 0; ++i, --j)
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

            for (int i = bishopPosition.X - 1, j = bishopPosition.Y + 1; i >= 0 && j < 8; --i, ++j)
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
       
        public override PieceName Name => PieceName.Bishop;

        public override bool IsLongRanged => true;
    }
}