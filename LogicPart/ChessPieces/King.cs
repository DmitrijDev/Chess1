
namespace Chess.LogicPart
{
    internal class King : ChessPiece
    {
        public King(PieceColor color) => Color = color;

        protected override List<Square> GetNewAttackedSquares()
        {
            var result = new List<Square>();

            for (var i = Vertical - 1; i <= Vertical + 1; ++i)
            {
                for (var j = Horizontal - 1; j <= Horizontal + 1; ++j)
                {
                    if (i >= 0 && j >= 0 && i < 8 && j < 8 && !(i == Vertical && j == Horizontal))
                    {
                        result.Add(Board[i, j]);
                    }
                }
            }

            return result;
        }

        public override List<Square> GetLegalMoveSquares()
        {
            var result = base.GetLegalMoveSquares();

            if (CanCastleKingside())
            {
                result.Add(Board[6, Horizontal]);
            }

            if (CanCastleQueenside())
            {
                result.Add(Board[2, Horizontal]);
            }

            return result;
        }

        protected override List<Square> FilterSafeForKingMoves(List<Square> list)
        {
            var result = list.Where(square => !square.IsMenaced()).ToList();

            if (!IsMenaced())
            {
                return result;
            }

            foreach (var menacingPiece in GetMenaces())
            {
                if (menacingPiece.Vertical == Vertical)
                {
                    result = result.Where(square => square.Vertical != Vertical || square == menacingPiece.Position).ToList();
                }

                if (menacingPiece.Horizontal == Horizontal)
                {
                    result = result.Where(square => square.Horizontal != Horizontal || square == menacingPiece.Position).ToList();
                }

                if (IsOnSameDiagonal(menacingPiece) && menacingPiece is not Pawn)
                {
                    result = result.Where(square => !square.IsOnSameDiagonal(menacingPiece) || square == menacingPiece.Position).ToList();
                }
            }

            return result;
        }

        public bool CanCastleKingside()
        {
            if ((!(Color == PieceColor.White && Position.Name == "e1") && !(Color == PieceColor.Black && Position.Name == "e8")) || HasMoved)
            {
                return false;
            }

            if (Board[7, Horizontal].IsEmpty || Board[7, Horizontal].ContainedPiece is not Rook || Board[7, Horizontal].ContainedPiece.Color != Color ||
                Board[7, Horizontal].ContainedPiece.HasMoved)
            {
                return false;
            }

            if (!Board[5, Horizontal].IsEmpty || !Board[6, Horizontal].IsEmpty)
            {
                return false;
            }

            if (IsMenaced() || Board[5, Horizontal].IsMenaced() || Board[6, Horizontal].IsMenaced())
            {
                return false;
            }

            return true;
        }

        public bool CanCastleQueenside()
        {
            if ((!(Color == PieceColor.White && Position.Name == "e1") && !(Color == PieceColor.Black && Position.Name == "e8")) || HasMoved)
            {
                return false;
            }

            if (Board[0, Horizontal].IsEmpty || Board[0, Horizontal].ContainedPiece is not Rook || Board[0, Horizontal].ContainedPiece.Color != Color ||
                Board[0, Horizontal].ContainedPiece.HasMoved)
            {
                return false;
            }

            if (!Board[1, Horizontal].IsEmpty || !Board[2, Horizontal].IsEmpty || !Board[3, Horizontal].IsEmpty)
            {
                return false;
            }

            if (IsMenaced() || Board[2, Horizontal].IsMenaced() || Board[3, Horizontal].IsMenaced())
            {
                return false;
            }

            return true;
        }

        public override ChessPiece Copy()
        {
            var newKing = new King(Color);
            newKing.HasMoved = HasMoved;
            return newKing;
        }

        public override string EnglishName => "King";

        public override string RussianName => "Король";

        public override string ShortEnglishName => "K";

        public override string ShortRussianName => "Кр";

        public override int NumeralIndex => Color == PieceColor.White ? 1 : 7;
    }
}