
namespace Chess.Players
{
    public class GameInterruptedException : ApplicationException
    {
        public GameInterruptedException()
        { }

        public GameInterruptedException(string message) : base(message)
        { }
    }
}
