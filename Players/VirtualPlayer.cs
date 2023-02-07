using Chess.LogicPart;

namespace Chess.Players
{
    public class VirtualPlayer
    {
        public Func<ChessBoard, string[]> SelectMove { get; private set; }

        public VirtualPlayer(Func<ChessBoard, string[]> selectMove) => SelectMove = selectMove;        
    }
}
