using Chess.LogicPart;

namespace Chess.ChessTree
{
    public class Node_Type1: Node
    {
        private PieceColor _movingPieceColor;

        internal override void SetMoveParams(Move move)
        {
            base.SetMoveParams(move);
            _movingPieceColor = move.MovingPieceColor;
        }

        public PieceColor MovingPieceColor => Depth > 0 ? _movingPieceColor : throw new InvalidOperationException("Этот узел не хранит цвета фигуры.");
    }
}
