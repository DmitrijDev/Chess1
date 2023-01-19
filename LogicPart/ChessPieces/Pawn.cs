
namespace Chess.LogicPart
{
    internal class Pawn : ChessPiece
    {
        public Pawn(PieceColor color) => Color = color;

        public override IEnumerable<Square> GetAttackedSquares()
        {
            if (Vertical > 0)
            {
                yield return Board[Vertical - 1, Color == PieceColor.White ? Horizontal + 1 : Horizontal - 1];
            }

            if (Vertical < 7)
            {
                yield return Board[Vertical + 1, Color == PieceColor.White ? Horizontal + 1 : Horizontal - 1];
            }
        }

        protected override IEnumerable<Square> GetLegalMoveSquares(bool savesUnsafeForKingSquares, out IEnumerable<Square> unsafeForKingSquares)
        {
            if (Board.Status != GameStatus.GameCanContinue || FriendlySide != Board.MovingSide)
            {
                unsafeForKingSquares = savesUnsafeForKingSquares ? Enumerable.Empty<Square>() : null;
                return Enumerable.Empty<Square>();
            }

            var moveSquares = GetAttackedSquares().
                Where(square => (!square.IsEmpty && square.ContainedPiece.Color != Color) || square == Board.PassedByPawnSquare);

            if (Color == PieceColor.White)
            {
                if (Board[Vertical, Horizontal + 1].IsEmpty)
                {
                    moveSquares = moveSquares.Append(Board[Vertical, Horizontal + 1]);
                }

                if (Horizontal == 1 && Board[Vertical, 2].IsEmpty && Board[Vertical, 3].IsEmpty)
                {
                    moveSquares = moveSquares.Append(Board[Vertical, 3]);
                }
            }
            else
            {
                if (Board[Vertical, Horizontal - 1].IsEmpty)
                {
                    moveSquares = moveSquares.Append(Board[Vertical, Horizontal - 1]);
                }

                if (Horizontal == 6 && Board[Vertical, 5].IsEmpty && Board[Vertical, 4].IsEmpty)
                {
                    moveSquares = moveSquares.Append(Board[Vertical, 4]);
                }
            }

            return FilterSafeForKingMoves(moveSquares, savesUnsafeForKingSquares, out unsafeForKingSquares);
        }

        public override IEnumerable<Move> GetLegalMoves()
        {
            foreach (var square in GetLegalMoveSquares())
            {
                if (square.Horizontal != 0 && square.Horizontal != 7)
                {
                    yield return new Move(this, square);
                }
                else
                {
                    yield return new Move(this, square, new Queen(Color));
                    yield return new Move(this, square, new Rook(Color));
                    yield return new Move(this, square, new Knight(Color));
                    yield return new Move(this, square, new Bishop(Color));
                }
            }
        }

        public override ChessPiece Copy()
        {
            var newPawn = new Pawn(Color);
            newPawn.FirstMoveMoment = FirstMoveMoment;
            return newPawn;
        }

        public override string EnglishName => "Pawn";

        public override string RussianName => "Пешка";

        public override string ShortEnglishName => "";

        public override string ShortRussianName => "";

        public override int NumeralIndex => Color == PieceColor.White ? 6 : 12;
    }
}
