using Chess.LogicPart;

namespace Chess.StrategicPart
{
    internal class PositionTree
    {
        public sbyte StartSquareVertical { get; } = -1;

        public sbyte StartSquareHorizontal { get; } = -1;

        public sbyte MoveSquareVertical { get; } = -1;

        public sbyte MoveSquareHorizontal { get; } = -1;

        public sbyte NewPieceName { get; } = -1;

        public int Evaluation { get; set; } = int.MinValue;

        public PositionTree[] Children { get; private set; }

        public PositionTree()
        { }

        public PositionTree(Move move)
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

        public void AddChidren(ChessBoard board) => Children = board.GetLegalMoves().Select(move => new PositionTree(move)).ToArray();

        public bool IsEvaluated => Evaluation != int.MinValue;
    }
}
