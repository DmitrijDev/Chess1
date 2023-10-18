
namespace Chess.VirtualPlayer
{
    public class GameInterruptedException : ApplicationException
    {
        internal GameInterruptedException(string message) : base(message)
        { }
    }
}
