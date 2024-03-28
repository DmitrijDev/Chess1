using Chess.ChessTree;
using Chess.LogicPart;

namespace Chess.StrategicPart
{
    public static class Strategy
    {
        public static bool CorrectParentEvaluation(this Node child)
        {
            var parent = child.Parent;

            if (parent == null)
            {
                return false;
            }

            if (!parent.IsEvaluated)
            {
                parent.Evaluation = child.Evaluation;
                return true;
            }

            if (parent.Evaluation == child.Evaluation)
            {
                return false;
            }

            if ((child.MovingPieceColor == ChessPieceColor.White && child.Evaluation > parent.Evaluation) ||
                (child.MovingPieceColor == ChessPieceColor.Black && child.Evaluation < parent.Evaluation))
            {
                parent.Evaluation = child.Evaluation;
                return true;
            }

            var evaluations = parent.GetChildren().Where(node => node.IsEvaluated).Select(node => node.Evaluation);
            var newEvaluation = child.MovingPieceColor == ChessPieceColor.White ? evaluations.Max() : evaluations.Min();

            if (parent.Evaluation == newEvaluation)
            {
                return false;
            }

            parent.Evaluation = newEvaluation;
            return true;
        }

        public static Node GetBestMoveNode(this IChessTree tree)
        {
            Node result = null;
            var rand = new Random();

            foreach (var node in tree.Root.GetChildren())
            {
                if (result == null)
                {
                    result = node;
                    continue;
                }

                if (!node.IsEvaluated)
                {
                    continue;
                }

                if (!result.IsEvaluated)
                {
                    result = node;
                    continue;
                }

                if (node.Evaluation == result.Evaluation)
                {
                    var r = rand.Next(2);

                    if (r == 0)
                    {
                        result = node;
                    }

                    continue;
                }

                if (node.MovingPieceColor == ChessPieceColor.White)
                {
                    if (node.Evaluation > result.Evaluation)
                    {
                        result = node;

                    }
                }
                else
                {
                    if (node.Evaluation < result.Evaluation)
                    {
                        result = node;
                    }
                }
            }

            return result;
        }
    }
}
