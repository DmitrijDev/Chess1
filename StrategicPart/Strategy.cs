using Chess.LogicPart;

namespace Chess.StrategicPart
{
    public static class Strategy
    {
        public static bool ThinkingDisabled { get; set; } // Партия может быть прервана, пока программа думает, тогда ей нужно перестать думать.

        public static VirtualPlayer Player1 { get; } = new VirtualPlayer(AnalyzeByPlayer1, EvaluatePositionByPlayer1);
        
        public static short EvaluatePositionByPlayer1(ChessBoard board)
        {
            if (board == null || board.Status == GameStatus.IllegalPosition || board.Status == GameStatus.ClearBoard)
            {
                throw new ArgumentException("Некорректный аргумент.");
            }

            if (board.Status != GameStatus.GameIsNotOver)
            {
                return board.Status switch
                {
                    GameStatus.WhiteWin => short.MaxValue,
                    GameStatus.BlackWin => -short.MaxValue,
                    _ => 0
                };
            }

            short result = 0;

            foreach (var piece in board.GetMaterial())
            {                
                short pieceEvaluation = piece.Name switch
                {
                    ChessPieceName.Pawn => 10,
                    ChessPieceName.Knight => 30,
                    ChessPieceName.Bishop => 30,
                    ChessPieceName.Rook => 45,
                    ChessPieceName.Queen => 90,
                    _ => 0
                };

                if (piece.Color == ChessPieceColor.Black)
                {
                    pieceEvaluation = (short)-pieceEvaluation;
                }

                result += pieceEvaluation;
            }

            return result;
        }

        internal static void AnalyzeByPlayer1(PositionTree tree)
        {
            if (tree.Count > 1)
            {
                tree.CancelAnalysis();
            }

            var depth = 2;

            while (tree.CanContinueAnalysis() && tree.GetFullEnumerationDepth() < depth)
            {
                if (tree.CurrentNodeDepth >= depth)
                {
                    tree.EvaluateCurrentPosition();
                    tree.MoveBack();
                    continue;
                }

                if (tree.MoveForward()) 
                {
                    continue;
                }

                tree.MoveBack();
            }
        }
    }
}
