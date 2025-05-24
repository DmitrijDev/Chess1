
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

        public bool IsCastling { get; }

        public bool IsEnPassantCapture { get; }

        public int Depth { get; }

        internal object Precedent { get; }

        public Move(ChessPiece movingPiece, Square moveSquare)
        {
            if (moveSquare == null)
            {
                throw new ArgumentNullException("Не указано поле для хода.");
            }

            var board = moveSquare.Board;

            lock (board.Locker)
            {
                if (movingPiece == null)
                {
                    throw new ArgumentNullException("Не указана фигура, делающая ход.");
                }

                if (movingPiece.Board != board)
                {
                    throw new ArgumentException("Указаны фигура и поле не на одной доске.");
                }

                if (board.Status != BoardStatus.GameIncomplete || movingPiece.Color != board.MoveTurn)
                {
                    throw new ArgumentException("Указанная фигура не может делать ходов.");
                }

                if (moveSquare.Contained?.Color == movingPiece.Color)
                {
                    if (movingPiece.Square == moveSquare)
                    {
                        throw new ArgumentException("Фигура не может пойти на поле, на котором уже находится.");
                    }

                    throw new ArgumentException("Фигура не может пойти на поле, занятое фигурой того же цвета.");
                }

                MovingPieceName = movingPiece.Name;
                MovingPieceColor = movingPiece.Color;
                Start = movingPiece.Location;
                Destination = moveSquare.Location;
                IsCastling = IsCastlingMove(movingPiece, moveSquare);
                IsEnPassantCapture = IsPawnMove && moveSquare.IsPawnPassed && movingPiece.Attacks(moveSquare);
                CapturedPieceName = IsEnPassantCapture ? PieceName.Pawn : moveSquare.Contained?.Name;
                Depth = board.MovesCount + 1;
                Precedent = board.LastMove == null ? board.GameStartPosition : board.LastMove;
            }
        }

        public Move(Pawn pawn, Square promotionSquare, PieceName newPieceName) :
        this(pawn, promotionSquare)
        {
            if ((MovingPieceColor == PieceColor.White && Destination.Y != 7) ||
                (MovingPieceColor == PieceColor.Black && Destination.Y != 0))
            {
                throw new ArgumentException("Белые пешки превращаются только на 8-й горизонтали, черные - на 1-й.");
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

        internal static bool IsCastlingMove(ChessPiece movingPiece, Square moveSquare)
        {
            if (movingPiece.Name != PieceName.King || !moveSquare.IsClear)
            {
                return false;
            }

            var horizontal = movingPiece.Color == PieceColor.White ? 0 : 7;

            if (movingPiece.X != 4 || movingPiece.Y != horizontal || moveSquare.Y != horizontal)
            {
                return false;
            }

            var board = moveSquare.Board;

            if (moveSquare.X == 6)
            {
                var cornerPiece = board.GetPiece(7, horizontal);

                if (cornerPiece?.Name != PieceName.Rook || cornerPiece.Color != movingPiece.Color)
                {
                    return false;
                }

                return board[5, horizontal].IsClear;
            }

            if (moveSquare.X == 2)
            {
                var cornerPiece = board.GetPiece(0, horizontal);

                if (cornerPiece?.Name != PieceName.Rook || cornerPiece.Color != movingPiece.Color)
                {
                    return false;
                }

                return board[1, horizontal].IsClear && board[3, horizontal].IsClear;
            }

            return false;
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

            if (IsCastling != other.IsCastling || IsEnPassantCapture != other.IsEnPassantCapture)
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

        public override bool Equals(object? obj)
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
            if (Depth == 1)
            {
                yield break;
            }

            var move = PrecedingMove;

            for (; ; )
            {
                yield return move;

                if (move.Depth == 1)
                {
                    yield break;
                }

                move = move.PrecedingMove;
            }
        }

        public GamePosition GetGameStartPosition()
        {
            if (Depth == 1)
            {
                return (GamePosition)Precedent;
            }

            return (GamePosition)GetPrecedingMoves().Last().Precedent;
        }

        public bool IsPawnMove => MovingPieceName == PieceName.Pawn;

        public bool IsPawnPromotion => NewPieceName != null;

        public bool IsCapture => CapturedPieceName != null;

        public Move PrecedingMove => Precedent as Move;
    }
}