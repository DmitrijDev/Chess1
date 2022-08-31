
namespace Chess.LogicPart
{
    public class IllegalMoveException : ApplicationException
    {
        public IllegalMoveException(string message) : base(message)
        { }
    }
}
