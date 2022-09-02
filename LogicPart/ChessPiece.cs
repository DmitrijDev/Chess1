
namespace Chess.LogicPart
{
    internal abstract class ChessPiece
    {
        private Square _position;
        private List<Square> _attackedSquares = new();

        public PieceColor Color { get; protected set; }

        public bool HasMoved { get; set; }

        public int LastAttacksRenewMoment { get; private set; } = -1;

        public List<Square> GetAttackedSquares()
        {
            if (LastAttacksRenewMoment < Board.MovesCount)
            {
                _attackedSquares = GetNewAttackedSquares();

                if (FriendlySide == Board.MovingSide.Enemy)
                {
                    foreach (var square in _attackedSquares)
                    {
                        square.AddMenace(this);
                    }
                }

                LastAttacksRenewMoment = Board.MovesCount;
            }

            return _attackedSquares;
        }

        protected abstract List<Square> GetNewAttackedSquares();

        // Реализация для всех фигур, кроме пешки и короля.
        public virtual List<Square> GetLegalMoveSquares()
        {
            var result = GetAttackedSquares().Where(square => square.IsEmpty || square.ContainedPiece.Color != Color).ToList();

            if (result.Count == 0)
            {
                return result;
            }

            return FilterSafeForKingMoves(result);
        }

        // Реализация для всех фигур, кроме короля.
        protected virtual List<Square> FilterSafeForKingMoves(List<Square> list)
        {
            var result = new List<Square>(list);

            if (FriendlyKing.IsMenaced())
            {
                if (FriendlyKing.GetMenaces().Count > 1) // От двойного шаха не закроешься, и две фигуры одним ходом не возьмешь.
                {
                    result.Clear();
                    return result;
                }

                var menacingPiece = FriendlyKing.GetMenaces()[0];

                if (menacingPiece.Vertical == FriendlyKing.Vertical)
                {
                    result = result.Where(square => square.Vertical == FriendlyKing.Vertical &&
                    ((FriendlyKing.Horizontal < square.Horizontal && menacingPiece.Horizontal > square.Horizontal) ||
                    (menacingPiece.Horizontal < square.Horizontal && FriendlyKing.Horizontal > square.Horizontal))).ToList();
                }

                if (menacingPiece.Horizontal == FriendlyKing.Horizontal)
                {
                    result = result.Where(square => square.Horizontal == FriendlyKing.Horizontal &&
                    ((FriendlyKing.Vertical < square.Vertical && menacingPiece.Vertical > square.Vertical) ||
                    (menacingPiece.Vertical < square.Vertical && FriendlyKing.Vertical > square.Vertical))).ToList();
                }

                if (menacingPiece.IsOnSameDiagonal(FriendlyKing) && menacingPiece is not Pawn)
                {
                    result = result.Where(square => square.IsOnSameDiagonal(FriendlyKing) && square.IsOnSameDiagonal(menacingPiece) &&
                    ((FriendlyKing.Vertical < square.Vertical && menacingPiece.Vertical > square.Vertical) ||
                    (menacingPiece.Vertical < square.Vertical && FriendlyKing.Vertical > square.Vertical))).ToList();
                }

                if (menacingPiece is Pawn)
                {
                    result.Clear();
                }

                if (list.Contains(menacingPiece.Position))
                {
                    result.Add(menacingPiece.Position);
                }
            }

            if (result.Count == 0)
            {
                return result;
            }

            if (IsPinnedVertically())
            {
                return result.Where(square => square.Vertical == Vertical).ToList();
            }

            if (IsPinnedHorizontally())
            {
                return result.Where(square => square.Horizontal == Horizontal).ToList();
            }

            if (!IsOnSameDiagonal(FriendlyKing))
            {
                return result;
            }

            foreach (var enemyPiece in FriendlySide.Enemy.Material)
            {
                if (IsPinnedByDiagonal(enemyPiece))
                {
                    return result.Where(square => square.IsOnSameDiagonal(FriendlyKing) && square.IsOnSameDiagonal(enemyPiece)).ToList();
                }
            }

            return result;
        }

        private bool IsPinnedVertically()
        {
            if (FriendlyKing.Vertical != Vertical)
            {
                return false;
            }

            ChessPiece nearestPiece = null;

            for (var i = FriendlyKing.Horizontal < Horizontal ? Horizontal + 1 : Horizontal - 1; i >= 0 && i < 8; i = FriendlyKing.Horizontal < Horizontal ? i + 1 : i - 1)
            {
                if (!Board[Vertical, i].IsEmpty)
                {
                    nearestPiece = Board[Vertical, i].ContainedPiece;
                    break;
                }
            }

            if (nearestPiece == null || (nearestPiece is not Queen && nearestPiece is not Rook) || nearestPiece.Color == Color)
            {
                return false;
            }

            for (var i = FriendlyKing.Horizontal < Horizontal ? Horizontal - 1 : Horizontal + 1; i != FriendlyKing.Horizontal;
                i = FriendlyKing.Horizontal < Horizontal ? i - 1 : i + 1)
            {
                if (!Board[Vertical, i].IsEmpty)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsPinnedHorizontally()
        {
            if (FriendlyKing.Horizontal != Horizontal)
            {
                return false;
            }

            ChessPiece nearestPiece = null;

            for (var i = FriendlyKing.Vertical < Vertical ? Vertical + 1 : Vertical - 1; i >= 0 && i < 8; i = FriendlyKing.Vertical < Vertical ? i + 1 : i - 1)
            {
                if (!Board[i, Horizontal].IsEmpty)
                {
                    nearestPiece = Board[i, Horizontal].ContainedPiece;
                    break;
                }
            }

            if (nearestPiece == null || (nearestPiece is not Queen && nearestPiece is not Rook) || nearestPiece.Color == Color)
            {
                return false;
            }

            for (var i = FriendlyKing.Vertical < Vertical ? Vertical - 1 : Vertical + 1; i != FriendlyKing.Vertical;
                i = FriendlyKing.Vertical < Vertical ? i - 1 : i + 1)
            {
                if (!Board[i, Horizontal].IsEmpty)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsPinnedByDiagonal(ChessPiece enemyPiece)
        {
            if (enemyPiece is not Queen && enemyPiece is not Bishop)
            {
                return false;
            }

            if (!(enemyPiece.IsOnSameDiagonal(FriendlyKing) && IsOnSameDiagonal(enemyPiece)))
            {
                return false;
            }

            if (!(FriendlyKing.Vertical < Vertical && enemyPiece.Vertical > Vertical) && !(enemyPiece.Vertical < Vertical && FriendlyKing.Vertical > Vertical))
            {
                return false;
            }

            var leftPiece = FriendlyKing.Vertical < Vertical ? FriendlyKing : enemyPiece;
            var rightPiece = FriendlyKing.Vertical > Vertical ? FriendlyKing : enemyPiece;

            for (int i = leftPiece.Vertical + 1, j = rightPiece.Horizontal > leftPiece.Horizontal ? leftPiece.Horizontal + 1 : leftPiece.Horizontal - 1;
                i < rightPiece.Vertical; ++i, j = rightPiece.Horizontal > leftPiece.Horizontal ? j + 1 : j - 1)
            {
                if (!Board[i, j].IsEmpty && i != Vertical)
                {
                    return false;
                }
            }

            return true;
        }

        protected bool IsOnSameDiagonal(ChessPiece otherPiece) => _position != null && otherPiece != null && _position.IsOnSameDiagonal(otherPiece._position);

        public bool IsMenaced() => FriendlySide == Board.MovingSide && _position.IsMenaced();

        public List<ChessPiece> GetMenaces() => FriendlySide == Board.MovingSide ? _position.GetMenaces() : new List<ChessPiece>();

        public abstract ChessPiece Copy();

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

        public King FriendlyKing => FriendlySide.King;

        public abstract string EnglishName { get; }

        public abstract string RussianName { get; }

        public abstract string ShortEnglishName { get; }

        public abstract string ShortRussianName { get; }

        public abstract int NumeralIndex { get; }
    }
}