using Chess.ChessTree;

namespace Chess.TacticalPart
{
    public static class TreeTraverse
    {
        public static IEnumerable<Node> Traverse_1(this IChessTree tree)
        {
            var nodePredicate = new Func<Node, bool>(node => node.Parent == null || !node.Parent.IsEvaluated ||
            (node.Parent.Evaluation != int.MaxValue && node.Parent.Evaluation != -int.MaxValue));

            return tree.Traverse(4, nodePredicate);
        }
    }
}
