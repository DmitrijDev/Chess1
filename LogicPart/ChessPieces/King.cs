
namespace Chess.LogicPart
{
    public class King : ChessPiece
    {
        public King(ChessPieceColor color) : base(color)
        { }

        public override IEnumerable<Square> GetAttackedSquares()
        {
            var board = Board;

            if (board == null)
            {
                yield break;
            }

            var vertical = Vertical;
            var horizontal = Horizontal;
            var modCount = board.ModCount;

            for (var i = vertical - 1; i <= vertical + 1; ++i)
            {
                for (var j = horizontal - 1; j <= horizontal + 1; ++j)
                {
                    if (i < 0 || j < 0 || i >= 8 || j >= 8)
                    {
                        continue;
                    }

                    if (i == vertical && j == horizontal)
                    {
                        continue;
                    }

                    if (board.ModCount != modCount)
                    {
                        throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                    }

                    yield return board[i, j];
                }
            }

            if (board.ModCount != modCount)
            {
                throw new InvalidOperationException("Изменение коллекции во время перечисления.");
            }
        }

        public override IEnumerable<Square> GetAccessibleSquares()
        {
            var result = base.GetAccessibleSquares();

            if (CheckCastleKingsideLegacy(out var message))
            {
                result = result.Append(Board[6, Horizontal]);
            }

            if (CheckCastleQueensideLegacy(out message))
            {
                result = result.Append(Board[2, Horizontal]);
            }

            return result;
        }

        protected override IEnumerable<Square> FilterSafeForKingMoves(IEnumerable<Square> moveSquares) => moveSquares.Where(CanSafelyMoveTo);

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
                else if (menacingPiece.Horizontal == Horizontal)
                {
                    if (square.Horizontal == Horizontal && square != menacingPiece.Position)
                    {
                        return false;
                    }
                }
                else if (IsOnSameDiagonal(menacingPiece) && menacingPiece.Name != ChessPieceName.Pawn)
                {
                    if (square.IsOnSameDiagonal(Position) && square.IsOnSameDiagonal(menacingPiece) && square != menacingPiece.Position)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool CheckCastleKingsideLegacy(out string castleFailureMessage)
        {
            if (!IsOnBoard || Board.Status != GameStatus.GameIsNotOver || Color != Board.MovingSideColor)
            {
                castleFailureMessage = null;
                return false;
            }

            if (!Move.IsCastleKingsideMove(this, Board[6, Horizontal]))
            {
                castleFailureMessage = null;
                return false;
            }

            if (FirstMoveMoment > 0)
            {
                castleFailureMessage = "Рокировка невозможна: король уже сделал ход.";
                return false;
            }

            if (Board[7, Horizontal].ContainedPiece.FirstMoveMoment > 0)
            {
                castleFailureMessage = "Рокировка невозможна: ладья уже сделала ход.";
                return false;
            }

            if (IsMenaced())
            {
                castleFailureMessage = "Рокировка невозможна: король под шахом.";
                return false;
            }

            if (Board[5, Horizontal].IsMenaced())
            {
                castleFailureMessage = "При рокировке король не может пересекать угрожаемое поле.";
                return false;
            }

            if (Board[6, Horizontal].IsMenaced())
            {
                castleFailureMessage = "Король не может становиться под шах.";
                return false;
            }

            castleFailureMessage = null;
            return true;
        }

        public bool CheckCastleQueensideLegacy(out string castleFailureMessage)
        {
            if (!IsOnBoard || Board.Status != GameStatus.GameIsNotOver || Color != Board.MovingSideColor)
            {
                castleFailureMessage = null;
                return false;
            }

            if (!Move.IsCastleQueensideMove(this, Board[2, Horizontal]))
            {
                castleFailureMessage = null;
                return false;
            }

            if (FirstMoveMoment > 0)
            {
                castleFailureMessage = "Рокировка невозможна: король уже сделал ход.";
                return false;
            }

            if (Board[0, Horizontal].ContainedPiece.FirstMoveMoment > 0)
            {
                castleFailureMessage = "Рокировка невозможна: ладья уже сделала ход.";
                return false;
            }

            if (IsMenaced())
            {
                castleFailureMessage = "Рокировка невозможна: король под шахом.";
                return false;
            }

            if (Board[3, Horizontal].IsMenaced())
            {
                castleFailureMessage = "При рокировке король не может пересекать угрожаемое поле.";
                return false;
            }

            if (Board[2, Horizontal].IsMenaced())
            {
                castleFailureMessage = "Король не может становиться под шах.";
                return false;
            }

            castleFailureMessage = null;
            return true;
        }

        public override string GetIllegalMoveMessage(Square square)
        {
            if (Board != square.Board)
            {
                throw new InvalidOperationException("Указано поле на другой доске.");
            }

            if (Move.IsCastleKingsideMove(this, square))
            {
                CheckCastleKingsideLegacy(out var message);
                return message;
            }

            if (Move.IsCastleQueensideMove(this, square))
            {
                CheckCastleQueensideLegacy(out var message);
                return message;
            }

            if (!Attacks(square))
            {
                return "Невозможный ход.";
            }

            if (!square.IsEmpty && square.ContainedPiece.Color == Color)
            {
                return "Невозможно пойти на поле, занятое своей фигурой.";
            }

            if (!CanSafelyMoveTo(square))
            {
                return IsMenaced() ? "Невозможный ход. Король не может оставаться под шахом." : "Король не может становиться под шах.";
            }

            return null;
        }

        public override ChessPieceName Name => ChessPieceName.King;
    }
}