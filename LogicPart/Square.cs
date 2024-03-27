
namespace Chess.LogicPart
{
    public class Square
    {
        public ChessBoard Board { get; }

        public int Vertical { get; }

        public int Horizontal { get; }

        public ChessPiece ContainedPiece { get; private set; }

        internal List<ChessPiece> WhiteMenaces { get; set; }

        internal List<ChessPiece> BlackMenaces { get; set; }

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

        public bool IsMenacedBy(ChessPieceColor color)
        {
            Board.RenewMenaces(color);
            var menaces = color == ChessPieceColor.White ? WhiteMenaces : BlackMenaces;
            return menaces != null && menaces.Any(piece => piece.Color == color);
        }

        public IEnumerable<ChessPiece> GetMenaces(ChessPieceColor color)
        {
            ulong modCount;
            ulong gameStartsCount;

            lock (Board)
            {
                modCount = Board.ModCount;
                gameStartsCount = Board.GameStartsCount;
            }

            Board.RenewMenaces(color);
            var menaces = color == ChessPieceColor.White ? WhiteMenaces : BlackMenaces;

            if (Board.ModCount != modCount || Board.GameStartsCount != gameStartsCount)
            {
                throw new InvalidOperationException("Изменение позиции во время перечисления.");
            }

            if (menaces == null)
            {
                yield break;
            }

            foreach (var piece in menaces)
            {
                if (Board.ModCount != modCount || Board.GameStartsCount != gameStartsCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return piece;
            }
        }

        public bool IsOnSameDiagonal(Square other) => other.Board == Board &&
            Math.Abs(Vertical - other.Vertical) == Math.Abs(Horizontal - other.Horizontal);

        public bool IsOnSameDiagonal(ChessPiece piece)
        {
            var position = piece.Position;
            return position != null && IsOnSameDiagonal(position);
        }

        public bool IsEmpty => ContainedPiece == null;
    }
}