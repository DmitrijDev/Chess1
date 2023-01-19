
using Chess.StringsUsing;

namespace Chess.LogicPart
{
    internal abstract class ChessPiece
    {
        private Square _position;

        public PieceColor Color { get; protected set; }

        public int FirstMoveMoment { get; set; }

        public abstract IEnumerable<Square> GetAttackedSquares();

        public bool Attacks(Square square) => square != null && GetAttackedSquares().Contains(square);

        public bool Attacks(ChessPiece otherPiece) => otherPiece != null && Attacks(otherPiece._position);

        // Реализация для всех фигур, кроме пешки и короля.
        protected virtual IEnumerable<Square> GetLegalMoveSquares(bool savesUnsafeForKingSquares, out IEnumerable<Square> unsafeForKingSquares)
        {
            if (Board.Status != GameStatus.GameCanContinue || FriendlySide != Board.MovingSide)
            {
                unsafeForKingSquares = savesUnsafeForKingSquares ? Enumerable.Empty<Square>() : null;
                return Enumerable.Empty<Square>();
            }

            var moveSquares = GetAttackedSquares().Where(square => square.IsEmpty || square.ContainedPiece.Color != Color);
            return FilterSafeForKingMoves(moveSquares, savesUnsafeForKingSquares, out unsafeForKingSquares);
        }

        public IEnumerable<Square> GetLegalMoveSquares() => GetLegalMoveSquares(false, out var unsafeForKingSquares);

        // Реализация для всех фигур, кроме короля.
        protected virtual IEnumerable<Square> FilterSafeForKingMoves(IEnumerable<Square> moveSquares, bool savesRemovedSquares,
            out IEnumerable<Square> removedSquares)
        {
            if (FriendlyKing.GetMenaces().Count > 1) // От двойного шаха не закроешься, и две фигуры одним ходом не возьмешь.
            {
                removedSquares = savesRemovedSquares ? moveSquares : null;
                return Enumerable.Empty<Square>();
            }

            var result = FriendlyKing.IsMenaced() ? moveSquares.Where(ProtectsKingByMoveTo) : moveSquares;

            if (IsPinnedVertically())
            {
                result = result.Where(square => square.Vertical == Vertical);
            }

            if (IsPinnedHorizontally())
            {
                result = result.Where(square => square.Horizontal == Horizontal);
            }

            if (IsPinnedByDiagonal())
            {
                result = result.Where(square => square.IsOnSameDiagonal(_position) && square.IsOnSameDiagonal(FriendlyKing));
            }

            removedSquares = savesRemovedSquares ? moveSquares.Except(result) : null;
            return result;
        }

        private bool ProtectsKingByMoveTo(Square square)
        {
            var menacingPiece = FriendlyKing.GetMenaces()[0];

            if (square == menacingPiece.Position)
            {
                return true;
            }

            if (square == Board.PassedByPawnSquare && menacingPiece is Pawn && this is Pawn)
            {
                return true;
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

        private bool IsPinnedVertically()
        {
            if (FriendlyKing.Vertical != Vertical)
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
                    return Board[Vertical, i].ContainedPiece.Color != Color && (Board[Vertical, i].ContainedPiece is Queen || Board[Vertical, i].ContainedPiece is Rook);
                }
            }

            return false;
        }

        private bool IsPinnedHorizontally()
        {
            if (FriendlyKing.Horizontal != Horizontal)
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
                    return Board[i, Horizontal].ContainedPiece.Color != Color && (Board[i, Horizontal].ContainedPiece is Queen || Board[i, Horizontal].ContainedPiece is Rook);
                }
            }

            return false;
        }

        private bool IsPinnedByDiagonal()
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
                    return Board[i, j].ContainedPiece.Color != Color && (Board[i, j].ContainedPiece is Queen || Board[i, j].ContainedPiece is Bishop);
                }
            }

            return false;
        }

        // Реализация для всех фигур, кроме пешки.
        public virtual IEnumerable<Move> GetLegalMoves() => GetLegalMoveSquares().Select(square => new Move(this, square));

        public bool IsOnSameDiagonal(ChessPiece otherPiece) => _position != null && _position.IsOnSameDiagonal(otherPiece);

        public bool IsMenaced() => FriendlySide == Board.MovingSide && _position.IsMenaced();

        public List<ChessPiece> GetMenaces() => FriendlySide == Board.MovingSide ? _position.GetMenaces() : new List<ChessPiece>();

        public bool CanMove() => FriendlySide == Board.MovingSide && GetLegalMoveSquares().Any();

        // Переопределено для короля.
        public virtual string GetIllegalMoveMessage(Square square)
        {
            GetLegalMoveSquares(true, out var unsafeForKingSquares);

            if (unsafeForKingSquares.Contains(square))
            {
                if (IsPinnedVertically() && square.Vertical != Vertical)
                {
                    return "Невозможный ход. Фигура связана.";
                }

                if (IsPinnedHorizontally() && square.Horizontal != Horizontal)
                {
                    return "Невозможный ход. Фигура связана.";
                }

                if (IsPinnedByDiagonal() && !(square.IsOnSameDiagonal(_position) && square.IsOnSameDiagonal(FriendlyKing)))
                {
                    return "Невозможный ход. Фигура связана.";
                }

                return "Невозможный ход. Ваш король под шахом.";
            }

            return "Невозможный ход.";
        }

        public abstract ChessPiece Copy();

        public static ChessPiece GetNewPiece(string name, PieceColor color)
        {
            if (name == null)
            {
                throw new ArgumentException("Не указано имя фигуры");
            }

            var pieces = new ChessPiece[6] { new King(color), new Queen(color), new Rook(color), new Knight(color), new Bishop(color), new Pawn(color) };
            var trimmedName = SharedItems.RemoveSpacesAndToLower(name);

            foreach (var piece in pieces)
            {
                if (SharedItems.RemoveSpacesAndToLower(piece.EnglishName) == trimmedName || SharedItems.RemoveSpacesAndToLower(piece.RussianName) == trimmedName)
                {
                    return piece;
                }
            }

            throw new ArgumentException("Фигуры с указанным полным именем не существует");
        }

        public Square Position
        {
            get => _position;

            set
            {
                if (_position != null && _position.ContainedPiece == this)
                {
                    _position.ContainedPiece = null;
                }

                _position = value;

                if (_position != null && _position.ContainedPiece != this)
                {
                    _position.ContainedPiece = this;
                }
            }
        }

        public ChessBoard Board => _position.Board;

        public int Vertical => _position.Vertical;

        public int Horizontal => _position.Horizontal;

        public GameSide FriendlySide => Color == PieceColor.White ? Board.White : Board.Black;

        public GameSide Enemy => FriendlySide.Enemy;

        public King FriendlyKing => FriendlySide.King;

        public abstract string EnglishName { get; }

        public abstract string RussianName { get; }

        public abstract string ShortEnglishName { get; }

        public abstract string ShortRussianName { get; }

        public abstract int NumeralIndex { get; }
    }
}