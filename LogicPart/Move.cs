
namespace Chess.LogicPart
{
    internal class Move
    {
        public ChessPiece MovingPiece { get; }

        public Square MoveSquare { get; }

        public ChessPiece CapturedPiece { get; set; } // ==null, если ход не явл. взятием.

        public ChessPiece NewPiece { get; } // ==null, если ход не явл. превращением пешки.

        public bool IsCapture { get; }

        public bool IsEnPassantCapture { get; }

        public bool IsPawnMove { get; }

        public bool IsPawnJump { get; }

        public bool IsPawnPromotion { get; }

        public bool IsCastleKingside { get; }

        public bool IsCastleQueenside { get; }

        public Move(ChessPiece movingPiece, Square moveSquare)
        {
            MovingPiece = movingPiece;
            MoveSquare = moveSquare;

            IsPawnMove = movingPiece is Pawn;
            IsPawnJump = IsPawnMove && Math.Abs(movingPiece.Horizontal - moveSquare.Horizontal) == 2;

            IsEnPassantCapture = IsPawnMove && moveSquare.Vertical != movingPiece.Vertical && moveSquare.IsEmpty;
            IsCapture = !moveSquare.IsEmpty || IsEnPassantCapture;
            
            IsCastleKingside = movingPiece is King && moveSquare.Vertical - movingPiece.Vertical == 2;
            IsCastleQueenside = movingPiece is King && movingPiece.Vertical - moveSquare.Vertical == 2;
        }

        public Move(ChessPiece movingPiece, Square moveSquare, ChessPiece newPiece) : this(movingPiece, moveSquare)
        {
            NewPiece = newPiece;
            IsPawnPromotion = newPiece != null;
        }
    }
}