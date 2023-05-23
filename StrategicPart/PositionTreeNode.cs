using Chess.LogicPart;

namespace Chess.StrategicPart
{
    internal class PositionTreeNode
    {
        public sbyte StartSquareVertical { get; } = -1;

        public sbyte StartSquareHorizontal { get; } = -1;

        public sbyte MoveSquareVertical { get; } = -1;

        public sbyte MoveSquareHorizontal { get; } = -1;

        public sbyte NewPieceName { get; } = -1;

        public short Evaluation { get; set; } = short.MinValue;

        public List<PositionTreeNode> Children { get; private set; }

        public PositionTreeNode()
        { }

        public PositionTreeNode(Move move)
        {
            StartSquareVertical = (sbyte)move.StartSquare.Vertical;
            StartSquareHorizontal = (sbyte)move.StartSquare.Horizontal;
            MoveSquareVertical = (sbyte)move.MoveSquare.Vertical;
            MoveSquareHorizontal = (sbyte)move.MoveSquare.Horizontal;

            if (move.NewPieceSelected)
            {
                NewPieceName = (sbyte)move.NewPiece.Name;
            }
        }

        public void AddChild(PositionTreeNode newChild)
        {
            Children ??= new List<PositionTreeNode>();
            Children.Add(newChild);
        }

        public void RemoveChildren() => Children = null;

        public bool IsEvaluated => Evaluation != short.MinValue;
    }
}
