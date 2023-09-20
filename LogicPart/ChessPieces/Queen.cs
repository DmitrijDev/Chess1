
namespace Chess.LogicPart
{
    public class Queen : ChessPiece
    {
        public Queen(ChessPieceColor color) : base(color)
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

        public override ChessPieceName Name => ChessPieceName.Queen;
    }
}
