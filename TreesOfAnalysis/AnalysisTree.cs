using Chess.LogicPart;

namespace Chess.TreesOfAnalysis
{
    public class AnalysisTree
    {
        private readonly ulong _boardModCount;

        public AnalysisTreeNode Root { get; private set; }

        public ChessBoard Board { get; private set; }

        public bool AnalysisDisabled { get; set; }

        public AnalysisTree(ChessBoard board)
        {
            if (board.Status != GameStatus.GameIsNotOver)
            {
                throw new ArgumentException("Анализ возможен только на непустой доске, с возможной по правилам шахмат позицией и незавершенной партией.");
            }

            Board = board;
            _boardModCount = Board.ModCount;
            Root = new AnalysisTreeNode(Board) { Tree = this };
        }

        public void CheckStartPositionChange()
        {
            if (Root == null)
            {
                throw new InvalidOperationException("Работа с деревом невозможна: на доске изменилась позиция во время анализа.");
            }

            if (Board.ModCount != _boardModCount)
            {
                Root = null;
                throw new InvalidOperationException("Работа с деревом невозможна: на доске изменилась позиция во время анализа.");
            }
        }

        private IEnumerable<AnalysisTreeNode> HandleNodes(int depth, Action<AnalysisTreeNode, int, ChessBoard> handleNode)
        {
            if (depth < 0)
            {
                throw new ArgumentException("Некорректный аргумент.");
            }

            if (depth == 0)
            {
                handleNode(Root, 0, new ChessBoard(Board));
                CheckStartPositionChange();
                yield return Root;
                yield break;
            }

            CheckStartPositionChange();
            var board = new ChessBoard(Board);
            Root.AddChidren(board);

            var queues = new Stack<Queue<AnalysisTreeNode>>();
            queues.Push(new Queue<AnalysisTreeNode>(Root.GetChildren()));

            var node = Root;
            var currentDepth = 0;

            handleNode(Root, 0, board);
            CheckStartPositionChange();
            yield return Root;

            while (queues.Peek().Count > 0 || currentDepth > 0)
            {
                if (AnalysisDisabled)
                {
                    throw new ApplicationException("Анализ позиции прерван.");
                }

                if (queues.Peek().Count == 0)
                {
                    board.TakebackMove();
                    node = node.Parent;
                    --currentDepth;
                    queues.Pop();
                    continue;
                }

                node = queues.Peek().Dequeue();
                ++currentDepth;
                var piece = board[node.StartSquareVertical, node.StartSquareHorizontal].ContainedPiece;
                var square = board[node.MoveSquareVertical, node.MoveSquareHorizontal];
                var move = !node.IsPawnPromotion ? new Move(piece, square) : new Move(piece, square, node.NewPieceName);
                board.MakeMove(move);
                handleNode(node, currentDepth, board);

                if (currentDepth < depth)
                {
                    node.AddChidren(board);
                    queues.Push(new Queue<AnalysisTreeNode>(node.GetChildren()));
                }
                else
                {
                    queues.Push(new Queue<AnalysisTreeNode>());
                }

                CheckStartPositionChange();
                yield return node;
            }
        }

        private IEnumerable<AnalysisTreeNode> HandleLeaves(int maximumDepth, Action<AnalysisTreeNode, ChessBoard> handleLeaf)
        {
            Action<AnalysisTreeNode, int, ChessBoard> handleNode = (node, depth, board) =>
            {
                if (depth == maximumDepth || board.Status != GameStatus.GameIsNotOver)
                {
                    handleLeaf(node, board);
                }
            };

            return HandleNodes(maximumDepth, handleNode);
        }

        public void Analyze(int depth, Func<ChessBoard, int> evaluatePosition)
        {
            Action<AnalysisTreeNode, ChessBoard> evaluate = (node, board) =>
            {
                var lastPosition = board.GetCurrentPosition();
                var evaluation = evaluatePosition(board);

                if (board.GetCurrentPosition() != lastPosition)
                {
                    throw new InvalidOperationException("Оценочная функция должна не менять позицию или возвращать доску к исходной позиции до завершения.");
                }

                node.Evaluation = evaluation;
                node.CorrectAncestorsEvaluations();
            };

            foreach (var node in HandleLeaves(depth, evaluate))
            { }
        }

        public bool IsAnalyzed => Root.IsEvaluated;
    }
}
