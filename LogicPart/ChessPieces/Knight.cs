
namespace Chess.LogicPart
{
    public class Knight : ChessPiece
    {
        public Knight(ChessPieceColor color) : base(color)
        { }

        internal override IEnumerable<Square> GetAttackedSquares()
        {
            var verticalShifts = new int[] { 2, 2, -2, -2, 1, 1, -1, -1 };
            var horizontalShifts = new int[] { -1, 1, -1, 1, -2, 2, -2, 2 };

            for (var i = 0; i < 8; ++i)
            {
                var targetVertical = Vertical + horizontalShifts[i];
                var targetHorizontal = Horizontal + verticalShifts[i];

                if (targetVertical < 0 || targetHorizontal < 0 || targetVertical >= 8 || targetHorizontal >= 8)
                {
                    continue;
                }

                yield return Board[targetVertical, targetHorizontal];
            }
        }

        public override ChessPieceName Name => ChessPieceName.Knight;
    }
}
