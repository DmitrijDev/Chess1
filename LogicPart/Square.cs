
namespace Chess.LogicPart
{
    public class Square
    {
        internal List<ChessPiece> Menaces { get; set; }

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

        public List<ChessPiece> GetMenaces(ChessPieceColor color)
        {
            Board.Lock();

            if (Menaces == null)
            {
                Board.Unlock();
                return new List<ChessPiece>();
            }

            var result = Menaces.Where(piece => piece.Color == color).ToList();
            Board.Unlock();
            return result;
        }

        public bool IsOnSameDiagonal(Square otherSquare) => otherSquare.Board == Board &&
            Math.Abs(Vertical - otherSquare.Vertical) == Math.Abs(Horizontal - otherSquare.Horizontal);

        public bool IsOnSameDiagonal(ChessPiece piece) => piece.IsOnBoard && IsOnSameDiagonal(piece.Position);

        public bool IsEmpty => ContainedPiece == null;
    }
}