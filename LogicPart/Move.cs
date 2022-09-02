
namespace Chess.LogicPart
{
    internal class Move
    {
        public ChessPiece MovingPiece { get; }

        public Square MoveSquare { get; }

        public ChessPiece NewPiece { get; } // ==null, если ход не явл. превращением пешки.

        public bool IsCapture { get; }

        public bool IsPawnMove { get; }

        public bool IsPawnPromotion { get; }

        public bool IsCastleKingside { get; }

        public bool IsCastleQueenside { get; }

        public Move(ChessPiece movingPiece, Square moveSquare)
        {
            MovingPiece = movingPiece;
            MoveSquare = moveSquare;

            IsCapture = !moveSquare.IsEmpty;
            IsPawnMove = movingPiece is Pawn;
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