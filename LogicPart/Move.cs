
namespace Chess.LogicPart
{
    public sealed class Move
    {
        public PieceName MovingPieceName { get; }

        public PieceColor MovingPieceColor { get; }

        public SquareLocation Start { get; }

        public SquareLocation Destination { get; }

        public PieceName? CapturedPieceName { get; }

        public PieceName? NewPieceName { get; }

        public bool IsEnPassantCapture { get; }

        public bool IsKingsideCastling { get; private set; }

        public bool IsQueensideCastling { get; private set; }

        public int Depth { get; }

        internal object Precedent { get; }

        public Move(ChessPiece movingPiece, Square destinationSquare)
        {
            if (destinationSquare == null)
            {
                throw new ArgumentNullException("Не указано поле для хода.");
            }

            var board = destinationSquare.Board;

            lock (board.Locker)
            {
                if (movingPiece == null)
                {
                    throw new ArgumentNullException("Не указана фигура, делающая ход.");
                }

                if (!movingPiece.IsOnBoard)
                {
                    throw new ArgumentException("Указанная фигура не на доске.");
                }

                if (movingPiece.Board != board)
                {
                    throw new ArgumentException("Указаны фигура и поле на разных досках.");
                }

                if (board.Status != BoardStatus.GameIncomplete || movingPiece.Color != board.MoveTurn)
                {
                    throw new ArgumentException("Указанная фигура не может делать ходов.");
                }

                if (movingPiece.Square == destinationSquare)
                {
                    throw new ArgumentException("Фигура не может пойти на поле, на котором уже находится.");
                }

                if (destinationSquare.Contained?.Color == movingPiece.Color)
                {
                    throw new ArgumentException("Фигура не может пойти на поле, занятое фигурой того же цвета.");
                }

                MovingPieceName = movingPiece.Name;
                MovingPieceColor = movingPiece.Color;
                Start = movingPiece.SquareLocation;
                Destination = destinationSquare.Location;

                CheckWhetherIsCastling(board);
                IsEnPassantCapture = IsPawnMove && destinationSquare.IsPawnPassed && movingPiece.Attacks(destinationSquare);
                CapturedPieceName = IsEnPassantCapture ? PieceName.Pawn : destinationSquare.Contained?.Name;

                Depth = board.MovesCount + 1;
                Precedent = board.LastMove == null ? board.GameStartPosition : board.LastMove;
            }
        }

        public Move(ChessPiece movingPiece, Square destinationSquare, PieceName newPieceName) :
        this(movingPiece, destinationSquare)
        {
            if (!IsPawnMove)
            {
                throw new ArgumentException("Только пешка может превращаться в фигуру.");
            }            

            if (newPieceName == PieceName.King)
            {
                throw new ArgumentException("Пешка не может превращаться в короля.");
            }

            if (newPieceName == PieceName.Pawn)
            {
                throw new ArgumentException("Пешка не может превращаться в пешку.");
            }

            NewPieceName = newPieceName;
        }

        public static bool operator ==(Move move1, Move move2)
        {
            if (ReferenceEquals(move1, move2))
            {
                return true;
            }

            if (move1 is null || move2 is null)
            {
                return false;
            }

            return move1.EqualsInProperties(move2);
        }

        public static bool operator !=(Move move1, Move move2) => !(move1 == move2);

        private void CheckWhetherIsCastling(ChessBoard board)
        {
            if (MovingPieceName != PieceName.King)
            {
                return;
            }

            var horizontal = MovingPieceColor == PieceColor.White ? 0 : 7;

            if (Start.X != 4 || Start.Y != horizontal || Destination.Y != horizontal)
            {
                return;
            }

            if (Destination.X == 6)
            {
                var cornerPiece = board.GetPiece(7, horizontal);

                if (cornerPiece?.Name != PieceName.Rook || cornerPiece.Color != MovingPieceColor)
                {
                    return;
                }

                IsKingsideCastling = board[5, horizontal].IsClear && board[6, horizontal].IsClear;
                return;
            }

            if (Destination.X == 2)
            {
                var cornerPiece = board.GetPiece(0, horizontal);

                if (cornerPiece?.Name != PieceName.Rook || cornerPiece.Color != MovingPieceColor)
                {
                    return;
                }

                IsQueensideCastling = board[1, horizontal].IsClear && board[2, horizontal].IsClear &&
                board[3, horizontal].IsClear;
            }
        }

        private bool EqualsInProperties(Move other)
        {
            if (MovingPieceName != other.MovingPieceName || MovingPieceColor != other.MovingPieceColor)
            {
                return false;
            }

            if (Start != other.Start || Destination != other.Destination)
            {
                return false;
            }

            if (CapturedPieceName != other.CapturedPieceName || NewPieceName != other.NewPieceName)
            {
                return false;
            }

            if (IsEnPassantCapture != other.IsEnPassantCapture || IsKingsideCastling != other.IsKingsideCastling ||
                IsQueensideCastling != other.IsQueensideCastling)
            {
                return false;
            }

            if (Depth != other.Depth)
            {
                return false;
            }

            if (Depth == 1)
            {
                return (GamePosition)Precedent == (GamePosition)other.Precedent;
            }

            return PrecedingMove == other.PrecedingMove;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null || obj is not Move)
            {
                return false;
            }

            return EqualsInProperties((Move)obj);
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

        public bool IsPawnMove => MovingPieceName == PieceName.Pawn;

        public bool IsPawnJump => IsPawnMove && (Start.Y - Destination.Y > 1 ||
        Destination.Y - Start.Y > 1);

        public bool IsPawnPromotion => NewPieceName != null;

        public bool IsCapture => CapturedPieceName != null;

        public bool IsCastling => IsKingsideCastling || IsQueensideCastling;

        public Move PrecedingMove => Precedent as Move;
    }
}