
namespace Chess.LogicPart
{
    public abstract class ChessPiece
    {
        public ChessPieceColor Color { get; protected set; }

        public Square Position { get; private set; }

        public int FirstMoveMoment { get; internal set; }

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

        public bool Attacks(Square square) => GetAttackedSquares().Contains(square);

        public bool Attacks(ChessPiece otherPiece) => otherPiece.IsOnBoard && Attacks(otherPiece.Position);

        // Реализация для всех фигур, кроме пешки и короля.
        public virtual IEnumerable<Square> GetAccessibleSquares()
        {
            if (!IsOnBoard || Board.Status != GameStatus.GameIsNotOver || Color != Board.MovingSideColor)
            {
                return Enumerable.Empty<Square>();
            }

            var moveSquares = GetAttackedSquares().Where(square => square.IsEmpty || square.ContainedPiece.Color != Color);
            return FilterSafeForKingMoves(moveSquares);
        }

        // Реализация для всех фигур, кроме короля.
        protected virtual IEnumerable<Square> FilterSafeForKingMoves(IEnumerable<Square> moveSquares)
        {
            if (FriendlyKing.GetMenaces().Count() > 1) // От двойного шаха не закроешься, и две фигуры одним ходом не возьмешь.
            {
                return Enumerable.Empty<Square>();
            }

            var result = FriendlyKing.IsMenaced() ? moveSquares.Where(ProtectsKingByMoveTo) : moveSquares;

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

        private bool ProtectsKingByMoveTo(Square square)
        {
            var menacingPiece = FriendlyKing.GetMenaces().Single();

            if (square == menacingPiece.Position)
            {
                return true;
            }

            if (square == Board.PassedByPawnSquare && menacingPiece.Name == ChessPieceName.Pawn)
            {
                return Name == ChessPieceName.Pawn;
            }

            if (menacingPiece.Vertical == FriendlyKing.Vertical)
            {
                return square.Vertical == FriendlyKing.Vertical && (FriendlyKing.Horizontal < square.Horizontal ^ menacingPiece.Horizontal < square.Horizontal);
            }

            if (menacingPiece.Horizontal == FriendlyKing.Horizontal)
            {
                return square.Horizontal == FriendlyKing.Horizontal && (FriendlyKing.Vertical < square.Vertical ^ menacingPiece.Vertical < square.Vertical);
            }

            if (menacingPiece.IsOnSameDiagonal(FriendlyKing))
            {
                return square.IsOnSameDiagonal(FriendlyKing) && square.IsOnSameDiagonal(menacingPiece) &&
                    (FriendlyKing.Vertical < square.Vertical ^ menacingPiece.Vertical < square.Vertical);
            }

            return false;
        }

        public bool IsPinnedVertically()
        {
            if (!IsOnBoard || Board.Status == GameStatus.IllegalPosition)
            {
                throw new InvalidOperationException();
            }

            if (FriendlyKing.Vertical != Vertical || Name == ChessPieceName.King)
            {
                return false;
            }

            for (var i = FriendlyKing.Horizontal > Horizontal ? Horizontal + 1 : Horizontal - 1; i != FriendlyKing.Horizontal; i = i > Horizontal ? i + 1 : i - 1)
            {
                if (!Board[Vertical, i].IsEmpty)
                {
                    return false;
                }
            }

            for (var i = FriendlyKing.Horizontal > Horizontal ? Horizontal - 1 : Horizontal + 1; i >= 0 && i < 8; i = i > Horizontal ? i + 1 : i - 1)
            {
                if (!Board[Vertical, i].IsEmpty)
                {
                    var nearestPiece = Board[Vertical, i].ContainedPiece;
                    return nearestPiece.Color != Color && (nearestPiece.Name == ChessPieceName.Queen || nearestPiece.Name == ChessPieceName.Rook);
                }
            }

            return false;
        }

        public bool IsPinnedHorizontally()
        {
            if (!IsOnBoard || Board.Status == GameStatus.IllegalPosition)
            {
                throw new InvalidOperationException();
            }

            if (FriendlyKing.Horizontal != Horizontal || Name == ChessPieceName.King)
            {
                return false;
            }

            for (var i = FriendlyKing.Vertical > Vertical ? Vertical + 1 : Vertical - 1; i != FriendlyKing.Vertical; i = i > Vertical ? i + 1 : i - 1)
            {
                if (!Board[i, Horizontal].IsEmpty)
                {
                    return false;
                }
            }

            for (var i = FriendlyKing.Vertical > Vertical ? Vertical - 1 : Vertical + 1; i >= 0 && i < 8; i = i > Vertical ? i + 1 : i - 1)
            {
                if (!Board[i, Horizontal].IsEmpty)
                {
                    var nearestPiece = Board[i, Horizontal].ContainedPiece;
                    return nearestPiece.Color != Color && (nearestPiece.Name == ChessPieceName.Queen || nearestPiece.Name == ChessPieceName.Rook);
                }
            }

            return false;
        }

        public bool IsPinnedDiagonally()
        {
            if (!IsOnBoard || Board.Status == GameStatus.IllegalPosition)
            {
                throw new InvalidOperationException();
            }

            if (!IsOnSameDiagonal(FriendlyKing) || Name == ChessPieceName.King)
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
                    return nearestPiece.Color != Color && (nearestPiece.Name == ChessPieceName.Queen || nearestPiece.Name == ChessPieceName.Bishop);
                }
            }

            return false;
        }

        public bool IsOnSameDiagonal(ChessPiece otherPiece) => Position.IsOnSameDiagonal(otherPiece);

        public bool IsMenaced() => Color == Board.MovingSideColor && Position.IsMenaced();

        public IEnumerable<ChessPiece> GetMenaces() => Color == Board.MovingSideColor ? Position.GetMenaces() : Enumerable.Empty<ChessPiece>();

        public bool CanMove() => GetAccessibleSquares().Any();

        // Переопределено для короля и пешки.
        public virtual string GetIllegalMoveMessage(Square square)
        {
            if (Board != square.Board)
            {
                throw new InvalidOperationException("Указано поле на другой доске.");
            }

            if (!Attacks(square))
            {
                return "Невозможный ход.";
            }

            if (!square.IsEmpty && square.ContainedPiece.Color == Color)
            {
                return "Невозможно пойти на поле, занятое своей фигурой.";
            }

            if (IsPinnedVertically() && square.Vertical != Vertical)
            {
                return "Невозможный ход. Фигура связана.";
            }

            if (IsPinnedHorizontally() && square.Horizontal != Horizontal)
            {
                return "Невозможный ход. Фигура связана.";
            }

            if (IsPinnedDiagonally() && !(square.IsOnSameDiagonal(Position) && square.IsOnSameDiagonal(FriendlyKing)))
            {
                return "Невозможный ход. Фигура связана.";
            }

            if (FriendlyKing.GetMenaces().Count() > 1)
            {
                return "Невозможный ход. Ваш король под шахом.";
            }

            if (FriendlyKing.IsMenaced() && !ProtectsKingByMoveTo(square))
            {
                return "Невозможный ход. Ваш король под шахом.";
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

        public King EnemyKing => !IsOnBoard ? null : Color == ChessPieceColor.White ? Board.BlackKing : Board.WhiteKing;

        public abstract ChessPieceName Name { get; }
    }
}