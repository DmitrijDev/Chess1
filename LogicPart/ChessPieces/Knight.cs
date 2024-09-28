
namespace Chess.LogicPart
{
    public sealed class Knight : ChessPiece
    {
        public Knight(PieceColor color) : base(color) { }

        public override IEnumerable<Square> GetAttackedSquares()
        {
            var square = Square;

            if (square == null)
            {
                yield break;
            }

            var board = square.Board;
            var gamesCount = board.GamesCount;
            var modCount = board.ModCount;

            var verticalShifts = new int[] { 2, 2, -2, -2, 1, 1, -1, -1 };
            var horizontalShifts = new int[] { -1, 1, -1, 1, -2, 2, -2, 2 };

            for (var i = 0; i < 8; ++i)
            {
                var targetSquareX = square.X + horizontalShifts[i];
                var targetSquareY = square.Y + verticalShifts[i];

                if (targetSquareX < 0 || targetSquareY < 0 || targetSquareX >= 8 || targetSquareY >= 8)
                {
                    continue;
                }

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[targetSquareX, targetSquareY];
            }
        }

        internal override void RemoveExcessMenaces(Square newSquare) => RemoveMenaces();

        internal override void AddMissingMenaces(Square oldSquare) => AddMenaces();

        internal override void OpenLine(Square oldPiecePosition, Square newPiecePosition) =>
        throw new NotImplementedException();

        internal override void BlockLine(Square blockSquare) =>
        throw new NotImplementedException();

        public override PieceName Name => PieceName.Knight;

        public override bool IsLongRanged => false;
    }
}
