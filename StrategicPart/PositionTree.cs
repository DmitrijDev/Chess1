using Chess.LogicPart;

namespace Chess.StrategicPart
{
    internal class PositionTree
    {
        private readonly ChessBoard _board;
        private readonly PositionTreeNode _root = new();
        private readonly Stack<PositionTreeNode> _nodesUnderAnalysis = new();

        public int Count { get; private set; } = 1;

        public Func<ChessBoard, int> MakeStaticPositionEvaluation { get; }

        public PositionTree(ChessBoard board, VirtualPlayer player)
        {
            _board = board;
            _nodesUnderAnalysis.Push(_root);
            MakeStaticPositionEvaluation = player.MakeStaticPositionEvaluation;
        }

        private void CheckGameInterruption()
        {
            if (Strategy.ThinkingDisabled)
            {
                RestoreInitialPosition();
                throw new GameInterruptedException();
            }
        }

        public void RestoreInitialPosition()
        {
            while (_nodesUnderAnalysis.Count > 1)
            {
                _board.TakebackMove();
                _nodesUnderAnalysis.Pop();
            }
        }

        public void Clear()
        {
            RestoreInitialPosition();
            _root.RemoveChildren();
            Count = 1;
        }

        private void MakeMoveOnBoard(PositionTreeNode node)
        {
            var piece = _board[node.StartSquareVertical, node.StartSquareHorizontal].ContainedPiece;
            var square = _board[node.MoveSquareVertical, node.MoveSquareHorizontal];
            var move = node.NewPieceName == -1 ? new Move(piece, square) : new Move(piece, square, (ChessPieceName)node.NewPieceName);
            _board.MakeMove(move);
        }

        public void EvaluateCurrentPosition()
        {
            var positionEvaluation = (short)MakeStaticPositionEvaluation(_board);
            _nodesUnderAnalysis.Peek().Evaluation = positionEvaluation;
            CheckForcedVariants();
            var whiteIsToMove = _board.MovingSideColor == ChessPieceColor.White;

            foreach (var node in _nodesUnderAnalysis.Skip(1))
            {
                whiteIsToMove = !whiteIsToMove;

                if (!node.IsEvaluated)
                {
                    node.Evaluation = positionEvaluation;
                    continue;
                }

                if (node.Evaluation == positionEvaluation)
                {
                    break;
                }

                if ((whiteIsToMove && positionEvaluation > node.Evaluation) || (!whiteIsToMove && positionEvaluation < node.Evaluation))
                {
                    node.Evaluation = positionEvaluation;
                    continue;
                }

                var nodeEvaluation = whiteIsToMove ? node.Children.Where(child => child.IsEvaluated).Select(child => child.Evaluation).Max() :
                    node.Children.Where(child => child.IsEvaluated).Select(child => child.Evaluation).Min();

                if (node.Evaluation == nodeEvaluation)
                {
                    break;
                }
                else
                {
                    node.Evaluation = nodeEvaluation;
                }
            }
        }

        public void MakeFullAnalysis(int depth)
        {
            if (Count > 1)
            {
                Clear();
            }

            var queues = new Stack<Queue<PositionTreeNode>>();
            var rootChildren = _board.GetLegalMoves().Select(move => new PositionTreeNode(move));
            queues.Push(new Queue<PositionTreeNode>(rootChildren));

            while (queues.Peek().Count > 0 || _nodesUnderAnalysis.Count > 1)
            {
                CheckGameInterruption();

                if (queues.Peek().Count == 0)
                {
                    _board.TakebackMove();
                    _nodesUnderAnalysis.Pop();
                    queues.Pop();
                    continue;
                }

                var newNode = queues.Peek().Dequeue();
                MakeMoveOnBoard(newNode);
                _nodesUnderAnalysis.Peek().AddChild(newNode);
                ++Count;
                _nodesUnderAnalysis.Push(newNode);

                if (_nodesUnderAnalysis.Count > depth)
                {
                    queues.Push(new Queue<PositionTreeNode>());
                    EvaluateCurrentPosition();
                }
                else
                {
                    var newQueueNodes = _board.GetLegalMoves().Select(move => new PositionTreeNode(move));
                    queues.Push(new Queue<PositionTreeNode>(newQueueNodes));
                }
            }
        }

        public void CheckForcedVariants()
        {
            CheckGameInterruption();
            var currentNode = _nodesUnderAnalysis.Peek();
            var newNodes = new List<PositionTreeNode>(_board.GetLegalMoves().Where(move => move.IsCapture).Select(move => new PositionTreeNode(move)));
            var targetNodes = newNodes;

            if (currentNode.Children != null)
            {
                var oldNodes = new List<PositionTreeNode>();

                foreach (var oldNode in currentNode.Children)
                {
                    foreach (var newNode in newNodes)
                    {
                        if (newNode.StartSquareVertical == oldNode.StartSquareVertical && newNode.StartSquareHorizontal == oldNode.StartSquareHorizontal
                        && newNode.MoveSquareVertical == oldNode.MoveSquareVertical && newNode.MoveSquareHorizontal == oldNode.MoveSquareHorizontal &&
                        newNode.NewPieceName == oldNode.NewPieceName)
                        {
                            oldNodes.Add(oldNode);
                            newNodes.Remove(newNode);
                        }
                    }
                }

                targetNodes = new List<PositionTreeNode>(oldNodes.Concat(newNodes));
            }

            foreach (var node in newNodes)
            {
                currentNode.AddChild(node);
                ++Count;
            }

            var evaluation = currentNode.IsEvaluated ? currentNode.Evaluation : MakeStaticPositionEvaluation(_board);

            foreach (var node in targetNodes)
            {
                MakeMoveOnBoard(node);
                _nodesUnderAnalysis.Push(node);
                CheckForcedVariants();
                _board.TakebackMove();
                _nodesUnderAnalysis.Pop();

                if ((_board.MovingSideColor == ChessPieceColor.White && node.Evaluation > evaluation) ||
                    (_board.MovingSideColor == ChessPieceColor.Black && node.Evaluation < evaluation))
                {
                    evaluation = node.Evaluation;
                }
            }

            currentNode.Evaluation = (short)evaluation;
        }

        public PositionTreeNode[] GetRootChildren() => _root.Children.ToArray();
    }
}
