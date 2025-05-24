
namespace Chess.LogicPart
{
    public class Square
    {
        private readonly List<ChessPiece> _whiteMenaces = new List<ChessPiece>();
        private readonly List<ChessPiece> _blackMenaces = new List<ChessPiece>();

        public ChessBoard Board { get; }

        public SquareLocation Location { get; }

        public ChessPiece Contained { get; internal set; }

        internal Square(ChessBoard board, int x, int y)
        {
            Board = board;
            Location = new(x, y);
        }

        public bool IsOnSameDiagonal(Square other) => other?.Board == Board &&
        Location.IsOnSameDiagonal(other.Location);

        public bool IsOnSameDiagonal(ChessPiece piece) => IsOnSameDiagonal(piece?.Square);

        public int GetMenacesCount(PieceColor color)
        {
            if (color == PieceColor.White)
            {
                return _whiteMenaces.Count;
            }

            return _blackMenaces.Count;
        }

        public bool IsMenacedBy(PieceColor color) => GetMenacesCount(color) > 0;

        public IEnumerable<ChessPiece> GetMenaces(PieceColor color)
        {
            if (color == PieceColor.White)
            {
                return _whiteMenaces.Select(piece => piece);
            }

            return _blackMenaces.Select(piece => piece);
        }

        internal void RemoveAllMenaces()
        {
            _whiteMenaces.Clear();
            _blackMenaces.Clear();
        }

        internal bool RemoveMenace(ChessPiece piece)
        {
            if (piece.Color == PieceColor.White)
            {
                return _whiteMenaces.Remove(piece);
            }

            return _blackMenaces.Remove(piece);
        }

        internal void AddMenace(ChessPiece piece)
        {
            if (piece.Color == PieceColor.White)
            {
                _whiteMenaces.Add(piece);
                return;
            }

            _blackMenaces.Add(piece);
        }

        internal void Clear()
        {
            Contained.RemoveMenaces();
            OpenLines(null);
            Contained.Square = null;
            Contained = null;
        }

        internal void OpenLines(Square newPieceSquare)
        {
            foreach (var menace in _whiteMenaces.Concat(_blackMenaces).Where(m => m.IsLongRanged))
            {
                menace.OpenLine(this, newPieceSquare);
            }
        }

        internal void BlockLines()
        {
            foreach (var menace in _whiteMenaces.Concat(_blackMenaces).Where(m => m.IsLongRanged))
            {
                menace.BlockLine(this);
            }
        }

        public int X => Location.X;

        public int Y => Location.Y;

        public bool IsClear => Contained == null;

        public bool IsPawnPassed
        {
            get
            {
                var lastMove = Board.LastMove;

                if (lastMove == null || !lastMove.IsPawnMove || lastMove.Start.X != X)
                {
                    return false;
                }

                if (Y == 2)
                {
                    return lastMove.Start.Y == 1 && lastMove.Destination.Y == 3;
                }

                if (Y == 5)
                {
                    return lastMove.Start.Y == 6 && lastMove.Destination.Y == 4;
                }

                return false;
            }
        }
    }
}