
namespace Chess.LogicPart
{
    public abstract class ChessPiece
    {
        public ChessPieceColor Color { get; }

        public Square Position { get; private set; }

        public int FirstMoveMoment { get; internal set; }

        internal ChessPiece(ChessPieceColor color) => Color = color;

        internal void PutTo(Square newPosition)
        {
            var oldPosition = Position;
            Position = newPosition;

            if (oldPosition != null && oldPosition.ContainedPiece == this)
            {
                oldPosition.Clear();
            }

            if (IsOnBoard && Position.ContainedPiece != this)
            {
                Position.Put(this);
            }
        }

        internal void Remove()
        {
            if (IsOnBoard)
            {
                PutTo(null);
            }
        }

        public abstract IEnumerable<Square> GetAttackedSquares();

        public bool Attacks(Square square)
        {
            var flag = Color == ChessPieceColor.White ? square.Board.WhiteMenacesActual :
                square.Board.BlackMenacesActual;

            if (flag)
            {
                var menaces = Color == ChessPieceColor.White ? square.WhiteMenaces : square.BlackMenaces;
                return menaces != null && menaces.Contains(this);
            }

            return GetAttackedSquares().Contains(square);
        }

        public bool Attacks(ChessPiece otherPiece)
        {
            var position = otherPiece.Position;
            return position != null && Attacks(position);
        }

        // Реализация для всех фигур, кроме пешки и короля.
        internal virtual IEnumerable<Square> GetAccessibleSquares()
        {
            var squares = GetAttackedSquares().Where(square => square.IsEmpty || square.ContainedPiece.Color != Color);
            return FilterSafeForKingMoves(squares);
        }

        // Реализация для всех фигур, кроме короля.
        private protected virtual IEnumerable<Square> FilterSafeForKingMoves(IEnumerable<Square> squares)
        {
            var checkingPieces = FriendlyKing.GetCheckingPieces().ToArray();

            if (checkingPieces.Length > 1) // От двойного шаха не закроешься, и две фигуры одним ходом не возьмешь.
            {
                return Enumerable.Empty<Square>();
            }

            var result = checkingPieces.Length == 1 ? squares.Where(square => ProtectsKingByMoveTo(square, checkingPieces[0])) : squares;

            if (IsPinnedVertically())
            {
                result = result.Where(square => square.Vertical == Vertical);
            }
            else if (IsPinnedHorizontally())
            {
                result = result.Where(square => square.Horizontal == Horizontal);
            }
            else if (IsPinnedDiagonally())
            {
                result = result.Where(square => square.IsOnSameDiagonal(Position) && square.IsOnSameDiagonal(FriendlyKing));
            }

            return result;
        }

        private protected bool ProtectsKingByMoveTo(Square square, ChessPiece checkingPiece)
        {
            if (square == checkingPiece.Position)
            {
                return true;
            }

            if (checkingPiece.Name == ChessPieceName.Pawn)
            {
                return square == Board.PassedByPawnSquare && Name == ChessPieceName.Pawn;
            }

            if (checkingPiece.Name == ChessPieceName.Knight)
            {
                return false;
            }

            if (checkingPiece.Vertical == FriendlyKing.Vertical)
            {
                return square.Vertical == FriendlyKing.Vertical &&
                (FriendlyKing.Horizontal < square.Horizontal ^ checkingPiece.Horizontal < square.Horizontal);
            }

            if (checkingPiece.Horizontal == FriendlyKing.Horizontal)
            {
                return square.Horizontal == FriendlyKing.Horizontal &&
                (FriendlyKing.Vertical < square.Vertical ^ checkingPiece.Vertical < square.Vertical);
            }

            return square.IsOnSameDiagonal(FriendlyKing) && square.IsOnSameDiagonal(checkingPiece) &&
            (FriendlyKing.Vertical < square.Vertical ^ checkingPiece.Vertical < square.Vertical);
        }

        private protected bool IsPinnedVertically()
        {
            if (FriendlyKing.Vertical != Vertical)
            {
                return false;
            }

            for (var i = FriendlyKing.Horizontal > Horizontal ? Horizontal + 1 : Horizontal - 1;
                i != FriendlyKing.Horizontal; i = i > Horizontal ? i + 1 : i - 1)
            {
                if (!Board[Vertical, i].IsEmpty)
                {
                    return false;
                }
            }

            for (var i = FriendlyKing.Horizontal > Horizontal ? Horizontal - 1 : Horizontal + 1;
                i >= 0 && i < 8; i = i > Horizontal ? i + 1 : i - 1)
            {
                if (!Board[Vertical, i].IsEmpty)
                {
                    var nearestPiece = Board[Vertical, i].ContainedPiece;

                    return nearestPiece.Color != Color && (nearestPiece.Name == ChessPieceName.Queen ||
                        nearestPiece.Name == ChessPieceName.Rook);
                }
            }

            return false;
        }

        private protected bool IsPinnedHorizontally()
        {
            if (FriendlyKing.Horizontal != Horizontal)
            {
                return false;
            }

            for (var i = FriendlyKing.Vertical > Vertical ? Vertical + 1 : Vertical - 1;
                i != FriendlyKing.Vertical; i = i > Vertical ? i + 1 : i - 1)
            {
                if (!Board[i, Horizontal].IsEmpty)
                {
                    return false;
                }
            }

            for (var i = FriendlyKing.Vertical > Vertical ? Vertical - 1 : Vertical + 1;
                i >= 0 && i < 8; i = i > Vertical ? i + 1 : i - 1)
            {
                if (!Board[i, Horizontal].IsEmpty)
                {
                    var nearestPiece = Board[i, Horizontal].ContainedPiece;

                    return nearestPiece.Color != Color && (nearestPiece.Name == ChessPieceName.Queen ||
                        nearestPiece.Name == ChessPieceName.Rook);
                }
            }

            return false;
        }

        private protected bool IsPinnedDiagonally()
        {
            if (!IsOnSameDiagonal(FriendlyKing))
            {
                return false;
            }

            for (int i = FriendlyKing.Vertical > Vertical ? Vertical + 1 : Vertical - 1, j = FriendlyKing.Horizontal > Horizontal ? Horizontal + 1 : Horizontal - 1;
                i != FriendlyKing.Vertical; i = i > Vertical ? i + 1 : i - 1, j = j > Horizontal ? j + 1 : j - 1)
            {
                if (!Board[i, j].IsEmpty)
                {
                    return false;
                }
            }

            for (int i = FriendlyKing.Vertical > Vertical ? Vertical - 1 : Vertical + 1, j = FriendlyKing.Horizontal > Horizontal ? Horizontal - 1 : Horizontal + 1;
                i >= 0 && j >= 0 && i < 8 && j < 8; i = i > Vertical ? i + 1 : i - 1, j = j > Horizontal ? j + 1 : j - 1)
            {
                if (!Board[i, j].IsEmpty)
                {
                    var nearestPiece = Board[i, j].ContainedPiece;

                    return nearestPiece.Color != Color && (nearestPiece.Name == ChessPieceName.Queen ||
                        nearestPiece.Name == ChessPieceName.Bishop);
                }
            }

            return false;
        }

        // Реализация для всех фигур, кроме короля.
        internal virtual bool CanMove() => GetAccessibleSquares().Any();

        public bool IsOnSameDiagonal(Square square)
        {
            var position = Position;
            return position != null && position.IsOnSameDiagonal(square);
        }

        public bool IsOnSameDiagonal(ChessPiece other)
        {
            var otherPosition = other.Position;
            return otherPosition != null && IsOnSameDiagonal(otherPosition);
        }

        // Переопределено для короля и пешки.        
        internal virtual void CheckLegacy(Move move)
        {
            if (!Attacks(move.MoveSquare))
            {
                throw new IllegalMoveException("Невозможный ход.");
            }

            if (IsPinnedVertically())
            {
                if (move.MoveSquare.Vertical != Vertical)
                {
                    throw new IllegalMoveException("Невозможный ход. Фигура связана.");
                }
            }
            else if (IsPinnedHorizontally())
            {
                if (move.MoveSquare.Horizontal != Horizontal)
                {
                    throw new IllegalMoveException("Невозможный ход. Фигура связана.");
                }
            }
            else if (IsPinnedDiagonally())
            {
                if (!IsOnSameDiagonal(move.MoveSquare) || !FriendlyKing.IsOnSameDiagonal(move.MoveSquare))
                {
                    throw new IllegalMoveException("Невозможный ход. Фигура связана.");
                }
            }

            var checkingPieces = FriendlyKing.GetCheckingPieces().ToArray();

            if (checkingPieces.Length > 1)
            {
                throw new IllegalMoveException("Невозможный ход. Ваш король под шахом.");
            }

            if (checkingPieces.Length == 1 && !ProtectsKingByMoveTo(move.MoveSquare, checkingPieces[0]))
            {
                throw new IllegalMoveException("Невозможный ход. Ваш король под шахом.");
            }
        }

        public static ChessPiece GetNewPiece(ChessPieceName name, ChessPieceColor color) => name switch
        {
            ChessPieceName.King => new King(color),
            ChessPieceName.Queen => new Queen(color),
            ChessPieceName.Rook => new Rook(color),
            ChessPieceName.Knight => new Knight(color),
            ChessPieceName.Bishop => new Bishop(color),
            _ => new Pawn(color)
        };

        public bool IsOnBoard => Position != null;

        public ChessBoard Board
        {
            get
            {
                var position = Position;
                return position != null ? position.Board : null;
            }
        }

        public int Vertical => Position.Vertical;

        public int Horizontal => Position.Horizontal;

        public King FriendlyKing
        {
            get
            {
                var position = Position;
                return position == null ? null : Color == ChessPieceColor.White ? position.Board.WhiteKing : position.Board.BlackKing;
            }
        }

        public abstract ChessPieceName Name { get; }

        public abstract bool IsLongRanged { get; }
    }
}