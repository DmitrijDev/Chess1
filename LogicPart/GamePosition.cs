
namespace Chess.LogicPart
{
    public class GamePosition
    {
        private readonly ChessPieceName?[,] _pieceNames = new ChessPieceName?[8, 8];
        private readonly ChessPieceColor?[,] _pieceColors = new ChessPieceColor?[8, 8];

        public ChessPieceColor MovingSideColor { get; private set; }

        public GamePosition(ChessBoard board)
        {
            lock (board)
            {
                for (var i = 0; i < 8; ++i)
                {
                    for (var j = 0; j < 8; ++j)
                    {
                        if (!board[i, j].IsEmpty)
                        {
                            _pieceNames[i, j] = board[i, j].ContainedPiece.Name;
                            _pieceColors[i, j] = board[i, j].ContainedPiece.Color;
                        }
                    }
                }

                MovingSideColor = board.MovingSideColor;
            }
        }

        public GamePosition(GamePosition other)
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _pieceNames[i, j] = other._pieceNames[i, j];
                    _pieceColors[i, j] = other._pieceColors[i, j];
                }
            }

            MovingSideColor = other.MovingSideColor;
        }

        public ChessPieceName? GetPieceName(int vertical, int horizontal) => _pieceNames[vertical, horizontal];

        public ChessPieceColor? GetPieceColor(int vertical, int horizontal) => _pieceColors[vertical, horizontal];

        internal void ToPreceding(Move lastMove)
        {
            _pieceNames[lastMove.StartSquare.Vertical, lastMove.StartSquare.Horizontal] = lastMove.MovingPiece.Name;
            _pieceColors[lastMove.StartSquare.Vertical, lastMove.StartSquare.Horizontal] = lastMove.MovingPiece.Color;

            _pieceNames[lastMove.MoveSquare.Vertical, lastMove.MoveSquare.Horizontal] = null;
            _pieceColors[lastMove.MoveSquare.Vertical, lastMove.MoveSquare.Horizontal] = null;

            if (lastMove.IsCastleKingside)
            {
                _pieceNames[5, lastMove.StartSquare.Horizontal] = null;
                _pieceColors[5, lastMove.StartSquare.Horizontal] = null;

                _pieceNames[7, lastMove.StartSquare.Horizontal] = ChessPieceName.Rook;
                _pieceColors[7, lastMove.StartSquare.Horizontal] = lastMove.MovingPiece.Color;
            }
            else if (lastMove.IsCastleQueenside)
            {
                _pieceNames[3, lastMove.StartSquare.Horizontal] = null;
                _pieceColors[3, lastMove.StartSquare.Horizontal] = null;

                _pieceNames[0, lastMove.StartSquare.Horizontal] = ChessPieceName.Rook;
                _pieceColors[0, lastMove.StartSquare.Horizontal] = lastMove.MovingPiece.Color;
            }

            MovingSideColor = MovingSideColor == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
        }

        public bool IsEqualTo(GamePosition other)
        {
            if (MovingSideColor != other.MovingSideColor)
            {
                return false;
            }

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (_pieceNames[i, j] != other._pieceNames[i, j] || _pieceColors[i, j] != other._pieceColors[i, j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}