
namespace Chess.LogicPart
{
    public class Move
    {
        public GamePosition PrecedingPosition { get; internal set; }

        public ChessPiece MovingPiece { get; internal set; }

        public Square StartSquare { get; internal set; }

        public Square MoveSquare { get; internal set; }

        public ChessPiece CapturedPiece { get; internal set; } // ==null, если ход не явл. взятием.

        public ChessPiece NewPiece { get; internal set; } // ==null, если ход не явл. превращением пешки.

        public bool IsEnPassantCapture { get; internal set; }

        public bool IsCastleKingside { get; internal set; }

        public bool IsCastleQueenside { get; internal set; }

        internal Move()
        { }

        public Move(ChessPiece movingPiece, Square moveSquare)
        {
            moveSquare.Board.Lock();

            if (movingPiece == null)
            {
                moveSquare.Board.Unlock();
                throw new NullReferenceException("Не указана фигура, делающая ход.");
            }

            if (!movingPiece.IsOnBoard)
            {
                moveSquare.Board.Unlock();
                throw new ArgumentException("Указанная фигура не на доске.");
            }

            if (movingPiece.Board != moveSquare.Board)
            {
                moveSquare.Board.Unlock();
                throw new ArgumentException("Указаны фигура и поле на разных досках.");
            }

            if (movingPiece.Board.Status != BoardStatus.GameIsIncomplete || movingPiece.Color != movingPiece.Board.MovingSideColor)
            {
                moveSquare.Board.Unlock();
                throw new ArgumentException("Указанная фигура не может делать ходов.");
            }

            if (movingPiece.Position == moveSquare)
            {
                moveSquare.Board.Unlock();
                throw new ArgumentException("Фигура не может пойти на поле, на котором уже находится.");
            }

            if (!moveSquare.IsEmpty && moveSquare.ContainedPiece.Color == movingPiece.Color)
            {
                moveSquare.Board.Unlock();
                throw new ArgumentException("Фигура не может пойти на поле, занятое фигурой того же цвета.");
            }

            PrecedingPosition = movingPiece.Board.GetCurrentPosition();
            MovingPiece = movingPiece;
            StartSquare = movingPiece.Position;
            MoveSquare = moveSquare;
            IsEnPassantCapture = CheckWhetherIsEnPassantCapture();
            CheckWhetherIsCastleMove();

            CapturedPiece = !IsEnPassantCapture ? moveSquare.ContainedPiece :
                Board[moveSquare.Vertical, movingPiece.Color == ChessPieceColor.White ? 4 : 3].ContainedPiece;

            moveSquare.Board.Unlock();
        }

        public Move(ChessPiece movingPiece, Square moveSquare, ChessPieceName newPieceName) : this(movingPiece, moveSquare)
        {
            if (!IsPawnPromotion)
            {
                throw new ArgumentException("Этот конструктор только для превращения пешки.");
            }

            NewPiece = ChessPiece.GetNewPiece(newPieceName, MovingPiece.Color);
        }

        private bool CheckWhetherIsEnPassantCapture()
        {
            if (!IsPawnMove)
            {
                return false;
            }

            if (MoveSquare != Board.PassedByPawnSquare)
            {
                return false;
            }

            if (!(MovingPiece.Color == ChessPieceColor.White && MovingPiece.Horizontal == 4) &&
                !(MovingPiece.Color == ChessPieceColor.Black && MovingPiece.Horizontal == 3))
            {
                return false;
            }

            return MoveSquare.Vertical == MovingPiece.Vertical + 1 || MoveSquare.Vertical == MovingPiece.Vertical - 1;
        }

        private void CheckWhetherIsCastleMove()
        {
            if (MovingPiece.Name != ChessPieceName.King || MovingPiece.Vertical != 4)
            {
                return;
            }

            if (!(MovingPiece.Color == ChessPieceColor.White && MovingPiece.Horizontal == 0) &&
                !(MovingPiece.Color == ChessPieceColor.Black && MovingPiece.Horizontal == 7))
            {
                return;
            }

            if (MoveSquare.Horizontal != MovingPiece.Horizontal)
            {
                return;
            }

            if (MoveSquare.Vertical == 6)
            {
                var rookPosition = Board[7, MovingPiece.Horizontal];

                if (rookPosition.IsEmpty || rookPosition.ContainedPiece.Name != ChessPieceName.Rook ||
                    rookPosition.ContainedPiece.Color != MovingPiece.Color)
                {
                    return;
                }

                IsCastleKingside = Board[5, MovingPiece.Horizontal].IsEmpty && Board[6, MovingPiece.Horizontal].IsEmpty;
                return;
            }

            if (MoveSquare.Vertical == 2)
            {
                var rookPosition = Board[0, MovingPiece.Horizontal];

                if (rookPosition.IsEmpty || rookPosition.ContainedPiece.Name != ChessPieceName.Rook ||
                    rookPosition.ContainedPiece.Color != MovingPiece.Color)
                {
                    return;
                }

                IsCastleQueenside = Board[1, MovingPiece.Horizontal].IsEmpty && Board[2, MovingPiece.Horizontal].IsEmpty
                    && Board[3, MovingPiece.Horizontal].IsEmpty;
            }
        }

        internal static Move CreateMove(ChessPiece movingPiece, Square moveSquare)
        {
            var move = new Move
            {
                PrecedingPosition = moveSquare.Board.GetCurrentPosition(),
                MovingPiece = movingPiece,
                StartSquare = movingPiece.Position,
                MoveSquare = moveSquare
            };

            move.IsEnPassantCapture = move.CheckWhetherIsEnPassantCapture();
            move.CheckWhetherIsCastleMove();

            move.CapturedPiece = !move.IsEnPassantCapture ? moveSquare.ContainedPiece :
                move.Board[moveSquare.Vertical, movingPiece.Color == ChessPieceColor.White ? 4 : 3].ContainedPiece;

            return move;
        }

        internal static Move CreateMove(ChessPiece movingPiece, Square moveSquare, ChessPieceName newPieceName)
        {
            var move = CreateMove(movingPiece, moveSquare);
            move.NewPiece = ChessPiece.GetNewPiece(newPieceName, move.MovingPiece.Color);
            return move;
        }

        public ChessBoard Board => StartSquare.Board;

        public bool IsPawnMove => MovingPiece.Name == ChessPieceName.Pawn;

        public bool IsPawnDoubleVerticalMove => IsPawnMove && StartSquare.Vertical == MoveSquare.Vertical &&
            (StartSquare.Horizontal - MoveSquare.Horizontal == 2 || MoveSquare.Horizontal - StartSquare.Horizontal == 2);

        public bool IsPawnPromotion => IsPawnMove && (MoveSquare.Horizontal == 0 || MoveSquare.Horizontal == 7);

        public bool IsCapture => CapturedPiece != null;

        public bool NewPieceSelected => NewPiece != null;
    }
}