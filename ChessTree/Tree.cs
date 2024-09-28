using Chess.LogicPart;
using Chess.StrategicPart;

namespace Chess.ChessTree
{
    public class Tree<TBoard> : IChessTree
        where TBoard : AnalysisBoard, new()
    {
        private readonly TBoard _board = new();
        private Node _activeNode;
        private readonly object _locker = new();

        public Node Root { get; }

        public Tree(ChessBoard board)
        {
            _board.CopyGame(board);

            if (_board.Status != BoardStatus.GameIncomplete)
            {
                throw new ArgumentException("На доске невозможно сделать ход.");
            }

            Root = _board.MovesCount > 0 ? new(_board.LastMove) : new();
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

        private void MakeMove(Node node)
        {
            if (node.IsPawnPromotion)
            {
                _board.MakeMove(node.StartX, node.StartY, node.DestinationX, node.DestinationY, node.NewPieceName);
                return;
            }

            _board.MakeMove(node.StartX, node.StartY, node.DestinationX, node.DestinationY);
        }        

        private void SetBoardTo(Node targetNode)
        {
            if (targetNode == null)
            {
                throw new ArgumentNullException();
            }

            if (targetNode == _activeNode)
            {
                return;
            }

            var node = targetNode;
            var nodes = new Stack<Node>();

            while (node != _activeNode)
            {
                var depth1 = node.Depth;
                var depth2 = _activeNode.Depth;
                var moved = false;

                if (depth1 >= depth2 && node.Parent != null)
                {
                    nodes.Push(node);
                    node = node.Parent;
                    moved = true;
                }

                if (depth2 >= depth1 && _activeNode != Root)
                {
                    _board.CancelMove();
                    _activeNode = _activeNode.Parent;
                    moved = true;
                }

                if (!moved)
                {
                    throw new InvalidOperationException("Указанный узел отсутствует в дереве.");
                }
            }

            while (nodes.Count > 0)
            {
                MakeMove(nodes.Pop());
            }

            _activeNode = targetNode;
        }

        public int Evaluate(Node node)
        {
            lock (_locker)
            {
                SetBoardTo(node);
                return _board.Evaluate();
            }
        }

        public bool EndsGameWith(Node node)
        {
            lock (_locker)
            {
                SetBoardTo(node);
                return _board.Status != BoardStatus.GameIncomplete;
            }
        }

        public void AddChildren(Node parent)
        {
            if (parent.Children != null)
            {
                return;
            }

            lock (_locker)
            {
                SetBoardTo(parent);

                if (_board.Status != BoardStatus.GameIncomplete)
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

        public bool IsInThis(Node node)
        {
            if (node.Parent == null)
            {
                return node == Root;
            }

            return node.GetPrecedents().Last() == Root;
        }

        public IEnumerable<Node> Traverse(Node start, int depth, Func<Node, bool> predicate)
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

            AddChildren(start);
            var nodes = predicate == null ? new Stack<Node>(start.Children) : new Stack<Node>(start.Children.Where(predicate));

            while (nodes.Count > 0)
            {
                var currentNode = nodes.Pop();

                if (currentNode.Depth == start.Depth + depth || EndsGameWith(currentNode))
                {
                    yield return currentNode;
                    continue;
                }

                AddChildren(currentNode);
                var children = predicate == null ? currentNode.Children : currentNode.Children.Where(predicate);

                foreach (var child in children)
                {
                    nodes.Push(child);
                }
            }
        }

        public IEnumerable<Node> Traverse(int depth, Func<Node, bool> nodePredicate) =>
            Traverse(Root, depth, nodePredicate);

        public IEnumerable<Node> Traverse(int depth) =>
            Traverse(Root, depth, null);

        public IEnumerable<Node> Traverse(Node start, int depth) =>
            Traverse(start, depth, null);

    }
}
