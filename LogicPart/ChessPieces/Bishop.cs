
namespace Chess.LogicPart
{
    public sealed class Bishop : ChessPiece
    {
        public Bishop(ChessPieceColor color) : base(color)
        { }

        public override IEnumerable<Square> GetAttackedSquares()
        {
            var position = Position;

            if (position == null)
            {
                yield break;
            }

            var board = position.Board;
            var vertical = position.Vertical;
            var horizontal = position.Horizontal;

            ulong modCount;
            ulong gameStartsCount;

            lock (board)
            {
                modCount = board.ModCount;
                gameStartsCount = board.GameStartsCount;
            }

            if (position != Position)
            {
                throw new InvalidOperationException("Изменение позиции во время перечисления.");
            }

            for (int i = vertical + 1, j = horizontal + 1; i < 8 && j < 8; ++i, ++j)
            {
                if (board.ModCount != modCount || board.GameStartsCount != gameStartsCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[i, j];

                if (!board[i, j].IsEmpty)
                {
                    break;
                }
            }

            for (int i = vertical - 1, j = horizontal - 1; i >= 0 && j >= 0; --i, --j)
            {
                if (board.ModCount != modCount || board.GameStartsCount != gameStartsCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[i, j];

                if (!board[i, j].IsEmpty)
                {
                    break;
                }
            }

            for (int i = vertical + 1, j = horizontal - 1; i < 8 && j >= 0; ++i, --j)
            {
                if (board.ModCount != modCount || board.GameStartsCount != gameStartsCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[i, j];

                if (!board[i, j].IsEmpty)
                {
                    break;
                }
            }

            for (int i = vertical - 1, j = horizontal + 1; i >= 0 && j < 8; --i, ++j)
            {
                if (board.ModCount != modCount || board.GameStartsCount != gameStartsCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[i, j];

                if (!board[i, j].IsEmpty)
                {
                    break;
                }
            }

            if (board.ModCount != modCount || board.GameStartsCount != gameStartsCount)
            {
                throw new InvalidOperationException("Изменение позиции во время перечисления.");
            }
        }

        public override ChessPieceName Name => ChessPieceName.Bishop;

        public override bool IsLongRanged => true;
    }
}