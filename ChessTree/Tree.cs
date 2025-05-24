using Chess.LogicPart;
using Chess.StrategicPart;

namespace Chess.ChessTree
{
    public class Tree
    {
        private Node _activeNode;
        private readonly object _locker = new();

        internal AnalysisBoard Board { get; private protected set; }

        public Node Root { get; internal set; }

        public Tree(ChessBoard board)
        {
            Board = GetBoard();
            Board.CopyGame(board);

            if (Board.Status != BoardStatus.GameIncomplete)
            {
                throw new ArgumentException("На доске невозможно сделать ход.");
            }

            var root = GetNode();
            root.SetToRootPosition(this);
            _activeNode = Root;
            
            var rootChildren = Board.GetLegalMoves().Select(move =>
            {
                var node = GetNode();
                node.SetMoveParams(move);
                return node;
            });

            Root.Join(rootChildren);
        }

        public Tree(ChessBoard board, Func<AnalysisBoard, int> evaluateFunc) : this(board)
        {
            Board.EvaluateFunc = evaluateFunc;
        }

        private protected virtual AnalysisBoard GetBoard() => new();

        private protected virtual Node GetNode() => new();

        private void MakeMove(Node node)
        {
            if (node.IsPawnPromotion)
            {
                Board.MakeMove(node.StartX, node.StartY, node.DestinationX, node.DestinationY, node.NewPieceName);
                return;
            }

            Board.MakeMove(node.StartX, node.StartY, node.DestinationX, node.DestinationY);
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
                    Board.CancelMove();
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
                return Board.Evaluate();
            }
        }

        public bool EndsGameWith(Node node, out BoardStatus gameResult)
        {
            lock (_locker)
            {
                SetBoardTo(node);
                gameResult = Board.Status;
                return Board.Status != BoardStatus.GameIncomplete;
            }
        }

        public bool CheckmatesWith(Node node) => EndsGameWith(node, out var gameResult) && gameResult != BoardStatus.Draw;        

        public void AddChildren(Node parent)
        {
            if (parent.Children != null)
            {
                return;
            }

            lock (_locker)
            {
                SetBoardTo(parent);

                if (Board.Status != BoardStatus.GameIncomplete)
                {
                    return;
                }

                var children = Board.GetLegalMoves().Select(move =>
                {
                    var node = GetNode();
                    node.SetMoveParams(move);
                    return node;
                });

                parent.Join(children);
            }
        }

        public bool Contains(Node node)
        {
            if (node.Parent == null)
            {
                return node == Root;
            }

            return node.GetPrecedents().Last() == Root;
        }

        public void RemoveChildren(Node parent)
        {
            lock (_locker)
            {
                if (!Contains(parent))
                {
                    throw new ArgumentException();
                }

                if (_activeNode.GetPrecedents().Contains(parent))
                {
                    while(_activeNode != parent)
                    {
                        Board.CancelMove();
                        _activeNode = _activeNode.Parent;
                    }
                }

                parent.RemoveChildren();
            }            
        }

        public Move GetMove(Node node)
        {
            if (node == null)
            {
                return null;
            }

            lock(_locker)
            {
                SetBoardTo(node);
                return Board.LastMove;
            }
        }

    }

    public class Tree<TBoard, TNode> : Tree
        where TBoard : AnalysisBoard, new()
        where TNode : Node, new()
    {
        public Tree(ChessBoard board) : base(board) { }

        public Tree(ChessBoard board, IEnumerable<string> boardPropNames, IEnumerable<object> boardPropValues) : base(board)
        {
            var newBoard = new TBoard();

            if (boardPropNames == null || boardPropValues == null)
            {
                throw new ArgumentNullException();
            }

            var propNames = boardPropNames.ToArray();
            var propValues = boardPropValues.ToArray();

            if (propNames.Length != propValues.Length)
            {
                throw new ArgumentException("Число указ. свойств должно быть равно числу указ. значений.");
            }

            if (propNames.Length == 0)
            {
                return;
            }

            var boardType = typeof(TBoard);

            for (var i = 0; i < propNames.Length; ++i)
            {                
                var property = boardType.GetProperty(propNames[i]);

                if (property == null)
                {
                    throw new ArgumentException($"Не найдено подходящего открытого свойства {propNames[i]}.");
                }

                if (!property.GetAccessors().Any(acs => acs.ReturnType == typeof(void)))
                {
                    throw new ArgumentException($"Свойству {propNames[i]} невозможно присвоить значение.");
                }

                if (propValues[i] is null)
                {
                    property.SetValue(newBoard, propValues[i]);

                    if (property.GetValue(newBoard) is not null)
                    {
                        throw new ArgumentException($"Свойству {propNames[i]} не удалось присвоить значение.");
                    }

                    continue;
                }

                var propType = property.PropertyType;
                var valueType = propValues[i].GetType();

                if (!valueType.IsAssignableTo(propType))
                {
                    throw new ArgumentException($"Свойству {propNames[i]} указано неподходящее по типу значение.");
                }

                property.SetValue(newBoard, propValues[i]);
            }

            newBoard.CopyGame(Board);
            Board = newBoard;
        }

        private protected override AnalysisBoard GetBoard() => new TBoard();

        private protected override Node GetNode() => new TNode();
    }
}
