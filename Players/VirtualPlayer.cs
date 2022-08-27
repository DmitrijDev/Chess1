using Chess.LogicPart;

namespace Chess.Players
{
    public class VirtualPlayer
    {
        private readonly Func<ChessBoard, int[]> _selectMove;

        public ChessBoard Board { get; set; }

        public VirtualPlayer(Func<ChessBoard, int[]> selectMove) => _selectMove = selectMove;

        public void SetBoard(ChessBoard board) => Board = board;

        public int[] SelectMove() => _selectMove(Board);
    }
}
