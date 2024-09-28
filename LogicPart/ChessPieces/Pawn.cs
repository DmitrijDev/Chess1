
namespace Chess.LogicPart
{
    public sealed class Pawn : ChessPiece
    {
        public Pawn(PieceColor color) : base(color) { }

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

            if (square.X > 0)
            {
                yield return board[square.X - 1, Color == PieceColor.White ? square.Y + 1 : square.Y - 1];
            }

            if (square.X < 7)
            {
                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return board[square.X + 1, Color == PieceColor.White ? square.Y + 1 : square.Y - 1];
            }
        }

        public override bool CanMove()
        {
            var board = Board;

            if (board == null)
            {
                return false;
            }

            lock (board.Locker)
            {
                if (Board != board)
                {
                    throw new InvalidOperationException("Во время вычисления этой функции нельзя менять позицию на доске.");
                }

                if ((board.Status != BoardStatus.GameIncomplete && !board.IsSettingPosition) ||
                     board.MoveTurn != Color)
                {
                    return false;
                }

                var unfiltered = GetAttackedSquares().
                Where(sq => (!sq.IsClear && sq.Contained.Color != Color) || sq.IsPawnPassed);

                if (Color == PieceColor.White)
                {
                    if (board[X, Y + 1].IsClear)
                    {
                        unfiltered = unfiltered.Append(board[X, Y + 1]);

                        if (Y == 1 && board[X, 3].IsClear)
                        {
                            unfiltered = unfiltered.Append(board[X, 3]);
                        }
                    }
                }
                else
                {
                    if (board[X, Y - 1].IsClear)
                    {
                        unfiltered = unfiltered.Append(board[X, Y - 1]);

                        if (Y == 6 && board[X, 4].IsClear)
                        {
                            unfiltered = unfiltered.Append(board[X, 4]);
                        }
                    }
                }

                return FilterKingSafe(unfiltered).Any();
            }
        }

        internal override bool CanMoveTo(Square square, out IllegalMoveException exception)
        {
            if (Attacks(square))
            {
                if (square.IsClear && !square.IsPawnPassed)
                {
                    exception = new();
                    return false;
                }
            }
            else
            {
                if (!square.IsClear || square.X != X)
                {
                    exception = new();
                    return false;
                }

                if (Color == PieceColor.White)
                {
                    if (square.Y < Y || square.Y > Y + 2)
                    {
                        exception = new();
                        return false;
                    }

                    if (square.Y == Y + 2 && !(Y == 1 && Board[X, 2].IsClear))
                    {
                        exception = new();
                        return false;
                    }
                }
                else
                {
                    if (square.Y > Y || square.Y < Y - 2)
                    {
                        exception = new();
                        return false;
                    }

                    if (square.Y == Y - 2 && !(Y == 6 && Board[X, 5].IsClear))
                    {
                        exception = new();
                        return false;
                    }
                }
            }

            if (IsPinnedVertically())
            {
                if (square.X != X)
                {
                    exception = new PawnPinnedException();
                    return false;
                }
            }
            else if (IsPinnedHorizontally())
            {
                exception = new PawnPinnedException();
                return false;
            }
            else if (IsPinnedDiagonally())
            {
                if (!IsOnSameDiagonal(square) || !King.IsOnSameDiagonal(square))
                {
                    exception = new PawnPinnedException();
                    return false;
                }
            }

            if (King.IsChecked)
            {
                if (King.Menaces.Count > 1 || !ProtectsKingByMoveTo(square))
                {
                    exception = new KingCheckedException();
                    return false;
                }
            }

            exception = null;
            return true;
        }

        public override Square[] GetAccessibleSquares()
        {
            var board = Board;

            if (board == null)
            {
                return Array.Empty<Square>();
            }

            lock (board.Locker)
            {
                if (Board != board)
                {
                    throw new InvalidOperationException("Во время вычисления этой функции нельзя менять позицию на доске.");
                }

                if (board.Status != BoardStatus.GameIncomplete || board.MoveTurn != Color)
                {
                    return Array.Empty<Square>();
                }

                var unfiltered = GetAttackedSquares().
                Where(sq => (!sq.IsClear && sq.Contained.Color != Color) || sq.IsPawnPassed);

                if (Color == PieceColor.White)
                {
                    if (board[X, Y + 1].IsClear)
                    {
                        unfiltered = unfiltered.Append(board[X, Y + 1]);

                        if (Y == 1 && board[X, 3].IsClear)
                        {
                            unfiltered = unfiltered.Append(board[X, 3]);
                        }
                    }
                }
                else
                {
                    if (board[X, Y - 1].IsClear)
                    {
                        unfiltered = unfiltered.Append(board[X, Y - 1]);

                        if (Y == 6 && board[X, 4].IsClear)
                        {
                            unfiltered = unfiltered.Append(board[X, 4]);
                        }
                    }
                }

                return FilterKingSafe(unfiltered).ToArray();
            }
        }

        internal override void RemoveExcessMenaces(Square newSquare) => RemoveMenaces();

        internal override void AddMissingMenaces(Square oldSquare) => AddMenaces();

        internal override void OpenLine(Square oldPiecePosition, Square newPiecePosition) =>
        throw new NotImplementedException();

        internal override void BlockLine(Square blockSquare) =>
        throw new NotImplementedException();

        public override PieceName Name => PieceName.Pawn;

        public override bool IsLongRanged => false;
    }
}
