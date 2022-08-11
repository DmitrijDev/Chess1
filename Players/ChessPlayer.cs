
namespace Chess.Players
{
    public abstract class ChessPlayer
    {
        public Func<int[]> ChooseMove { get; protected set; }
    }
}
