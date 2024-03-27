using Chess.StrategicPart;
using Chess.TacticalPart;

namespace Chess.VirtualPlayer
{
    internal class SourceRobotsProvider
    {
        public SourceRobotsProvider()
        { }

        public IChessRobot[] GetSourceRobots() => new IChessRobot[] { Robot0 };

        public ChessRobot<MaterialEvaluatingBoard> Robot0 => new(board => board.EvaluatePiece = PieceEvaluation.GetBasicValue, tree => tree.TraverseAtDepth(4),
                PositionEvaluation.EvaluatePosition_1, Strategy.CorrectParentEvaluation, Strategy.GetBestMoveNode);
    }
}
