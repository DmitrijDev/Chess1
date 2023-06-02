
namespace Chess.LogicPart
{
    public class Bishop : ChessPiece
    {
        public Bishop(ChessPieceColor color) : base(color)
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

        public override ChessPieceName Name => ChessPieceName.Bishop;
    }
}
