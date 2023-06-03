using Chess.LogicPart;

namespace Chess.TreesOfAnalysis
{
    public class AnalysisTreeNode
    {
        private readonly sbyte _startSquareVertical = -1;
        private readonly sbyte _startSquareHorizontal = -1;
        private readonly sbyte _moveSquareVertical = -1;
        private readonly sbyte _moveSquareHorizontal = -1;
        private readonly sbyte _newPieceName = -1;

        private AnalysisTreeNode[] _children;
        private int _evaluation = int.MinValue;

        public AnalysisTree Tree { get; internal set; }

        public AnalysisTreeNode Parent { get; private set; }

        public long DescendantsCount { get; private set; }

        internal AnalysisTreeNode(Move move)
        {
            _startSquareVertical = (sbyte)move.StartSquare.Vertical;
            _startSquareHorizontal = (sbyte)move.StartSquare.Horizontal;
            _moveSquareVertical = (sbyte)move.MoveSquare.Vertical;
            _moveSquareHorizontal = (sbyte)move.MoveSquare.Horizontal;

            if (move.NewPieceSelected)
            {
                _newPieceName = (sbyte)move.NewPiece.Name;
            }
        }

        internal AnalysisTreeNode(ChessBoard board)
        {
            if (board == null)
            {
                throw new ArgumentNullException();
            }

            if (board.MovesCount > 0)
            {
                var move = board.GetLastMove();
                _startSquareVertical = (sbyte)move.StartSquare.Vertical;
                _startSquareHorizontal = (sbyte)move.StartSquare.Horizontal;
                _moveSquareVertical = (sbyte)move.MoveSquare.Vertical;
                _moveSquareHorizontal = (sbyte)move.MoveSquare.Horizontal;

                if (move.NewPieceSelected)
                {
                    _newPieceName = (sbyte)move.NewPiece.Name;
                }
            }
        }

        public IEnumerable<AnalysisTreeNode> GetAncestors()
        {
            var node = Parent;

            while (node != null)
            {
                yield return node;
                node = node.Parent;
            }
        }

        public int GetDepth() => GetAncestors().Count();

        internal void AddChidren(ChessBoard board)
        {
            if (_children != null)
            {
                return;
            }

            _children = board.GetLegalMoves().Select(move => new AnalysisTreeNode(move)).ToArray();

            foreach (var child in _children)
            {
                child.Parent = this;
                child.Tree = Tree;
            }

            DescendantsCount = _children.Length;

            foreach (var ancestor in GetAncestors())
            {
                ancestor.DescendantsCount += _children.Length;
            }
        }

        public IEnumerable<AnalysisTreeNode> GetChildren() => _children ?? Enumerable.Empty<AnalysisTreeNode>();

        public void CorrectAncestorsEvaluations()
        {
            if (!IsEvaluated)
            {
                throw new InvalidOperationException("Невозможно скорректировать оценки предков узла, не имеющего оценки.");
            }

            Tree.CheckStartPositionChange();
            var whiteIsToMove = (Tree.Board.MovingSideColor == ChessPieceColor.White && GetDepth() % 2 == 0) ||
                (Tree.Board.MovingSideColor == ChessPieceColor.Black && GetDepth() % 2 != 0);
            var lastEvaluation = _evaluation;

            foreach (var ancestor in GetAncestors())
            {
                whiteIsToMove = !whiteIsToMove;

                if (!ancestor.IsEvaluated)
                {
                    ancestor._evaluation = lastEvaluation;
                    continue;
                }

                if (ancestor._evaluation == lastEvaluation)
                {
                    break;
                }

                if ((whiteIsToMove && lastEvaluation > ancestor._evaluation) || (!whiteIsToMove && lastEvaluation < ancestor._evaluation))
                {
                    ancestor._evaluation = lastEvaluation;
                    continue;
                }

                var ancestorEvaluation = whiteIsToMove ? ancestor._children.Where(child => child.IsEvaluated).Select(child => child._evaluation).Max() :
                    ancestor._children.Where(child => child.IsEvaluated).Select(child => child._evaluation).Min();

                if (ancestor._evaluation == ancestorEvaluation)
                {
                    break;
                }
                else
                {
                    ancestor._evaluation = ancestorEvaluation;
                    lastEvaluation = ancestorEvaluation;
                }
            }
        }        

        public bool IsEvaluated => _evaluation != int.MinValue;

        public int Evaluation
        {
            get
            {
                if (!IsEvaluated)
                {
                    throw new InvalidOperationException("Узел не имеет оценки.");
                }

                return _evaluation;
            }

            set
            {
                if (value == int.MinValue)
                {
                    throw new ArgumentException("Для аргумента недопустимо значение, равное int.MinValue.");
                }

                _evaluation = value;
            }
        }

        public int StartSquareVertical => _startSquareVertical >= 0 ? _startSquareVertical : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public int StartSquareHorizontal => _startSquareHorizontal >= 0 ? _startSquareHorizontal : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public int MoveSquareVertical => _moveSquareVertical >= 0 ? _moveSquareVertical : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public int MoveSquareHorizontal => _moveSquareHorizontal >= 0 ? _moveSquareHorizontal : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public bool IsPawnPromotion => _newPieceName >= 0;

        public ChessPieceName NewPieceName => IsPawnPromotion ? (ChessPieceName)_newPieceName :
            throw new InvalidOperationException("Это свойство может быть вычислено только для узла, соотв. превращению пешки.");

        public bool HasChildren => _children != null;
    }
}
