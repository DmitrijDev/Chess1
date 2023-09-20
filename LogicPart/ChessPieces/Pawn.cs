
namespace Chess.LogicPart
{
    public class Pawn : ChessPiece
    {
        public Pawn(ChessPieceColor color) : base(color)
        { }

        internal override IEnumerable<Square> GetAttackedSquares()
        {
            if (Horizontal == 0 || Horizontal == 7)
            {
                yield break;
            }

            if (Vertical > 0)
            {
                yield return Board[Vertical - 1, Color == ChessPieceColor.White ? Horizontal + 1 : Horizontal - 1];
            }

            if (Vertical < 7)
            {
                yield return Board[Vertical + 1, Color == ChessPieceColor.White ? Horizontal + 1 : Horizontal - 1];
            }
        }

        internal override IEnumerable<Square> GetAccessibleSquares()
        {
            var moveSquares = GetAttackedSquares().
             Where(square => (!square.IsEmpty && square.ContainedPiece.Color != Color) || square == Board.PassedByPawnSquare);

            if (Color == ChessPieceColor.White)
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

            return FilterSafeForKingMoves(moveSquares);
        }

        internal override string CheckMoveLegacy(Move move)
        {
            if (move.MoveSquare.Menaces != null && move.MoveSquare.Menaces.Contains(this))
            {
                if (move.MoveSquare.IsEmpty && move.MoveSquare != Board.PassedByPawnSquare)
                {
                    return "Невозможный ход.";
                }
            }
            else
            {
                if (!move.MoveSquare.IsEmpty || move.MoveSquare.Vertical != Vertical)
                {
                    return "Невозможный ход.";
                }

                if (Color == ChessPieceColor.White)
                {
                    if (move.MoveSquare.Horizontal != Horizontal + 1 && move.MoveSquare.Horizontal != Horizontal + 2)
                    {
                        return "Невозможный ход.";
                    }

                    if (move.MoveSquare.Horizontal == Horizontal + 2 && !(Horizontal == 1 && Board[Vertical, 2].IsEmpty))
                    {
                        return "Невозможный ход.";
                    }
                }
                else
                {
                    if (move.MoveSquare.Horizontal != Horizontal - 1 && move.MoveSquare.Horizontal != Horizontal - 2)
                    {
                        return "Невозможный ход.";
                    }

                    if (move.MoveSquare.Horizontal == Horizontal - 2 && !(Horizontal == 6 && Board[Vertical, 5].IsEmpty))
                    {
                        return "Невозможный ход.";
                    }
                }
            }

            if (IsPinnedVertically() && move.MoveSquare.Vertical != Vertical)
            {
                return "Невозможный ход. Фигура связана.";
            }

            if (IsPinnedHorizontally())
            {
                return "Невозможный ход. Фигура связана.";
            }

            if (IsPinnedDiagonally() && !(IsOnSameDiagonal(move.MoveSquare) && FriendlyKing.IsOnSameDiagonal(move.MoveSquare)))
            {
                return "Невозможный ход. Фигура связана.";
            }

            if (FriendlyKing.Position.Menaces == null)
            {
                return null;
            }

            var checkingPieces = FriendlyKing.Position.Menaces.Where(piece => piece.Color != Color).ToArray();

            if (checkingPieces.Length > 1)
            {
                return "Невозможный ход. Ваш король под шахом.";
            }

            if (checkingPieces.Length == 1 && !ProtectsKingByMoveTo(move.MoveSquare, checkingPieces[0]))
            {
                return "Невозможный ход. Ваш король под шахом.";
            }

            return null;
        }

        public override ChessPieceName Name => ChessPieceName.Pawn;
    }
}
