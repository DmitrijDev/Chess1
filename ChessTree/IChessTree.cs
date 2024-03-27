
using Chess.LogicPart;

namespace Chess.ChessTree
{
    public interface IChessTree
    {
        Node Root { get; }

        void AddChildren(Node parent);

        void AddChildren(Node parent, Func<Move, bool> predicate);
    }
}
