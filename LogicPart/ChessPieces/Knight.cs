
namespace Chess.LogicPart
{
    public sealed class Knight : ChessPiece
    {
        public Knight(ChessPieceColor color) : base(color)
        { }

        public override IEnumerable<Square> GetAttackedSquares()
        {
            var position = Position;

            if (position == null)
            {
                yield break;
            }

            var board = position.Board;
            var vertical = position.Vertical;
            var horizontal = position.Horizontal;

            ulong modCount;
            ulong gameStartsCount;

            lock (board)
            {
                modCount = board.ModCount;
                gameStartsCount = board.GameStartsCount;
            }

            if (position != Position)
            {
                throw new InvalidOperationException("Изменение позиции во время перечисления.");
            }

            var verticalShifts = new int[] { 2, 2, -2, -2, 1, 1, -1, -1 };
            var horizontalShifts = new int[] { -1, 1, -1, 1, -2, 2, -2, 2 };

            for (var i = 0; i < 8; ++i)
            {
                var targetVertical = vertical + horizontalShifts[i];
                var targetHorizontal = horizontal + verticalShifts[i];

                if (targetVertical < 0 || targetHorizontal < 0 || targetVertical >= 8 || targetHorizontal >= 8)
                {
                    continue;
                }

                if (board.ModCount != modCount || board.GameStartsCount != gameStartsCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[targetVertical, targetHorizontal];
            }
        }

        public override ChessPieceName Name => ChessPieceName.Knight;

        public override bool IsLongRanged => false;
    }
}
