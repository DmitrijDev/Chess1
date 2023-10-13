using Chess.LogicPart;
using Chess.TreesOfAnalysis;

namespace Chess.StrategicPart
{
    public static class Strategy
    {
        public static int CompareTo(this GamePosition position1, GamePosition position2)
        {
            if (position1.MovingSideColor == ChessPieceColor.White)
            {
                if (position2.MovingSideColor == ChessPieceColor.Black)
                {
                    return -1;
                }
            }
            else
            {
                if (position2.MovingSideColor == ChessPieceColor.White)
                {
                    return 1;
                }
            }

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (position1.GetPieceColor(i, j) != null)
                    {
                        if (position2.GetPieceColor(i, j) == null)
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        if (position2.GetPieceColor(i, j) == null)
                        {
                            continue;
                        }
                        else
                        {
                            return -1;
                        }
                    }

                    if (position1.GetPieceColor(i, j) == ChessPieceColor.White)
                    {
                        if (position2.GetPieceColor(i, j) == ChessPieceColor.Black)
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        if (position2.GetPieceColor(i, j) == ChessPieceColor.White)
                        {
                            return 1;
                        }
                    }

                    if (position1.GetPieceName(i, j) != position2.GetPieceName(i, j))
                    {
                        return (int)position1.GetPieceName(i, j) - (int)position2.GetPieceName(i, j);
                    }
                }
            }

            return 0;
        }

        public static Tree BuildTree(ChessBoard board) => new Tree(board, 4);

        public static IEnumerable<TreeNode[]> Traverse(this Tree tree) => tree.GetGameLines();

        public static int EvaluatePiece(this GamePosition position, int vertical, int horizontal)
        {
            var pieceName = position.GetPieceName(vertical, horizontal);

            var result = pieceName switch
            {
                ChessPieceName.Pawn => 100,
                ChessPieceName.Knight => 300,
                ChessPieceName.Bishop => 300,
                ChessPieceName.Rook => 500,
                ChessPieceName.Queen => 900,
                ChessPieceName.King => throw new ApplicationException("Короля невозможно оценить в баллах."),
                _ => throw new ApplicationException("Указано пустое поле.")
            };

            if (position.GetPieceColor(vertical, horizontal) == ChessPieceColor.Black)
            {
                result = -result;
            }

            return result;
        }

        public static int Evaluate(this Tree tree, TreeNode node, Func<GamePosition, int, int, int> evaluatePiece)
        {
            if (tree.EndsGame(node, out var gameResult))
            {
                return gameResult switch
                {
                    BoardStatus.WhiteWin => int.MaxValue,
                    BoardStatus.BlackWin => -int.MaxValue,
                    _ => 0
                };
            }

            var position = tree.GetPosition(node);
            var evaluation = 0;

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    var pieceName = position.GetPieceName(i, j);

                    if (pieceName != null && pieceName != ChessPieceName.King)
                    {
                        evaluation += evaluatePiece(position, i, j);
                    }
                }
            }            

            return evaluation;
        }
    }
}
