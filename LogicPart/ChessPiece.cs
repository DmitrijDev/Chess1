
namespace Chess.LogicPart
{
    public abstract class ChessPiece
    {
        public ChessPieceColor Color { get; }

        public Square Position { get; private set; }

        public int FirstMoveMoment { get; internal set; }

        public ChessPiece(ChessPieceColor color) => Color = color;

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

        internal abstract IEnumerable<Square> GetAttackedSquares();

        public bool Attacks(Square square)
        {
            square.Board.Lock();
            var result = square.IsMenacedBy(this);
            square.Board.Unlock();
            return result;
        }

        public bool Attacks(ChessPiece otherPiece) => otherPiece.IsOnBoard && Attacks(otherPiece.Position);

        // Реализация для всех фигур, кроме пешки и короля.
        internal virtual IEnumerable<Square> GetAccessibleSquares()
        {
            var moveSquares = GetAttackedSquares().Where(square => square.IsEmpty || square.ContainedPiece.Color != Color);
            return FilterSafeForKingMoves(moveSquares);
        }

        // Реализация для всех фигур, кроме короля.
        private protected virtual IEnumerable<Square> FilterSafeForKingMoves(IEnumerable<Square> moveSquares)
        {
            var checkingPieces = FriendlyKing.GetCheckingPieces().ToArray();

            if (checkingPieces.Length > 1) // От двойного шаха не закроешься, и две фигуры одним ходом не возьмешь.
            {
                return Enumerable.Empty<Square>();
            }

            var result = checkingPieces.Length == 1 ? moveSquares.Where(square => ProtectsKingByMoveTo(square, checkingPieces[0])) : moveSquares;

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

            if (square == Board.PassedByPawnSquare && checkingPiece.Name == ChessPieceName.Pawn)
            {
                return Name == ChessPieceName.Pawn;
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

            if (checkingPiece.IsOnSameDiagonal(FriendlyKing))
            {
                return square.IsOnSameDiagonal(FriendlyKing) && square.IsOnSameDiagonal(checkingPiece) &&
                    (FriendlyKing.Vertical < square.Vertical ^ checkingPiece.Vertical < square.Vertical);
            }

            return false;
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

        public bool IsOnSameDiagonal(Square square) => IsOnBoard && Position.IsOnSameDiagonal(square);

        public bool IsOnSameDiagonal(ChessPiece otherPiece) => IsOnBoard && Position.IsOnSameDiagonal(otherPiece);

        // Переопределено для короля и пешки.        
        internal virtual IllegalMoveException CheckMoveLegacy(Move move)
        {
            if (!move.MoveSquare.IsMenacedBy(this))
            {
                return new IllegalMoveException("Невозможный ход.");
            }

            if (IsPinnedVertically())
            {
                if (move.MoveSquare.Vertical != Vertical)
                {
                    return new IllegalMoveException("Невозможный ход. Фигура связана.");
                }
            }
            else if (IsPinnedHorizontally())
            {
                if (move.MoveSquare.Horizontal != Horizontal)
                {
                    return new IllegalMoveException("Невозможный ход. Фигура связана.");
                }
            }
            else if (IsPinnedDiagonally())
            {
                if (!IsOnSameDiagonal(move.MoveSquare) || !FriendlyKing.IsOnSameDiagonal(move.MoveSquare))
                {
                    return new IllegalMoveException("Невозможный ход. Фигура связана.");
                }
            }

            var checkingPieces = FriendlyKing.GetCheckingPieces().ToArray();

            if (checkingPieces.Length > 1)
            {
                return new IllegalMoveException("Невозможный ход. Ваш король под шахом.");
            }

            if (checkingPieces.Length == 1 && !ProtectsKingByMoveTo(move.MoveSquare, checkingPieces[0]))
            {
                return new IllegalMoveException("Невозможный ход. Ваш король под шахом.");
            }

            return null;
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

        public ChessBoard Board => IsOnBoard ? Position.Board : null;

        public int Vertical => Position.Vertical;

        public int Horizontal => Position.Horizontal;

        public King FriendlyKing => !IsOnBoard ? null : Color == ChessPieceColor.White ? Board.WhiteKing : Board.BlackKing;

        public abstract ChessPieceName Name { get; }
    }
}