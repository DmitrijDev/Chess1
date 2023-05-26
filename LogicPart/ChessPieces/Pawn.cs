
namespace Chess.LogicPart
{
    public class Pawn : ChessPiece
    {
        public Pawn(ChessPieceColor color) => Color = color;

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

            if ((Color == ChessPieceColor.White && horizontal == 7) || (Color == ChessPieceColor.Black && horizontal == 0))
            {
                yield break;
            }

            if (vertical > 0)
            {
                if (board.ModCount != modCount)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return board[vertical - 1, Color == ChessPieceColor.White ? horizontal + 1 : horizontal - 1];
            }

            if (vertical < 7)
            {
                if (board.ModCount != modCount)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return board[vertical + 1, Color == ChessPieceColor.White ? horizontal + 1 : horizontal - 1];
            }

            if (board.ModCount != modCount)
            {
                throw new InvalidOperationException("Изменение коллекции во время перечисления.");
            }
        }

        private IEnumerable<Square> GetAccessibleSquares(out IEnumerable<Square> unsafeForKingSquares)
        {
            if (!IsOnBoard || Board.Status != GameStatus.GameIsNotOver || Color != Board.MovingSideColor)
            {
                unsafeForKingSquares = Enumerable.Empty<Square>();
                return Enumerable.Empty<Square>();
            }

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

            var result = FilterSafeForKingMoves(moveSquares);
            unsafeForKingSquares = moveSquares.Except(result);
            return result;
        }

        public override IEnumerable<Square> GetAccessibleSquares() => GetAccessibleSquares(out var unsafeForKingSquares);

        public override string GetIllegalMoveMessage(Square square)
        {
            if (Board != square.Board)
            {
                throw new InvalidOperationException("Указано поле на другой доске.");
            }

            var accessibleSquares = GetAccessibleSquares(out var unsafeForKingSquares);

            if (accessibleSquares.Contains(square))
            {
                return null;
            }

            if (!square.IsEmpty && square.ContainedPiece.Color == Color)
            {
                return "Невозможно пойти на поле, занятое своей фигурой.";
            }

            if (unsafeForKingSquares.Contains(square))
            {
                if (IsPinnedVertically() && square.Vertical != Vertical)
                {
                    return "Невозможный ход. Фигура связана.";
                }

                if (IsPinnedHorizontally() && square.Horizontal != Horizontal)
                {
                    return "Невозможный ход. Фигура связана.";
                }

                if (IsPinnedDiagonally() && !(square.IsOnSameDiagonal(Position) && square.IsOnSameDiagonal(FriendlyKing)))
                {
                    return "Невозможный ход. Фигура связана.";
                }

                return "Невозможный ход. Ваш король под шахом.";
            }

            return "Невозможный ход.";
        }

        public override ChessPieceName Name => ChessPieceName.Pawn;
    }
}
