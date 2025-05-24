using Chess.StrategicPart;
using Chess.TacticalPart;
using Chess.ChessTree;

namespace Chess.VirtualPlayer
{
    public static class RobotsCreator
    {
        private static Func<ChessRobot>[] GetConstructFuncs() => new Func<ChessRobot>[] { GetRobot1 };        

        private static ChessRobot GetRobot1()
        {
            Func<Tree, IEnumerable<Node>> traverse = (tree) => TreeTraverse.TraverseEntirely(tree, 4, (node1, node2) => 0);
            Func<Tree, Node, int> dynamicEvaluation = (tree, node) => tree.Evaluate(node);
            Func<Node, bool> correctParentEvaluation = Tactics.CorrectParentEvaluation_1;
            Func<Tree, Func<Node, Node, bool>, Node> getBestMoveNode = Tactics.GetBestMoveNode_1;
            Func<Node_Type1, Node_Type1, bool> compareResultNodes = Tactics.CompareResultNodes_1;

            var boardPropNames = new string[] { "EvaluatePieceFunc", "EvaluatePositionFunc" };
            var boardPropValues = new object[] { PieceEvaluation.GetBasicValue, PositionEvaluation.EvaluatePosition_1 };

            return new ChessRobot<AnalysisBoard_Type1, Node_Type1>(traverse, dynamicEvaluation,
            correctParentEvaluation, getBestMoveNode, compareResultNodes, boardPropNames, boardPropValues);
        }

        public static ChessRobot GetRobot(int strengthLevel)
        {
            if (strengthLevel < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var funcs = GetConstructFuncs();

            if (strengthLevel >= funcs.Length)
            {
                throw new ArgumentOutOfRangeException($"Максимальный уровень силы - {funcs.Length - 1}.");
            }

            return funcs[strengthLevel].Invoke();
        }
    }
}
