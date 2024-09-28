using Chess.LogicPart;

namespace Chess.ChessTree
{
    public class Node
    {
        private readonly PieceColor _movingPieceColor;
        private readonly sbyte _startX = -1;
        private readonly sbyte _startY = -1;
        private readonly sbyte _destinationX = -1;
        private readonly sbyte _destinationY = -1;
        private readonly PieceName _newPieceName;
        private int _evaluation = int.MinValue;

        public Node Parent { get; }

        internal Node[] Children { get; set; }

        public ushort Depth { get; internal set; }

        public long DescendantsCount { get; internal set; }

        internal Node() { }        

        internal Node(Move move)
        {
            _movingPieceColor = move.MovingPieceColor;
            _startX = (sbyte)move.Start.X;
            _startY = (sbyte)move.Start.Y;
            _destinationX = (sbyte)move.Destination.X;
            _destinationY = (sbyte)move.Destination.Y;

            if (move.IsPawnPromotion)
            {
                _newPieceName = (PieceName)move.NewPieceName;
            }

            Depth = (ushort)move.Depth;
        }

        internal Node(Move move, Node parent) : this(move)
        {
            Parent = parent;
        }

        public Node GetChild(int startX, int startY, int destinationX, int destinationY, PieceName newPieceName)
        {
            var children = Children;

            if (children == null)
            {
                return null;
            }

            return children.Where(child =>
            child._startX == startX &&
            child._startY == startY &&
            child._destinationX == destinationX &&
            child._destinationY == destinationY &&
            child._newPieceName == newPieceName).
            SingleOrDefault();
        }

        public Node GetChild(int startX, int startY, int destinationX, int destinationY) =>
        GetChild(startX, startY, destinationX, destinationY, default);

        public IEnumerable<Node> GetPrecedents()
        {
            var node = Parent;

            while (node != null)
            {
                yield return node;
                node = node.Parent;
            }
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

        public int StartX => _startX >= 0 ? _startX : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public int StartY => _startY >= 0 ? _startY : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public int DestinationX => _destinationX >= 0 ? _destinationX : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public int DestinationY => _destinationY >= 0 ? _destinationY : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public bool IsPawnPromotion => _newPieceName != default;

        public PieceName NewPieceName => IsPawnPromotion ? _newPieceName :
        throw new InvalidOperationException("Это свойство может быть вычислено только для узла, соотв. превращению пешки.");

        public PieceColor MovingPieceColor => _startX >= 0 ? _movingPieceColor : throw new InvalidOperationException("Этот корневой узел не хранит цвета фигуры.");

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
