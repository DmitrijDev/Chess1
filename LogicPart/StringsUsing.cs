using System.Text;

namespace Chess.LogicPart
{
    internal static class StringsUsing
    {
        public static int[] GetChessSquareCoordinates(string squareName)
        {
            if (squareName == null)
            {
                throw new ArgumentNullException(squareName);
            }

            var trimmedName = TrimAndLower(squareName);

            if (trimmedName.Length != 2)
            {
                throw new ArgumentException(squareName);
            }

            var vertical = -1;
            const string verticalNames = "abcdefgh";

            for (var i = 0; i < verticalNames.Length; ++i)
            {
                if (verticalNames[i] == trimmedName[0])
                {
                    vertical = i;
                    break;
                }
            }

            if (vertical == -1)
            {
                throw new ArgumentException(squareName);
            }

            var horizontal = -1;
            const string horizontalIndices = "12345678";

            for (var i = 0; i < horizontalIndices.Length; ++i)
            {
                if (horizontalIndices[i] == squareName[1])
                {
                    horizontal = i;
                    break;
                }
            }

            if (horizontal == -1)
            {
                throw new ArgumentException(squareName);
            }

            return new int[2] { vertical, horizontal };
        }

        private static string TrimAndLower(string s)
        {
            if (s == "")
            {
                return s;
            }

            var result = new StringBuilder();

            foreach (var c in s)
            {
                if (c != ' ')
                {
                    result.Append(char.ToLower(c));
                }
            }

            return result.ToString();
        }

        public static string GetChessSquareName(int squareVertical, int squareHorizontal)
        {
            const string verticalNames = "abcdefgh";
            return new StringBuilder().Append(verticalNames[squareVertical]).Append(squareHorizontal + 1).ToString();
        }
    }
}
