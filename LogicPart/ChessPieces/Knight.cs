
namespace Chess.LogicPart
{
    public sealed class Knight : ChessPiece
    {
        internal Knight(PieceColor color) : base(color) { }

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

            var verticalShifts = new int[] { 2, 2, -2, -2, 1, 1, -1, -1 };
            var horizontalShifts = new int[] { -1, 1, -1, 1, -2, 2, -2, 2 };

            for (var i = 0; i < 8; ++i)
            {
                var targetSquareX = square.X + horizontalShifts[i];

                if (targetSquareX < 0 || targetSquareX > 7)
                {
                    continue;
                }

                var targetSquareY = square.Y + verticalShifts[i];

                if (targetSquareY < 0 || targetSquareY > 7)
                {
                    continue;
                }

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[targetSquareX, targetSquareY];
            }
        }

        internal override void RemoveUnactualMenaces(Square newSquare) => RemoveMenaces();

        internal override void AddMissingMenaces(Square oldSquare) => AddMenaces();

        public override PieceName Name => PieceName.Knight;

        public override bool IsLongRanged => false;
    }
}
