
namespace Chess.LogicPart
{
    internal class Move
    {
        public ChessPiece MovingPiece { get; }

        public Square MoveSquare { get; }

        public ChessPiece CapturedPiece { get; } // ==null, если ход не явл. взятием.

        public ChessPiece NewPiece { get; } // ==null, если ход не явл. превращением пешки.

        public bool IsCapture { get; }

        public bool IsEnPassantCapture { get; }

        public bool IsPawnMove { get; }

        public bool IsPawnDoubleMove { get; }

        public bool IsPawnPromotion { get; }

        public bool IsCastleKingside { get; }

        public bool IsCastleQueenside { get; }

        public Move(ChessPiece movingPiece, Square moveSquare)
        {
            MovingPiece = movingPiece;
            MoveSquare = moveSquare;

            IsPawnMove = movingPiece is Pawn;
            IsPawnDoubleMove = IsPawnMove && Math.Abs(movingPiece.Horizontal - moveSquare.Horizontal) == 2;

            IsEnPassantCapture = IsPawnMove && moveSquare.Vertical != movingPiece.Vertical && moveSquare.IsEmpty;
            IsCapture = !moveSquare.IsEmpty || IsEnPassantCapture;

            IsCastleKingside = MoveIsCastleKingside(movingPiece, moveSquare);
            IsCastleQueenside = MoveIsCastleQueenside(movingPiece, moveSquare);

            if (IsCapture)
            {
                CapturedPiece = !IsEnPassantCapture ? MoveSquare.ContainedPiece :
                    MovingPiece.Color == PieceColor.White ? Board[MoveSquare.Vertical, MoveSquare.Horizontal - 1].ContainedPiece :
                    Board[MoveSquare.Vertical, MoveSquare.Horizontal + 1].ContainedPiece;
            }
        }

        public Move(ChessPiece movingPiece, Square moveSquare, ChessPiece newPiece) : this(movingPiece, moveSquare)
        {
            NewPiece = newPiece;
            IsPawnPromotion = newPiece != null;
        }        

        public static bool MoveIsCastleKingside(ChessPiece movingPiece, Square moveSquare)
        {
            if (movingPiece is not King)
            {
                return false;
            }

            if (!(movingPiece.Color == PieceColor.White && movingPiece.Position.Name == "e1") && 
                !(movingPiece.Color == PieceColor.Black && movingPiece.Position.Name == "e8"))
            {
                return false;
            }

            if (moveSquare != movingPiece.Board[6,movingPiece.Horizontal])
            {
                return false;
            }

            if (movingPiece.Board[7, movingPiece.Horizontal].IsEmpty || movingPiece.Board[7, movingPiece.Horizontal].ContainedPiece is not Rook
                || movingPiece.Board[7, movingPiece.Horizontal].ContainedPiece.Color != movingPiece.Color)
            {
                return false;
            }

            return movingPiece.Board[5, movingPiece.Horizontal].IsEmpty && movingPiece.Board[6, movingPiece.Horizontal].IsEmpty;
        }

        public static bool MoveIsCastleQueenside(ChessPiece movingPiece, Square moveSquare)
        {
            if (movingPiece is not King)
            {
                return false;
            }

            if (!(movingPiece.Color == PieceColor.White && movingPiece.Position.Name == "e1") &&
                !(movingPiece.Color == PieceColor.Black && movingPiece.Position.Name == "e8"))
            {
                return false;
            }

            if (moveSquare != movingPiece.Board[2, movingPiece.Horizontal])
            {
                return false;
            }

            if (movingPiece.Board[0, movingPiece.Horizontal].IsEmpty || movingPiece.Board[0, movingPiece.Horizontal].ContainedPiece is not Rook
                || movingPiece.Board[0, movingPiece.Horizontal].ContainedPiece.Color != movingPiece.Color)
            {
                return false;
            }

            return movingPiece.Board[1, movingPiece.Horizontal].IsEmpty && movingPiece.Board[2, movingPiece.Horizontal].IsEmpty 
                && movingPiece.Board[3, movingPiece.Horizontal].IsEmpty;
        }

        public ChessBoard Board => MovingPiece.Board;
    }
}