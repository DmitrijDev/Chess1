
namespace Chess.LogicPart
{
    public class King : ChessPiece
    {
        public King(ChessPieceColor color) : base(color)
        { }

        internal override IEnumerable<Square> GetAttackedSquares()
        {
            for (var i = Vertical > 0 ? Vertical - 1 : 0; i <= Vertical + 1 && i < 8; ++i)
            {
                for (var j = Horizontal > 0 ? Horizontal - 1 : 0; j <= Horizontal + 1 && j < 8; ++j)
                {
                    if (i == Vertical && j == Horizontal)
                    {
                        continue;
                    }

                    yield return Board[i, j];
                }
            }
        }

        internal override IEnumerable<Square> GetAccessibleSquares()
        {
            var result = GetAttackedSquares().Where(square => square.IsEmpty || square.ContainedPiece.Color != Color);
            result = FilterSafeForKingMoves(result);

            if (!(Color == ChessPieceColor.White && Vertical == 4 && Horizontal == 0) &&
                !(Color == ChessPieceColor.Black && Vertical == 4 && Horizontal == 7))
            {
                return result;
            }

            if (FirstMoveMoment > 0 || (Position.Menaces != null && Position.Menaces.Any(piece => piece.Color != Color)))
            {
                return result;
            }

            if (CanCastleKingside())
            {
                result = result.Append(Board[6, Horizontal]);
            }

            if (CanCastleQueenside())
            {
                result = result.Append(Board[2, Horizontal]);
            }

            return result;
        }

        private bool CanCastleKingside()
        {
            var rookPosition = Board[7, Horizontal];

            if (rookPosition.IsEmpty || rookPosition.ContainedPiece.Name != ChessPieceName.Rook ||
                rookPosition.ContainedPiece.Color != Color)
            {
                return false;
            }

            if (!Board[5, Horizontal].IsEmpty || !Board[6, Horizontal].IsEmpty)
            {
                return false;
            }

            if (rookPosition.ContainedPiece.FirstMoveMoment > 0)
            {
                return false;
            }

            if (Board[5, Horizontal].Menaces != null && Board[5, Horizontal].Menaces.Any(piece => piece.Color != Color))
            {
                return false;
            }

            if (Board[6, Horizontal].Menaces != null && Board[6, Horizontal].Menaces.Any(piece => piece.Color != Color))
            {
                return false;
            }

            return true;
        }

        private bool CanCastleQueenside()
        {
            var rookPosition = Board[0, Horizontal];

            if (rookPosition.IsEmpty || rookPosition.ContainedPiece.Name != ChessPieceName.Rook ||
                rookPosition.ContainedPiece.Color != Color)
            {
                return false;
            }

            if (!Board[1, Horizontal].IsEmpty || !Board[2, Horizontal].IsEmpty || !Board[3, Horizontal].IsEmpty)
            {
                return false;
            }

            if (rookPosition.ContainedPiece.FirstMoveMoment > 0)
            {
                return false;
            }

            if (Board[3, Horizontal].Menaces != null && Board[3, Horizontal].Menaces.Any(piece => piece.Color != Color))
            {
                return false;
            }

            if (Board[2, Horizontal].Menaces != null && Board[2, Horizontal].Menaces.Any(piece => piece.Color != Color))
            {
                return false;
            }

            return true;
        }

        private protected override IEnumerable<Square> FilterSafeForKingMoves(IEnumerable<Square> moveSquares) => moveSquares.Where(CanSafelyMoveTo);

        private bool CanSafelyMoveTo(Square square)
        {
            if (square.Menaces != null && square.Menaces.Any(piece => piece.Color != Color))
            {
                return false;
            }

            if (Position.Menaces == null)
            {
                return true;
            }

            foreach (var menacingPiece in Position.Menaces.Where(piece => piece.Color != Color))
            {
                if (menacingPiece.Vertical == Vertical)
                {
                    if (square.Vertical == Vertical && square != menacingPiece.Position)
                    {
                        return false;
                    }
                }
                else if (menacingPiece.Horizontal == Horizontal)
                {
                    if (square.Horizontal == Horizontal && square != menacingPiece.Position)
                    {
                        return false;
                    }
                }
                else if (IsOnSameDiagonal(menacingPiece) && menacingPiece.Name != ChessPieceName.Pawn)
                {
                    if (square.IsOnSameDiagonal(menacingPiece) && square != menacingPiece.Position)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal override string CheckMoveLegacy(Move move)
        {
            if (move.IsCastleKingside)
            {
                if (FirstMoveMoment > 0)
                {
                    return "Рокировка невозможна: король уже сделал ход.";
                }

                if (Board[7, Horizontal].ContainedPiece.FirstMoveMoment > 0)
                {
                    return "Рокировка невозможна: ладья уже сделала ход.";
                }

                var menaces = Position.Menaces;

                if (menaces != null && menaces.Any(piece => piece.Color != Color))
                {
                    return "Рокировка невозможна: король под шахом.";
                }

                menaces = Board[5, Horizontal].Menaces;

                if (menaces != null && menaces.Any(piece => piece.Color != Color))
                {
                    return "При рокировке король не может пересекать угрожаемое поле.";
                }

                menaces = Board[6, Horizontal].Menaces;

                if (menaces != null && menaces.Any(piece => piece.Color != Color))
                {
                    return "Король не может становиться под шах.";
                }

                return null;
            }

            if (move.IsCastleQueenside)
            {
                if (FirstMoveMoment > 0)
                {
                    return "Рокировка невозможна: король уже сделал ход.";
                }

                if (Board[0, Horizontal].ContainedPiece.FirstMoveMoment > 0)
                {
                    return "Рокировка невозможна: ладья уже сделала ход.";
                }

                var menaces = Position.Menaces;

                if (menaces != null && menaces.Any(piece => piece.Color != Color))
                {
                    return "Рокировка невозможна: король под шахом.";
                }

                menaces = Board[3, Horizontal].Menaces;

                if (menaces != null && menaces.Any(piece => piece.Color != Color))
                {
                    return "При рокировке король не может пересекать угрожаемое поле.";
                }

                menaces = Board[2, Horizontal].Menaces;

                if (menaces != null && menaces.Any(piece => piece.Color != Color))
                {
                    return "Король не может становиться под шах.";
                }

                return null;
            }

            if (move.MoveSquare.Menaces == null || !move.MoveSquare.Menaces.Contains(this))
            {
                return "Невозможный ход.";
            }

            if (!CanSafelyMoveTo(move.MoveSquare))
            {
                var message = Position.Menaces != null && Position.Menaces.Any(piece => piece.Color != Color) ? "Невозможный ход. Король не может оставаться под шахом." : 
                    "Король не может становиться под шах.";

                return message;
            }

            return null;
        }

        public override ChessPieceName Name => ChessPieceName.King;
    }
}