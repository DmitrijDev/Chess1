using Chess.ChessTree;
using Chess.LogicPart;

namespace Chess.TacticalPart
{
    public static class TreeTraverse
    {
        public static IEnumerable<Node> TraverseEntirely(Tree tree, ushort depth, Comparison<Node> nodesComparison)
        {
            if (depth < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(depth));
            }

            if (nodesComparison == null)
            {
                throw new ArgumentNullException(nameof(nodesComparison));
            }

            tree.Root.SortChildren(nodesComparison);
            var nodes = new Stack<Node>(tree.Root.GetChildren());

            while (nodes.Count > 0)
            {
                var currentNode = nodes.Pop();

                if (tree.EndsGameWith(currentNode, out var gameResult))
                {
                    if (gameResult != BoardStatus.Draw)
                    {
                        while (nodes.Count > 0 && nodes.Peek().Parent == currentNode.Parent)
                        {
                            nodes.Pop();
                        }
                    }

                    yield return currentNode;
                    continue;
                }

                if (currentNode.Depth == tree.Root.Depth + depth)
                {
                    yield return currentNode;
                    continue;
                }

                tree.AddChildren(currentNode);
                currentNode.SortChildren(nodesComparison);

                foreach (var child in currentNode.GetChildren())
                {
                    nodes.Push(child);
                }
            }
        }
    }
}
