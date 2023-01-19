
namespace Chess.LogicPart
{
    internal class King : ChessPiece
    {
        public King(PieceColor color) => Color = color;

        public override IEnumerable<Square> GetAttackedSquares()
        {
            for (var i = Vertical - 1; i <= Vertical + 1; ++i)
            {
                for (var j = Horizontal - 1; j <= Horizontal + 1; ++j)
                {
                    if (i >= 0 && j >= 0 && i < 8 && j < 8 && !(i == Vertical && j == Horizontal))
                    {
                        yield return Board[i, j];
                    }
                }
            }
        }

        protected override IEnumerable<Square> GetLegalMoveSquares(bool savesUnsafeForKingSquares, out IEnumerable<Square> unsafeForKingSquares)
        {
            if (Board.Status != GameStatus.GameCanContinue || FriendlySide != Board.MovingSide)
            {
                unsafeForKingSquares = savesUnsafeForKingSquares ? Enumerable.Empty<Square>() : null;
                return Enumerable.Empty<Square>();
            }

            var result = base.GetLegalMoveSquares(savesUnsafeForKingSquares, out unsafeForKingSquares);

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

        protected override IEnumerable<Square> FilterSafeForKingMoves(IEnumerable<Square> moveSquares, bool savesRemovedSquares,
            out IEnumerable<Square> removedSquares)
        {
            var result = moveSquares.Where(CanSafelyMoveTo);
            removedSquares = savesRemovedSquares ? moveSquares.Except(result) : null;
            return result;
        }

        private bool CanSafelyMoveTo(Square square)
        {
            if (square.IsMenaced())
            {
                return false;
            }

            foreach (var menacingPiece in GetMenaces())
            {
                if (menacingPiece.Vertical == Vertical)
                {
                    if (square.Vertical == Vertical && square != menacingPiece.Position)
                    {
                        return false;
                    }
                }

                if (menacingPiece.Horizontal == Horizontal)
                {
                    if (square.Horizontal == Horizontal && square != menacingPiece.Position)
                    {
                        return false;
                    }
                }

                if (IsOnSameDiagonal(menacingPiece) && menacingPiece is not Pawn)
                {
                    if (square.IsOnSameDiagonal(menacingPiece) && square != menacingPiece.Position)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool CanCastleKingside()
        {
            if (FirstMoveMoment > 0 || Board[7, Horizontal].IsEmpty || Board[7, Horizontal].ContainedPiece.FirstMoveMoment > 0)
            {
                return false;
            }

            if (!Board[5, Horizontal].IsEmpty || !Board[6, Horizontal].IsEmpty)
            {
                return false;
            }

            return !IsMenaced() && !Board[5, Horizontal].IsMenaced() && !Board[6, Horizontal].IsMenaced();
        }

        public bool CanCastleQueenside()
        {
            if (FirstMoveMoment > 0 || Board[0, Horizontal].IsEmpty || Board[0, Horizontal].ContainedPiece.FirstMoveMoment > 0)
            {
                return false;
            }

            if (!Board[1, Horizontal].IsEmpty || !Board[2, Horizontal].IsEmpty || !Board[3, Horizontal].IsEmpty)
            {
                return false;
            }

            return !IsMenaced() && !Board[3, Horizontal].IsMenaced() && !Board[2, Horizontal].IsMenaced();
        }

        public override string GetIllegalMoveMessage(Square square)
        {
            if (Move.MoveIsCastleKingside(this, square))
            {
                return GetCastleKingsideFailureMessage();
            }

            if (Move.MoveIsCastleQueenside(this, square))
            {
                return GetCastleQueensideFailureMessage();
            }

            if (!Attacks(square))
            {
                return "Невозможный ход.";
            }

            return IsMenaced() ? "Невозможный ход. Король не может оставаться под шахом." : "Король не может становиться под шах.";
        }

        private string GetCastleKingsideFailureMessage()
        {
            if (FirstMoveMoment > 0)
            {
                return "Рокировка невозможна: король уже сделал ход.";
            }

            if (Board[7, Horizontal].ContainedPiece.FirstMoveMoment > 0)
            {
                return "Рокировка невозможна: ладья уже сделала ход.";
            }

            if (IsMenaced())
            {
                return "Рокировка невозможна: король под шахом.";
            }

            if (Board[5, Horizontal].IsMenaced())
            {
                return "При рокировке король не может пересекать угрожаемое поле.";
            }

            return "Король не может становиться под шах.";
        }

        private string GetCastleQueensideFailureMessage()
        {
            if (FirstMoveMoment > 0)
            {
                return "Рокировка невозможна: король уже сделал ход.";
            }

            if (Board[0, Horizontal].ContainedPiece.FirstMoveMoment > 0)
            {
                return "Рокировка невозможна: ладья уже сделала ход.";
            }

            if (IsMenaced())
            {
                return "Рокировка невозможна: король под шахом.";
            }

            if (Board[3, Horizontal].IsMenaced())
            {
                return "При рокировке король не может пересекать угрожаемое поле.";
            }

            return "Король не может становиться под шах.";
        }

        public override ChessPiece Copy()
        {
            var newKing = new King(Color);
            newKing.FirstMoveMoment = FirstMoveMoment;
            return newKing;
        }

        public override string EnglishName => "King";

        public override string RussianName => "Король";

        public override string ShortEnglishName => "K";

        public override string ShortRussianName => "Кр";

        public override int NumeralIndex => Color == PieceColor.White ? 1 : 7;
    }
}