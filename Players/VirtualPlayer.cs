
namespace Chess.Players
{
    public class VirtualPlayer : ChessPlayer
    {
        public VirtualPlayer(Func<int[]> chooseMove) => ChooseMove = chooseMove;
    }
}
