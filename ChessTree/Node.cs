using Chess.LogicPart;

namespace Chess.ChessTree
{
    public class Node
    {
        private readonly sbyte _startSquareVertical = -1;
        private readonly sbyte _startSquareHorizontal = -1;
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
            _startSquareVertical = (sbyte)move.StartSquare.Vertical;
            _startSquareHorizontal = (sbyte)move.StartSquare.Horizontal;
            _moveSquareVertical = (sbyte)move.MoveSquare.Vertical;
            _moveSquareHorizontal = (sbyte)move.MoveSquare.Horizontal;

            if (move.NewPieceSelected)
            {
                _newPieceName = move.NewPiece.Name;
            }

            _movingPieceColor = move.MovingPiece.Color;
        }

        internal Node(Move move, Node parent) : this(move)
        {
            Parent = parent;
            Depth = (ushort)(Parent.Depth + 1);
        }

        public Node Find(int startSquareVertical, int startSquareHorizontal, int moveSquareVertical, int moveSquareHorizontal) =>
            Children.Where(child => child._startSquareVertical == startSquareVertical &&
            child._startSquareHorizontal == startSquareHorizontal &&
            child._moveSquareVertical == moveSquareVertical &&
            child._moveSquareHorizontal == moveSquareHorizontal).
            FirstOrDefault();


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
            if (_startSquareVertical != other._startSquareVertical)
            {
                return false;
            }

            if (_startSquareHorizontal != other._startSquareHorizontal)
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

        public IEnumerable<Node> GetChildren()
        {
            var children = Children;

            if (children == null)
            {
                yield break;
            }

            foreach (var child in children)
            {
                if (children != Children)
                {
                    throw new InvalidOperationException("Коллекция была изменена во время перечисления.");
                }

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

        public int StartSquareVertical => _startSquareVertical >= 0 ? _startSquareVertical : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public int StartSquareHorizontal => _startSquareHorizontal >= 0 ? _startSquareHorizontal : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public int MoveSquareVertical => _moveSquareVertical >= 0 ? _moveSquareVertical : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public int MoveSquareHorizontal => _moveSquareHorizontal >= 0 ? _moveSquareHorizontal : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public bool IsPawnPromotion => _newPieceName != default;

        public ChessPieceName NewPieceName => IsPawnPromotion ? _newPieceName :
        throw new InvalidOperationException("Это свойство может быть вычислено только для узла, соотв. превращению пешки.");

        public ChessPieceColor MovingPieceColor => _startSquareVertical >= 0 ? _movingPieceColor : throw new InvalidOperationException("Этот корневой узел не хранит цвета фигуры.");

        public bool HasChildren => Children != null;

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
