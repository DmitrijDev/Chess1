using Chess.StrategicPart;
using Chess.TacticalPart;

namespace Chess.VirtualPlayer
{
    public static class RobotsConstructor
    {
        public static IChessRobot GetRobot(int strengthLevel)
        {
            if (strengthLevel < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var robots = GetAllRobots();

            if (strengthLevel >= robots.Length)
            {
                throw new ArgumentOutOfRangeException($"Максимальный уровень силы - {robots.Length - 1}.");
            }

            return robots[strengthLevel];
        }

        private static IChessRobot[] GetAllRobots() => new IChessRobot[] { GetRobot1() };

        private static ChessRobot<AnalysisBoard_Type1> GetRobot1() =>
        new(TreeTraverse.Traverse_1, (node, tree) => tree.Evaluate(node), Tactics.CorrectParentEvaluation, Tactics.GetBestMoveNode,
        new string[] { "EvaluatePieceFunc", "EvaluatePositionFunc" },
        new Delegate[] { PieceEvaluation.GetBasicValue, PositionEvaluation.EvaluatePosition_1 });
    }
}
