
namespace Chess.LogicPart
{
    public class Queen : ChessPiece
    {
        public Queen(ChessPieceColor color) => Color = color;

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

            for (var i = vertical + 1; i < 8; ++i)
            {
                if (board.ModCount != modCount)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return board[i, horizontal];

                if (!board[i, horizontal].IsEmpty)
                {
                    break;
                }
            }

            for (var i = vertical - 1; i >= 0; --i)
            {
                if (board.ModCount != modCount)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return board[i, horizontal];

                if (!board[i, horizontal].IsEmpty)
                {
                    break;
                }
            }

            for (var i = horizontal + 1; i < 8; ++i)
            {
                if (board.ModCount != modCount)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return board[vertical, i];

                if (!board[vertical, i].IsEmpty)
                {
                    break;
                }
            }

            for (var i = horizontal - 1; i >= 0; --i)
            {
                if (board.ModCount != modCount)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return board[vertical, i];

                if (!board[vertical, i].IsEmpty)
                {
                    break;
                }
            }

            for (int i = vertical + 1, j = horizontal + 1; i < 8 && j < 8; ++i, ++j)
            {
                if (board.ModCount != modCount)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return board[i, j];

                if (!board[i, j].IsEmpty)
                {
                    break;
                }
            }

            for (int i = vertical - 1, j = horizontal - 1; i >= 0 && j >= 0; --i, --j)
            {
                if (board.ModCount != modCount)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return board[i, j];

                if (!board[i, j].IsEmpty)
                {
                    break;
                }
            }

            for (int i = vertical + 1, j = horizontal - 1; i < 8 && j >= 0; ++i, --j)
            {
                if (board.ModCount != modCount)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return board[i, j];

                if (!board[i, j].IsEmpty)
                {
                    break;
                }
            }

            for (int i = vertical - 1, j = horizontal + 1; i >= 0 && j < 8; --i, ++j)
            {
                if (board.ModCount != modCount)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return board[i, j];

                if (!board[i, j].IsEmpty)
                {
                    break;
                }
            }

            if (board.ModCount != modCount)
            {
                throw new InvalidOperationException("Изменение коллекции во время перечисления.");
            }
        }

        public override ChessPieceName Name => ChessPieceName.Queen;
    }
}
