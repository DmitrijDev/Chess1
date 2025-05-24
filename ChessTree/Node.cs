using Chess.LogicPart;

namespace Chess.ChessTree
{
    public class Node
    {
        private sbyte _startX = -1;
        private sbyte _startY = -1;
        private sbyte _destinationX = -1;
        private sbyte _destinationY = -1;
        private PieceName _newPieceName;

        private int _evaluation = int.MinValue;

        public Node Parent { get; private set; }

        internal Node[] Children { get; private set; }

        public ushort Depth { get; private set; }        

        internal virtual void SetMoveParams(Move move)
        {
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

        internal virtual void SetToRootPosition(Tree tree)
        {
            if (tree.Board.MovesCount > 0)
            {
                SetMoveParams(tree.Board.LastMove);
            }

            tree.Root = this;
        }

        internal virtual void Join(IEnumerable<Node> children)
        {
            Children = children.ToArray();
            Array.ForEach(Children, child => child.Parent = this);
        }

        internal virtual void RemoveChildren()
        {
            Array.ForEach(Children, child => child.Parent = null);
            Children = null;
        }

        public void SortChildren(Comparison<Node> comparison)
        {
            var children = Children;

            if (children == null)
            {
                return;
            }

            Array.Sort(children, comparison);
        }

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
            var children = Children;

            if (children == null)
            {
                yield break;
            }

            foreach (var child in children)
            {
                if (Children != children)
                {
                    throw new InvalidOperationException("Коллекция изменена во время перечисления.");
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

        public int StartX => _startX >= 0 ? _startX : throw new InvalidOperationException("Этот узел не хранит координат полей.");

        public int StartY => _startY >= 0 ? _startY : throw new InvalidOperationException("Этот узел не хранит координат полей.");

        public int DestinationX => _destinationX >= 0 ? _destinationX : throw new InvalidOperationException("Этот узел не хранит координат полей.");

        public int DestinationY => _destinationY >= 0 ? _destinationY : throw new InvalidOperationException("Этот узел не хранит координат полей.");

        public bool IsPawnPromotion => _newPieceName != default;

        public PieceName NewPieceName => IsPawnPromotion ? _newPieceName :
        throw new InvalidOperationException("Это свойство может быть вычислено только для узла, соотв. превращению пешки.");

        public int ChildrenCount
        {
            get
            {
                var children = Children;
                return children == null ? 0 : children.Length;
            }
        }

        public bool HasChildren => Children != null;
    }
}
