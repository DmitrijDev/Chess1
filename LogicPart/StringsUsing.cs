using System.Text;

namespace Chess.LogicPart
{
    internal static class StringsUsing
    {
        public static int[] GetSquareCoordinates(string squareName)
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

        public static string GetSquareName(int squareVertical, int squareHorizontal)
        {
            const string verticalNames = "abcdefgh";
            return new StringBuilder().Append(verticalNames[squareVertical]).Append(squareHorizontal + 1).ToString();
        }

        public static string GetName(this Square square) => GetSquareName(square.Vertical, square.Horizontal);

        public static string GetShortName(this ChessPiece piece) => piece.Name switch
        {
            ChessPieceName.King => "Кр",
            ChessPieceName.Queen => " Ф",
            ChessPieceName.Rook => " Л",
            ChessPieceName.Knight => " К",
            ChessPieceName.Bishop => " С",
            _ => "  "
        };

        public static string Write(this Move move)
        {
            if (move.IsCastleKingside)
            {
                return "   0 - 0   ";
            }

            if (move.IsCastleQueenside)
            {
                return " 0 - 0 - 0 ";
            }

            var result = new StringBuilder(move.MovingPiece.GetShortName()).Append(move.StartSquare.GetName()).
                Append(move.IsCapture ? " : " : " - ").Append(move.MoveSquare.GetName());

            if (move.IsEnPassantCapture)
            {
                result.Append(" ep");
            }

            if (move.NewPieceSelected)
            {
                result.Append(move.NewPiece.GetShortName());
            }

            return result.ToString();
        }

        public static string GetGameText(ChessBoard board)
        {
            lock (board)
            {
                if (board.MovesCount == 0)
                {
                    return "";
                }

                var gameText = new StringBuilder();
                var lastMove = board.GetLastMove();
                var gameOver = board.Status == BoardStatus.WhiteWin || board.Status == BoardStatus.BlackWin || board.Status == BoardStatus.Draw;
                var lineIndex = 0;
                var lastLineIndex = board.MovesCount / 2 + (board.MovesCount % 2);

                if (lastMove.MovingPiece.Color == ChessPieceColor.Black && gameOver)
                {
                    ++lastLineIndex;
                }

                foreach (var move in lastMove.GetPrecedingMoves().Reverse().Append(lastMove))
                {
                    if (move.MovingPiece.Color == ChessPieceColor.White || move.Depth == 1)
                    {
                        ++lineIndex;
                        var spacesCount = GetDigitsCount(lastLineIndex) - GetDigitsCount(lineIndex);

                        for (var i = 0; i < spacesCount; ++i)
                        {
                            gameText.Append(' ');
                        }

                        gameText.Append(lineIndex).Append(". ");

                        if (move.MovingPiece.Color == ChessPieceColor.Black)
                        {
                            gameText.Append("...        ");
                        }
                    }

                    var moveText = move.Write();
                    gameText.Append(moveText);

                    if (move.MovingPiece.Color == ChessPieceColor.White)
                    {
                        var spacesCount = 13 - moveText.Length;

                        for (var i = 0; i < spacesCount; ++i)
                        {
                            gameText.Append(' ');
                        }
                    }
                    else
                    {
                        gameText.Append(Environment.NewLine);
                    }
                }

                if (!gameOver)
                {
                    return gameText.ToString();
                }

                if (lastMove.MovingPiece.Color == ChessPieceColor.Black)
                {
                    gameText.Append(lastLineIndex).Append(". ");
                }

                var gameResult = board.Status switch
                {
                    BoardStatus.WhiteWin => "   1 - 0   ",
                    BoardStatus.BlackWin => "   0 - 1   ",
                    _ => " 1/2 - 1/2 "
                };

                gameText.Append(gameResult);
                return gameText.ToString();
            }
        }

        private static int GetDigitsCount(int number)
        {
            var remained = number;
            var count = 0;

            while (remained > 0)
            {
                remained /= 10;
                ++count;
            }

            return count;
        }
    }
}
