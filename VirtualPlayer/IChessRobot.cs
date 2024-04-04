using Chess.ChessTree;
using Chess.LogicPart;

namespace Chess.VirtualPlayer
{
    public interface IChessRobot
    {
        Func<IChessTree, IEnumerable<Node>> Traverse { get; }

        Func<Node, IChessTree, int> Evaluate { get; }

        Func<Node, bool> CorrectParentEvaluation { get; }

        Func<IChessTree, Node> GetBestMoveNode { get; }

        bool ThinkingDisabled { get; set; }

        IChessRobot Copy();

        Move SelectMove(ChessBoard board);        
    }
}
