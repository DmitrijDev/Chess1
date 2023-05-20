using Chess.LogicPart;

namespace Chess.StrategicPart
{
    internal class PositionTree
    {
        private readonly ChessBoard _board;
        private readonly Stack<PositionTreeNode> _nodesUnderAnalysis = new();
        private readonly Stack<Queue<PositionTreeNode>> _nodesQueues = new();

        public PositionTreeNode Root { get; } = new();

        public int Count { get; private set; } = 1;

        public Func<ChessBoard, short> EvaluatePosition { get; }

        public PositionTree(ChessBoard board, VirtualPlayer player)
        {
            _board = board;
            _nodesUnderAnalysis.Push(Root);
            var rootChildren = _board.GetLegalMoves().Select(move => new PositionTreeNode(move));
            _nodesQueues.Push(new Queue<PositionTreeNode>(rootChildren));
            EvaluatePosition = player.EvaluatePosition;
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
                _nodesQueues.Pop();
            }
        }

        public void CancelAnalysis()
        {
            CheckGameInterruption();
            RestoreInitialPosition();
            Root.RemoveChildren();
            Count = 1;
            _nodesQueues.Clear();
            var rootChildren = _board.GetLegalMoves().Select(move => new PositionTreeNode(move));
            _nodesQueues.Push(new Queue<PositionTreeNode>(rootChildren));
        }

        private PositionTreeNode GetCurrentNode()
        {
            CheckGameInterruption();
            return _nodesUnderAnalysis.Peek();
        }

        public bool MoveForward()
        {
            CheckGameInterruption();

            if (_nodesQueues.Peek().Count == 0)
            {
                return false;
            }

            var newNode = _nodesQueues.Peek().Dequeue();
            GetCurrentNode().AddChild(newNode);
            ++Count;
            _nodesUnderAnalysis.Push(newNode);
            MakeMove();
            var currentNodeChildren = _board.GetLegalMoves().Select(move => new PositionTreeNode(move));
            _nodesQueues.Push(new Queue<PositionTreeNode>(currentNodeChildren));
            return true;
        }

        private void MakeMove()
        {
            var currentNode = GetCurrentNode();
            var piece = _board[currentNode.StartSquareVertical, currentNode.StartSquareHorizontal].ContainedPiece;
            var square = _board[currentNode.MoveSquareVertical, currentNode.MoveSquareHorizontal];
            var move = currentNode.NewPieceName == -1 ? new Move(piece, square) : new Move(piece, square, (ChessPieceName)currentNode.NewPieceName);
            _board.MakeMove(move);
        }

        public bool MoveBack()
        {
            CheckGameInterruption();

            if (_nodesUnderAnalysis.Count == 1)
            {
                return false;
            }

            _board.TakebackMove();
            _nodesUnderAnalysis.Pop();
            _nodesQueues.Pop();
            return true;
        }

        public void EvaluateCurrentPosition()
        {
            CheckGameInterruption();

            var evaluaion = EvaluatePosition(_board);
            GetCurrentNode().Evaluation = evaluaion;
            var whiteIsToMove = _board.MovingSideColor == ChessPieceColor.White;

            foreach (var node in _nodesUnderAnalysis.Skip(1))
            {
                whiteIsToMove = !whiteIsToMove;

                if (!node.IsEvaluated)
                {
                    node.Evaluation = evaluaion;
                    continue;
                }

                if (whiteIsToMove)
                {
                    if (evaluaion > node.Evaluation)
                    {
                        node.Evaluation = evaluaion;
                    }
                }
                else
                {
                    if (evaluaion < node.Evaluation)
                    {
                        node.Evaluation = evaluaion;
                    }
                }
            }
        }

        public int GetFullEnumerationDepth() => _nodesQueues.Reverse().TakeWhile(queue => queue.Count == 0).Count();

        public bool CanContinueAnalysis() => _nodesQueues.Any(queue => queue.Count > 0);

        public int CurrentNodeDepth => _nodesUnderAnalysis.Count - 1;
    }
}
