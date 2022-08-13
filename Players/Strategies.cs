using Chess.LogicPart;

namespace Chess.Players
{
    public static class Strategies
    {
        public static int[] ChooseMoveForVirtualFool(ChessBoard board)
        {
            if (board.Status != GameStatus.GameCanContinue)
            {
                return null;
            }

            var legalMoves = board.GetLegalMoves();
            var moveIndex = new Random().Next(legalMoves.Count);
            return legalMoves[moveIndex];
        }
    }
}
