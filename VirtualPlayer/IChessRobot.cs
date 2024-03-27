
using Chess.LogicPart;

namespace Chess.VirtualPlayer
{
    public interface IChessRobot
    {
        bool ThinkingDisabled { get; set; }

        Move SelectMove(ChessBoard board);

        IChessRobot Copy();
    }
}
