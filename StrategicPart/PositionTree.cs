using Chess.LogicPart;

namespace Chess.StrategicPart
{
    internal class PositionTree
    {
        private readonly ChessBoard _board;
        private readonly PositionTreeNode _root = new();
        private readonly Stack<PositionTreeNode> _nodesUnderAnalysis = new();
        //private readonly Stack<Queue<PositionTreeNode>> _nodesQueues = new();        

        public int Count { get; private set; } = 1;

        public Func<ChessBoard, int> MakeStaticPositionEvaluation { get; }

        //internal Func<Move, bool> CheckAnalyzingNecessity { get; set; }

        public PositionTree(ChessBoard board, VirtualPlayer player)
        {
            _board = board;
            _nodesUnderAnalysis.Push(_root);
            //_nodesUnderAnalysis.Push(Root);
            //_nodesQueues.Push(CreateNodesQueue());
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
                //_nodesQueues.Pop();
            }
        }

        public void Clear()
        {
            //CheckGameInterruption();
            RestoreInitialPosition();
            _root.RemoveChildren();
            Count = 1;
            //_nodesQueues.Clear();
            //var rootChildren = _board.GetLegalMoves().Select(move => new PositionTreeNode(move));
            //_nodesQueues.Push(new Queue<PositionTreeNode>(rootChildren));
        }

        /*public PositionTreeNode GetCurrentNode()
        {
            CheckGameInterruption();
            return _nodesUnderAnalysis.Peek();
        }*/

        /*private Queue<PositionTreeNode> CreateNodesQueue()
        {
            var moves = CheckAnalyzingNecessity == null ? _board.GetLegalMoves() : _board.GetLegalMoves().Where(CheckAnalyzingNecessity);
            var nodes = moves.Select(move => new PositionTreeNode(move));
            return new Queue<PositionTreeNode>(nodes);
        }

        public bool MoveForward()
        {
            //CheckGameInterruption();

            if (_nodesQueues.Peek().Count == 0)
            {
                return false;
            }

            var newNode = _nodesQueues.Peek().Dequeue();
            //GetCurrentNode().AddChild(newNode);
            _nodesUnderAnalysis.Peek().AddChild(newNode);
            ++Count;
            _nodesUnderAnalysis.Push(newNode);
            MakeMove();
            _nodesQueues.Push(CreateNodesQueue());
            return true;
        }

        private void MakeMove()
        {
            var currentNode = _nodesUnderAnalysis.Peek();
            var piece = _board[currentNode.StartSquareVertical, currentNode.StartSquareHorizontal].ContainedPiece;
            var square = _board[currentNode.MoveSquareVertical, currentNode.MoveSquareHorizontal];
            var move = currentNode.NewPieceName == -1 ? new Move(piece, square) : new Move(piece, square, (ChessPieceName)currentNode.NewPieceName);
            _board.MakeMove(move);
        }*/

        private void MakeMoveOnBoard(PositionTreeNode node)
        {
            var piece = _board[node.StartSquareVertical, node.StartSquareHorizontal].ContainedPiece;
            var square = _board[node.MoveSquareVertical, node.MoveSquareHorizontal];
            var move = node.NewPieceName == -1 ? new Move(piece, square) : new Move(piece, square, (ChessPieceName)node.NewPieceName);
            _board.MakeMove(move);
        }

        /*public bool MoveBack()
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
        }*/

        public void EvaluateCurrentPosition()
        {
            //CheckGameInterruption();
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

        //public int GetFullEnumerationDepth() => _nodesQueues.Reverse().TakeWhile(queue => queue.Count == 0).Count();

        /*public void MakeFullAnalysis(int depth)
        {
            //RestoreInitialPosition();
            //Root.Children = null;
            //Count = 1;

            var nodesUnderAnalyis = new Stack<PositionTreeNode>();
            nodesUnderAnalyis.Push(Root);

            var rootChildren = _board.GetLegalMoves().Select(move => new PositionTreeNode(move));
            var firstQueue = new Queue<PositionTreeNode>(rootChildren);
            var queues = new Stack<Queue<PositionTreeNode>>();
            queues.Push(firstQueue);

            while (queues.Peek().Count > 0 || nodesUnderAnalyis.Count > 1)
            {
                if (Strategy.ThinkingDisabled)
                {
                    TakebackMoves(nodesUnderAnalyis.Count - 1);
                    throw new GameInterruptedException();
                }

                if (queues.Peek().Count == 0)
                {
                    _board.TakebackMove();
                    queues.Pop();
                    nodesUnderAnalyis.Pop();
                    continue;
                }

                var newNode = queues.Peek().Dequeue();
                MakeMoveOnBoard(newNode);
                nodesUnderAnalyis.Peek().AddChild(newNode);
                //++Count;
                nodesUnderAnalyis.Push(newNode);

                var moves = nodesUnderAnalyis.Count <= depth ? _board.GetLegalMoves() : _board.GetLegalMoves().Where(move => move.IsCapture);
                var newQueueNodes = moves.Select(move => new PositionTreeNode(move));
                queues.Push(new Queue<PositionTreeNode>(newQueueNodes));

                if (nodesUnderAnalyis.Count <= depth)
                {
                    continue;
                }

                var positionEvaluation = EvaluatePosition(_board);
                nodesUnderAnalyis.Peek().Evaluation = (short)positionEvaluation;
                var whiteIsToMove = _board.MovingSideColor == ChessPieceColor.White;

                foreach (var node in nodesUnderAnalyis.Skip(1))
                {
                    whiteIsToMove = !whiteIsToMove;

                    if (!node.IsEvaluated)
                    {
                        node.Evaluation = (short)positionEvaluation;
                        continue;
                    }

                    if (node.Evaluation == positionEvaluation)
                    {
                        break;
                    }

                    if ((whiteIsToMove && positionEvaluation > node.Evaluation) || (!whiteIsToMove && positionEvaluation < node.Evaluation))
                    {
                        node.Evaluation = (short)positionEvaluation;
                        continue;
                    }

                    var nodeEvaluation = whiteIsToMove ? node.Children.Select(child => child.Evaluation).Max() :
                        node.Children.Select(child => child.Evaluation).Min();

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
        }*/

        /*public void MakeFullAnalysis(int depth)
        {
            _nodesUnderAnalysis.Push(_root);

            var queues = new Stack<Queue<PositionTreeNode>>();
            var rootChildren = _board.GetLegalMoves().Select(move => new PositionTreeNode(move));
            queues.Push(new Queue<PositionTreeNode>(rootChildren));

            while (queues.Peek().Count > 0 || _nodesUnderAnalysis.Count > 1)
            {
                if (Strategy.ThinkingDisabled)
                {
                    TakebackMoves(_nodesUnderAnalysis.Count - 1);
                    throw new GameInterruptedException();
                }

                if (queues.Peek().Count == 0)
                {
                    _board.TakebackMove();
                    queues.Pop();
                    _nodesUnderAnalysis.Pop();
                    continue;
                }
                else
                {
                    var newNode = queues.Peek().Dequeue();
                    MakeMoveOnBoard(newNode);
                    _nodesUnderAnalysis.Peek().AddChild(newNode);
                    _nodesUnderAnalysis.Push(newNode);

                    var moves = _nodesUnderAnalysis.Count <= depth ? _board.GetLegalMoves() : _board.GetLegalMoves().Where(move => move.IsCapture);
                    var newQueueNodes = moves.Select(move => new PositionTreeNode(move));
                    queues.Push(new Queue<PositionTreeNode>(newQueueNodes));
                }

                /*var uzel = nodesUnderAnalyis.Peek();
                if (uzel.StartSquareVertical == 1 && uzel.StartSquareHorizontal == 6 && uzel.MoveSquareVertical == 1 &&
                    uzel.MoveSquareHorizontal == 4)
                {
                    var n = 0;
                }*/

        /*if (_nodesUnderAnalysis.Count <= depth)
        {
            continue;
        }

        _nodesUnderAnalysis.Peek().Evaluation = (short)MakeStaticPositionEvaluation(_board);

        if (queues.Peek().Count > 0)
        {
            continue;
        }

        var whiteIsToMove = _board.MovingSideColor == ChessPieceColor.White;
        var currentParentNodeDepth = _nodesUnderAnalysis.Count;
        short borderNodeEvaluation = _nodesUnderAnalysis.Peek().Evaluation;

        foreach (var parentNode in _nodesUnderAnalysis.Skip(1))
        {
            --currentParentNodeDepth;
            whiteIsToMove = !whiteIsToMove;

            if (currentParentNodeDepth > depth)
            {
                var bestChildEvaluation = whiteIsToMove ? parentNode.Children.Select(child => child.Evaluation).Max() :
                    parentNode.Children.Select(child => child.Evaluation).Min();

                if ((whiteIsToMove && bestChildEvaluation < parentNode.Evaluation) || (!whiteIsToMove && bestChildEvaluation > parentNode.Evaluation))
                {
                    break;
                }
                else
                {
                    parentNode.Evaluation = bestChildEvaluation;

                    if (currentParentNodeDepth == depth + 1)
                    {
                        borderNodeEvaluation = bestChildEvaluation;
                    }

                    continue;
                }
            }

            if (!parentNode.IsEvaluated)
            {
                parentNode.Evaluation = borderNodeEvaluation;
                continue;
            }

            if (parentNode.Evaluation == borderNodeEvaluation)
            {
                break;
            }

            if ((whiteIsToMove && borderNodeEvaluation > parentNode.Evaluation) || (!whiteIsToMove && borderNodeEvaluation < parentNode.Evaluation))
            {
                parentNode.Evaluation = borderNodeEvaluation;
                continue;
            }

            var parentNodeNewEvaluation = whiteIsToMove ? parentNode.Children.Select(child => child.Evaluation).Max() :
                parentNode.Children.Select(child => child.Evaluation).Min();

            if (parentNode.Evaluation == parentNodeNewEvaluation)
            {
                break;
            }
            else
            {
                parentNode.Evaluation = parentNodeNewEvaluation;
            }
        }
    }
     }*/

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

        /*private void TakebackMoves(int movesCount)
        {
            for (var i = 0; i < movesCount; ++i)
            {
                _board.TakebackMove();
            }
        }*/

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

                if ((_board.MovingSideColor == ChessPieceColor.White && node.Evaluation > evaluation) || (_board.MovingSideColor == ChessPieceColor.Black && node.Evaluation < evaluation))
                {
                    evaluation = node.Evaluation;
                }
            }

            currentNode.Evaluation = (short)evaluation;
        }


        //public bool CanContinueAnalysis() => _nodesQueues.Any(queue => queue.Count > 0);

        //public int CurrentNodeDepth => _nodesUnderAnalysis.Count - 1;

        public PositionTreeNode[] GetRootChildren() => _root.Children.ToArray();
    }
}
