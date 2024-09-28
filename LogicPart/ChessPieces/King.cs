
namespace Chess.LogicPart
{
    public sealed class King : ChessPiece
    {
        public King(PieceColor color) : base(color) { }

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

            for (var i = square.X > 0 ? square.X - 1 : 0; i <= square.X + 1 && i < 8; ++i)
            {
                for (var j = square.Y > 0 ? square.Y - 1 : 0; j <= square.Y + 1 && j < 8; ++j)
                {
                    if (i == square.X && j == square.Y)
                    {
                        continue;
                    }

                    if (board.ModCount != modCount || board.GamesCount != gamesCount)
                    {
                        throw new InvalidOperationException("Изменение позиции во время перечисления.");
                    }

                    yield return board[i, j];
                }
            }
        }

        internal override bool CanMoveTo(Square square, out IllegalMoveException exception)
        {
            if (!Attacks(square))
            {
                exception = new();
                return false;
            }

            if (!CanSafelyMoveTo(square))
            {
                exception = new KingMovesToCheckedSquareException();
                return false;
            }

            exception = null;
            return true;
        }

        private bool CanSafelyMoveTo(Square square)
        {
            var enemyColor = Color == PieceColor.White ? PieceColor.Black : PieceColor.White;

            if (square.IsMenacedBy(enemyColor))
            {
                return false;
            }

            foreach (var menace in Menaces.Where(piece => piece.IsLongRanged))
            {
                if (menace.X == X)
                {
                    if (square.X == X && square != menace.Square)
                    {
                        return false;
                    }
                }
                else if (menace.Y == Y)
                {
                    if (square.Y == Y && square != menace.Square)
                    {
                        return false;
                    }
                }
                else
                {
                    if (square.IsOnSameDiagonal(menace) && square != menace.Square)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal bool CanCastleKingside(out IllegalMoveException exception)
        {
            if (FirstMoveMoment > 0)
            {
                exception = new CastlingKingHasMovedException();
                return false;
            }

            if (Board.GetPiece(7, Y).FirstMoveMoment > 0)
            {
                exception = new CastlingRookHasMovedException();
                return false;
            }

            if (IsChecked)
            {
                exception = new CastlingKingCheckedException();
                return false;
            }

            var enemyColor = Color == PieceColor.White ? PieceColor.Black : PieceColor.White;

            if (Board[5, Y].IsMenacedBy(enemyColor))
            {
                exception = new CastlingKingCrossesMenacedSquareException();
                return false;
            }

            if (Board[6, Y].IsMenacedBy(enemyColor))
            {
                exception = new KingMovesToCheckedSquareException();
                return false;
            }

            exception = null;
            return true;
        }

        internal bool CanCastleQueenside(out IllegalMoveException exception)
        {
            if (FirstMoveMoment > 0)
            {
                exception = new CastlingKingHasMovedException();
                return false;
            }

            if (Board.GetPiece(0, Y).FirstMoveMoment > 0)
            {
                exception = new CastlingRookHasMovedException();
                return false;
            }

            if (IsChecked)
            {
                exception = new CastlingKingCheckedException();
                return false;
            }

            var enemyColor = Color == PieceColor.White ? PieceColor.Black : PieceColor.White;

            if (Board[3, Y].IsMenacedBy(enemyColor))
            {
                exception = new CastlingKingCrossesMenacedSquareException();
                return false;
            }

            if (Board[2, Y].IsMenacedBy(enemyColor))
            {
                exception = new KingMovesToCheckedSquareException();
                return false;
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

                return FilterKingSafe(GetAttackedSquares().Where(sq => sq.Contained?.Color != Color)).
                Concat(GetCastlingSquares()).ToArray();
            }
        }

        private IEnumerable<Square> GetCastlingSquares()
        {
            var horizontal = Color == PieceColor.White ? 0 : 7;

            if (X != 4 || Y != horizontal)
            {
                yield break;
            }

            if (FirstMoveMoment > 0 || IsChecked)
            {
                yield break;
            }

            var enemyColor = Color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            var cornerPiece = Board.GetPiece(7, horizontal);

            if (cornerPiece?.Name != PieceName.Rook || cornerPiece.Color != Color || cornerPiece.FirstMoveMoment > 0)
            { }
            else if (!Board[5, horizontal].IsClear || !Board[6, horizontal].IsClear)
            { }
            else if (!Board[5, horizontal].IsMenacedBy(enemyColor) && !Board[6, horizontal].IsMenacedBy(enemyColor))
            {
                yield return Board[6, horizontal];
            }

            cornerPiece = Board.GetPiece(0, horizontal);

            if (cornerPiece?.Name != PieceName.Rook || cornerPiece.Color != Color || cornerPiece.FirstMoveMoment > 0)
            {
                yield break;
            }

            if (!Board[1, horizontal].IsClear || !Board[2, horizontal].IsClear || !Board[3, horizontal].IsClear)
            {
                yield break;
            }

            if (!Board[2, horizontal].IsMenacedBy(enemyColor) && !Board[3, horizontal].IsMenacedBy(enemyColor))
            {
                yield return Board[2, horizontal];
            }
        }

        private protected override IEnumerable<Square> FilterKingSafe(IEnumerable<Square> unfiltered) =>
        unfiltered.Where(square => CanSafelyMoveTo(square));

        internal void CastleKingside()
        {
            var horizontal = Color == PieceColor.White ? 0 : 7;
            var rook = Board.GetPiece(7, horizontal);

            RemoveExcessMenaces(Board[6, horizontal]);
            rook.RemoveVerticalMenaces();
            rook.RemoveMenace(Board[5, horizontal]);

            Square.Contained = null;
            Square = Board[6, horizontal];
            Square.Contained = this;

            rook.Square.Contained = null;
            rook.Square = Board[5, horizontal];
            rook.Square.Contained = rook;

            var list = Color == PieceColor.White ? Board[4, 0].WhiteMenaces : Board[4, 7].BlackMenaces;
            var piece = list.Where(p => p.Y == horizontal && p.X < 4).FirstOrDefault();
            piece?.AddMenace(Board[5, horizontal]);

            AddMissingMenaces(Board[4, horizontal]);
            rook.AddVerticalMenaces();

            for (var i = 3; i >= 0; --i)
            {
                var square = Board[i, horizontal];
                rook.AddMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }
        }

        internal void CastleQueenside()
        {
            var horizontal = Color == PieceColor.White ? 0 : 7;
            var rook = Board.GetPiece(0, horizontal);

            RemoveExcessMenaces(Board[2, horizontal]);
            rook.RemoveVerticalMenaces();
            rook.RemoveMenace(Board[1, horizontal]);
            rook.RemoveMenace(Board[3, horizontal]);

            Square.Contained = null;
            Square = Board[2, horizontal];
            Square.Contained = this;

            rook.Square.Contained = null;
            rook.Square = Board[3, horizontal];
            rook.Square.Contained = rook;

            var list = Color == PieceColor.White ? Board[4, 0].WhiteMenaces : Board[4, 7].BlackMenaces;
            var piece = list.Where(p => p.Y == horizontal && p.X > 4).FirstOrDefault();
            piece?.AddMenace(Board[3, horizontal]);

            AddMissingMenaces(Board[4, horizontal]);
            rook.AddVerticalMenaces();

            for (var i = 5; i < 8; ++i)
            {
                var square = Board[i, horizontal];
                rook.AddMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }
        }

        internal void CancelKingsideCastling()
        {
            var horizontal = Color == PieceColor.White ? 0 : 7;
            var rook = Board.GetPiece(5, horizontal);

            RemoveExcessMenaces(Board[4, horizontal]);
            rook.RemoveVerticalMenaces();

            for (var i = 3; i >= 0; --i)
            {
                var square = Board[i, horizontal];
                rook.RemoveMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }

            Square.Contained = null;
            Square = Board[4, horizontal];
            Square.Contained = this;

            rook.Square.Contained = null;
            rook.Square = Board[7, horizontal];
            rook.Square.Contained = rook;

            var list = Color == PieceColor.White ? Board[5, 0].WhiteMenaces : Board[5, 7].BlackMenaces;
            var piece = list.Where(p => p.Y == horizontal && p.X < 4).FirstOrDefault();
            piece?.RemoveMenace(Board[5, horizontal]);

            AddMissingMenaces(Board[6, horizontal]);
            rook.AddVerticalMenaces();
            rook.AddMenace(Board[5, horizontal]);
        }

        internal void CancelQueensideCastling()
        {
            var horizontal = Color == PieceColor.White ? 0 : 7;
            var rook = Board.GetPiece(3, horizontal);

            RemoveExcessMenaces(Board[4, horizontal]);
            rook.RemoveVerticalMenaces();

            for (var i = 5; i < 8; ++i)
            {
                var square = Board[i, horizontal];
                rook.RemoveMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }

            Square.Contained = null;
            Square = Board[4, horizontal];
            Square.Contained = this;

            rook.Square.Contained = null;
            rook.Square = Board[0, horizontal];
            rook.Square.Contained = rook;

            var list = Color == PieceColor.White ? Board[3, 0].WhiteMenaces : Board[3, 7].BlackMenaces;
            var piece = list.Where(p => p.Y == horizontal && p.X > 4).FirstOrDefault();
            piece?.RemoveMenace(Board[3, horizontal]);

            AddMissingMenaces(Board[2, horizontal]);
            rook.AddVerticalMenaces();
            rook.AddMenace(Board[1, horizontal]);
            rook.AddMenace(Board[3, horizontal]);
        }

        internal override void RemoveExcessMenaces(Square newSquare)
        {
            if (newSquare.X >= X - 1 && newSquare.X <= X + 1)
            {
                RemoveMenace(newSquare);
            }

            foreach (var square in GetAttackedSquares())
            {
                if (square.X < newSquare.X - 1 ||
                    square.X > newSquare.X + 1 ||
                    square.Y < newSquare.Y - 1 ||
                    square.Y > newSquare.Y + 1)
                {
                    RemoveMenace(square);
                }
            }
        }

        internal override void AddMissingMenaces(Square oldSquare)
        {
            if (oldSquare.X >= X - 1 && oldSquare.X <= X + 1)
            {
                AddMenace(oldSquare);
            }

            foreach (var square in GetAttackedSquares())
            {
                if (square.X < oldSquare.X - 1 ||
                    square.X > oldSquare.X + 1 ||
                    square.Y < oldSquare.Y - 1 ||
                    square.Y > oldSquare.Y + 1)
                {
                    AddMenace(square);
                }
            }
        }

        internal override void OpenLine(Square oldPiecePosition, Square newPiecePosition) =>
        throw new NotImplementedException();

        internal override void BlockLine(Square blockSquare) =>
        throw new NotImplementedException();

        public override PieceName Name => PieceName.King;

        public override bool IsLongRanged => false;

        public bool IsChecked
        {
            get
            {
                var menaces = Menaces;
                return menaces != null && menaces.Count > 0;
            }
        }
    }
}