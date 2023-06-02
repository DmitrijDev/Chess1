
namespace Chess.LogicPart
{
    public class GamePosition
    {
        private readonly ChessPieceName?[,] _pieceNames = new ChessPieceName?[8, 8];
        private readonly ChessPieceColor?[,] _pieceColors = new ChessPieceColor?[8, 8];

        public ChessPieceColor MovingSideColor { get; }

        public GamePosition(ChessBoard board)
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

        public ChessPieceName? GetPieceName(int vertical, int horizontal) => _pieceNames[vertical, horizontal];

        public ChessPieceColor? GetPieceColor(int vertical, int horizontal) => _pieceColors[vertical, horizontal];

        public bool IsEqualTo(GamePosition otherPosition)
        {
            if (otherPosition.MovingSideColor != MovingSideColor)
            {
                return false;
            }

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (otherPosition._pieceNames[i, j] != _pieceNames[i, j] || otherPosition._pieceColors[i, j] != _pieceColors[i, j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }        
    }
}