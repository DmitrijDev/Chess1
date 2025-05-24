
namespace Chess.LogicPart
{
    public sealed class GamePosition
    {
        private readonly PieceName?[,] _pieceNames = new PieceName?[8, 8];
        private readonly PieceColor?[,] _pieceColors = new PieceColor?[8, 8];

        public PieceColor MoveTurn { get; private set; }

        public GamePosition(ChessBoard board)
        {
            if (board == null)
            {
                throw new ArgumentNullException();
            }

            lock (board.Locker)
            {
                foreach (var piece in board.GetMaterial())
                {
                    _pieceNames[piece.X, piece.Y] = piece.Name;
                    _pieceColors[piece.X, piece.Y] = piece.Color;
                }

                MoveTurn = board.MoveTurn;
            }
        }

        public GamePosition(IEnumerable<PieceName> whiteMaterial, IEnumerable<string> whitePositions,
        IEnumerable<PieceName> blackMaterial, IEnumerable<string> blackPositions, PieceColor moveTurn)
        {
            if (whiteMaterial == null || whitePositions == null || blackMaterial == null || blackPositions == null)
            {
                throw new ArgumentNullException();
            }

            var material = whiteMaterial.ToArray();
            var positions = whitePositions.ToArray();

            if (material.Length != positions.Length)
            {
                throw new ArgumentException("Для белых должно быть указано равное число фигур и полей.");
            }

            for (var i = 0; i < material.Length; ++i)
            {
                var location = new SquareLocation(positions[i]);

                if (HasPieceAt(location))
                {
                    throw new ArgumentException("Для двух фигур указана одна и та же позиция.");
                }

                _pieceNames[location.X, location.Y] = material[i];
                _pieceColors[location.X, location.Y] = PieceColor.White;
            }

            material = blackMaterial.ToArray();
            positions = blackPositions.ToArray();

            if (material.Length != positions.Length)
            {
                throw new ArgumentException("Для черных должно быть указано равное число фигур и полей.");
            }

            for (var i = 0; i < material.Length; ++i)
            {
                var location = new SquareLocation(positions[i]);

                if (HasPieceAt(location))
                {
                    throw new ArgumentException("Для двух фигур указана одна и та же позиция.");
                }

                _pieceNames[location.X, location.Y] = material[i];
                _pieceColors[location.X, location.Y] = PieceColor.Black;
            }

            MoveTurn = moveTurn;
        }

        public static bool operator ==(GamePosition first, GamePosition second)
        {
            if (ReferenceEquals(first, second))
            {
                return true;
            }

            if (first is null || second is null)
            {
                return false;
            }

            return first.EqualsInProperties(second);
        }

        public static bool operator !=(GamePosition first, GamePosition second) => !(first == second);

        public bool HasPieceAt(int x, int y) => _pieceNames[x, y] != null;

        public bool HasPieceAt(SquareLocation location) => _pieceNames[location.X, location.Y] != null;

        public PieceName? GetPieceName(int x, int y) => _pieceNames[x, y];

        public PieceName? GetPieceName(SquareLocation location) => _pieceNames[location.X, location.Y];

        public PieceColor? GetPieceColor(int x, int y) => _pieceColors[x, y];

        public PieceColor? GetPieceColor(SquareLocation location) => _pieceColors[location.X, location.Y];

        private bool EqualsInProperties(GamePosition other)
        {
            if (MoveTurn != other.MoveTurn)
            {
                return false;
            }

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (_pieceNames[i, j] != other._pieceNames[i, j] ||
                        _pieceColors[i, j] != other._pieceColors[i, j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal void ToPreceding(Move move)
        {
            _pieceNames[move.Destination.X, move.Destination.Y] = null;
            _pieceColors[move.Destination.X, move.Destination.Y] = null;
            _pieceNames[move.Start.X, move.Start.Y] = move.MovingPieceName;
            _pieceColors[move.Start.X, move.Start.Y] = move.MovingPieceColor;
            MoveTurn = move.MovingPieceColor;

            if (move.IsCastling)
            {
                if (move.Destination.X == 6)
                {
                    _pieceNames[5, move.Start.Y] = null;
                    _pieceColors[5, move.Start.Y] = null;
                    _pieceNames[7, move.Start.Y] = PieceName.Rook;
                    _pieceColors[7, move.Start.Y] = move.MovingPieceColor;
                    return;
                }

                _pieceNames[3, move.Start.Y] = null;
                _pieceColors[3, move.Start.Y] = null;
                _pieceNames[0, move.Start.Y] = PieceName.Rook;
                _pieceColors[0, move.Start.Y] = move.MovingPieceColor;
            }
        }

        public bool IsLegal()
        {
            var whiteHaveKing = false;
            var blackHaveKing = false;

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (_pieceNames[i, j] == PieceName.Pawn)
                    {
                        if (j == 0 || j == 7)
                        {
                            return false;
                        }

                        continue;
                    }

                    if (_pieceNames[i, j] != PieceName.King)
                    {
                        continue;
                    }

                    if (_pieceColors[i, j] == PieceColor.White)
                    {
                        if (whiteHaveKing)
                        {
                            return false;
                        }

                        if (MoveTurn == PieceColor.Black && HasMenacedPieceAt(i, j))
                        {
                            return false;
                        }

                        whiteHaveKing = true;
                        continue;
                    }

                    if (blackHaveKing)
                    {
                        return false;
                    }

                    if (MoveTurn == PieceColor.White && HasMenacedPieceAt(i, j))
                    {
                        return false;
                    }

                    blackHaveKing = true;
                }
            }

            return whiteHaveKing && blackHaveKing;
        }

        private bool HasMenacedPieceAt(int x, int y) => HasVerticallyMenacedPieceAt(x, y) ||
        HasHorizontallyMenacedPieceAt(x, y) || HasDiagonallyMenacedPieceAt(x, y) || HasPieceMenacedByKnightAt(x, y);

        private bool HasVerticallyMenacedPieceAt(int x, int y)
        {
            for (var i = y + 1; i < 8; ++i)
            {
                var pieceName = _pieceNames[x, i];

                if (pieceName == null)
                {
                    continue;
                }

                if (_pieceColors[x, i] == _pieceColors[x, y])
                {
                    break;
                }

                if (pieceName == PieceName.King)
                {
                    if (i == y + 1)
                    {
                        return true;
                    }

                    break;
                }

                if (pieceName == PieceName.Queen || pieceName == PieceName.Rook)
                {
                    return true;
                }

                break;
            }

            for (var i = y - 1; i >= 0; --i)
            {
                var pieceName = _pieceNames[x, i];

                if (pieceName == null)
                {
                    continue;
                }

                if (_pieceColors[x, i] == _pieceColors[x, y])
                {
                    break;
                }

                if (pieceName == PieceName.King)
                {
                    if (i == y - 1)
                    {
                        return true;
                    }

                    break;
                }

                if (pieceName == PieceName.Queen || pieceName == PieceName.Rook)
                {
                    return true;
                }

                break;
            }

            return false;
        }

        private bool HasHorizontallyMenacedPieceAt(int x, int y)
        {
            for (var i = x + 1; i < 8; ++i)
            {
                var pieceName = _pieceNames[i, y];

                if (pieceName == null)
                {
                    continue;
                }

                if (_pieceColors[i, y] == _pieceColors[x, y])
                {
                    break;
                }

                if (pieceName == PieceName.King)
                {
                    if (i == x + 1)
                    {
                        return true;
                    }

                    break;
                }

                if (pieceName == PieceName.Queen || pieceName == PieceName.Rook)
                {
                    return true;
                }

                break;
            }

            for (var i = x - 1; i >= 0; --i)
            {
                var pieceName = _pieceNames[i, y];

                if (pieceName == null)
                {
                    continue;
                }

                if (_pieceColors[i, y] == _pieceColors[x, y])
                {
                    break;
                }

                if (pieceName == PieceName.King)
                {
                    if (i == x - 1)
                    {
                        return true;
                    }

                    break;
                }

                if (pieceName == PieceName.Queen || pieceName == PieceName.Rook)
                {
                    return true;
                }

                break;
            }

            return false;
        }

        private bool HasDiagonallyMenacedPieceAt(int x, int y)
        {
            for (int i = x + 1, j = y + 1; i < 8 && j < 8; ++i, ++j)
            {
                var pieceName = _pieceNames[i, j];

                if (pieceName == null)
                {
                    continue;
                }

                if (_pieceColors[i, j] == _pieceColors[x, y])
                {
                    break;
                }

                if (pieceName == PieceName.King)
                {
                    if (i == x + 1)
                    {
                        return true;
                    }

                    break;
                }

                if (pieceName == PieceName.Queen || pieceName == PieceName.Bishop)
                {
                    return true;
                }

                if (pieceName == PieceName.Pawn)
                {
                    if (i == x + 1 && _pieceColors[i, j] == PieceColor.Black)
                    {
                        return true;
                    }
                }

                break;
            }

            for (int i = x - 1, j = y - 1; i >= 0 && j >= 0; --i, --j)
            {
                var pieceName = _pieceNames[i, j];

                if (pieceName == null)
                {
                    continue;
                }

                if (_pieceColors[i, j] == _pieceColors[x, y])
                {
                    break;
                }

                if (pieceName == PieceName.King)
                {
                    if (i == x - 1)
                    {
                        return true;
                    }

                    break;
                }

                if (pieceName == PieceName.Queen || pieceName == PieceName.Bishop)
                {
                    return true;
                }

                if (pieceName == PieceName.Pawn)
                {
                    if (i == x - 1 && _pieceColors[i, j] == PieceColor.White)
                    {
                        return true;
                    }
                }

                break;
            }

            for (int i = x + 1, j = y - 1; i < 8 && j >= 0; ++i, --j)
            {
                var pieceName = _pieceNames[i, j];

                if (pieceName == null)
                {
                    continue;
                }

                if (_pieceColors[i, j] == _pieceColors[x, y])
                {
                    break;
                }

                if (pieceName == PieceName.King)
                {
                    if (i == x + 1)
                    {
                        return true;
                    }

                    break;
                }

                if (pieceName == PieceName.Queen || pieceName == PieceName.Bishop)
                {
                    return true;
                }

                if (pieceName == PieceName.Pawn)
                {
                    if (i == x + 1 && _pieceColors[i, j] == PieceColor.White)
                    {
                        return true;
                    }
                }

                break;
            }

            for (int i = x - 1, j = y + 1; i >= 0 && j < 8; --i, ++j)
            {
                var pieceName = _pieceNames[i, j];

                if (pieceName == null)
                {
                    continue;
                }

                if (_pieceColors[i, j] == _pieceColors[x, y])
                {
                    break;
                }

                if (pieceName == PieceName.King)
                {
                    if (i == x - 1)
                    {
                        return true;
                    }

                    break;
                }

                if (pieceName == PieceName.Queen || pieceName == PieceName.Bishop)
                {
                    return true;
                }

                if (pieceName == PieceName.Pawn)
                {
                    if (i == x - 1 && _pieceColors[i, j] == PieceColor.Black)
                    {
                        return true;
                    }
                }

                break;
            }

            return false;
        }

        private bool HasPieceMenacedByKnightAt(int x, int y)
        {
            var verticalShifts = new int[] { 2, 2, -2, -2, 1, 1, -1, -1 };
            var horizontalShifts = new int[] { -1, 1, -1, 1, -2, 2, -2, 2 };

            for (var i = 0; i < 8; ++i)
            {
                var targetX = x + horizontalShifts[i];

                if (targetX < 0 || targetX > 7)
                {
                    continue;
                }

                var targetY = y + verticalShifts[i];

                if (targetY < 0 || targetY > 7)
                {
                    continue;
                }

                if (_pieceNames[targetX, targetY] == PieceName.Knight &&
                    _pieceColors[targetX, targetY] != _pieceColors[x, y])
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsClear()
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (HasPieceAt(i, j))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null || obj is not GamePosition)
            {
                return false;
            }

            return EqualsInProperties((GamePosition)obj);
        }
    }
}