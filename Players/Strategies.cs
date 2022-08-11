using Board = Chess.GameBoard.GameBoard;

namespace Chess.Players
{
    public static class Strategies
    {
        public static int[] ChooseMoveForVirtualFool()
        {
            var legalMoves = Board.Board.GetLegalMoves();
            var moveIndex = new Random().Next(legalMoves.Count);
            return legalMoves[moveIndex];
        }
    }
}
