
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

        public IEnumerable<ChessPiece> GetMenaces() => _menaces != null ? _menaces.ToArray() : Enumerable.Empty<ChessPiece>();

        public bool IsMenaced() => _menaces != null;

        public static void RenewMenaces(ChessBoard board)
        {
            if (board.ModCount != board.GameStartMoment || board.MovesCount > 0)
            {
                for (var i = 0; i < 8; ++i)
                {
                    for (var j = 0; j < 8; ++j)
                    {
                        board[i, j].RemoveMenacesList();
                    }
                }
            }

            foreach (var piece in board.GetMaterial(board.MovingSideColor == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White))
            {
                foreach (var square in piece.GetAttackedSquares())
                {
                    square._menaces ??= new List<ChessPiece>();
                    square._menaces.Add(piece);
                }
            }
        }

        internal void RemoveMenacesList() => _menaces = null;

        public bool IsOnSameDiagonal(Square otherSquare) => otherSquare != null && otherSquare.Board == Board &&
            Math.Abs(Vertical - otherSquare.Vertical) == Math.Abs(Horizontal - otherSquare.Horizontal);

        public bool IsOnSameDiagonal(ChessPiece piece) => IsOnSameDiagonal(piece.Position);

        public bool IsEmpty => ContainedPiece == null;
    }
}