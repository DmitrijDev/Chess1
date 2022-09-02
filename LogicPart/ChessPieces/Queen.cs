
namespace Chess.LogicPart
{
    internal class Queen : ChessPiece
    {
        public Queen(PieceColor color) => Color = color;

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
            var newQueen = new Queen(Color);
            newQueen.HasMoved = HasMoved;
            return newQueen;
        }

        public override string EnglishName => "Queen";

        public override string RussianName => "Ферзь";

        public override string ShortEnglishName => "Q";

        public override string ShortRussianName => "Ф";

        public override int NumeralIndex => Color == PieceColor.White ? 2 : 8;
    }
}
