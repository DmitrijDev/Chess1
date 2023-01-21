using Chess.LogicPart;

namespace Chess.Players
{
    public class VirtualPlayer
    {
        private readonly Func<ChessBoard, int[]> _selectMove;

        public VirtualPlayer(Func<ChessBoard, int[]> selectMove) => _selectMove = selectMove;

        public int[] SelectMove(ChessBoard board) => _selectMove(board);
    }
}
