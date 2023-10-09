
namespace Chess.Players
{
    public class GameInterruptedException : ApplicationException
    {
        internal GameInterruptedException(string message) : base(message)
        { }
    }
}
