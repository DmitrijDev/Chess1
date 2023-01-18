
namespace Chess.LogicPart
{
    internal class Knight : ChessPiece
    {
        public Knight(PieceColor color) => Color = color;

        public override IEnumerable<Square> GetAttackedSquares()
        {
            var verticalShifts = new int[8] { 2, 2, -2, -2, 1, 1, -1, -1 };
            var horizontalShifts = new int[8] { -1, 1, -1, 1, -2, 2, -2, 2 };

            for (var i = 0; i < 8; ++i)
            {
                var targetVertical = Vertical + horizontalShifts[i];
                var targetHorizontal = Horizontal + verticalShifts[i];

                if (targetVertical >= 0 && targetHorizontal >= 0 && targetVertical < 8 && targetHorizontal < 8)
                {
                    yield return Board[targetVertical, targetHorizontal];
                }
            }
        }

        public override ChessPiece Copy()
        {
            var newKnight = new Knight(Color);
            newKnight.FirstMoveMoment = FirstMoveMoment;
            return newKnight;
        }

        public override string EnglishName => "Knight";

        public override string RussianName => "Конь";

        public override string ShortEnglishName => "N";

        public override string ShortRussianName => "К";

        public override int NumeralIndex => Color == PieceColor.White ? 4 : 10;
    }
}
