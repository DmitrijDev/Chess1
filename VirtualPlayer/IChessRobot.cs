using Chess.LogicPart;

namespace Chess.VirtualPlayer
{
    public interface IChessRobot
    {
        Move GetMove(ChessBoard board);
    }
}
