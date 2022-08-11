
namespace Chess.LogicPart
{
    internal class Rook : ChessPiece
    {
        public Rook(PieceColor color) => Color = color;

        protected override List<Square> GetNewAttackedSquares()
        {
            var result = new List<Square>();

            for (var i = Vertical + 1; i < 8; ++i)
            {
                result.Add(Board[i, Horizontal]);

                if (!Board[i, Horizontal].IsEmpty)
                {
                    break;
                }
            }

            for (var i = Vertical - 1; i >= 0; --i)
            {
                result.Add(Board[i, Horizontal]);

                if (!Board[i, Horizontal].IsEmpty)
                {
                    break;
                }
            }

            for (var i = Horizontal + 1; i < 8; ++i)
            {
                result.Add(Board[Vertical, i]);

                if (!Board[Vertical, i].IsEmpty)
                {
                    break;
                }
            }

            for (var i = Horizontal - 1; i >= 0; --i)
            {
                result.Add(Board[Vertical, i]);

                if (!Board[Vertical, i].IsEmpty)
                {
                    break;
                }
            }

            return result;
        }

        public override string EnglishName => "Rook";

        public override string RussianName => "Ладья";

        public override string ShortEnglishName => "R";

        public override string ShortRussianName => "Л";

        public override int NumeralIndex => Color == PieceColor.White ? 3 : 9;
    }
}