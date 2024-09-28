using System.Text;
using Chess.LogicPart;

namespace Chess
{
    internal static class ChessGameText
    {
        public static string GetShortName(this PieceName pieceName) => pieceName switch
        {
            PieceName.King => "Кр",
            PieceName.Queen => "Ф",
            PieceName.Rook => "Л",
            PieceName.Knight => "К",
            PieceName.Bishop => "С",
            _ => ""
        };

        public static string GetSquareName(int x, int y)
        {
            var verticalNames = "abcdefgh";
            return new StringBuilder().Append(verticalNames[x]).Append(y + 1).ToString();
        }

        public static string GetSquareName(this SquareLocation location) => GetSquareName(location.X, location.Y);

        public static string Write(this Move move)
        {
            if (move.IsKingsideCastling)
            {
                return "0 - 0";
            }

            if (move.IsQueensideCastling)
            {
                return "0 - 0 - 0";
            }

            var builder = new StringBuilder(move.MovingPieceName.GetShortName()).Append(move.Start.GetSquareName()).
            Append(move.IsCapture ? " : " : " - ").Append(move.Destination.GetSquareName());

            if (move.IsEnPassantCapture)
            {
                builder.Append(' ').Append("ep");
            }

            if (move.IsPawnPromotion)
            {
                var newPieceName = (PieceName)move.NewPieceName;
                builder.Append(newPieceName.GetShortName());
            }

            return builder.ToString();
        }

        public static string GetGameText(this ChessBoard board)
        {
            if (board.MovesCount == 0)
            {
                return "";
            }

            var gameText = new StringBuilder();
            var gameOver = board.Status != BoardStatus.GameIncomplete;
            var lastMove = board.LastMove;
            var linesCount = board.MovesCount / 2 + (board.MovesCount % 2) + (lastMove.MovingPieceColor == PieceColor.Black && gameOver ? 1 : 0);
            var indexColumnBreadth = linesCount.ToString().Length;
            var lineIndex = 1;

            foreach (var move in lastMove.GetPrecedingMoves().Reverse().Append(lastMove))
            {
                if (move.MovingPieceColor == PieceColor.White || move.Depth == 1)
                {
                    gameText.Append(lineIndex.ToString().PadLeft(indexColumnBreadth, ' ')).Append('.');

                    if (move.MovingPieceColor == PieceColor.Black)
                    {
                        gameText.Append("...".PadLeft(13, ' '));
                    }
                }

                var moveText = move.Write();
                var leftSpaceLength = move.IsCastling ? 3 : 3 - move.MovingPieceName.GetShortName().Length;
                moveText = moveText.PadLeft(moveText.Length + leftSpaceLength, ' ');
                moveText = moveText.PadRight(13, ' ');
                gameText.Append(moveText);

                if (move.MovingPieceColor == PieceColor.Black && move != lastMove)
                {
                    gameText.Append(Environment.NewLine);
                    ++lineIndex;
                }
            }

            if (!gameOver)
            {
                return gameText.ToString();
            }

            if (lastMove.MovingPieceColor == PieceColor.Black)
            {
                ++lineIndex;
                gameText.Append(Environment.NewLine).Append(lineIndex).Append('.');
            }

            var gameResult = board.Status switch
            {
                BoardStatus.WhiteWon => "1 - 0",
                BoardStatus.BlackWon => "0 - 1",
                _ => "1/2 - 1/2"
            };

            gameText.Append(gameResult.PadLeft(gameResult.Length + 3, ' '));
            return gameText.ToString();
        }
    }
}
