using Chess.LogicPart;

namespace Chess.Players
{
    public static class Strategies
    {
        public static int[] SelectMoveForVirtualFool(ChessBoard board)
        {
            if (board.Status != GameStatus.GameCanContinue)
            {
                return null;
            }

            var legalMoves = board.LegalMovesToInt().ToArray();
            var moveIndex = new Random().Next(legalMoves.Length);
            return legalMoves[moveIndex];
        }
    }
}
