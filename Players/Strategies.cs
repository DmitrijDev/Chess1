using Chess.LogicPart;

namespace Chess.Players
{
    public static class Strategies
    {
        public static string[] SelectMoveForVirtualFool(ChessBoard board)
        {
            if (board.Status != GameStatus.GameCanContinue)
            {
                return null;
            }

            var legalMoves = board.GetLegalMovesAsStrings().ToArray();
            var moveIndex = new Random().Next(legalMoves.Length);
            return legalMoves[moveIndex];
        }
    }
}
