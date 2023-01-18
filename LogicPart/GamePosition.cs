
namespace Chess.LogicPart
{
    public class GamePosition
    {
        private readonly int[,] _board = new int[8, 8];

        public PieceColor MovingSideColor { get; private set; }

        public GamePosition(ChessBoard board)
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _board[i, j] = board[i, j].IsEmpty ? 0 : board[i, j].ContainedPiece.NumeralIndex;
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
                    _board[i, j] = sourcePosition._board[i, j];
                }
            }

            MovingSideColor = sourcePosition.MovingSideColor;
        }

        public int this[int vertical, int horizontal]
        {
            get
            {
                if (vertical < 0 || horizontal < 0 || vertical >= 8 || horizontal >= 8)
                {
                    throw new IndexOutOfRangeException("Поля с указанными координатами не существует.");
                }

                return _board[vertical, horizontal];
            }
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

            var otherPosition = (GamePosition) obj;

            if (otherPosition.MovingSideColor != MovingSideColor)
            {
                return false;
            }

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (otherPosition._board[i, j] != _board[i, j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}