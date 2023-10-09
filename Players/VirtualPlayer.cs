using Chess.LogicPart;
using Chess.TreesOfAnalysis;

namespace Chess.Players
{
    public class VirtualPlayer
    {
        private readonly object _locker = new();
        private ulong _boardModCount;
        private ChessBoard _board;
        private ChessTree _tree;

        public Func<ChessBoard, ChessTree> BuildTree { get; internal set; }

        public Func<ChessTree, IEnumerable<TreeNode[]>> Traverse { get; internal set; }

        public Func<GamePosition, int, int, int> EvaluatePiece { get; internal set; }

        public Func<ChessTree, TreeNode, Func<GamePosition, int, int, int>, int> EvaluateNode { get; internal set; }

        public bool ThinkingDisabled { get; set; }

        public VirtualPlayer(Func<ChessBoard, ChessTree> buildTree, Func<ChessTree, IEnumerable<TreeNode[]>> traverse,
            Func<GamePosition, int, int, int> evaluatePiece, Func<ChessTree, TreeNode, Func<GamePosition, int, int, int>, int> evaluateNode)
        {
            BuildTree = buildTree;
            Traverse = traverse;
            EvaluatePiece = evaluatePiece;
            EvaluateNode = evaluateNode;
        }

        public VirtualPlayer(VirtualPlayer other)
        {
            BuildTree = other.BuildTree;
            Traverse = other.Traverse;
            EvaluatePiece = other.EvaluatePiece;
            EvaluateNode = other.EvaluateNode;
        }

        private int Evaluate(TreeNode node) => EvaluateNode(_tree, node, EvaluatePiece);

        private void Analyze()
        {
            foreach (var gameLine in Traverse(_tree))
            {
                if (ThinkingDisabled)
                {
                    throw new GameInterruptedException("Виртуальному игроку запрещен анализ позиций.");
                }

                if (_board.ModCount != _boardModCount)
                {
                    throw new InvalidOperationException("На доске изменилась позиция во время анализа.");
                }

                SetEvaluations(gameLine);
            }
        }

        private void SetEvaluations(TreeNode[] gameLine)
        {
            var lastNode = gameLine[0];
            lastNode.Evaluation = Evaluate(lastNode);
            var lastEvaluation = lastNode.Evaluation;
            var whiteIsToMove = (gameLine.Length > 1 && !lastNode.IsWhiteMove) || (gameLine.Length == 1 && _tree.StartPositionMoveTurn == ChessPieceColor.White);

            foreach (var ancestor in gameLine.Skip(1))
            {
                whiteIsToMove = !whiteIsToMove;

                if (lastEvaluation >= WhiteCheckmatingMovesLowerEvaluation)
                {
                    --lastEvaluation;
                }

                if (lastEvaluation <= -WhiteCheckmatingMovesLowerEvaluation)
                {
                    ++lastEvaluation;
                }

                if (!ancestor.IsEvaluated)
                {
                    ancestor.Evaluation = lastEvaluation;
                    continue;
                }

                if (ancestor.Evaluation == lastEvaluation)
                {
                    break;
                }

                if ((whiteIsToMove && lastEvaluation > ancestor.Evaluation) || (!whiteIsToMove && lastEvaluation < ancestor.Evaluation))
                {
                    ancestor.Evaluation = lastEvaluation;
                    continue;
                }

                var ancestorEvaluation = whiteIsToMove ? _tree.GetChildren(ancestor).Where(child => child.IsEvaluated).Select(child => child.Evaluation).Max() :
                    _tree.GetChildren(ancestor).Where(child => child.IsEvaluated).Select(child => child.Evaluation).Min();

                if (ancestorEvaluation >= WhiteCheckmatingMovesLowerEvaluation)
                {
                    --ancestorEvaluation;
                }

                if (ancestorEvaluation <= -WhiteCheckmatingMovesLowerEvaluation)
                {
                    ++ancestorEvaluation;
                }

                if (ancestor.Evaluation == ancestorEvaluation)
                {
                    break;
                }
                else
                {
                    ancestor.Evaluation = ancestorEvaluation;
                    lastEvaluation = ancestorEvaluation;
                }
            }
        }

        public Move SelectMove(ChessBoard board)
        {
            lock (_locker)
            {
                _board = board;
                _boardModCount = _board.ModCount;
                _tree = BuildTree(_board);

                if (_board.ModCount != _boardModCount)
                {
                    throw new InvalidOperationException("На доске изменилась позиция во время анализа.");
                }

                var rootChildren = _tree.GetChildren(_tree.Root);

                if (rootChildren.Length == 0)
                {
                    throw new ArgumentException("На доске невозможно сделать ход.");
                }

                TreeNode resultNode;

                if (rootChildren.Length == 1)
                {
                    resultNode = rootChildren[0];
                }
                else
                {
                    Analyze();
                    resultNode = GetBestMoveNode();
                }

                var piece = _board[resultNode.StartSquareVertical, resultNode.StartSquareHorizontal].ContainedPiece;
                var square = _board[resultNode.MoveSquareVertical, resultNode.MoveSquareHorizontal];
                Move result;

                try
                {
                    result = resultNode.IsPawnPromotion ? new Move(piece, square, resultNode.NewPieceName) : new Move(piece, square);
                }

                catch
                {
                    throw new InvalidOperationException("На доске изменилась позиция во время анализа.");
                }

                if (_board.ModCount != _boardModCount)
                {
                    throw new InvalidOperationException("На доске изменилась позиция во время анализа.");
                }

                return result;
            }
        }

        private TreeNode GetBestMoveNode()
        {
            if (!_tree.Root.IsEvaluated)
            {
                throw new InvalidOperationException("Ошибка: анализ не завершен.");
            }

            var movesEvaluations = _tree.GetChildren(_tree.Root).Where(child => child.IsEvaluated).Select(child => child.Evaluation);

            if (!movesEvaluations.Any())
            {
                throw new InvalidOperationException("Ошибка: анализ не завершен.");
            }

            var bestEvaluation = _tree.StartPositionMoveTurn == ChessPieceColor.White ? movesEvaluations.Max() : movesEvaluations.Min();
            var bestMoves = _tree.GetChildren(_tree.Root).Where(child => child.IsEvaluated && child.Evaluation == bestEvaluation).ToArray();

            if (bestMoves.Length == 1)
            {
                return bestMoves[0];
            }

            var index = new Random().Next(bestMoves.Length);
            return bestMoves[index];
        }

        public int WhiteCheckmatingMovesLowerEvaluation => int.MaxValue - 20000;
    }
}
