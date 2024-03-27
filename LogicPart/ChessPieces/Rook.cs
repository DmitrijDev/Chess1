
namespace Chess.LogicPart
{
    public sealed class Rook : ChessPiece
    {
        public Rook(ChessPieceColor color) : base(color)
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

            for (var i = vertical + 1; i < 8; ++i)
            {
                if (board.ModCount != modCount || board.GameStartsCount != gameStartsCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[i, horizontal];

                if (!board[i, horizontal].IsEmpty)
                {
                    break;
                }
            }

            for (var i = vertical - 1; i >= 0; --i)
            {
                if (board.ModCount != modCount || board.GameStartsCount != gameStartsCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[i, horizontal];

                if (!board[i, horizontal].IsEmpty)
                {
                    break;
                }
            }

            for (var i = horizontal + 1; i < 8; ++i)
            {
                if (board.ModCount != modCount || board.GameStartsCount != gameStartsCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[vertical, i];

                if (!board[vertical, i].IsEmpty)
                {
                    break;
                }
            }

            for (var i = horizontal - 1; i >= 0; --i)
            {
                if (board.ModCount != modCount || board.GameStartsCount != gameStartsCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[vertical, i];

                if (!board[vertical, i].IsEmpty)
                {
                    break;
                }
            }

            if (board.ModCount != modCount || board.GameStartsCount != gameStartsCount)
            {
                throw new InvalidOperationException("Изменение позиции во время перечисления.");
            }
        }

        public override ChessPieceName Name => ChessPieceName.Rook;

        public override bool IsLongRanged => true;
    }
}