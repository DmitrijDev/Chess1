using Chess.LogicPart;
using Chess.StrategicPart;

namespace Chess.ChessTree
{
    public class Tree<TBoard> : IChessTree
        where TBoard : AnalysisBoard, new()
    {
        private readonly TBoard _board = new();
        private GamePosition _boardInitialPosition;
        private Move _rootMove;

        private Node _activeNode;
        private bool _workWithBoardIncomplete;
        private Node _lastActiveNode;
        private Move _lastActiveNodeMove;

        private readonly Node[] _nodes = new Node[35200]; // Больше ходов в партии не может быть даже теоретически.
        private readonly Move[] _moves = new Move[35200];

        public Node Root { get; }

        public Tree(ChessBoard board)
        {
            _board.CopyGameState(board);

            if (_board.Status != BoardStatus.GameIsIncomplete)
            {
                throw new ArgumentException("На доске невозможно сделать ход.");
            }

            _boardInitialPosition = _board.InitialPosition;
            _rootMove = _board.GetLastMove();

            Root = _board.MovesCount > 0 ? new(_rootMove) : new();
            _activeNode = Root;

            Root.Children = _board.GetLegalMoves().Select(move => new Node(move, Root)).ToArray();
            Root.DescendantsCount = Root.Children.Length;
        }

        public Tree(ChessBoard board, IEnumerable<string> boardPropNames, IEnumerable<Delegate> boardFuncs) : this(board)
        {
            var propNames = boardPropNames.ToArray();
            var delegates = boardFuncs.ToArray();

            if (propNames.Length != delegates.Length)
            {
                throw new ArgumentException("Число указ. свойств должно быть равно числу указ. значений.");
            }

            var boardType = typeof(TBoard);

            for (var i = 0; i < propNames.Length; ++i)
            {
                if (delegates[i] == null)
                {
                    throw new ArgumentNullException("Среди указ. значений свойств не может быть == null.");
                }

                var property = boardType.GetProperty(propNames[i]);

                if (property == null)
                {
                    throw new ArgumentException($"Не найдено подходящего открытого свойства {propNames[i]}.");
                }

                if (!property.CanWrite)
                {
                    throw new ArgumentException($"Свойству {propNames[i]} невозможно присвоить значение.");
                }

                var propType = property.PropertyType;
                var valueType = delegates[i].GetType();

                if (!propType.IsAssignableFrom(valueType))
                {
                    throw new ArgumentException($"Свойству {propNames[i]} указано неподходящее значение.");
                }

                property.SetValue(_board, delegates[i]);
            }
        }

        private void RestoreCorrectWork()
        {
            if (!_workWithBoardIncomplete)
            {
                return;
            }

            if (_board.GetLastMove() == _lastActiveNodeMove && _activeNode == _lastActiveNode)
            {
                _workWithBoardIncomplete = false;
                return;
            }

            if (_board.InitialPosition != _boardInitialPosition)
            {
                _board.SetPosition(_boardInitialPosition);
                _boardInitialPosition = _board.InitialPosition;

                if (_rootMove != null)
                {
                    foreach (var move in _rootMove.GetPrecedingMoves().Reverse().Append(_rootMove))
                    {
                        var piece = _board[move.StartSquare.Vertical, move.StartSquare.Horizontal].ContainedPiece;
                        var square = _board[move.MoveSquare.Vertical, move.MoveSquare.Horizontal];
                        var newMove = move.IsPawnPromotion ? new Move(piece, square, move.NewPiece.Name) : new Move(piece, square);
                        _board.MakeMove(newMove);
                    }

                    _rootMove = _board.GetLastMove();
                }

                _activeNode = Root;
                _workWithBoardIncomplete = false;
                return;
            }

            var move1 = _lastActiveNodeMove;
            var node = _lastActiveNode;
            var move2 = _board.GetLastMove();

            var movesCount = 0;
            var nodesCount = 0;

            while (move1 != move2)
            {
                var depth1 = move1 != null ? move1.Depth : 0;
                var depth2 = move2 != null ? move2.Depth : 0;

                if (depth1 >= depth2)
                {
                    _moves[movesCount] = move1;
                    ++movesCount;
                    move1 = move1.PrecedingMove;

                    if (node != null)
                    {
                        _nodes[nodesCount] = node;
                        ++nodesCount;
                        node = node.Parent;
                    }
                }

                if (depth2 >= depth1)
                {
                    _board.TakebackMove();
                    move2 = move2.PrecedingMove;
                }
            }

            if (movesCount == 0 || (nodesCount == movesCount && _nodes[nodesCount - 1] != Root))
            {
                _activeNode = nodesCount > 0 ? _nodes[nodesCount - 1] : _lastActiveNode;
                _workWithBoardIncomplete = false;
                return;
            }

            for (var i = movesCount - 1; ; --i)
            {
                _board.MakeMove(_moves[i]);

                if (i < nodesCount)
                {
                    _activeNode = _nodes[i];
                    _workWithBoardIncomplete = false;
                    return;
                }
            }
        }

        private void SetBoardTo(Node targetNode)
        {
            RestoreCorrectWork();

            if (targetNode == _activeNode)
            {
                return;
            }

            _lastActiveNode = _activeNode;
            _lastActiveNodeMove = _board.GetLastMove();
            _workWithBoardIncomplete = true;

            var node = targetNode;
            var count = 0;

            while (node != _activeNode)
            {
                var depth1 = node.Depth;
                var depth2 = _activeNode.Depth;
                var moved = false;

                if (depth1 >= depth2 && node.Parent != null)
                {
                    _nodes[count] = node;
                    ++count;
                    node = node.Parent;
                    moved = true;
                }

                if (depth2 >= depth1 && _activeNode != Root)
                {
                    _board.TakebackMove();
                    _activeNode = _activeNode.Parent;
                    moved = true;
                }

                if (!moved)
                {
                    throw new InvalidOperationException("Указанный узел отсутствует в дереве.");
                }
            }

            for (var i = count - 1; i >= 0; --i)
            {
                MakeMove(_nodes[i]);
            }

            _activeNode = targetNode;
            _workWithBoardIncomplete = false;
        }

        private void MakeMove(Node node)
        {
            var piece = _board[node.StartVertical, node.StartHorizontal].ContainedPiece;
            var square = _board[node.MoveSquareVertical, node.MoveSquareHorizontal];
            var move = node.IsPawnPromotion ? new Move(piece, square, node.NewPieceName) : new Move(piece, square);
            _board.MakeMove(move);
        }

        public int Evaluate(Node node)
        {
            lock (this)
            {
                SetBoardTo(node);
                _lastActiveNode = node;
                _lastActiveNodeMove = _board.GetLastMove();
                _workWithBoardIncomplete = true;
                return _board.Evaluate();
            }
        }

        public bool EndsGameWith(Node node)
        {
            SetBoardTo(node);
            return _board.Status != BoardStatus.GameIsIncomplete;
        }

        public void AddChildren(Node parent)
        {
            if (parent.Children != null)
            {
                return;
            }

            lock (this)
            {
                SetBoardTo(parent);

                if (_board.Status != BoardStatus.GameIsIncomplete)
                {
                    return;
                }

                parent.Children = _board.GetLegalMoves().Select(move => new Node(move, parent)).ToArray();
                parent.DescendantsCount = parent.Children.Length;

                foreach (var precedent in parent.GetPrecedents())
                {
                    precedent.DescendantsCount += parent.Children.Length;
                }
            }
        }

        public IEnumerable<Node> Traverse(Node start, int depth, Func<Node, bool> nodePredicate)
        {
            if (start == null)
            {
                throw new ArgumentNullException(nameof(start));
            }

            if (depth < 1)
            {
                throw new ArgumentException("Глубина перебора должна быть положительным числом.");
            }

            if (!IsInThis(start))
            {
                throw new InvalidOperationException("Указанный стартовый узел отсутствует в дереве.");
            }

            var nodes = new Stack<Node>();
            nodes.Push(start);

            while (nodes.Count > 0)
            {
                var currentNode = nodes.Pop();

                if (nodePredicate != null && !nodePredicate(currentNode))
                {
                    continue;
                }

                if (currentNode.Depth == start.Depth + depth || EndsGameWith(currentNode))
                {
                    yield return currentNode;
                    continue;
                }

                AddChildren(currentNode);
                var children = nodePredicate == null ? currentNode.Children : currentNode.Children.Where(nodePredicate);

                foreach (var node in children)
                {
                    nodes.Push(node);
                }
            }
        }        

        public IEnumerable<Node> Traverse(int depth, Func<Node, bool> nodePredicate) =>
            Traverse(Root, depth, nodePredicate);

        public IEnumerable<Node> Traverse(int depth) =>
            Traverse(Root, depth, null);

        public IEnumerable<Node> Traverse(Node start, int depth) =>
            Traverse(start, depth, null);

        public bool IsInThis(Node node)
        {
            lock (this)
            {
                if (node.Parent == null)
                {
                    return node == Root;
                }

                return node.GetPrecedents().Last() == Root;
            }
        }
    }
}
