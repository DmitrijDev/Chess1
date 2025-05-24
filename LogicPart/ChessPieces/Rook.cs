
namespace Chess.LogicPart
{
    public sealed class Rook : ChessPiece
    {
        internal Rook(PieceColor color) : base(color) { }

        public override IEnumerable<Square> GetAttackedSquares()
        {
            var board = Board;

            if (board == null)
            {
                yield break;
            }

            Square rookPosition;
            ulong gamesCount;
            ulong modCount;

            lock (board.Locker)
            {
                if (Board != board)
                {
                    yield break;
                }

                rookPosition = Square;
                gamesCount = board.GamesCount;
                modCount = board.ModCount;
            }

            for (var i = rookPosition.Y + 1; i < 8; ++i)
            {
                var square = board[rookPosition.X, i];
                var squareClear = square.IsClear;

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return square;

                if (!squareClear)
                {
                    break;
                }
            }

            for (var i = rookPosition.Y - 1; i >= 0; --i)
            {
                var square = board[rookPosition.X, i];
                var squareClear = square.IsClear;

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return square;

                if (!squareClear)
                {
                    break;
                }
            }

            for (var i = rookPosition.X + 1; i < 8; ++i)
            {
                var square = board[i, rookPosition.Y];
                var squareClear = square.IsClear;

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return square;

                if (!squareClear)
                {
                    break;
                }
            }

            for (var i = rookPosition.X - 1; i >= 0; --i)
            {
                var square = board[i, rookPosition.Y];
                var squareClear = square.IsClear;

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return square;

                if (!squareClear)
                {
                    break;
                }
            }
        }

        internal override void RemoveUnactualMenaces(Square newSquare)
        {
            newSquare.RemoveMenace(this);

            if (newSquare.X == X)
            {
                RemoveHorizontalMenaces();
                return;
            }

            RemoveVerticalMenaces();
        }

        internal override void AddMissingMenaces(Square oldSquare)
        {
            if (oldSquare == null)
            {
                AddMenaces();
                return;
            }

            oldSquare.AddMenace(this);

            if (oldSquare.X == X)
            {
                AddHorizontalMenaces();
                return;
            }

            AddVerticalMenaces();
        }    
        
        public override PieceName Name => PieceName.Rook;

        public override bool IsLongRanged => true;
    }
}