
namespace Chess.LogicPart
{
    public sealed class Bishop : ChessPiece
    {
        public Bishop(PieceColor color) : base(color) { }

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

            if (Square != square)
            {
                throw new InvalidOperationException("Изменение позиции во время перечисления.");
            }

            for (int i = square.X + 1, j = square.Y + 1; i < 8 && j < 8; ++i, ++j)
            {
                var piece = board.GetPiece(i, j);

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[i, j];

                if (piece != null)
                {
                    break;
                }
            }

            for (int i = square.X - 1, j = square.Y - 1; i >= 0 && j >= 0; --i, --j)
            {
                var piece = board.GetPiece(i, j);

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[i, j];

                if (piece != null)
                {
                    break;
                }
            }

            for (int i = square.X + 1, j = square.Y - 1; i < 8 && j >= 0; ++i, --j)
            {
                var piece = board.GetPiece(i, j);

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[i, j];

                if (piece != null)
                {
                    break;
                }
            }

            for (int i = square.X - 1, j = square.Y + 1; i >= 0 && j < 8; --i, ++j)
            {
                var piece = board.GetPiece(i, j);

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[i, j];

                if (piece != null)
                {
                    break;
                }
            }
        }        

        internal override void RemoveExcessMenaces(Square newSquare)
        {
            RemoveMenace(newSquare);

            if ((newSquare.X > X && newSquare.Y > Y) ||
                (newSquare.X < X && newSquare.Y < Y))
            {
                RemoveMenacesToLowerRight();
                RemoveMenacesToUpperLeft();
                return;
            }

            RemoveMenacesToUpperRight();
            RemoveMenacesToLowerLeft();
        }

        internal override void AddMissingMenaces(Square oldSquare)
        {
            if (oldSquare == null)
            {
                AddMenaces();
                return;
            }

            AddMenace(oldSquare);

            if ((X > oldSquare.X && Y > oldSquare.Y) ||
                (X < oldSquare.X && Y < oldSquare.Y))
            {
                AddMenacesToLowerRight();
                AddMenacesToUpperLeft();
                return;
            }

            AddMenacesToUpperRight();
            AddMenacesToLowerLeft();
        }

        internal override void OpenLine(Square oldPiecePosition, Square newPiecePosition)
        {
            if (IsOnSameDiagonal(newPiecePosition) && oldPiecePosition.IsOnSameDiagonal(newPiecePosition) &&
                (X < newPiecePosition.X ^ oldPiecePosition.X < newPiecePosition.X))
            {
                return;
            }

            for (int i = oldPiecePosition.X > X ? oldPiecePosition.X + 1 : oldPiecePosition.X - 1,
                 j = oldPiecePosition.Y > Y ? oldPiecePosition.Y + 1 : oldPiecePosition.Y - 1;
                 i >= 0 && j >= 0 && i < 8 && j < 8;
                 i = i > oldPiecePosition.X ? i + 1 : i - 1, j = j > oldPiecePosition.Y ? j + 1 : j - 1)
            {
                var square = Board[i, j];
                AddMenace(square);

                if (!square.IsClear || square == newPiecePosition)
                {
                    return;
                }
            }
        }

        internal override void BlockLine(Square blockSquare)
        {
            for (int i = blockSquare.X > X ? blockSquare.X + 1 : blockSquare.X - 1,
                 j = blockSquare.Y > Y ? blockSquare.Y + 1 : blockSquare.Y - 1;
                 i >= 0 && j >= 0 && i < 8 && j < 8;
                 i = i > blockSquare.X ? i + 1 : i - 1, j = j > blockSquare.Y ? j + 1 : j - 1)
            {
                var square = Board[i, j];

                if (!RemoveMenace(square) || !square.IsClear)
                {
                    return;
                }
            }
        }

        public override PieceName Name => PieceName.Bishop;

        public override bool IsLongRanged => true;
    }
}