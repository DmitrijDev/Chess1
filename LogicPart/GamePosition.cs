
namespace Chess.LogicPart
{
    public sealed class GamePosition
    {
        internal PieceName?[,] PieceNames { get; } = new PieceName?[8, 8];

        internal PieceColor?[,] PieceColors { get; } = new PieceColor?[8, 8];

        public PieceColor MoveTurn { get; private set; }

        public GamePosition(ChessBoard board)
        {
            lock (board.Locker)
            {
                foreach (var piece in board.GetMaterial())
                {
                    PieceNames[piece.X, piece.Y] = piece.Name;
                    PieceColors[piece.X, piece.Y] = piece.Color;
                }

                MoveTurn = board.MoveTurn;
            }
        }

        internal GamePosition(GamePosition other)
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    PieceNames[i, j] = other.PieceNames[i, j];
                    PieceColors[i, j] = other.PieceColors[i, j];
                }
            }

            MoveTurn = other.MoveTurn;
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

                if (PieceNames[location.X, location.Y] != null)
                {
                    throw new ArgumentException("Для двух фигур указана одна и та же позиция.");
                }

                PieceNames[location.X, location.Y] = material[i];
                PieceColors[location.X, location.Y] = PieceColor.White;
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

                if (PieceNames[location.X, location.Y] != null)
                {
                    throw new ArgumentException("Для двух фигур указана одна и та же позиция.");
                }

                PieceNames[location.X, location.Y] = material[i];
                PieceColors[location.X, location.Y] = PieceColor.Black;
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

        public PieceName? GetPieceName(int x, int y) => PieceNames[x, y];

        public PieceColor? GetPieceColor(int x, int y) => PieceColors[x, y];

        internal bool EqualsInProperties(GamePosition other)
        {
            if (MoveTurn != other.MoveTurn)
            {
                return false;
            }

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (PieceNames[i, j] != other.PieceNames[i, j] ||
                        PieceColors[i, j] != other.PieceColors[i, j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal void ToPreceding(Move move)
        {
            PieceNames[move.Destination.X, move.Destination.Y] = null;
            PieceColors[move.Destination.X, move.Destination.Y] = null;

            PieceNames[move.Start.X, move.Start.Y] = move.MovingPieceName;
            PieceColors[move.Start.X, move.Start.Y] = move.MovingPieceColor;

            MoveTurn = move.MovingPieceColor;

            if (move.IsKingsideCastling)
            {
                PieceNames[5, move.Start.Y] = null;
                PieceColors[5, move.Start.Y] = null;

                PieceNames[7, move.Start.Y] = PieceName.Rook;
                PieceColors[7, move.Start.Y] = move.MovingPieceColor;

                return;
            }

            if (move.IsQueensideCastling)
            {
                PieceNames[3, move.Start.Y] = null;
                PieceColors[3, move.Start.Y] = null;

                PieceNames[0, move.Start.Y] = PieceName.Rook;
                PieceColors[0, move.Start.Y] = move.MovingPieceColor;
            }
        }

        public bool IsLegal()
        {
            SquareLocation whiteKingLocation = null;
            SquareLocation blackKingLocation = null;

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (PieceNames[i, j] == PieceName.King)
                    {
                        if (PieceColors[i, j] == PieceColor.White)
                        {
                            if (whiteKingLocation != null)
                            {
                                return false;
                            }

                            whiteKingLocation = new(i, j);
                        }
                        else
                        {
                            if (blackKingLocation != null)
                            {
                                return false;
                            }

                            blackKingLocation = new(i, j);
                        }
                    }

                    if (PieceNames[i, j] == PieceName.Pawn && (j == 0 || j == 7))
                    {
                        return false;
                    }
                }
            }

            if (whiteKingLocation == null || blackKingLocation == null)
            {
                return false;
            }

            if (MoveTurn == PieceColor.White)
            {
                return !HasMenacedPieceAt(blackKingLocation);
            }

            return !HasMenacedPieceAt(whiteKingLocation);
        }

        private bool HasMenacedPieceAt(SquareLocation location) => HasVerticallyMenacedPieceAt(location) ||
        HasHorizontallyMenacedPieceAt(location) || HasDiagonallyMenacedPieceAt(location) || HasPieceMenacedByKnightAt(location);

        private bool HasVerticallyMenacedPieceAt(SquareLocation location)
        {
            for (var i = location.Y + 1; i < 8; ++i)
            {
                if (PieceNames[location.X, i] == null)
                {
                    continue;
                }

                if (PieceColors[location.X, i] == PieceColors[location.X, location.Y])
                {
                    break;
                }

                if (PieceNames[location.X, i] == PieceName.King)
                {
                    if (i == location.Y + 1)
                    {
                        return true;
                    }

                    break;
                }

                if (PieceNames[location.X, i] == PieceName.Queen || PieceNames[location.X, i] == PieceName.Rook)
                {
                    return true;
                }

                break;
            }

            for (var i = location.Y - 1; i >= 0; --i)
            {
                if (PieceNames[location.X, i] == null)
                {
                    continue;
                }

                if (PieceColors[location.X, i] == PieceColors[location.X, location.Y])
                {
                    break;
                }

                if (PieceNames[location.X, i] == PieceName.King)
                {
                    if (i == location.Y - 1)
                    {
                        return true;
                    }

                    break;
                }

                if (PieceNames[location.X, i] == PieceName.Queen || PieceNames[location.X, i] == PieceName.Rook)
                {
                    return true;
                }

                break;
            }

            return false;
        }

        private bool HasHorizontallyMenacedPieceAt(SquareLocation location)
        {
            for (var i = location.X + 1; i < 8; ++i)
            {
                if (PieceNames[i, location.Y] == null)
                {
                    continue;
                }

                if (PieceColors[i, location.Y] == PieceColors[location.X, location.Y])
                {
                    break;
                }

                if (PieceNames[i, location.Y] == PieceName.King)
                {
                    if (i == location.X + 1)
                    {
                        return true;
                    }

                    break;
                }

                if (PieceNames[i, location.Y] == PieceName.Queen || PieceNames[i, location.Y] == PieceName.Rook)
                {
                    return true;
                }

                break;
            }

            for (var i = location.X - 1; i >= 0; --i)
            {
                if (PieceNames[i, location.Y] == null)
                {
                    continue;
                }

                if (PieceColors[i, location.Y] == PieceColors[location.X, location.Y])
                {
                    break;
                }

                if (PieceNames[i, location.Y] == PieceName.King)
                {
                    if (i == location.X - 1)
                    {
                        return true;
                    }

                    break;
                }

                if (PieceNames[i, location.Y] == PieceName.Queen || PieceNames[i, location.Y] == PieceName.Rook)
                {
                    return true;
                }

                break;
            }

            return false;
        }

        private bool HasDiagonallyMenacedPieceAt(SquareLocation location)
        {
            for (int i = location.X + 1, j = location.Y + 1; i < 8 && j < 8; ++i, ++j)
            {
                if (PieceNames[i, j] == null)
                {
                    continue;
                }

                if (PieceColors[i, j] == PieceColors[location.X, location.Y])
                {
                    break;
                }

                if (PieceNames[i, j] == PieceName.King)
                {
                    if (i == location.X + 1)
                    {
                        return true;
                    }

                    break;
                }

                if (PieceNames[i, j] == PieceName.Queen || PieceNames[i, j] == PieceName.Bishop)
                {
                    return true;
                }

                if (PieceNames[i, j] == PieceName.Pawn)
                {
                    if (i == location.X + 1 && PieceColors[i, j] == PieceColor.Black)
                    {
                        return true;
                    }

                    break;
                }

                break;
            }

            for (int i = location.X - 1, j = location.Y - 1; i >= 0 && j >= 0; --i, --j)
            {
                if (PieceNames[i, j] == null)
                {
                    continue;
                }

                if (PieceColors[i, j] == PieceColors[location.X, location.Y])
                {
                    break;
                }

                if (PieceNames[i, j] == PieceName.King)
                {
                    if (i == location.X - 1)
                    {
                        return true;
                    }

                    break;
                }

                if (PieceNames[i, j] == PieceName.Queen || PieceNames[i, j] == PieceName.Bishop)
                {
                    return true;
                }

                if (PieceNames[i, j] == PieceName.Pawn)
                {
                    if (i == location.X - 1 && PieceColors[i, j] == PieceColor.White)
                    {
                        return true;
                    }

                    break;
                }

                break;
            }

            for (int i = location.X + 1, j = location.Y - 1; i < 8 && j >= 0; ++i, --j)
            {
                if (PieceNames[i, j] == null)
                {
                    continue;
                }

                if (PieceColors[i, j] == PieceColors[location.X, location.Y])
                {
                    break;
                }

                if (PieceNames[i, j] == PieceName.King)
                {
                    if (i == location.X + 1)
                    {
                        return true;
                    }

                    break;
                }

                if (PieceNames[i, j] == PieceName.Queen || PieceNames[i, j] == PieceName.Bishop)
                {
                    return true;
                }

                if (PieceNames[i, j] == PieceName.Pawn)
                {
                    if (i == location.X + 1 && PieceColors[i, j] == PieceColor.White)
                    {
                        return true;
                    }

                    break;
                }

                break;
            }

            for (int i = location.X - 1, j = location.Y + 1; i >= 0 && j < 8; --i, ++j)
            {
                if (PieceNames[i, j] == null)
                {
                    continue;
                }

                if (PieceColors[i, j] == PieceColors[location.X, location.Y])
                {
                    break;
                }

                if (PieceNames[i, j] == PieceName.King)
                {
                    if (i == location.X - 1)
                    {
                        return true;
                    }

                    break;
                }

                if (PieceNames[i, j] == PieceName.Queen || PieceNames[i, j] == PieceName.Bishop)
                {
                    return true;
                }

                if (PieceNames[i, j] == PieceName.Pawn)
                {
                    if (i == location.X - 1 && PieceColors[i, j] == PieceColor.Black)
                    {
                        return true;
                    }

                    break;
                }

                break;
            }

            return false;
        }

        private bool HasPieceMenacedByKnightAt(SquareLocation location)
        {
            var verticalShifts = new int[] { 2, 2, -2, -2, 1, 1, -1, -1 };
            var horizontalShifts = new int[] { -1, 1, -1, 1, -2, 2, -2, 2 };

            for (var i = 0; i < 8; ++i)
            {
                var targetVertical = location.X + horizontalShifts[i];
                var targetHorizontal = location.Y + verticalShifts[i];

                if (targetVertical < 0 || targetHorizontal < 0 || targetVertical >= 8 || targetHorizontal >= 8)
                {
                    continue;
                }

                if (PieceNames[targetVertical, targetHorizontal] == PieceName.Knight &&
                    PieceColors[targetVertical, targetHorizontal] != PieceColors[location.X, location.Y])
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
                    if (PieceNames[i, j] != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override bool Equals(object obj)
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