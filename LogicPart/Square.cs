
namespace Chess.LogicPart
{
    public class Square
    {
        private List<ChessPiece> _menaces;

        public ChessBoard Board { get; }

        public int Vertical { get; }

        public int Horizontal { get; }

        public ChessPiece ContainedPiece { get; private set; }

        internal Square(ChessBoard board, int vertical, int horizontal)
        {
            Board = board;
            Vertical = vertical;
            Horizontal = horizontal;
        }

        internal void Put(ChessPiece newPiece)
        {
            var oldPiece = ContainedPiece;
            ContainedPiece = newPiece;

            if (oldPiece != null && oldPiece.Position == this)
            {
                oldPiece.Remove();
            }

            if (!IsEmpty && ContainedPiece.Position != this)
            {
                ContainedPiece.PutTo(this);
            }
        }

        internal void Clear()
        {
            if (!IsEmpty)
            {
                Put(null);
            }
        }

        internal bool IsMenacedBy(ChessPiece piece)
        {
            Board.RenewMenaces();
            return _menaces != null && _menaces.Contains(piece);
        }

        internal bool IsMenacedBy(ChessPieceColor color)
        {
            Board.RenewMenaces();
            return _menaces != null && _menaces.Any(piece => piece.Color == color);
        }

        internal void AddMenace(ChessPiece piece)
        {
            _menaces ??= new List<ChessPiece>();
            _menaces.Add(piece);
        }

        internal void ClearMenaces() => _menaces = null;

        internal IEnumerable<ChessPiece> EnumerateMenaces(ChessPieceColor color)
        {
            Board.RenewMenaces();
            return _menaces == null ? Enumerable.Empty<ChessPiece>() : _menaces.Where(piece => piece.Color == color);
        }

        public bool IsOnSameDiagonal(Square otherSquare) => otherSquare.Board == Board &&
            Math.Abs(Vertical - otherSquare.Vertical) == Math.Abs(Horizontal - otherSquare.Horizontal);

        public bool IsOnSameDiagonal(ChessPiece piece) => piece.IsOnBoard && IsOnSameDiagonal(piece.Position);

        public bool IsEmpty => ContainedPiece == null;
    }
}