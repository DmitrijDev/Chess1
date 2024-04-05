using Chess.LogicPart;
using Chess.ChessTree;
using Chess.StrategicPart;

namespace Chess.VirtualPlayer
{
    public class ChessRobot<TBoard> : IChessRobot
       where TBoard : AnalysisBoard, new()
    {
        private ChessBoard _gameBoard;
        private ulong _gameBoardModCount;
        private ulong _gameBoardGameStartsCount;
        private IChessTree _tree;

        private readonly string[] _boardPropNames;
        private readonly Delegate[] _boardFuncs;

        public Func<IChessTree, IEnumerable<Node>> Traverse { get; }

        public Func<Node, IChessTree, int> Evaluate { get; }

        public Func<Node, bool> CorrectParentEvaluation { get; }

        public Func<IChessTree, Node> GetBestMoveNode { get; }

        public bool ThinkingDisabled { get; set; }

        public ChessRobot(Func<IChessTree, IEnumerable<Node>> traverse, Func<Node, IChessTree, int> evaluate,
        Func<Node, bool> correctParentEvaluation, Func<IChessTree, Node> getBestMoveNode)
        {
            if (traverse == null || evaluate == null || correctParentEvaluation == null ||
            getBestMoveNode == null)
            {
                throw new ArgumentNullException();
            }

            Traverse = traverse;
            Evaluate = evaluate;
            CorrectParentEvaluation = correctParentEvaluation;
            GetBestMoveNode = getBestMoveNode;
        }

        public ChessRobot(Func<IChessTree, IEnumerable<Node>> traverse, Func<Node, IChessTree, int> evaluate,
        Func<Node, bool> correctParentEvaluation, Func<IChessTree, Node> getBestMoveNode,
        IEnumerable<string> boardPropNames, IEnumerable<Delegate> boardFuncs) :
        this(traverse, evaluate, correctParentEvaluation, getBestMoveNode)
        {
            if (boardPropNames == null || boardFuncs == null)
            {
                throw new ArgumentNullException();
            }

            _boardPropNames = boardPropNames.ToArray();
            _boardFuncs = boardFuncs.ToArray();
            CheckBoardProps();
        }

        public ChessRobot(ChessRobot<TBoard> other)
        {
            Traverse = other.Traverse;
            Evaluate = other.Evaluate;
            CorrectParentEvaluation = other.CorrectParentEvaluation;
            GetBestMoveNode = other.GetBestMoveNode;

            if (other._boardPropNames.Length == 0)
            {
                return;
            }

            _boardPropNames = new string[other._boardPropNames.Length];
            Array.Copy(other._boardPropNames, _boardPropNames, _boardPropNames.Length);

            _boardFuncs = new Delegate[other._boardFuncs.Length];
            Array.Copy(other._boardFuncs, _boardFuncs, _boardFuncs.Length);
        }

        private void CheckBoardProps()
        {
            if (_boardPropNames.Length != _boardFuncs.Length)
            {
                throw new ArgumentException("Число указ. свойств доски должно быть равно числу указ. значений.");
            }

            var boardType = typeof(TBoard);

            for (var i = 0; i < _boardPropNames.Length; ++i)
            {
                if (_boardFuncs[i] == null)
                {
                    throw new ArgumentNullException("Среди указ. значений свойств не может быть == null.");
                }

                var property = boardType.GetProperty(_boardPropNames[i]);

                if (property == null)
                {
                    throw new ArgumentException($"Не найдено подходящего открытого свойства {_boardPropNames[i]}.");
                }

                if (!property.CanWrite)
                {
                    throw new ArgumentException($"Свойству {_boardPropNames[i]} невозможно присвоить значение.");
                }

                var propType = property.PropertyType;
                var valueType = _boardFuncs[i].GetType();

                if (!propType.IsAssignableFrom(valueType))
                {
                    throw new ArgumentException($"Свойству {_boardPropNames[i]} указано неподходящее значение.");
                }
            }
        }

        public IChessRobot Copy() => new ChessRobot<TBoard>(this);

        public Move SelectMove(ChessBoard board)
        {
            lock (this)
            {
                lock (board)
                {
                    _gameBoard = board;
                    _gameBoardModCount = _gameBoard.ModCount;
                    _gameBoardGameStartsCount = _gameBoard.GameStartsCount;

                    _tree = _boardPropNames == null || _boardPropNames.Length == 0 ? new Tree<TBoard>(_gameBoard) :
                    new Tree<TBoard>(_gameBoard, _boardPropNames, _boardFuncs);
                }

                Node resultNode;

                if (_tree.Root.ChildrenCount == 1)
                {
                    resultNode = _tree.Root.GetChildren().Single();
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

                lock (_gameBoard)
                {
                    if (ThinkingDisabled)
                    {
                        _gameBoard = null;
                        _tree = null;
                        throw new GameInterruptedException("Виртуальному игроку запрещен анализ позиций.");
                    }

                    if (_gameBoard.ModCount != _gameBoardModCount || _gameBoardGameStartsCount != _gameBoard.GameStartsCount)
                    {
                        throw new InvalidOperationException("На доске изменилась позиция во время анализа.");
                    }

                    var piece = _gameBoard[resultNode.StartVertical, resultNode.StartHorizontal].ContainedPiece;
                    var square = _gameBoard[resultNode.MoveSquareVertical, resultNode.MoveSquareHorizontal];
                    return resultNode.IsPawnPromotion ? new Move(piece, square, resultNode.NewPieceName) : new Move(piece, square);
                }
            }
        }

        private void Analyze()
        {
            foreach (var node in Traverse(_tree))
            {
                lock (_gameBoard)
                {
                    if (ThinkingDisabled)
                    {
                        _gameBoard = null;
                        _tree = null;
                        throw new GameInterruptedException("Виртуальному игроку запрещен анализ позиций.");
                    }

                    if (_gameBoard.ModCount != _gameBoardModCount || _gameBoardGameStartsCount != _gameBoard.GameStartsCount)
                    {
                        throw new InvalidOperationException("На доске изменилась позиция во время анализа.");
                    }
                }

                node.Evaluation = Evaluate(node, _tree);
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