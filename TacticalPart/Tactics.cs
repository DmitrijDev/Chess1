using Chess.ChessTree;
using Chess.LogicPart;

namespace Chess.TacticalPart
{
    public static class Tactics
    {
        public static bool CorrectParentEvaluation_1(this Node child)
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

            var movingPieceColor = ((Node_Type1)child).MovingPieceColor;

            if ((movingPieceColor == PieceColor.White && child.Evaluation > parent.Evaluation) ||
                (movingPieceColor == PieceColor.Black && child.Evaluation < parent.Evaluation))
            {
                parent.Evaluation = child.Evaluation;
                return true;
            }

            var evaluations = parent.GetChildren().Where(node => node.IsEvaluated).Select(node => node.Evaluation);
            var newEvaluation = movingPieceColor == PieceColor.White ? evaluations.Max() : evaluations.Min();

            if (parent.Evaluation == newEvaluation)
            {
                return false;
            }

            parent.Evaluation = newEvaluation;
            return true;
        }

        public static bool CompareResultNodes_1(Node_Type1 node1, Node_Type1 node2)
        {           
            if (node1.Evaluation == node2.Evaluation)
            {
                return new Random().Next(2) == 0;
            }

            if (node1.MovingPieceColor == PieceColor.White)
            {
                return node1.Evaluation > node2.Evaluation;
            }
            else
            {
                return node1.Evaluation < node2.Evaluation;
            }
        }

        public static Node GetBestMoveNode_1(Tree tree, Func<Node, Node, bool> isBetter)
        {
            var evaluatedNodes = tree.Root.GetChildren().Where(node => node.IsEvaluated).ToList();

            if (evaluatedNodes.Count == 0)
            {
                return null;
            }

            var node = evaluatedNodes[0];

            for (; ; )
            {
                if (!node.HasChildren)
                {
                    break;
                }

                if (node.GetChildren().First().IsEvaluated)
                {
                    node = node.GetChildren().First();
                    continue;
                }

                if (!node.GetChildren().Any(child => tree.CheckmatesWith(child)))
                {
                    evaluatedNodes.RemoveAt(0);

                    if (evaluatedNodes.Count == 0)
                    {
                        return null;
                    }
                }

                break;
            }

            if (evaluatedNodes.Count == 1)
            {
                return evaluatedNodes[0];
            }

            var result = evaluatedNodes[0];

            foreach(var n in evaluatedNodes.Skip(1))
            {                
                if (isBetter(n, result))
                {
                    result = n;
                }
            }

            return result;
        }
    }
}
