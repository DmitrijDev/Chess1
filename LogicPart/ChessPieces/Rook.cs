
namespace Chess.LogicPart
{
    internal class Rook : ChessPiece
    {
        public Rook(PieceColor color) => Color = color;

        public override IEnumerable<Square> GetAttackedSquares()
        {
            for (var i = Vertical + 1; i < 8; ++i)
            {
                yield return Board[i, Horizontal];

                if (!Board[i, Horizontal].IsEmpty)
                {
                    break;
                }
            }

            for (var i = Vertical - 1; i >= 0; --i)
            {
                yield return Board[i, Horizontal];

                if (!Board[i, Horizontal].IsEmpty)
                {
                    break;
                }
            }

            for (var i = Horizontal + 1; i < 8; ++i)
            {
                yield return Board[Vertical, i];

                if (!Board[Vertical, i].IsEmpty)
                {
                    break;
                }
            }

            for (var i = Horizontal - 1; i >= 0; --i)
            {
                yield return Board[Vertical, i];

                if (!Board[Vertical, i].IsEmpty)
                {
                    break;
                }
            }
        }

        public override ChessPiece Copy()
        {
            var newRook = new Rook(Color);
            newRook.FirstMoveMoment = FirstMoveMoment;
            return newRook;
        }

        public override string EnglishName => "Rook";

        public override string RussianName => "Ладья";

        public override string ShortEnglishName => "R";

        public override string ShortRussianName => "Л";

        public override int NumeralIndex => Color == PieceColor.White ? 3 : 9;
    }
}