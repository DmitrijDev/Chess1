
namespace Chess.LogicPart
{
    public class Bishop : ChessPiece
    {
        public Bishop(ChessPieceColor color) : base(color)
        { }

        internal override IEnumerable<Square> GetAttackedSquares()
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

        public override ChessPieceName Name => ChessPieceName.Bishop;
    }
}
