using Chess.LogicPart;

namespace Chess.ChessTree
{
    public class Node
    {
        private readonly sbyte _startVertical = -1;
        private readonly sbyte _startHorizontal = -1;
        private readonly sbyte _moveSquareVertical = -1;
        private readonly sbyte _moveSquareHorizontal = -1;
        private readonly ChessPieceName _newPieceName;

        private readonly ChessPieceColor _movingPieceColor;
        private int _evaluation = int.MinValue;

        public Node Parent { get; }

        internal Node[] Children { get; set; }

        public ushort Depth { get; internal set; }

        public long DescendantsCount { get; internal set; }

        internal Node() { }

        internal Node(Move move)
        {
            _startVertical = (sbyte)move.StartSquare.Vertical;
            _startHorizontal = (sbyte)move.StartSquare.Horizontal;
            _moveSquareVertical = (sbyte)move.MoveSquare.Vertical;
            _moveSquareHorizontal = (sbyte)move.MoveSquare.Horizontal;

            if (move.NewPieceSelected)
            {
                _newPieceName = move.NewPiece.Name;
            }

            _movingPieceColor = move.MovingPiece.Color;
            Depth = (ushort)move.Depth;
        }

        internal Node(Move move, Node parent) : this(move)
        {
            Parent = parent;
        }

        public Node GetChild(int startVertical, int startHorizontal,
            int moveSquareVertical, int moveSquareHorizontal, ChessPieceName newPieceName)
        {
            var children = Children;

            if (children == null)
            {
                return null;
            }

            return children.Where(child => child._startVertical == startVertical &&
            child._startHorizontal == startHorizontal &&
            child._moveSquareVertical == moveSquareVertical &&
            child._moveSquareHorizontal == moveSquareHorizontal &&
            child._newPieceName == newPieceName).
            SingleOrDefault();
        }

        public Node GetChild(int startVertical, int startHorizontal,
            int moveSquareVertical, int moveSquareHorizontal) =>
        GetChild(startVertical, startHorizontal,
            moveSquareVertical, moveSquareHorizontal, default);

        public IEnumerable<Node> GetPrecedents()
        {
            var node = Parent;

            while (node != null)
            {
                yield return node;
                node = node.Parent;
            }
        }

        internal bool Coincides(Node other)
        {
            if (_startVertical != other._startVertical)
            {
                return false;
            }

            if (_startHorizontal != other._startHorizontal)
            {
                return false;
            }

            if (_moveSquareVertical != other._moveSquareVertical)
            {
                return false;
            }

            if (_moveSquareHorizontal != other._moveSquareHorizontal)
            {
                return false;
            }

            return _newPieceName == other._newPieceName;
        }

        internal bool Corresponds(Move move)
        {
            if (_startVertical != move.StartSquare.Vertical)
            {
                return false;
            }

            if (_startHorizontal != move.StartSquare.Horizontal)
            {
                return false;
            }

            if (_moveSquareVertical != move.MoveSquare.Vertical)
            {
                return false;
            }

            if (_moveSquareHorizontal != move.MoveSquare.Horizontal)
            {
                return false;
            }

            return !move.NewPieceSelected || _newPieceName == move.NewPiece.Name;
        }

        public IEnumerable<Node> GetChildren()
        {
            if (Children == null)
            {
                yield break;
            }

            foreach (var child in Children)
            {
                yield return child;
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
                    throw new ArgumentException("Для этого свойства недопустимо присвоение значения, равного int.MinValue.");
                }

                _evaluation = value;
            }
        }

        public int StartVertical => _startVertical >= 0 ? _startVertical : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public int StartHorizontal => _startHorizontal >= 0 ? _startHorizontal : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public int MoveSquareVertical => _moveSquareVertical >= 0 ? _moveSquareVertical : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public int MoveSquareHorizontal => _moveSquareHorizontal >= 0 ? _moveSquareHorizontal : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public bool IsPawnPromotion => _newPieceName != default;

        public ChessPieceName NewPieceName => IsPawnPromotion ? _newPieceName :
        throw new InvalidOperationException("Это свойство может быть вычислено только для узла, соотв. превращению пешки.");

        public ChessPieceColor MovingPieceColor => _startVertical >= 0 ? _movingPieceColor : throw new InvalidOperationException("Этот корневой узел не хранит цвета фигуры.");

        public int ChildrenCount
        {
            get
            {
                var children = Children;
                return children != null ? Children.Length : 0;
            }
        }
    }
}
