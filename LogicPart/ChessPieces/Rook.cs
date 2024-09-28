
namespace Chess.LogicPart
{
    public sealed class Rook : ChessPiece
    {
        public Rook(PieceColor color) : base(color) { }

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

            for (var i = square.Y + 1; i < 8; ++i)
            {
                var piece = board.GetPiece(square.X, i);

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[square.X, i];

                if (piece != null)
                {
                    break;
                }
            }

            for (var i = square.Y - 1; i >= 0; --i)
            {
                var piece = board.GetPiece(square.X, i);

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[square.X, i];

                if (piece != null)
                {
                    break;
                }
            }

            for (var i = square.X + 1; i < 8; ++i)
            {
                var piece = board.GetPiece(i, square.Y);

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[i, square.Y];

                if (piece != null)
                {
                    break;
                }
            }

            for (var i = square.X - 1; i >= 0; --i)
            {
                var piece = board.GetPiece(i, square.Y);

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[i, square.Y];

                if (piece != null)
                {
                    break;
                }
            }
        }        

        internal override void RemoveExcessMenaces(Square newSquare)
        {
            RemoveMenace(newSquare);

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

            AddMenace(oldSquare);

            if (oldSquare.X == X)
            {
                AddHorizontalMenaces();
                return;
            }

            AddVerticalMenaces();
        }

        internal override void OpenLine(Square oldPiecePosition, Square newPiecePosition)
        {
            if (oldPiecePosition.X == X)
            {
                if (newPiecePosition?.X == X && (Y < newPiecePosition.Y ^ oldPiecePosition.Y < newPiecePosition.Y))
                {
                    return;
                }

                for (var i = oldPiecePosition.Y > Y ? oldPiecePosition.Y + 1 : oldPiecePosition.Y - 1; i >= 0 && i < 8;
                i = i > oldPiecePosition.Y ? i + 1 : i - 1)
                {
                    var square = Board[X, i];
                    AddMenace(square);

                    if (!square.IsClear || square == newPiecePosition)
                    {
                        return;
                    }
                }

                return;
            }

            if (newPiecePosition?.Y == Y && (X < newPiecePosition.X ^ oldPiecePosition.X < newPiecePosition.X))
            {
                return;
            }

            for (var i = oldPiecePosition.X > X ? oldPiecePosition.X + 1 : oldPiecePosition.X - 1; i >= 0 && i < 8;
            i = i > oldPiecePosition.X ? i + 1 : i - 1)
            {
                var square = Board[i, Y];
                AddMenace(square);

                if (!square.IsClear || square == newPiecePosition)
                {
                    return;
                }
            }
        }

        internal override void BlockLine(Square blockSquare)
        {
            if (blockSquare.X == X)
            {
                for (var i = blockSquare.Y > Y ? blockSquare.Y + 1 : blockSquare.Y - 1; i >= 0 && i < 8;
                i = i > blockSquare.Y ? i + 1 : i - 1)
                {
                    var square = Board[X, i];

                    if (!RemoveMenace(square) || !square.IsClear)
                    {
                        return;
                    }
                }

                return;
            }

            for (var i = blockSquare.X > X ? blockSquare.X + 1 : blockSquare.X - 1; i >= 0 && i < 8;
                 i = i > blockSquare.X ? i + 1 : i - 1)
            {
                var square = Board[i, Y];

                if (!RemoveMenace(square) || !square.IsClear)
                {
                    return;
                }
            }
        }

        public override PieceName Name => PieceName.Rook;

        public override bool IsLongRanged => true;
    }
}