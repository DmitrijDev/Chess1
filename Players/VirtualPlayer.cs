using Chess.LogicPart;

namespace Chess.Players
{
    public class VirtualPlayer
    {
        public Func<ChessBoard, int[]> Strategy { get; }

        public ChessBoard Board { get; set; }

        public VirtualPlayer(Func<ChessBoard, int[]> strategy) => Strategy = strategy;

        public void SetBoard(ChessBoard board) => Board = board;

        public int[] ChooseMove() => Strategy(Board);
    }
}
