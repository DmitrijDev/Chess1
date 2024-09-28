using Chess.ChessTree;

namespace Chess.TacticalPart
{
    public static class TreeTraverse
    {
        public static IEnumerable<Node> Traverse_1(this IChessTree tree) => tree.Traverse(4);

    }
}
