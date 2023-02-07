
namespace Chess.LogicPart
{
    internal class Bishop : ChessPiece
    {
        public Bishop(PieceColor color) => Color = color;

        public override IEnumerable<Square> GetAttackedSquares()
        {
            for (int i = Vertical + 1, j = Horizontal + 1; i < 8 && j < 8; ++i, ++j)
            {
                yield return Board[i, j];

                if (!Board[i, j].IsEmpty)
                {
                    break;
                }
            }

            for (int i = Vertical - 1, j = Horizontal - 1; i >= 0 && j >= 0; --i, --j)
            {
                yield return Board[i, j];

                if (!Board[i, j].IsEmpty)
                {
                    break;
                }
            }

            for (int i = Vertical + 1, j = Horizontal - 1; i < 8 && j >= 0; ++i, --j)
            {
                yield return Board[i, j];

                if (!Board[i, j].IsEmpty)
                {
                    break;
                }
            }

            for (int i = Vertical - 1, j = Horizontal + 1; i >= 0 && j < 8; --i, ++j)
            {
                yield return Board[i, j];

                if (!Board[i, j].IsEmpty)
                {
                    break;
                }
            }
        }

        public override ChessPiece Copy()
        {
            var newBishop = new Bishop(Color);
            newBishop.FirstMoveMoment = FirstMoveMoment;
            return newBishop;
        }

        public override PieceName Name => PieceName.Bishop;        

        public bool IsLightSquared => Position != null && Vertical % 2 != Horizontal % 2;

        public bool IsDarkSquared => Position != null && Vertical % 2 == Horizontal % 2;
    }
}
