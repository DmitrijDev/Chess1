
namespace Chess.LogicPart
{
    public class Rook : ChessPiece
    {
        public Rook(ChessPieceColor color): base (color)
        { }

        internal override IEnumerable<Square> GetAttackedSquares()
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

        public override ChessPieceName Name => ChessPieceName.Rook;
    }
}