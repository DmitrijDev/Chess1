using Chess.ChessTree;

namespace Chess.StrategicPart
{
    public static class TreeTraverse
    {
        public static IEnumerable<Node> TraverseAtDepth(this IChessTree tree, int depth)
        {
            if (depth < 0)
            {
                throw new ArgumentOutOfRangeException("Отрицательная глубина.");
            }

            if (depth == 0)
            {
                yield return tree.Root;
                yield break;
            }

            var upper = new Queue<Node>(tree.Root.GetChildren());
            var lower = new Queue<Node>();

            for (var i = 1; i < depth; ++i)
            {
                while (upper.Count > 0)
                {
                    var node = upper.Dequeue();
                    tree.AddChildren(node);

                    if (!node.HasChildren)
                    {
                        yield return node;
                        continue;
                    }

                    foreach (var child in node.GetChildren())
                    {
                        lower.Enqueue(child);
                    }
                }

                upper = lower;
                lower = new();
            }

            foreach (var node in upper)
            {
                yield return node;
            }
        }
    }
}
