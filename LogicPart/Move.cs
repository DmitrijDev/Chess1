
namespace Chess.LogicPart
{
    public class Move
    {
        public ChessPiece MovingPiece { get; }

        public Square StartSquare { get; }

        public Square MoveSquare { get; }

        public ChessPiece CapturedPiece { get; } // ==null, если ход не явл. взятием.

        public ChessPiece NewPiece { get; } // ==null, если ход не явл. превращением пешки.

        public bool IsEnPassantCapture { get; }

        public bool IsCastleKingside { get; }

        public bool IsCastleQueenside { get; }

        public ulong CreationMoment { get; }

        public Move(ChessPiece movingPiece, Square moveSquare)
        {
            MovingPiece = movingPiece;

            if (!MovingPiece.IsOnBoard)
            {
                throw new ArgumentException("Указанная фигура не на доске.");
            }

            StartSquare = MovingPiece.Position;            
            CreationMoment = Board.ModCount;

            if (Board.Status != GameStatus.GameIsNotOver || MovingPiece.Color != Board.MovingSideColor)
            {
                throw new ArgumentException("Указанная фигура не может делать ходов.");
            }

            MoveSquare = moveSquare;

            if (MoveSquare.Board != Board)
            {
                throw new ArgumentException("Указаны фигура и поле на разных досках.");
            }

            IsEnPassantCapture = IsPawnMove && MoveSquare == Board.PassedByPawnSquare;

            IsCastleKingside = IsCastleKingsideMove(MovingPiece, MoveSquare);
            IsCastleQueenside = IsCastleQueensideMove(MovingPiece, MoveSquare);

            CapturedPiece = !IsEnPassantCapture ? MoveSquare.ContainedPiece :
                Board[MoveSquare.Vertical, MovingPiece.Color == ChessPieceColor.White ? 4 : 3].ContainedPiece;
        }

        public Move(ChessPiece movingPiece, Square moveSquare, ChessPieceName newPieceName) : this(movingPiece, moveSquare)
        {
            if (!IsPawnPromotion)
            {
                throw new ArgumentException("Этот конструктор только для превращения пешки.");
            }

            NewPiece = ChessPiece.GetNewPiece(newPieceName, MovingPiece.Color);
        }

        public static bool IsCastleKingsideMove(ChessPiece movingPiece, Square moveSquare)
        {
            if (movingPiece.Name != ChessPieceName.King)
            {
                return false;
            }

            if (!movingPiece.IsOnBoard)
            {
                return false;
            }

            if (!(movingPiece.Color == ChessPieceColor.White && movingPiece.Vertical == 4 && movingPiece.Horizontal == 0) &&
                !(movingPiece.Color == ChessPieceColor.Black && movingPiece.Vertical == 4 && movingPiece.Horizontal == 7))
            {
                return false;
            }

            if (moveSquare != movingPiece.Board[6, movingPiece.Horizontal])
            {
                return false;
            }

            var rookPosition = movingPiece.Board[7, movingPiece.Horizontal];

            if (rookPosition.IsEmpty || rookPosition.ContainedPiece.Name != ChessPieceName.Rook || rookPosition.ContainedPiece.Color != movingPiece.Color)
            {
                return false;
            }

            return movingPiece.Board[5, movingPiece.Horizontal].IsEmpty && moveSquare.IsEmpty;
        }

        public static bool IsCastleQueensideMove(ChessPiece movingPiece, Square moveSquare)
        {
            if (movingPiece.Name != ChessPieceName.King)
            {
                return false;
            }

            if (!movingPiece.IsOnBoard)
            {
                return false;
            }

            if (!(movingPiece.Color == ChessPieceColor.White && movingPiece.Vertical == 4 && movingPiece.Horizontal == 0) &&
                !(movingPiece.Color == ChessPieceColor.Black && movingPiece.Vertical == 4 && movingPiece.Horizontal == 7))
            {
                return false;
            }

            if (moveSquare != movingPiece.Board[2, movingPiece.Horizontal])
            {
                return false;
            }

            var rookPosition = movingPiece.Board[0, movingPiece.Horizontal];

            if (rookPosition.IsEmpty || rookPosition.ContainedPiece.Name != ChessPieceName.Rook || rookPosition.ContainedPiece.Color != movingPiece.Color)
            {
                return false;
            }

            return movingPiece.Board[1, movingPiece.Horizontal].IsEmpty && moveSquare.IsEmpty && movingPiece.Board[3, movingPiece.Horizontal].IsEmpty;
        }

        public ChessBoard Board => StartSquare.Board;

        public bool IsPawnMove => MovingPiece.Name == ChessPieceName.Pawn;

        public bool IsPawnDoubleMove => IsPawnMove && (StartSquare.Horizontal - MoveSquare.Horizontal == 2 || MoveSquare.Horizontal - StartSquare.Horizontal == 2);

        public bool IsPawnPromotion => IsPawnMove && (MoveSquare.Horizontal == 0 || MoveSquare.Horizontal == 7);

        public bool IsCapture => CapturedPiece != null;

        public bool NewPieceSelected => NewPiece != null;
    }
}