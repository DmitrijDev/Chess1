using Chess.LogicPart;

namespace Chess.TreesOfAnalysis
{
    public class AnalysisTree
    {
        private readonly ChessBoard _board;
        private readonly ulong _boardModCount;
        //private AnalysisPosition _startPosition;
        private AnalysisTreeNode _root;
        //private AnalysisTreeNode _activeNode;
        //private Stack<AnalysisTreeNode> _activeNodeAncestors = new();

        //internal ChessBoard AnalysisBoard { get; private set; }

        public bool AnalysisDisabled { get; set; }

        public AnalysisTree(ChessBoard board)
        {
            if (board.Status != GameStatus.GameIsNotOver)
            {
                throw new ArgumentException("Анализ возможен только на непустой доске, с возможной по правилам шахмат позицией и незавершенной партией.");
            }

            _board = board;
            _boardModCount = _board.ModCount;
            //AnalysisBoard = new ChessBoard(board);            
            _root = new AnalysisTreeNode(_board);
            //_startPosition = new AnalysisPosition(this);
            //_activeNode = _root;
        }

        private void CheckStartPositionChange()
        {
            if (_root == null)
            {
                throw new InvalidOperationException("Работа с деревом невозможна: на доске изменилась позиция во время анализа.");
            }

            if (_board.ModCount != _boardModCount)
            {
                //Clear();
                _root = null;
                throw new InvalidOperationException("Работа с деревом невозможна: на доске изменилась позиция во время анализа.");
            }
        }

        public void Analyze(int depth, Func<ChessBoard, int> evaluatePosition)
        {
            if (depth < 1)
            {
                throw new ArgumentException("Некорректный аргумент.");
            }

            //ReturnToRoot();           
            CheckStartPositionChange();
            var board = new ChessBoard(_board);
            //board.ForbidToTakebackMove(board.MovesCount);
            //board.ForbidToStartNewGame();

            _root.AddChidren(board);

            var nodesUnderAnalysis = new Stack<AnalysisTreeNode>();
            nodesUnderAnalysis.Push(_root);

            var queues = new Stack<Queue<AnalysisTreeNode>>();
            queues.Push(new Queue<AnalysisTreeNode>(_root.GetChildren()));

            /*CheckStartPositionChange();
            yield return _board.GetCurrentPosition();

            if (AnalysisBoard.ModCount != modCount)
            {
                throw new InvalidOperationException("Изменение позиции во время перечисления.");
            }*/

            while (queues.Peek().Count > 0 || nodesUnderAnalysis.Count > 1)
            {
                CheckStartPositionChange();

                if (AnalysisDisabled)
                {
                    throw new AnalysisStoppedException("Анализ позиции прерван.");
                }

                if (queues.Peek().Count == 0)
                {
                    board.TakebackMove();
                    nodesUnderAnalysis.Pop();
                    queues.Pop();
                    continue;
                }

                var nextNode = queues.Peek().Dequeue();
                var piece = board[nextNode.StartSquareVertical, nextNode.StartSquareHorizontal].ContainedPiece;
                var square = board[nextNode.MoveSquareVertical, nextNode.MoveSquareHorizontal];
                var move = !nextNode.IsPawnPromotion ? new Move(piece, square) : new Move(piece, square, nextNode.NewPieceName);
                board.MakeMove(move);
                nodesUnderAnalysis.Push(nextNode);

                if (nodesUnderAnalysis.Count <= depth)
                {
                    if (!nextNode.HasChildren)
                    {
                        nextNode.AddChidren(board);

                        foreach (var node in nodesUnderAnalysis.Skip(1))
                        {
                            node.DescendantsCount += nextNode.DescendantsCount;
                        }
                    }

                    queues.Push(new Queue<AnalysisTreeNode>(nextNode.GetChildren()));
                }
                else
                {
                    queues.Push(new Queue<AnalysisTreeNode>());
                    var lastMove = board.MovesCount > 0 ? board.GetLastMove() : null;
                    var gameStartMoment = board.GameStartMoment;
                    var evaluation = evaluatePosition(board);

                    if (board.GameStartMoment != gameStartMoment)
                    {
                        throw new InvalidOperationException("Оценочная функция должна не может начинать на доске новую партию.");
                    }

                    if (board.Status == GameStatus.ClearBoard)
                    {
                        throw new InvalidOperationException("Оценочная функция должна не может очищать доску.");
                    }

                    if (lastMove == null)
                    {
                        if (board.MovesCount != 0)
                        {
                            throw new InvalidOperationException("Оценочная функция должна не менять позицию или возвращать доску к исходной позиции до завершения.");
                        }
                    }
                    else
                    {
                        if (board.MovesCount == 0 || board.GetLastMove() != lastMove)
                        {
                            throw new InvalidOperationException("Оценочная функция должна не менять позицию или возвращать доску к исходной позиции до завершения.");
                        }
                    }

                    nextNode.Evaluation = evaluation;
                    CorrectEvaluations(nodesUnderAnalysis);
                }
                /*modCount = AnalysisBoard.ModCount;
                  yield return AnalysisBoard.GetCurrentPosition();

                  if (AnalysisBoard.ModCount != modCount)
                  {
                      throw new InvalidOperationException("Изменение позиции во время перечисления.");
                  }*/
            }
        }

        private void CorrectEvaluations(Stack<AnalysisTreeNode> nodes)
        {
            var whiteIsToMove = (_board.MovingSideColor == ChessPieceColor.White && nodes.Count % 2 != 0) || (_board.MovingSideColor == ChessPieceColor.Black && nodes.Count % 2 == 0);
            var evaluation = nodes.Peek().Evaluation;

            foreach (var ancestorNode in nodes.Skip(1))
            {
                whiteIsToMove = !whiteIsToMove;

                if (!ancestorNode.IsEvaluated)
                {
                    ancestorNode.Evaluation = evaluation;
                    continue;
                }

                if (ancestorNode.Evaluation == evaluation)
                {
                    break;
                }

                if ((whiteIsToMove && evaluation > ancestorNode.Evaluation) || (!whiteIsToMove && evaluation < ancestorNode.Evaluation))
                {
                    ancestorNode.Evaluation = evaluation;
                    continue;
                }

                var ancestorEvaluation = whiteIsToMove ? ancestorNode.GetChildren().Where(child => child.IsEvaluated).Select(child => child.Evaluation).Max() :
                    ancestorNode.GetChildren().Where(child => child.IsEvaluated).Select(child => child.Evaluation).Min();

                if (ancestorNode.Evaluation == ancestorEvaluation)
                {
                    break;
                }
                else
                {
                    ancestorNode.Evaluation = ancestorEvaluation;
                    evaluation = ancestorEvaluation;
                }
            }
        }

        public Move GetBestMove()
        {           
            if (!_root.IsEvaluated || !_root.HasChildren)
            {
                throw new InvalidOperationException("Ошибка: дерево не проанализировано.");
            }

            var movesEvaluations = _root.GetChildren().Where(child => child.IsEvaluated).Select(child => child.Evaluation);

            if (!movesEvaluations.Any())
            {
                throw new InvalidOperationException("Ошибка: дерево не проанализировано.");
            }

            CheckStartPositionChange();
            var bestEvaluation = _board.MovingSideColor == ChessPieceColor.White ? movesEvaluations.Max() : movesEvaluations.Min();
            var bestMoves = _root.GetChildren().Where(child => child.IsEvaluated && child.Evaluation == bestEvaluation).ToArray();
            AnalysisTreeNode resultNode;

            if (bestMoves.Length == 1)
            {
                resultNode = bestMoves.Single();
            }
            else
            {
                var index = new Random().Next(bestMoves.Length);
                resultNode = bestMoves[index];
            }

            //ReturnToRoot();
            var piece = _board[resultNode.StartSquareVertical, resultNode.StartSquareHorizontal].ContainedPiece;
            var square = _board[resultNode.MoveSquareVertical, resultNode.MoveSquareHorizontal];
            return !resultNode.IsPawnPromotion ? new Move(piece, square) : new Move(piece, square, resultNode.NewPieceName);
        }
    }
}
