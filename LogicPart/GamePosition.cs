
namespace Chess.LogicPart
{
    public class GamePosition
    {
        private readonly PieceName?[,] _pieceNames = new PieceName?[8, 8];
        private readonly PieceColor?[,] _pieceColors = new PieceColor?[8, 8];

        public PieceColor MovingSideColor { get; private set; }

        public GamePosition(ChessBoard board)
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _pieceNames[i, j] = board[i, j].IsEmpty ? null : board[i, j].ContainedPiece.Name;
                    _pieceColors[i, j] = board[i, j].IsEmpty ? null : board[i, j].ContainedPiece.Color;
                }
            }

            MovingSideColor = board.MovingSide.Color;
        }

        public GamePosition(GamePosition sourcePosition)
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _pieceNames[i, j] = sourcePosition._pieceNames[i, j];
                    _pieceColors[i, j] = sourcePosition._pieceColors[i, j];
                }
            }

            MovingSideColor = sourcePosition.MovingSideColor;
        }

        public PieceName? GetPieceName(int vertical, int horizontal)
        {
            if (vertical < 0 || horizontal < 0 || vertical >= 8 || horizontal >= 8)
            {
                throw new IndexOutOfRangeException("Поля с указанными координатами не существует.");
            }

            return _pieceNames[vertical, horizontal];
        }

        public PieceColor? GetPieceColor(int vertical, int horizontal)
        {
            if (vertical < 0 || horizontal < 0 || vertical >= 8 || horizontal >= 8)
            {
                throw new IndexOutOfRangeException("Поля с указанными координатами не существует.");
            }

            return _pieceColors[vertical, horizontal];
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
            {
                return true;
            }

            if (obj is null || obj.GetType() != GetType())
            {
                return false;
            }

            var otherPosition = (GamePosition)obj;

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