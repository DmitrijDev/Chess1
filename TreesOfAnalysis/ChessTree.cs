using Chess.LogicPart;

namespace Chess.TreesOfAnalysis
{
    public class ChessTree
    {
        private readonly Stack<TreeNode> _activeNodes = new();
        private bool _activeNodesCorrespondBoardPosition;
        private readonly object _locker = new();

        internal ChessBoard Board { get; }

        public TreeNode Root { get; }

        public ChessPieceColor StartPositionMoveTurn { get; }

        public ChessTree(ChessBoard board, int depth)
        {
            lock (_locker)
            {
                if (depth < 0)
                {
                    throw new ArgumentOutOfRangeException("Глубина дерева не может быть отрицательной.");
                }

                Board = new(board);
                StartPositionMoveTurn = Board.MovingSideColor;
                Root = new(this);
                _activeNodes.Push(Root);

                if (depth == 0)
                {
                    return;
                }

                CreateNewChildren(Root);

                if (Root.Children == null || depth == 1)
                {
                    return;
                }

                var queues = new Stack<Queue<TreeNode>>();
                queues.Push(new(Root.Children));

                while (queues.Count > 0)
                {
                    var node = queues.Peek().Dequeue();
                    CreateNewChildren(node);

                    if (queues.Count < depth - 1 && node.Children != null)
                    {
                        queues.Push(new(node.Children));
                    }

                    while (queues.Count > 0 && queues.Peek().Count == 0)
                    {
                        queues.Pop();
                    }
                }
            }
        }

        private void MakeMove(TreeNode node)
        {
            var piece = Board[node.StartSquareVertical, node.StartSquareHorizontal].ContainedPiece;
            var square = Board[node.MoveSquareVertical, node.MoveSquareHorizontal];
            var move = node.IsPawnPromotion ? new Move(piece, square, node.NewPieceName) : new Move(piece, square);
            Board.MakeMove(move);
        }

        private void ReturnToRoot()
        {
            while (Board.MovesCount > Root.Depth)
            {
                Board.TakebackMove();
            }

            _activeNodes.Clear();
            _activeNodes.Push(Root);
        }

        private IEnumerable<TreeNode> TryGetGameLineTo(TreeNode targetNode)
        {
            yield return Root;

            var node = Root;

            if (targetNode.Path != null)
            {
                foreach (var index in targetNode.Path)
                {
                    node = node.Children.Where(child => child.Index == index).FirstOrDefault();

                    if (node == null)
                    {
                        yield break;
                    }

                    yield return node;
                }
            }            

            if (node.Children.Contains(targetNode))
            {
                yield return targetNode;
            }
        }

        private void SetBoardTo(TreeNode targetNode)
        {
            if (!_activeNodesCorrespondBoardPosition)
            {
                ReturnToRoot();
            }

            _activeNodesCorrespondBoardPosition = false;

            if (_activeNodes.Peek() == targetNode)
            {
                _activeNodesCorrespondBoardPosition = true;
                return;
            }

            var gameLine = TryGetGameLineTo(targetNode).ToArray();

            if (gameLine[gameLine.Length - 1] != targetNode)
            {
                throw new InvalidOperationException("Указанный узел отсутствует в дереве.");
            }

            while (!gameLine.Contains(_activeNodes.Peek()))
            {
                Board.TakebackMove();
                _activeNodes.Pop();
            }

            if (_activeNodes.Peek() == targetNode)
            {
                _activeNodesCorrespondBoardPosition = true;
                return;
            }

            var activeNode = _activeNodes.Peek();

            foreach (var node in gameLine.SkipWhile(node => node != activeNode).Skip(1))
            {
                MakeMove(node);
                _activeNodes.Push(node);
            }

            _activeNodesCorrespondBoardPosition = true;
        }

        private void CreateNewChildren(TreeNode parent)
        {
            SetBoardTo(parent);

            if (Board.Status != BoardStatus.GameIsIncomplete)
            {
                return;
            }

            var path = parent == Root ? null : parent.Path == null ? new short[] { parent.Index } : parent.Path.Append(parent.Index).ToArray();

            parent.Children = Board.GetLegalMoves().Select(move => new TreeNode(move)
            {
                Path = path,
                Depth = (short)(parent.Depth + 1),
                IsWhiteMove = Board.MovingSideColor == ChessPieceColor.White,
            }).ToArray();

            foreach (var node in TryGetGameLineTo(parent))
            {
                node.DescendantsCount += parent.Children.Length;
            }
        }

        public IEnumerable<TreeNode[]> GetGameLines()
        {
            var comparison = new Comparison<TreeNode>((node1, node2) => !node1.IsEvaluated ? (!node2.IsEvaluated ? 0 : 1) :
            !node2.IsEvaluated ? -1 : node1.Evaluation == node2.Evaluation ? 0 :
            node1.IsWhiteMove ? (node1.Evaluation > node2.Evaluation ? -1 : 1) : (node1.Evaluation < node2.Evaluation ? -1 : 1));

            var gameLine = new Stack<TreeNode>();
            gameLine.Push(Root);

            var queues = new Stack<Queue<TreeNode>>();

            if (Root.Children == null)
            {
                queues.Push(new());
            }
            else
            {
                Array.Sort(Root.Children, comparison);
                queues.Push(new(Root.Children));
            }

            while (gameLine.Count > 1 || queues.Peek().Count > 0)
            {
                if (queues.Peek().Count > 0)
                {
                    var node = queues.Peek().Dequeue();
                    gameLine.Push(node);

                    if (node.Children == null)
                    {
                        queues.Push(new());
                    }
                    else
                    {
                        Array.Sort(node.Children, comparison);
                        queues.Push(new(node.Children));
                    }

                    continue;
                }

                if (gameLine.Peek().Children == null)
                {
                    yield return gameLine.ToArray();
                }

                gameLine.Pop();
                queues.Pop();
            }
        }

        public bool EndsGame(TreeNode node, out BoardStatus? gameResult)
        {
            lock (_locker)
            {
                SetBoardTo(node);

                var endsGame = Board.Status == BoardStatus.WhiteWin || Board.Status == BoardStatus.BlackWin ||
                Board.Status == BoardStatus.Draw;

                gameResult = endsGame ? Board.Status : null;
                return endsGame;
            }
        }

        public GamePosition GetPosition(TreeNode node)
        {
            lock (_locker)
            {
                SetBoardTo(node);
                return Board.GetCurrentPosition();
            }
        }

        public TreeNode[] GetChildren(TreeNode node)
        {
            var children = node.Children;

            if (children == null)
            {
                return Array.Empty<TreeNode>();
            }

            var result = new TreeNode[children.Length];
            Array.Copy(children, result, children.Length);
            return result;
        }
    }
}
