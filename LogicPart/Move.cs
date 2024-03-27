
namespace Chess.LogicPart
{
    public class Move
    {
        internal object Precedent { get; set; }

        public int Depth { get; internal set; }

        public ChessPiece MovingPiece { get; internal set; }

        public Square StartSquare { get; internal set; }

        public Square MoveSquare { get; internal set; }

        public ChessPiece CapturedPiece { get; internal set; } // ==null, если ход не явл. взятием.

        public ChessPiece NewPiece { get; internal set; } // ==null, если ход не явл. превращением пешки.

        public bool IsEnPassantCapture { get; internal set; }

        public bool IsCastleKingside { get; internal set; }

        public bool IsCastleQueenside { get; internal set; }

        internal Move() { }

        public Move(ChessPiece movingPiece, Square moveSquare)
        {
            if (moveSquare == null)
            {
                throw new ArgumentNullException("Не указано поле для хода.");
            }

            lock (moveSquare.Board)
            {
                if (movingPiece == null)
                {
                    throw new ArgumentNullException("Не указана фигура, делающая ход.");
                }

                if (!movingPiece.IsOnBoard)
                {
                    throw new ArgumentException("Указанная фигура не на доске.");
                }

                if (movingPiece.Board != moveSquare.Board)
                {
                    throw new ArgumentException("Указаны фигура и поле на разных досках.");
                }

                if (moveSquare.Board.Status != BoardStatus.GameIsIncomplete || movingPiece.Color != movingPiece.Board.MovingSideColor)
                {
                    throw new ArgumentException("Указанная фигура не может делать ходов.");
                }

                if (movingPiece.Position == moveSquare)
                {
                    throw new ArgumentException("Фигура не может пойти на поле, на котором уже находится.");
                }

                if (!moveSquare.IsEmpty && moveSquare.ContainedPiece.Color == movingPiece.Color)
                {
                    throw new ArgumentException("Фигура не может пойти на поле, занятое фигурой того же цвета.");
                }

                MovingPiece = movingPiece;
                MoveSquare = moveSquare;

                Precedent = Board.MovesCount > 0 ? Board.GetLastMove() : Board.InitialPosition;
                Depth = Board.MovesCount + 1;
                StartSquare = MovingPiece.Position;

                CheckWhetherIsEnPassantCapture();
                CheckWhetherIsCastleMove();

                CapturedPiece = !IsEnPassantCapture ? MoveSquare.ContainedPiece :
                    Board[MoveSquare.Vertical, StartSquare.Horizontal].ContainedPiece;
            }
        }

        public Move(ChessPiece movingPiece, Square moveSquare, ChessPieceName newPieceName) : this(movingPiece, moveSquare)
        {
            if (!IsPawnPromotion)
            {
                throw new ArgumentException("Этот конструктор только для хода пешкой на крайнюю горизонталь.");
            }

            if (newPieceName == ChessPieceName.King)
            {
                throw new ArgumentException("Пешка не может превращаться в короля.");
            }

            if (newPieceName == ChessPieceName.Pawn)
            {
                throw new ArgumentException("Пешка не может превращаться в пешку.");
            }

            NewPiece = ChessPiece.GetNewPiece(newPieceName, MovingPiece.Color);
        }

        private void CheckWhetherIsEnPassantCapture()
        {
            if (!IsPawnMove || MoveSquare != Board.PassedByPawnSquare)
            {
                return;
            }

            if (!(MovingPiece.Color == ChessPieceColor.White && MovingPiece.Horizontal == 4) &&
                !(MovingPiece.Color == ChessPieceColor.Black && MovingPiece.Horizontal == 3))
            {
                return;
            }

            IsEnPassantCapture = Math.Abs(MovingPiece.Vertical - MoveSquare.Vertical) == 1;
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

                IsCastleKingside = Board[5, MovingPiece.Horizontal].IsEmpty && MoveSquare.IsEmpty;
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

                IsCastleQueenside = Board[1, MovingPiece.Horizontal].IsEmpty && MoveSquare.IsEmpty &&
                    Board[3, MovingPiece.Horizontal].IsEmpty;
            }
        }

        public IEnumerable<Move> GetPrecedingMoves()
        {
            var preceding = PrecedingMove;

            while (preceding != null)
            {
                yield return preceding;
                preceding = preceding.PrecedingMove;
            }
        }

        public ChessBoard Board => MoveSquare.Board;

        public bool IsPawnMove => MovingPiece.Name == ChessPieceName.Pawn;

        public bool IsPawnDoubleVerticalMove => IsPawnMove && StartSquare.Vertical == MoveSquare.Vertical &&
            Math.Abs(StartSquare.Horizontal - MoveSquare.Horizontal) == 2;

        public bool IsPawnPromotion => IsPawnMove && (MoveSquare.Horizontal == 0 || MoveSquare.Horizontal == 7);

        public bool IsCapture => CapturedPiece != null;

        public bool NewPieceSelected => NewPiece != null;

        public Move PrecedingMove => Precedent as Move;
    }
}