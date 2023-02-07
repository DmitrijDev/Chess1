
namespace Chess.LogicPart
{
    internal class Queen : ChessPiece
    {
        public Queen(PieceColor color) => Color = color;

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
            var newQueen = new Queen(Color);
            newQueen.FirstMoveMoment = FirstMoveMoment;
            return newQueen;
        }

        public override PieceName Name => PieceName.Queen;        
    }
}
