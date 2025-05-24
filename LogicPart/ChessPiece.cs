
namespace Chess.LogicPart
{
    public abstract class ChessPiece
    {
        public PieceColor Color { get; }

        public Square Square { get; internal set; }

        internal int FirstMoveDepth { get; set; }

        internal ChessPiece(PieceColor color) => Color = color;

        internal static ChessPiece GetNewPiece(PieceName name, PieceColor color) => name switch
        {
            PieceName.King => new King(color),
            PieceName.Queen => new Queen(color),
            PieceName.Rook => new Rook(color),
            PieceName.Knight => new Knight(color),
            PieceName.Bishop => new Bishop(color),
            _ => new Pawn(color)
        };

        internal ChessPiece Copy()
        {
            var copy = GetNewPiece(Name, Color);
            copy.FirstMoveDepth = FirstMoveDepth;
            return copy;
        }

        public abstract IEnumerable<Square> GetAttackedSquares();

        public bool Attacks(Square square)
        {
            if (square == null)
            {
                return false;
            }

            return square.GetMenaces(Color).Contains(this);
        }

        // Переопределено для короля и пешки.
        internal virtual bool CanMove()
        {
            var isKingSafe = GetKingSafetyFunc();
            return GetAttackedSquares().Any(sq => sq.Contained?.Color != Color && isKingSafe(sq));
        }

        private protected Func<Square, bool> GetKingSafetyFunc()
        {
            var king = Color == PieceColor.White ? Board.WhiteKing : Board.BlackKing;

            if (ShieldsByVertical(king.Square))
            {
                if (king.Square.IsMenacedBy(EnemyColor))
                {
                    return square => false;
                }

                var vertical = X;
                return square => square.X == vertical;
            }

            if (ShieldsByHorizontal(king.Square))
            {
                if (king.Square.IsMenacedBy(EnemyColor))
                {
                    return square => false;
                }

                var horizontal = Y;
                return square => square.Y == horizontal;
            }

            if (ShieldsByDiagonal(king.Square))
            {
                if (king.Square.IsMenacedBy(EnemyColor))
                {
                    return square => false;
                }

                var position = Square;
                var kingPosition = king.Square;
                return square => square.IsOnSameDiagonal(position) && square.IsOnSameDiagonal(kingPosition);
            }

            var checksCount = king.Square.GetMenacesCount(EnemyColor);

            if (checksCount > 1)
            {
                return square => false;
            }

            if (checksCount == 1)
            {
                return square => ProtectsKingWithMoveTo(square);
            }

            return square => true;
        }

        private protected bool ShieldsByVertical(Square square)
        {
            if (square.X != X)
            {
                return false;
            }

            if (square.Y > Y)
            {
                for (var i = Y + 1; i < square.Y; ++i)
                {
                    if (!Board[X, i].IsClear)
                    {
                        return false;
                    }
                }

                for (var i = Y - 1; i >= 0; --i)
                {
                    var piece = Board.GetPiece(X, i);

                    if (piece == null)
                    {
                        continue;
                    }

                    return piece.Color != Color && (piece.Name == PieceName.Queen || piece.Name == PieceName.Rook);
                }

                return false;
            }

            for (var i = Y - 1; i > square.Y; --i)
            {
                if (!Board[X, i].IsClear)
                {
                    return false;
                }
            }

            for (var i = Y + 1; i < 8; ++i)
            {
                var piece = Board.GetPiece(X, i);

                if (piece == null)
                {
                    continue;
                }

                return piece.Color != Color && (piece.Name == PieceName.Queen || piece.Name == PieceName.Rook);
            }

            return false;
        }

        private protected bool ShieldsByHorizontal(Square square)
        {
            if (square.Y != Y)
            {
                return false;
            }

            if (square.X > X)
            {
                for (var i = X + 1; i < square.X; ++i)
                {
                    if (!Board[i, Y].IsClear)
                    {
                        return false;
                    }
                }

                for (var i = X - 1; i >= 0; --i)
                {
                    var piece = Board.GetPiece(i, Y);

                    if (piece == null)
                    {
                        continue;
                    }

                    return piece.Color != Color && (piece.Name == PieceName.Queen || piece.Name == PieceName.Rook);
                }

                return false;
            }

            for (var i = X - 1; i > square.X; --i)
            {
                if (!Board[i, Y].IsClear)
                {
                    return false;
                }
            }

            for (var i = X + 1; i < 8; ++i)
            {
                var piece = Board.GetPiece(i, Y);

                if (piece == null)
                {
                    continue;
                }

                return piece.Color != Color && (piece.Name == PieceName.Queen || piece.Name == PieceName.Rook);
            }

            return false;
        }

        private protected bool ShieldsByDiagonal(Square square)
        {
            if (!Square.IsOnSameDiagonal(square))
            {
                return false;
            }

            if (square.X > X)
            {
                if (square.Y > Y)
                {
                    for (int i = X + 1, j = Y + 1; i < square.X; ++i, ++j)
                    {
                        if (!Board[i, j].IsClear)
                        {
                            return false;
                        }
                    }

                    for (int i = X - 1, j = Y - 1; i >= 0 && j >= 0; --i, --j)
                    {
                        var piece = Board.GetPiece(i, j);

                        if (piece == null)
                        {
                            continue;
                        }

                        return piece.Color != Color && (piece.Name == PieceName.Queen || piece.Name == PieceName.Bishop);
                    }

                    return false;
                }

                for (int i = X + 1, j = Y - 1; i < square.X; ++i, --j)
                {
                    if (!Board[i, j].IsClear)
                    {
                        return false;
                    }
                }

                for (int i = X - 1, j = Y + 1; i >= 0 && j < 8; --i, ++j)
                {
                    var piece = Board.GetPiece(i, j);

                    if (piece == null)
                    {
                        continue;
                    }

                    return piece.Color != Color && (piece.Name == PieceName.Queen || piece.Name == PieceName.Bishop);
                }

                return false;
            }

            if (square.Y > Y)
            {
                for (int i = X - 1, j = Y + 1; i > square.X; --i, ++j)
                {
                    if (!Board[i, j].IsClear)
                    {
                        return false;
                    }
                }

                for (int i = X + 1, j = Y - 1; i < 8 && j >= 0; ++i, --j)
                {
                    var piece = Board.GetPiece(i, j);

                    if (piece == null)
                    {
                        continue;
                    }

                    return piece.Color != Color && (piece.Name == PieceName.Queen || piece.Name == PieceName.Bishop);
                }

                return false;
            }

            for (int i = X - 1, j = Y - 1; i > square.X; --i, --j)
            {
                if (!Board[i, j].IsClear)
                {
                    return false;
                }
            }

            for (int i = X + 1, j = Y + 1; i < 8 && j < 8; ++i, ++j)
            {
                var piece = Board.GetPiece(i, j);

                if (piece == null)
                {
                    continue;
                }

                return piece.Color != Color && (piece.Name == PieceName.Queen || piece.Name == PieceName.Bishop);
            }

            return false;
        }

        private protected bool ProtectsKingWithMoveTo(Square moveSquare)
        {
            var board = moveSquare.Board;

            lock (board.Locker)
            {
                var king = Color == PieceColor.White ? board.WhiteKing : board.BlackKing;

                if (king == null || king.Square.GetMenacesCount(EnemyColor) != 1)
                {
                    return false;
                }

                var menace = king.Square.GetMenaces(EnemyColor).First();

                if (moveSquare == menace.Square)
                {
                    return true;
                }

                if (moveSquare.IsPawnPassed && menace.Name == PieceName.Pawn &&
                    Name == PieceName.Pawn && Attacks(moveSquare))
                {
                    return true;
                }

                if (!menace.IsLongRanged)
                {
                    return false;
                }

                if (menace.X == king.X)
                {
                    return moveSquare.X == king.X &&
                    (king.Y < moveSquare.Y ^ menace.Y < moveSquare.Y);
                }

                if (menace.Y == king.Y)
                {
                    return moveSquare.Y == king.Y &&
                    (king.X < moveSquare.X ^ menace.X < moveSquare.X);
                }

                return moveSquare.IsOnSameDiagonal(king) && moveSquare.IsOnSameDiagonal(menace) &&
                (king.X < moveSquare.X ^ menace.X < moveSquare.X);
            }
        }

        // Переопределено для короля и пешки.        
        internal virtual bool CanMoveTo(Square moveSquare, out Type exceptionType)
        {
            if (!Attacks(moveSquare))
            {
                exceptionType = typeof(IllegalMoveException);
                return false;
            }

            var king = Color == PieceColor.White ? Board.WhiteKing : Board.BlackKing;

            if (moveSquare.X != X && ShieldsByVertical(king.Square))
            {
                exceptionType = typeof(PiecePinnedException);
                return false;
            }

            if (moveSquare.Y != Y && ShieldsByHorizontal(king.Square))
            {
                exceptionType = typeof(PiecePinnedException);
                return false;
            }

            if (!(moveSquare.IsOnSameDiagonal(Square) && moveSquare.IsOnSameDiagonal(king)) && ShieldsByDiagonal(king.Square))
            {
                exceptionType = typeof(PiecePinnedException);
                return false;
            }

            if (king.Square.IsMenacedBy(EnemyColor) && !ProtectsKingWithMoveTo(moveSquare))
            {
                exceptionType = typeof(KingCheckedException);
                return false;
            }

            exceptionType = null;
            return true;
        }

        // Переопределено для короля и пешки.
        public virtual IEnumerable<Square> GetAccessibleSquares()
        {
            Func<Square, bool> isKingSafe;
            ulong modCount;
            ulong gamesCount;
            var board = Board;

            if (board == null)
            {
                yield break;
            }

            lock (board.Locker)
            {
                if (!IsOnBoard)
                {
                    yield break;
                }

                if (board.Status != BoardStatus.GameIncomplete || board.MoveTurn != Color)
                {
                    yield break;
                }

                isKingSafe = GetKingSafetyFunc();
                modCount = board.ModCount;
                gamesCount = board.GamesCount;
            }

            Square previous = null;

            foreach (var current in GetAttackedSquares().
            Where(sq => sq.Contained?.Color != Color && isKingSafe(sq)))
            {
                if (previous == null)
                {
                    previous = current;
                    continue;
                }

                if (board.ModCount != modCount || board.GamesCount != gamesCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return previous;
                previous = current;
            }

            if (board.ModCount != modCount || board.GamesCount != gamesCount)
            {
                throw new InvalidOperationException("Изменение позиции во время перечисления.");
            }

            if (previous != null)
            {
                yield return previous;
            }
        }

        internal void RemoveMenaces()
        {
            foreach (var square in GetAttackedSquares())
            {
                square.RemoveMenace(this);
            }
        }

        internal void AddMenaces()
        {
            foreach (var square in GetAttackedSquares())
            {
                square.AddMenace(this);
            }
        }

        internal abstract void RemoveUnactualMenaces(Square newSquare);

        internal abstract void AddMissingMenaces(Square oldSquare);

        internal void RemoveVerticalMenaces()
        {
            for (var i = Y + 1; i < 8; ++i)
            {
                var square = Board[X, i];
                square.RemoveMenace(this);

                if (!square.IsClear)
                {
                    break;
                }
            }

            for (var i = Y - 1; i >= 0; --i)
            {
                var square = Board[X, i];
                square.RemoveMenace(this);

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
                square.AddMenace(this);

                if (!square.IsClear)
                {
                    break;
                }
            }

            for (var i = Y - 1; i >= 0; --i)
            {
                var square = Board[X, i];
                square.AddMenace(this);

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
                square.RemoveMenace(this);

                if (!square.IsClear)
                {
                    break;
                }
            }

            for (var i = X - 1; i >= 0; --i)
            {
                var square = Board[i, Y];
                square.RemoveMenace(this);

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
                square.AddMenace(this);

                if (!square.IsClear)
                {
                    break;
                }
            }

            for (var i = X - 1; i >= 0; --i)
            {
                var square = Board[i, Y];
                square.AddMenace(this);

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
                square.RemoveMenace(this);

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
                square.AddMenace(this);

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
                square.RemoveMenace(this);

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
                square.AddMenace(this);

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
                square.RemoveMenace(this);

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
                square.AddMenace(this);

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
                square.RemoveMenace(this);

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
                square.AddMenace(this);

                if (!square.IsClear)
                {
                    break;
                }
            }
        }

        internal void OpenLine(Square blockerOldSquare, Square blockerNewSquare)
        {
            if (blockerOldSquare.X == X)
            {
                if (blockerOldSquare.Y > Y)
                {
                    if (blockerNewSquare?.X == X && blockerNewSquare.Y < blockerOldSquare.Y)
                    {
                        return;
                    }

                    for (var i = blockerOldSquare.Y + 1; i < 8; ++i)
                    {
                        var square = Board[X, i];
                        square.AddMenace(this);

                        if (!square.IsClear || square == blockerNewSquare)
                        {
                            return;
                        }
                    }

                    return;
                }

                if (blockerNewSquare?.X == X && blockerNewSquare.Y > blockerOldSquare.Y)
                {
                    return;
                }

                for (var i = blockerOldSquare.Y - 1; i >= 0; --i)
                {
                    var square = Board[X, i];
                    square.AddMenace(this);

                    if (!square.IsClear || square == blockerNewSquare)
                    {
                        return;
                    }
                }

                return;
            }

            if (blockerOldSquare.Y == Y)
            {
                if (blockerOldSquare.X > X)
                {
                    if (blockerNewSquare?.Y == Y && blockerNewSquare.X < blockerOldSquare.X)
                    {
                        return;
                    }

                    for (var i = blockerOldSquare.X + 1; i < 8; ++i)
                    {
                        var square = Board[i, Y];
                        square.AddMenace(this);

                        if (!square.IsClear || square == blockerNewSquare)
                        {
                            return;
                        }
                    }

                    return;
                }

                if (blockerNewSquare?.Y == Y && blockerNewSquare.X > blockerOldSquare.X)
                {
                    return;
                }

                for (var i = blockerOldSquare.X - 1; i >= 0; --i)
                {
                    var square = Board[i, Y];
                    square.AddMenace(this);

                    if (!square.IsClear || square == blockerNewSquare)
                    {
                        return;
                    }
                }

                return;
            }

            if (blockerOldSquare.X > X)
            {
                if (blockerOldSquare.Y > Y)
                {
                    if (blockerOldSquare.IsOnSameDiagonal(blockerNewSquare) &&
                        blockerNewSquare.X < blockerOldSquare.X &&
                        blockerNewSquare.Y < blockerOldSquare.Y)
                    {
                        return;
                    }

                    for (int i = blockerOldSquare.X + 1, j = blockerOldSquare.Y + 1; i < 8 && j < 8; ++i, ++j)
                    {
                        var square = Board[i, j];
                        square.AddMenace(this);

                        if (!square.IsClear || square == blockerNewSquare)
                        {
                            return;
                        }
                    }

                    return;
                }

                if (blockerOldSquare.IsOnSameDiagonal(blockerNewSquare) &&
                    blockerNewSquare.X < blockerOldSquare.X &&
                    blockerNewSquare.Y > blockerOldSquare.Y)
                {
                    return;
                }

                for (int i = blockerOldSquare.X + 1, j = blockerOldSquare.Y - 1; i < 8 && j >= 0; ++i, --j)
                {
                    var square = Board[i, j];
                    square.AddMenace(this);

                    if (!square.IsClear || square == blockerNewSquare)
                    {
                        return;
                    }
                }

                return;
            }

            if (blockerOldSquare.Y > Y)
            {
                if (blockerOldSquare.IsOnSameDiagonal(blockerNewSquare) &&
                    blockerNewSquare.X > blockerOldSquare.X &&
                    blockerNewSquare.Y < blockerOldSquare.Y)
                {
                    return;
                }

                for (int i = blockerOldSquare.X - 1, j = blockerOldSquare.Y + 1; i >= 0 && j < 8; --i, ++j)
                {
                    var square = Board[i, j];
                    square.AddMenace(this);

                    if (!square.IsClear || square == blockerNewSquare)
                    {
                        return;
                    }
                }

                return;
            }

            if (blockerOldSquare.IsOnSameDiagonal(blockerNewSquare) &&
                blockerNewSquare.X > blockerOldSquare.X &&
                blockerNewSquare.Y > blockerOldSquare.Y)
            {
                return;
            }

            for (int i = blockerOldSquare.X - 1, j = blockerOldSquare.Y - 1; i >= 0 && j >= 0; --i, --j)
            {
                var square = Board[i, j];
                square.AddMenace(this);

                if (!square.IsClear || square == blockerNewSquare)
                {
                    return;
                }
            }
        }

        internal void BlockLine(Square blockerSquare)
        {
            if (blockerSquare.X == X)
            {
                if (blockerSquare.Y > Y)
                {
                    for (var i = blockerSquare.Y + 1; i < 8; ++i)
                    {
                        var square = Board[X, i];

                        if (!square.RemoveMenace(this) || !square.IsClear)
                        {
                            return;
                        }
                    }

                    return;
                }

                for (var i = blockerSquare.Y - 1; i >= 0; --i)
                {
                    var square = Board[X, i];

                    if (!square.RemoveMenace(this) || !square.IsClear)
                    {
                        return;
                    }
                }

                return;
            }

            if (blockerSquare.Y == Y)
            {
                if (blockerSquare.X > X)
                {
                    for (var i = blockerSquare.X + 1; i < 8; ++i)
                    {
                        var square = Board[i, Y];

                        if (!square.RemoveMenace(this) || !square.IsClear)
                        {
                            return;
                        }
                    }

                    return;
                }

                for (var i = blockerSquare.X - 1; i >= 0; --i)
                {
                    var square = Board[i, Y];

                    if (!square.RemoveMenace(this) || !square.IsClear)
                    {
                        return;
                    }
                }

                return;
            }

            if (blockerSquare.X > X)
            {
                if (blockerSquare.Y > Y)
                {
                    for (int i = blockerSquare.X + 1, j = blockerSquare.Y + 1; i < 8 && j < 8; ++i, ++j)
                    {
                        var square = Board[i, j];

                        if (!square.RemoveMenace(this) || !square.IsClear)
                        {
                            return;
                        }
                    }

                    return;
                }

                for (int i = blockerSquare.X + 1, j = blockerSquare.Y - 1; i < 8 && j >= 0; ++i, --j)
                {
                    var square = Board[i, j];

                    if (!square.RemoveMenace(this) || !square.IsClear)
                    {
                        return;
                    }
                }

                return;
            }

            if (blockerSquare.Y > Y)
            {
                for (int i = blockerSquare.X - 1, j = blockerSquare.Y + 1; i >= 0 && j < 8; --i, ++j)
                {
                    var square = Board[i, j];

                    if (!square.RemoveMenace(this) || !square.IsClear)
                    {
                        return;
                    }
                }

                return;
            }

            for (int i = blockerSquare.X - 1, j = blockerSquare.Y - 1; i >= 0 && j >= 0; --i, --j)
            {
                var square = Board[i, j];

                if (!square.RemoveMenace(this) || !square.IsClear)
                {
                    return;
                }
            }
        }

        internal void MoveTo(Square destination)
        {
            var oldSquare = Square;

            if (IsOnBoard)
            {
                RemoveUnactualMenaces(destination);
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
            RemoveUnactualMenaces(destination);

            if (IsLongRanged)
            {
                OpenLine(destination, null);
            }

            destination.Contained.RemoveMenaces();
            destination.Contained.Square = null;
            Square.Contained = null;
            Square = destination;
            Square.Contained = this;
            oldSquare.OpenLines(Square);
            AddMissingMenaces(oldSquare);
        }

        internal void PromoteAt(Square promotionSquare, PieceName newPieceName)
        {
            var newPiece = GetNewPiece(newPieceName, Color);

            if (promotionSquare.IsClear)
            {
                Remove();
                newPiece.MoveTo(promotionSquare);
                return;
            }

            var capturedPiece = promotionSquare.Contained;
            capturedPiece.RemoveMenaces();
            capturedPiece.Square = null;
            newPiece.Square = promotionSquare;
            promotionSquare.Contained = newPiece;
            newPiece.AddMenaces();
            Remove();
        }

        internal void Remove()
        {
            RemoveMenaces();
            Square.OpenLines(null);
            Square.Contained = null;
            Square = null;
        }

        public abstract PieceName Name { get; }

        public abstract bool IsLongRanged { get; }

        public PieceColor EnemyColor => Color == PieceColor.White ? PieceColor.Black : PieceColor.White;

        public bool IsOnBoard => Square != null;

        public ChessBoard Board => Square?.Board;

        public SquareLocation Location
        {
            get
            {
                var square = Square;
                return square == null ? throw new InvalidOperationException("Фигура не на доске.") : square.Location;
            }
        }

        public int X
        {
            get
            {
                var square = Square;
                return square == null ? throw new InvalidOperationException("Фигура не на доске.") : square.X;
            }
        }

        public int Y
        {
            get
            {
                var square = Square;
                return square == null ? throw new InvalidOperationException("Фигура не на доске.") : square.Y;
            }
        }

        public bool HasMoved => FirstMoveDepth > 0;
    }
}