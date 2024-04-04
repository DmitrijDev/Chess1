using Chess.StrategicPart;
using Chess.TacticalPart;

namespace Chess.VirtualPlayer
{
    internal class SourceRobotsProvider
    {
        public SourceRobotsProvider()
        { }

        public IChessRobot[] GetSourceRobots() => new IChessRobot[] { Robot1 };

        public ChessRobot<MaterialCheckingBoard> Robot1 => new(TreeTraverse.Traverse_1, (node, tree) => tree.Evaluate(node),
        Tactics.CorrectParentEvaluation, Tactics.GetBestMoveNode, new string[] { "EvaluatePieceFunc", "EvaluatePositionFunc" },
        new Delegate[] { PieceEvaluation.GetBasicValue, PositionEvaluation.EvaluatePosition_1 });
    }
}
