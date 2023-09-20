
namespace Chess.LogicPart
{
    public class GamePosition
    {
        private readonly ChessPieceName?[,] _pieceNames = new ChessPieceName?[8, 8];
        private readonly ChessPieceColor?[,] _pieceColors = new ChessPieceColor?[8, 8];

        public ChessPieceColor MovingSideColor { get; }

        internal GamePosition(ChessBoard board)
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

        public bool IsEqualTo(GamePosition other)
        {
            if (other.MovingSideColor != MovingSideColor)
            {
                return false;
            }

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (other._pieceNames[i, j] != _pieceNames[i, j] || other._pieceColors[i, j] != _pieceColors[i, j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}