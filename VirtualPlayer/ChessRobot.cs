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

        private readonly object _locker = new();

        public Func<IChessTree, IEnumerable<Node>> Traverse { get; }

        public Func<Node, IChessTree, int> Evaluate { get; }

        public Func<Node, bool> CorrectParentEvaluation { get; }

        public Func<IChessTree, Node> GetBestMoveNode { get; }

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

        public Move GetMove(ChessBoard board)
        {
            lock (_locker)
            {
                _gameBoardGameStartsCount = board.GamesCount;
                _gameBoardModCount = board.ModCount;
                _gameBoard = board;
                _tree = GetTree();

                Node resultNode;

                if (_tree.Root.ChildrenCount == 1)
                {
                    resultNode = _tree.Root.GetChildren().Single();
                }
                else
                {
                    AnalyzeTree();
                    resultNode = GetBestMoveNode(_tree);

                    if (resultNode == null || resultNode.Parent != _tree.Root)
                    {
                        throw new InvalidOperationException("Некорректный результат ф-ии Player.GetBestMoveNode: " +
                            "узел-результат должен быть из детей корня дерева-аргумента и != null.");
                    }
                }

                var piece = _gameBoard.GetPiece(resultNode.StartX, resultNode.StartY);
                var square = _gameBoard[resultNode.DestinationX, resultNode.DestinationY];
                Move resultMove;

                try
                {
                    resultMove = resultNode.IsPawnPromotion ? new Move(piece, square, resultNode.NewPieceName) : new Move(piece, square);
                }

                catch
                {
                    throw new ThinkingInterruptedException();
                }

                CheckGameBoardPosition();
                return resultMove;
            }
        }

        private protected virtual IChessTree GetTree() => _boardPropNames == null || _boardPropNames.Length == 0 ?
        new Tree<TBoard>(_gameBoard) : new Tree<TBoard>(_gameBoard, _boardPropNames, _boardFuncs);

        private void AnalyzeTree()
        {
            foreach (var node in Traverse(_tree))
            {
                CheckGameBoardPosition();
                node.Evaluation = Evaluate(node, _tree);
                CorrectAncestorsEvaluations(node);
            }
        }

        private void CorrectAncestorsEvaluations(Node node)
        {
            if (!CorrectParentEvaluation(node))
            {
                return;
            }

            foreach (var precedent in node.GetPrecedents())
            {
                CheckGameBoardPosition();

                if (!CorrectParentEvaluation(precedent))
                {
                    return;
                }
            }
        }        

        private void CheckGameBoardPosition()
        {
            if (_gameBoard.ModCount != _gameBoardModCount || _gameBoardGameStartsCount != _gameBoard.GamesCount)
            {
                throw new ThinkingInterruptedException();
            }
        }
    }
}