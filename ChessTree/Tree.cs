using Chess.LogicPart;

namespace Chess.ChessTree
{
    public class Tree<TBoard> : IChessTree
        where TBoard : ChessBoard, new()
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

        private Func<TBoard, int> _evaluatePosition = new(board => 0);

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
            Root.Depth = (ushort)_board.MovesCount;

            _activeNode = Root;

            Root.Children = _board.GetLegalMoves().Select(move => new Node(move, Root)).ToArray();
            Root.DescendantsCount = Root.Children.Length;
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

                foreach (var move in _rootMove.GetPrecedingMoves().Reverse())
                {
                    var piece = _board[move.StartSquare.Vertical, move.StartSquare.Horizontal].ContainedPiece;
                    var square = _board[move.MoveSquare.Vertical, move.MoveSquare.Horizontal];
                    var newMove = move.IsPawnPromotion ? new Move(piece, square, move.NewPiece.Name) : new Move(piece, square);
                    _board.MakeMove(newMove);

                    if (_board.MovesCount == Root.Depth)
                    {
                        break;
                    }
                }

                _rootMove = _board.GetLastMove();
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

            if (movesCount == nodesCount)
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
            var piece = _board[node.StartSquareVertical, node.StartSquareHorizontal].ContainedPiece;
            var square = _board[node.MoveSquareVertical, node.MoveSquareHorizontal];
            var move = node.IsPawnPromotion ? new Move(piece, square, node.NewPieceName) : new Move(piece, square);
            _board.MakeMove(move);
        }

        public void Evaluate(Node node)
        {
            lock (this)
            {
                SetBoardTo(node);
                _lastActiveNode = node;
                _lastActiveNodeMove = _board.GetLastMove();
                _workWithBoardIncomplete = true;
                node.Evaluation = _evaluatePosition(_board);
            }
        }

        public void AddChildren(Node parent, Func<Move, bool> predicate)
        {
            lock (this)
            {
                SetBoardTo(parent);

                if (_board.Status != BoardStatus.GameIsIncomplete)
                {
                    return;
                }

                var oldChildrenCount = parent.Children != null ? parent.Children.Length : 0;
                var legalMoves = _board.GetLegalMoves();

                if (legalMoves.Count == oldChildrenCount)
                {
                    return;
                }

                var newChildren = legalMoves.Where(predicate).Select(move => new Node(move, parent));

                if (oldChildrenCount == 0)
                {
                    parent.Children = newChildren.ToArray();
                }
                else
                {
                    newChildren = newChildren.Where(newChild => !parent.Children.Any(oldChild => oldChild.Coincides(newChild)));
                    parent.Children = parent.Children.Concat(newChildren).ToArray();
                }

                var newChildrenCount = parent.Children.Length - oldChildrenCount;
                parent.DescendantsCount += newChildrenCount;

                foreach (var precedent in parent.GetPrecedents())
                {
                    precedent.DescendantsCount += newChildrenCount;
                }
            }
        }

        public void AddChildren(Node parent) => AddChildren(parent, move => true);

        public void DoWithBoard(Action<TBoard> work)
        {
            lock (this)
            {
                RestoreCorrectWork();
                _lastActiveNode = _activeNode;
                _lastActiveNodeMove = _board.GetLastMove();
                _workWithBoardIncomplete = true;
                work(_board);
            }
        }

        public Func<TBoard, int> EvaluatePosition
        {
            get => _evaluatePosition;

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                _evaluatePosition = value;
            }
        }
    }
}
