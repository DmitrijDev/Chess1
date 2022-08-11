
namespace Chess.LogicPart
{
    internal class Move
    {
        public ChessPiece MovingPiece { get; }

        public Square MoveSquare { get; }

        public bool IsCapture { get; }

        public bool IsCastleKingside { get; }

        public bool IsCastleQueenside { get; }

        public Move(ChessPiece movingPiece, Square moveSquare)
        {
            MovingPiece = movingPiece;
            MoveSquare = moveSquare;

            IsCapture = !moveSquare.IsEmpty;
            IsCastleKingside = movingPiece is King && moveSquare.Vertical - movingPiece.Vertical == 2;
            IsCastleQueenside = movingPiece is King && movingPiece.Vertical - moveSquare.Vertical == 2;
        }
    }
}