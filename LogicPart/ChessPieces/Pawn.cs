
namespace Chess.LogicPart
{
    internal class Pawn : ChessPiece
    {
        public Pawn(PieceColor color) => Color = color;

        protected override List<Square> GetNewAttackedSquares()
        {
            var result = new List<Square>();

            if (Color == PieceColor.White)
            {
                if (Vertical > 0)
                {
                    result.Add(Board[Vertical - 1, Horizontal + 1]);
                }

                if (Vertical < 7)
                {
                    result.Add(Board[Vertical + 1, Horizontal + 1]);
                }
            }
            else
            {
                if (Vertical > 0)
                {
                    result.Add(Board[Vertical - 1, Horizontal - 1]);
                }

                if (Vertical < 7)
                {
                    result.Add(Board[Vertical + 1, Horizontal - 1]);
                }
            }

            return result;
        }

        public override List<Square> GetLegalMoveSquares()
        {
            var result = GetAttackedSquares().Where(square => !square.IsEmpty && square.ContainedPiece.Color != Color).ToList();

            if (Color == PieceColor.White)
            {
                if (Board[Vertical, Horizontal + 1].IsEmpty)
                {
                    result.Add(Board[Vertical, Horizontal + 1]);
                }

                if (Horizontal == 1 && Board[Vertical, Horizontal + 1].IsEmpty && Board[Vertical, Horizontal + 2].IsEmpty)
                {
                    result.Add(Board[Vertical, Horizontal + 2]);
                }
            }
            else 
            {
                if (Board[Vertical, Horizontal - 1].IsEmpty)
                {
                    result.Add(Board[Vertical, Horizontal - 1]);
                }

                if (Horizontal == 6 && Board[Vertical, Horizontal - 1].IsEmpty && Board[Vertical, Horizontal - 2].IsEmpty)
                {
                    result.Add(Board[Vertical, Horizontal - 2]);
                }
            }

            if (result.Count > 0)
            {
                return FilterSafeForKingMoves(result);
            }

            return result;
        }

        public override string EnglishName => "Pawn";

        public override string RussianName => "Пешка";

        public override string ShortEnglishName => "p";

        public override string ShortRussianName => "п";

        public override int NumeralIndex => Color == PieceColor.White ? 6 : 12;
    }
}
