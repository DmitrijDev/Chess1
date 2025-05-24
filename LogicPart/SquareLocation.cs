using System.Text;

namespace Chess.LogicPart
{
    public struct SquareLocation
    {
        public int X { get; }

        public int Y { get; }

        public SquareLocation(int x, int y)
        {
            if (x < 0 || y < 0 || x > 7 || y > 7)
            {
                throw new ArgumentOutOfRangeException();
            }

            X = x;
            Y = y;
        }

        public SquareLocation(string squareName)
        {
            if (squareName == null)
            {
                throw new ArgumentNullException();
            }

            var trimmedName = TrimAndToLower(squareName);

            if (trimmedName.Length != 2)
            {
                throw new ArgumentException("Указана строка, не явл. именем поля доски.");
            }

            int? x = null;
            var verticalNames = "abcdefgh";

            for (var i = 0; i < verticalNames.Length; ++i)
            {
                if (verticalNames[i] == trimmedName[0])
                {
                    x = i;
                    break;
                }
            }

            if (x == null)
            {
                throw new ArgumentException("Указана строка, не явл. именем поля доски.");
            }

            int? y = null;
            var horizontalIndices = "12345678";

            for (var i = 0; i < horizontalIndices.Length; ++i)
            {
                if (horizontalIndices[i] == squareName[1])
                {
                    y = i;
                    break;
                }
            }

            if (y == null)
            {
                throw new ArgumentException("Указана строка, не явл. именем поля доски.");
            }

            X = (int)x;
            Y = (int)y;
        }

        public static bool operator ==(SquareLocation first, SquareLocation second) => first.X == second.X && first.Y == second.Y;        

        public static bool operator !=(SquareLocation first, SquareLocation second) => !(first == second);        

        private static string TrimAndToLower(string str)
        {
            var result = new StringBuilder();

            foreach (var ch in str)
            {
                if (ch != ' ')
                {
                    result.Append(char.ToLower(ch));
                }
            }

            return result.ToString();
        }

        public bool Corresponds(int x, int y) => X == x && Y == y;

        public bool IsOnSameDiagonal(SquareLocation other) => Math.Abs(X - other.X) == Math.Abs(Y - other.Y);        
    }
}