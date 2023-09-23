using Chess.LogicPart;

namespace Chess.TreesOfAnalysis
{
    public class AnalysisTree
    {
        private readonly ulong _boardModCount;

        public ChessBoard Board { get; private set; }

        public AnalysisTreeNode Root { get; private set; }

        internal TreeEnumerator Enumerator { get; set; }

        public bool AnalysisDisabled { get; set; }

        public AnalysisTree(ChessBoard board)
        {
            if (board.Status != BoardStatus.GameIsIncomplete)
            {
                throw new ArgumentException("Анализ возможен только на непустой доске, с возможной по правилам шахмат позицией и незавершенной партией.");
            }

            Board = board;
            _boardModCount = Board.ModCount;
            Root = new AnalysisTreeNode(Board);
        }

        public void CheckStartPositionChange()
        {
            if (Board.ModCount != _boardModCount)
            {
                Root = null;
                throw new InvalidOperationException("Работа с деревом невозможна: на доске изменилась позиция во время анализа.");
            }
        }

        public IEnumerable<AnalysisTreeNode> EvaluateLeaves(int depth, Func<ChessBoard, int> evaluatePosition, Predicate<AnalysisTreeNode> shouldStopAt)
        {
            var enumeration = new TreeEnumeration(this, depth, shouldStopAt);

            foreach (var node in enumeration)
            {
                if (enumeration.CurrentDepth == depth || enumeration.Board.Status != BoardStatus.GameIsIncomplete)
                {
                    node.Evaluation = evaluatePosition(enumeration.Board);
                    yield return node; 
                }                
            }
        }

        public bool IsAnalyzed => Root.IsEvaluated;
    }
}
