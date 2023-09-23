
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

            if (FirstMoveMoment > 0 || IsChecked())
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

            var enemyColor = Color == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;

            if (Board[5, Horizontal].IsMenacedBy(enemyColor) || Board[6, Horizontal].IsMenacedBy(enemyColor))
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

            var enemyColor = Color == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;

            if (Board[3, Horizontal].IsMenacedBy(enemyColor) || Board[2, Horizontal].IsMenacedBy(enemyColor))
            {
                return false;
            }

            return true;
        }

        internal bool IsChecked()
        {
            var enemyColor = Color == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
            return Position.IsMenacedBy(enemyColor);
        }

        internal IEnumerable<ChessPiece> GetCheckingPieces()
        {
            var enemyColor = Color == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
            return Position.EnumerateMenaces(enemyColor);
        }

        private protected override IEnumerable<Square> FilterSafeForKingMoves(IEnumerable<Square> moveSquares) => moveSquares.Where(CanSafelyMoveTo);

        private bool CanSafelyMoveTo(Square square)
        {
            var enemyColor = Color == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;

            if (square.IsMenacedBy(enemyColor))
            {
                return false;
            }

            foreach (var menacingPiece in GetCheckingPieces())
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

        internal override IllegalMoveException CheckMoveLegacy(Move move)
        {
            if (move.IsCastleKingside)
            {
                if (FirstMoveMoment > 0)
                {
                    return new IllegalMoveException("Рокировка невозможна: король уже сделал ход.");
                }

                if (Board[7, Horizontal].ContainedPiece.FirstMoveMoment > 0)
                {
                    return new IllegalMoveException("Рокировка невозможна: ладья уже сделала ход.");
                }

                if (IsChecked())
                {
                    return new IllegalMoveException("Рокировка невозможна: король под шахом.");
                }

                var enemyColor = Color == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;

                if (Board[5, Horizontal].IsMenacedBy(enemyColor))
                {
                    return new IllegalMoveException("При рокировке король не может пересекать угрожаемое поле.");
                }

                if (Board[6, Horizontal].IsMenacedBy(enemyColor))
                {
                    return new IllegalMoveException("Король не может становиться под шах.");
                }

                return null;
            }

            if (move.IsCastleQueenside)
            {
                if (FirstMoveMoment > 0)
                {
                    return new IllegalMoveException("Рокировка невозможна: король уже сделал ход.");
                }

                if (Board[0, Horizontal].ContainedPiece.FirstMoveMoment > 0)
                {
                    return new IllegalMoveException("Рокировка невозможна: ладья уже сделала ход.");
                }

                if (IsChecked())
                {
                    return new IllegalMoveException("Рокировка невозможна: король под шахом.");
                }

                var enemyColor = Color == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;

                if (Board[3, Horizontal].IsMenacedBy(enemyColor))
                {
                    return new IllegalMoveException("При рокировке король не может пересекать угрожаемое поле.");
                }

                if (Board[2, Horizontal].IsMenacedBy(enemyColor))
                {
                    return new IllegalMoveException("Король не может становиться под шах.");
                }

                return null;
            }

            if (!move.MoveSquare.IsMenacedBy(this))
            {
                return new IllegalMoveException("Невозможный ход.");
            }

            if (!CanSafelyMoveTo(move.MoveSquare))
            {
                var message = IsChecked() ? "Невозможный ход. Король не может оставаться под шахом." : "Король не может становиться под шах.";
                return new IllegalMoveException(message);
            }

            return null;
        }

        public override ChessPieceName Name => ChessPieceName.King;
    }
}