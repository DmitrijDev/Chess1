
namespace Chess.LogicPart
{
    public abstract class ChessPiece
    {
        public PieceColor Color { get; }

        public Square Square { get; internal set; }

        public int FirstMoveMoment { get; internal set; }

        internal ChessPiece(PieceColor color) => Color = color;

        public static ChessPiece GetNewPiece(PieceName name, PieceColor color) => name switch
        {
            PieceName.King => new King(color),
            PieceName.Queen => new Queen(color),
            PieceName.Rook => new Rook(color),
            PieceName.Knight => new Knight(color),
            PieceName.Bishop => new Bishop(color),
            _ => new Pawn(color)
        };

        public abstract IEnumerable<Square> GetAttackedSquares();

        public bool Attacks(Square square) => Color == PieceColor.White ?
        square.WhiteMenaces.Contains(this) : square.BlackMenaces.Contains(this);

        // Переопределено для пешки.
        public virtual bool CanMove()
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
                    throw new InvalidOperationException("Во время вычисления этой функции нельзя менять позицию фигуры.");
                }

                if ((board.Status != BoardStatus.GameIncomplete && !board.IsSettingPosition) ||
                     board.MoveTurn != Color)
                {
                    return false;
                }

                return FilterKingSafe(GetAttackedSquares().Where(sq => sq.Contained?.Color != Color)).Any();
            }
        }

        // Переопределено для короля и пешки.        
        internal virtual bool CanMoveTo(Square square, out IllegalMoveException exception)
        {
            if (!Attacks(square))
            {
                exception = new();
                return false;
            }

            if (IsPinnedVertically())
            {
                if (square.X != X)
                {
                    exception = new PiecePinnedException();
                    return false;
                }
            }
            else if (IsPinnedHorizontally())
            {
                if (square.Y != Y)
                {
                    exception = new PiecePinnedException();
                    return false;
                }
            }
            else if (IsPinnedDiagonally())
            {
                if (!IsOnSameDiagonal(square) || !King.IsOnSameDiagonal(square))
                {
                    exception = new PiecePinnedException();
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

        // Переопределено для короля и пешки.
        public virtual Square[] GetAccessibleSquares()
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
                    throw new InvalidOperationException("Во время вычисления этой функции нельзя менять позицию фигуры.");
                }

                if (board.Status != BoardStatus.GameIncomplete || board.MoveTurn != Color)
                {
                    return Array.Empty<Square>();
                }

                return FilterKingSafe(GetAttackedSquares().Where(sq => sq.Contained?.Color != Color)).ToArray();
            }
        }

        // Переопределено для короля.
        private protected virtual IEnumerable<Square> FilterKingSafe(IEnumerable<Square> unfiltered)
        {
            var result = unfiltered;

            if (King.IsChecked)
            {
                if (King.Menaces.Count > 1)
                {
                    return Enumerable.Empty<Square>();
                }

                result = result.Where(sq => ProtectsKingByMoveTo(sq));
            }

            if (IsPinnedVertically())
            {
                result = result.Where(sq => sq.X == X);
            }
            else if (IsPinnedHorizontally())
            {
                result = result.Where(sq => sq.Y == Y);
            }
            else if (IsPinnedDiagonally())
            {
                result = result.Where(sq => IsOnSameDiagonal(sq) && King.IsOnSameDiagonal(sq));
            }

            return result;
        }

        private protected bool ProtectsKingByMoveTo(Square square)
        {
            var menace = King.Menaces[0];

            if (square == menace.Square)
            {
                return true;
            }

            if (square.IsPawnPassed && Name == PieceName.Pawn && menace.Name == PieceName.Pawn)
            {
                return true;
            }

            if (!menace.IsLongRanged)
            {
                return false;
            }

            if (menace.X == King.X)
            {
                return square.X == menace.X && (King.Y < square.Y ^ menace.Y < square.Y);
            }

            if (menace.Y == King.Y)
            {
                return square.Y == menace.Y && (King.X < square.X ^ menace.X < square.X);
            }

            return square.IsOnSameDiagonal(King) && square.IsOnSameDiagonal(menace) &&
            (King.X < square.X ^ menace.X < square.X);
        }

        public bool IsPinnedVertically()
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
                    throw new InvalidOperationException("Во время вычисления этой функции нельзя менять позицию фигуры.");
                }

                if (King.X != X)
                {
                    return false;
                }

                for (var i = King.Y > Y ? Y + 1 : Y - 1; i != King.Y; i = i > Y ? i + 1 : i - 1)
                {
                    if (!board[X, i].IsClear)
                    {
                        return false;
                    }
                }

                for (var i = King.Y > Y ? Y - 1 : Y + 1; i >= 0 && i < 8; i = i > Y ? i + 1 : i - 1)
                {
                    var piece = board.GetPiece(X, i);

                    if (piece != null)
                    {
                        return piece.Color != Color && (piece.Name == PieceName.Queen ||
                        piece.Name == PieceName.Rook);
                    }
                }

                return false;
            }
        }

        public bool IsPinnedHorizontally()
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
                    throw new InvalidOperationException("Во время вычисления этой функции нельзя менять позицию фигуры.");
                }

                if (King.Y != Y)
                {
                    return false;
                }

                for (var i = King.X > X ? X + 1 : X - 1; i != King.X; i = i > X ? i + 1 : i - 1)
                {
                    if (!board[i, Y].IsClear)
                    {
                        return false;
                    }
                }

                for (var i = King.X > X ? X - 1 : X + 1; i >= 0 && i < 8; i = i > X ? i + 1 : i - 1)
                {
                    var piece = board.GetPiece(i, Y);

                    if (piece != null)
                    {
                        return piece.Color != Color && (piece.Name == PieceName.Queen ||
                        piece.Name == PieceName.Rook);
                    }
                }

                return false;
            }
        }

        public bool IsPinnedDiagonally()
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
                    throw new InvalidOperationException("Во время вычисления этой функции нельзя менять позицию фигуры.");
                }

                if (!IsOnSameDiagonal(King))
                {
                    return false;
                }

                for (int i = King.X > X ? X + 1 : X - 1, j = King.Y > Y ? Y + 1 : Y - 1;
                     i != King.X; i = i > X ? i + 1 : i - 1, j = j > Y ? j + 1 : j - 1)
                {
                    if (!board[i, j].IsClear)
                    {
                        return false;
                    }
                }

                for (int i = King.X > X ? X - 1 : X + 1, j = King.Y > Y ? Y - 1 : Y + 1;
                     i >= 0 && j >= 0 && i < 8 && j < 8;
                     i = i > X ? i + 1 : i - 1, j = j > Y ? j + 1 : j - 1)
                {
                    var piece = board.GetPiece(i, j);

                    if (piece != null)
                    {
                        return piece.Color != Color && (piece.Name == PieceName.Queen ||
                        piece.Name == PieceName.Bishop);
                    }
                }

                return false;
            }
        }

        public bool IsOnSameDiagonal(Square square)
        {
            var position = Square;
            return position != null && position.IsOnSameDiagonal(square);
        }

        public bool IsOnSameDiagonal(ChessPiece other)
        {
            var position = Square;
            return position != null && position.IsOnSameDiagonal(other);
        }

        internal bool RemoveMenace(Square square)
        {
            var list = Color == PieceColor.White ? square.WhiteMenaces : square.BlackMenaces;
            return list.Remove(this);
        }

        internal void AddMenace(Square square)
        {
            var list = Color == PieceColor.White ? square.WhiteMenaces : square.BlackMenaces;
            list.Add(this);
        }

        internal void RemoveVerticalMenaces()
        {
            for (var i = Y + 1; i < 8; ++i)
            {
                var square = Board[X, i];
                RemoveMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }

            for (var i = Y - 1; i >= 0; --i)
            {
                var square = Board[X, i];
                RemoveMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }
        }

        internal void AddVerticalMenaces()
        {
            for (var i = Y + 1; i < 8; ++i)
            {
                var square = Board[X, i];
                AddMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }

            for (var i = Y - 1; i >= 0; --i)
            {
                var square = Board[X, i];
                AddMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }
        }

        private protected void RemoveHorizontalMenaces()
        {
            for (var i = X + 1; i < 8; ++i)
            {
                var square = Board[i, Y];
                RemoveMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }

            for (var i = X - 1; i >= 0; --i)
            {
                var square = Board[i, Y];
                RemoveMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }
        }

        private protected void AddHorizontalMenaces()
        {
            for (var i = X + 1; i < 8; ++i)
            {
                var square = Board[i, Y];
                AddMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }

            for (var i = X - 1; i >= 0; --i)
            {
                var square = Board[i, Y];
                AddMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }
        }

        private protected void RemoveDiagonalMenaces()
        {
            RemoveMenacesToUpperRight();
            RemoveMenacesToLowerLeft();
            RemoveMenacesToLowerRight();
            RemoveMenacesToUpperLeft();
        }

        private protected void AddDiagonalMenaces()
        {
            AddMenacesToUpperRight();
            AddMenacesToLowerLeft();
            AddMenacesToLowerRight();
            AddMenacesToUpperLeft();
        }

        private protected void RemoveMenacesToUpperRight()
        {
            for (int i = X + 1, j = Y + 1; i < 8 && j < 8; ++i, ++j)
            {
                var square = Board[i, j];
                RemoveMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }
        }

        private protected void AddMenacesToUpperRight()
        {
            for (int i = X + 1, j = Y + 1; i < 8 && j < 8; ++i, ++j)
            {
                var square = Board[i, j];
                AddMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }
        }

        private protected void RemoveMenacesToLowerLeft()
        {
            for (int i = X - 1, j = Y - 1; i >= 0 && j >= 0; --i, --j)
            {
                var square = Board[i, j];
                RemoveMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }
        }

        private protected void AddMenacesToLowerLeft()
        {
            for (int i = X - 1, j = Y - 1; i >= 0 && j >= 0; --i, --j)
            {
                var square = Board[i, j];
                AddMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }
        }

        private protected void RemoveMenacesToLowerRight()
        {
            for (int i = X + 1, j = Y - 1; i < 8 && j >= 0; ++i, --j)
            {
                var square = Board[i, j];
                RemoveMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }
        }

        private protected void AddMenacesToLowerRight()
        {
            for (int i = X + 1, j = Y - 1; i < 8 && j >= 0; ++i, --j)
            {
                var square = Board[i, j];
                AddMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }
        }

        private protected void RemoveMenacesToUpperLeft()
        {
            for (int i = X - 1, j = Y + 1; i >= 0 && j < 8; --i, ++j)
            {
                var square = Board[i, j];
                RemoveMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }
        }

        private protected void AddMenacesToUpperLeft()
        {
            for (int i = X - 1, j = Y + 1; i >= 0 && j < 8; --i, ++j)
            {
                var square = Board[i, j];
                AddMenace(square);

                if (!square.IsClear)
                {
                    break;
                }
            }
        }

        internal void RemoveMenaces()
        {
            foreach (var square in GetAttackedSquares())
            {
                RemoveMenace(square);
            }
        }

        internal void AddMenaces()
        {
            foreach (var square in GetAttackedSquares())
            {
                AddMenace(square);
            }
        }

        internal abstract void RemoveExcessMenaces(Square newSquare);

        internal abstract void AddMissingMenaces(Square oldSquare);

        internal abstract void OpenLine(Square oldPiecePosition, Square newPiecePosition);

        internal abstract void BlockLine(Square blockSquare);

        internal void MoveTo(Square destination)
        {
            var oldSquare = Square;

            if (IsOnBoard)
            {
                RemoveExcessMenaces(destination);
                Square.OpenLines(destination);
                Square.Contained = null;
            }

            Square = destination;
            Square.Contained = this;
            Square.BlockLines();
            AddMissingMenaces(oldSquare);
        }

        internal void CaptureAt(Square destination)
        {
            var oldSquare = Square;
            var capturedPiece = destination.Contained;
            capturedPiece.RemoveMenaces();
            RemoveExcessMenaces(destination);
            Square.OpenLines(destination);

            if (IsLongRanged)
            {
                OpenLine(destination, null);
            }

            Square.Contained = null;
            Square = destination;
            Square.Contained = this;
            capturedPiece.Square = null;
            AddMissingMenaces(oldSquare);
        }

        internal void Remove()
        {
            RemoveMenaces();
            Square.OpenLines(null);
            Square.Contained = null;
            Square = null;
        }

        public bool IsOnBoard => Square != null;

        public ChessBoard Board => Square?.Board;

        public SquareLocation SquareLocation => Square?.Location;

        public int X => Square.X;

        public int Y => Square.Y;

        public King King
        {
            get
            {
                if (Name == PieceName.King)
                {
                    return (King)this;
                }

                var board = Board;

                if (board == null)
                {
                    return null;
                }

                var king = Color == PieceColor.White ? board.WhiteKing : board.BlackKing;
                return Board == board ? king : null;
            }
        }

        public abstract PieceName Name { get; }

        public abstract bool IsLongRanged { get; }

        internal List<ChessPiece> Menaces
        {
            get
            {
                var square = Square;

                if (square == null)
                {
                    return null;
                }

                var board = square.Board;
                var gamesCount = board.GamesCount;
                var modCount = board.ModCount;

                if (Square != square)
                {
                    throw new InvalidOperationException("Позиция на доске изменилась во время вычисления.");
                }

                var menaces = Color == PieceColor.White ? square.BlackMenaces : square.WhiteMenaces;

                return board.ModCount == modCount && board.GamesCount == gamesCount ? menaces :
                throw new InvalidOperationException("Позиция на доске изменилась во время вычисления.");
            }
        }
    }
}