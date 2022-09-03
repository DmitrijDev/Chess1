
namespace Chess.LogicPart
{
    internal class Knight : ChessPiece
    {
        public Knight(PieceColor color) => Color = color;

        protected override List<Square> GetNewAttackedSquares()
        {
            var result = new List<Square>();

            var verticalShifts = new int[8] { 2, 2, -2, -2, 1, 1, -1, -1 };
            var horizontalShifts = new int[8] { -1, 1, -1, 1, -2, 2, -2, 2 };

            for (var i = 0; i < 8; ++i)
            {
                var targetVertical = Vertical + horizontalShifts[i];
                var targetHorizontal = Horizontal + verticalShifts[i];

                if (targetVertical >= 0 && targetHorizontal >= 0 && targetVertical < 8 && targetHorizontal < 8)
                {
                    result.Add(Board[targetVertical, targetHorizontal]);
                }
            }

            return result;
        }

        public override ChessPiece Copy()
        {
            var newKnight = new Knight(Color);
            newKnight.HasMoved = HasMoved;
            return newKnight;
        }

        public override string EnglishName => "Knight";

        public override string RussianName => "Конь";

        public override string ShortEnglishName => "N";

        public override string ShortRussianName => "К";

        public override int NumeralIndex => Color == PieceColor.White ? 4 : 10;
    }
}
