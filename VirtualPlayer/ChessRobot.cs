using Chess.LogicPart;
using Chess.ChessTree;

namespace Chess.VirtualPlayer
{
    public class ChessRobot<TBoard> : IChessRobot
        where TBoard : ChessBoard, new()
    {
        private ChessBoard _board;
        private ulong _boardModCount;
        private ulong _boardGameStartsCount;
        private Tree<TBoard> _tree;

        public Action<TBoard> SetBoardParams { get; }

        public Func<Tree<TBoard>, IEnumerable<Node>> Traverse { get; }

        public Func<TBoard, int> EvaluatePosition { get; }

        public Func<Node, bool> CorrectParentEvaluation { get; }

        public Func<IChessTree, Node> GetBestMoveNode { get; }

        public bool ThinkingDisabled { get; set; }

        public ChessRobot(Action<TBoard> setBoardParams, Func<Tree<TBoard>, IEnumerable<Node>> traverse, Func<TBoard, int> evaluatePosition,
            Func<Node, bool> correctParentEvaluation, Func<IChessTree, Node> getBestMoveNode)
        {
            SetBoardParams = setBoardParams;
            Traverse = traverse;
            EvaluatePosition = evaluatePosition;
            CorrectParentEvaluation = correctParentEvaluation;
            GetBestMoveNode = getBestMoveNode;
        }

        public ChessRobot(ChessRobot<TBoard> other)
        {
            SetBoardParams = other.SetBoardParams;
            Traverse = other.Traverse;
            EvaluatePosition = other.EvaluatePosition;
            CorrectParentEvaluation = other.CorrectParentEvaluation;
            GetBestMoveNode = other.GetBestMoveNode;
        }

        public IChessRobot Copy() => new ChessRobot<TBoard>(this);

        public Move SelectMove(ChessBoard board)
        {
            lock (this)
            {
                lock (board)
                {
                    _board = board;
                    _boardModCount = _board.ModCount;
                    _boardGameStartsCount = _board.GameStartsCount;

                    _tree = new(_board)
                    {
                        EvaluatePosition = EvaluatePosition
                    };
                }

                _tree.DoWithBoard(SetBoardParams);
                var rootChildren = _tree.Root.GetChildren();
                Node resultNode;

                if (_tree.Root.ChildrenCount == 1)
                {
                    resultNode = rootChildren.Single();
                }
                else
                {
                    Analyze();
                    resultNode = GetBestMoveNode(_tree);

                    if (resultNode == null || resultNode.Parent != _tree.Root)
                    {
                        throw new InvalidOperationException("Некорректный результат ф-ии Player.GetBestMoveNode: " +
                            "узел-результат должен быть из детей корня дерева-аргумента и != null.");
                    }
                }

                lock (_board)
                {
                    if (ThinkingDisabled)
                    {
                        _board = null;
                        _tree = null;
                        throw new GameInterruptedException("Виртуальному игроку запрещен анализ позиций.");
                    }

                    if (_board.ModCount != _boardModCount || _boardGameStartsCount != _board.GameStartsCount)
                    {
                        throw new InvalidOperationException("На доске изменилась позиция во время анализа.");
                    }

                    var piece = _board[resultNode.StartSquareVertical, resultNode.StartSquareHorizontal].ContainedPiece;
                    var square = _board[resultNode.MoveSquareVertical, resultNode.MoveSquareHorizontal];
                    return resultNode.IsPawnPromotion ? new Move(piece, square, resultNode.NewPieceName) : new Move(piece, square);
                }
            }
        }

        private void Analyze()
        {
            foreach (var node in Traverse(_tree))
            {
                lock (_board)
                {
                    if (ThinkingDisabled)
                    {
                        _board = null;
                        _tree = null;
                        throw new GameInterruptedException("Виртуальному игроку запрещен анализ позиций.");
                    }

                    if (_board.ModCount != _boardModCount || _boardGameStartsCount != _board.GameStartsCount)
                    {
                        throw new InvalidOperationException("На доске изменилась позиция во время анализа.");
                    }
                }

                _tree.Evaluate(node);
                CorrectEvaluations(node);
            }
        }

        private void CorrectEvaluations(Node node)
        {
            if (!CorrectParentEvaluation(node))
            {
                return;
            }

            foreach (var precedent in node.GetPrecedents())
            {
                if (!CorrectParentEvaluation(precedent))
                {
                    return;
                }
            }
        }
    }
}