
namespace Chess.LogicPart
{
    public class Square
    {
        public ChessBoard Board { get; }

        public SquareLocation Location { get; }

        public ChessPiece Contained { get; internal set; }

        internal List<ChessPiece> WhiteMenaces { get; } = new List<ChessPiece>();

        internal List<ChessPiece> BlackMenaces { get; } = new List<ChessPiece>();

        internal Square(ChessBoard board, int x, int y)
        {
            Board = board;
            Location = new(x, y);
        }       
                
        public bool IsOnSameDiagonal(Square other) => other?.Board == Board &&
        Math.Abs(X - other.X) == Math.Abs(Y - other.Y);

        public bool IsOnSameDiagonal(ChessPiece piece) => IsOnSameDiagonal(piece?.Square);

        public bool IsMenacedBy(PieceColor color) => color == PieceColor.White ?
        WhiteMenaces.Count > 0 : BlackMenaces.Count > 0;

        internal void Clear()
        {
            Contained.RemoveMenaces();
            OpenLines(null);
            Contained.Square = null;
            Contained = null;
        }

        internal void OpenLines(Square newPiecePosition)
        {
            foreach (var menace in WhiteMenaces.Concat(BlackMenaces).Where(piece => piece.IsLongRanged))
            {
                menace.OpenLine(this, newPiecePosition);
            }
        }

        internal void BlockLines()
        {
            foreach (var menace in WhiteMenaces.Concat(BlackMenaces).Where(piece => piece.IsLongRanged))
            {
                menace.BlockLine(this);
            }
        }

        public int X => Location.X;

        public int Y => Location.Y;

        public bool IsClear => Contained == null;

        public bool IsPawnPassed => Board.PawnPassedSquare == this;
    }
}