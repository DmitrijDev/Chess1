
namespace Chess.LogicPart
{
    public class Knight : ChessPiece
    {
        public Knight(ChessPieceColor color) : base(color)
        { }

        public override IEnumerable<Square> GetAttackedSquares()
        {
            var board = Board;

            if (board == null)
            {
                yield break;
            }

            var vertical = Vertical;
            var horizontal = Horizontal;
            var modCount = board.ModCount;

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

                if (board.ModCount != modCount)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return board[targetVertical, targetHorizontal];
            }

            if (board.ModCount != modCount)
            {
                throw new InvalidOperationException("Изменение коллекции во время перечисления.");
            }
        }

        public override ChessPieceName Name => ChessPieceName.Knight;
    }
}
