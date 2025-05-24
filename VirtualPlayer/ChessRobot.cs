using Chess.LogicPart;
using Chess.ChessTree;
using Chess.StrategicPart;

namespace Chess.VirtualPlayer
{
    public class ChessRobot
    {        
        private readonly Func<AnalysisBoard, int> _staticEvaluation;
        private Delegate _nodesCompareFunc;
        
        public Func<Tree, IEnumerable<Node>> Traverse { get; private protected set; }

        public Func<Tree, Node, int> DynamicEvaluation { get; private protected set; }

        public Func<Node, bool> CorrectParentEvaluation { get; private protected set; }

        public Func<Tree, Func<Node, Node, bool>, Node> GetBestMoveNode { get; private protected set; }

        internal ChessRobot() { }

        public ChessRobot(Func<Tree, IEnumerable<Node>> traverse, Func<Tree, Node, int> dynamicEvaluation,
        Func<Node, bool> correctParentEvaluation, Func<Tree, Func<Node, Node, bool>, Node> getBestMoveNode,
        Func<Node, Node, bool> isBetter)
        {
            if (traverse == null || dynamicEvaluation == null || correctParentEvaluation == null ||
                getBestMoveNode == null || isBetter == null)
            {
                throw new ArgumentNullException();
            }

            Traverse = traverse;
            DynamicEvaluation = dynamicEvaluation;
            CorrectParentEvaluation = correctParentEvaluation;
            GetBestMoveNode = getBestMoveNode;
            _nodesCompareFunc = isBetter;
        }

        public ChessRobot(Func<Tree, IEnumerable<Node>> traverse, Func<Tree, Node, int> dynamicEvaluation,
        Func<Node, bool> correctParentEvaluation, Func<Tree, Func<Node, Node, bool>, Node> getBestMoveNode,
        Func<Node, Node, bool> isBetter, Func<AnalysisBoard, int> staticEvaluation) :
        this(traverse, dynamicEvaluation, correctParentEvaluation, getBestMoveNode, isBetter)
        {
            _staticEvaluation = staticEvaluation;
        }

        private protected void SetNodesCompareFunc(Delegate func) => _nodesCompareFunc = func;

        public Delegate GetNodesCompareFunc() => _nodesCompareFunc;

        private protected virtual bool IsBetter(Node node1, Node node2)
        {
            var compareFunc = (Func<Node, Node, bool>)_nodesCompareFunc;
            return compareFunc(node1, node2);
        }

        private protected virtual Tree GetTree(ChessBoard board) => _staticEvaluation == null ?
        new Tree(board) : new Tree(board, _staticEvaluation);

        public Task<Move> GetMove(ChessBoard board, Func<bool> breakCondition)
        {
            var tree = GetTree(board);
            Node resultNode;

            var task = Task.Run(() =>
            {
                if (tree.Root.ChildrenCount == 1)
                {
                    resultNode = tree.Root.GetChildren().Single();
                }
                else
                {
                    Analyze(tree, breakCondition);
                    resultNode = GetBestMoveNode(tree, IsBetter);
                }

                if (resultNode != null && resultNode.Parent != tree.Root)
                {
                    throw new InvalidOperationException("Некорректный результат ф-ии GetBestMoveNode: " +
                        "узел-результат должен быть из детей корня дерева.");
                }

                return tree.GetMove(resultNode);
            });

            return task;
        }

        private void Analyze(Tree tree, Func<bool> breakCondition)
        {
            foreach (var node in Traverse(tree))
            {
                if (breakCondition())
                {
                    return;
                }

                node.Evaluation = DynamicEvaluation(tree, node);
                CorrectPrecedingEvaluations(node);
            }
        }

        private void CorrectPrecedingEvaluations(Node node)
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

    public class ChessRobot<TBoard, TNode> : ChessRobot
       where TBoard : AnalysisBoard, new()
       where TNode : Node, new()
    {        
        private readonly string[] _boardPropNames;
        private readonly object[] _boardPropValues;
                
        public ChessRobot(Func<Tree, IEnumerable<Node>> traverse, Func<Tree, Node, int> dynamicEvaluation,
        Func<Node, bool> correctParentEvaluation, Func<Tree, Func<Node, Node, bool>, Node> getBestMoveNode,
        Func<TNode, TNode, bool> isBetter)
        {
            if (traverse == null || dynamicEvaluation == null || correctParentEvaluation == null ||
                getBestMoveNode == null || isBetter == null)
            {
                throw new ArgumentNullException();
            }

            Traverse = traverse;
            DynamicEvaluation = dynamicEvaluation;
            CorrectParentEvaluation = correctParentEvaluation;
            GetBestMoveNode = getBestMoveNode;
            SetNodesCompareFunc(isBetter);
        }

        public ChessRobot(Func<Tree, IEnumerable<Node>> traverse, Func<Tree, Node, int> dynamicEvaluation,
        Func<Node, bool> correctParentEvaluation, Func<Tree, Func<Node, Node, bool>, Node> getBestMoveNode,
        Func<TNode, TNode, bool> isBetter, IEnumerable<string> boardPropNames, IEnumerable<object> boardPropValues) :
        this(traverse, dynamicEvaluation, correctParentEvaluation, getBestMoveNode, isBetter)
        {
            if (boardPropNames == null || boardPropValues == null)
            {
                throw new ArgumentNullException();
            }

            _boardPropNames = boardPropNames.ToArray();
            _boardPropValues = boardPropValues.ToArray();
            CheckBoardProps();
        }

        private void CheckBoardProps()
        {
            if (_boardPropNames.Length != _boardPropValues.Length)
            {
                throw new ArgumentException("Число указ. свойств доски должно быть равно числу указ. значений.");
            }

            if (_boardPropNames.Length == 0)
            {
                return;
            }

            var boardType = typeof(TBoard);

            for (var i = 0; i < _boardPropNames.Length; ++i)
            {               
                var property = boardType.GetProperty(_boardPropNames[i]);

                if (property == null)
                {
                    throw new ArgumentException($"Не найдено подходящего открытого свойства {_boardPropNames[i]}.");
                }

                if (!property.GetAccessors().Any(acs => acs.ReturnType == typeof(void)))
                {
                    throw new ArgumentException($"Свойству {_boardPropNames[i]} невозможно присвоить значение.");
                }

                if (_boardPropValues[i] is null)
                {
                    continue;
                }

                var propType = property.PropertyType;
                var valueType = _boardPropValues[i].GetType();

                if (!propType.IsAssignableFrom(valueType))
                {
                    throw new ArgumentException($"Свойству {_boardPropNames[i]} указано неподходящее по типу значение.");
                }
            }
        }

        private protected override bool IsBetter(Node node1, Node node2)
        {
            var compareFunc = (Func<TNode, TNode, bool>)GetNodesCompareFunc();
            return compareFunc((TNode)node1, (TNode)node2);
        }        

        private protected override Tree GetTree(ChessBoard board) => _boardPropNames == null || _boardPropNames.Length == 0 ?
        new Tree<TBoard, TNode>(board) : new Tree<TBoard, TNode>(board, _boardPropNames, _boardPropValues);
    }
}