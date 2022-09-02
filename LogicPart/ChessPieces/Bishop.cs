
namespace Chess.LogicPart
{
    internal class Bishop : ChessPiece
    {
        public Bishop(PieceColor color) => Color = color;

        protected override List<Square> GetNewAttackedSquares()
        {
            var result = new List<Square>();

            for (int i = Vertical + 1, j = Horizontal + 1; i < 8 && j < 8; ++i, ++j)
            {
                result.Add(Board[i, j]);

                if (!Board[i, j].IsEmpty)
                {
                    break;
                }
            }

            for (int i = Vertical - 1, j = Horizontal - 1; i >= 0 && j >= 0; --i, --j)
            {
                result.Add(Board[i, j]);

                if (!Board[i, j].IsEmpty)
                {
                    break;
                }
            }

            for (int i = Vertical + 1, j = Horizontal - 1; i < 8 && j >= 0; ++i, --j)
            {
                result.Add(Board[i, j]);

                if (!Board[i, j].IsEmpty)
                {
                    break;
                }
            }

            for (int i = Vertical - 1, j = Horizontal + 1; i >= 0 && j < 8; --i, ++j)
            {
                result.Add(Board[i, j]);

                if (!Board[i, j].IsEmpty)
                {
                    break;
                }
            }

            return result;
        }

        public override ChessPiece Copy()
        {
            var newBishop = new Bishop(Color);
            newBishop.HasMoved = HasMoved;
            return newBishop;
        }

        public override string EnglishName => "Bishop";

        public override string RussianName => "Слон";

        public override string ShortEnglishName => "B";

        public override string ShortRussianName => "С";

        public override int NumeralIndex => Color == PieceColor.White ? 5 : 11;

        public bool IsLightSquared => Position != null && Vertical % 2 != Horizontal % 2;

        public bool IsDarkSquared => Position != null && Vertical % 2 == Horizontal % 2;
    }
}
