
namespace Chess.ChessTree
{
    public interface IChessTree
    {
        Node Root { get; }

        int Evaluate(Node node);

        bool EndsGameWith(Node node);

        void AddChildren(Node parent);

        IEnumerable<Node> Traverse(Node start, int depth, Func<Node, bool> nodePredicate);

        IEnumerable<Node> Traverse(int depth, Func<Node, bool> nodePredicate);

        IEnumerable<Node> Traverse(int depth);

        IEnumerable<Node> Traverse(Node start, int depth);

        bool IsInThis(Node node);
    }
}
