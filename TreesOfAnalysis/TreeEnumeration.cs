using Chess.LogicPart;
using System.Collections;

namespace Chess.TreesOfAnalysis
{
    internal class TreeEnumeration : IEnumerable<AnalysisTreeNode>
    {
        private readonly TreeEnumerator _enumerator;

        internal TreeEnumeration(AnalysisTree tree, int depth, Predicate<AnalysisTreeNode> shouldStopAt) =>
            _enumerator = new TreeEnumerator(tree, depth, shouldStopAt);

        public IEnumerator<AnalysisTreeNode> GetEnumerator() => _enumerator;

        IEnumerator IEnumerable.GetEnumerator() => _enumerator;

        public int CurrentDepth => _enumerator.CurrentDepth;

        public AnalysisTreeNode CurrentNode => _enumerator.Current;

        public ChessBoard Board => _enumerator.Board;
    }
}
