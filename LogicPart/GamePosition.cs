
namespace Chess.LogicPart
{
    public class GamePosition
    {
        public int[,] Board { get; } = new int[8, 8];

        public PieceColor MovingSide { get; set; }

        public GamePosition()
        { }

        public GamePosition(ChessBoard board)
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    Board[i, j] = board[i, j].IsEmpty ? 0 : board[i, j].ContainedPiece.NumeralIndex;
                }
            }

            MovingSide = board.MovingSide.Color;
        }

        public int this[int vertical, int horizontal]
        {
            get
            {
                if (vertical < 0 || horizontal < 0 || vertical >= 8 || horizontal >= 8)
                {
                    throw new IndexOutOfRangeException("Поля с указанными координатами не существует.");
                }

                return Board[vertical, horizontal];
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

            var otherPosition = (GamePosition)obj;

            if (otherPosition.MovingSide != MovingSide)
            {
                return false;
            }

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (otherPosition.Board[i, j] != Board[i, j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}