using Chess.LogicPart;

namespace Chess.TacticalPart
{
    public class MaterialEvaluatingBoard : ChessBoard
    {
        private Func<ChessPiece, int> _evaluate = new(piece => 0);

        public int MaterialValue { get; private set; }

        public MaterialEvaluatingBoard() : base()
        { }

        protected override void DoAfterClear() => MaterialValue = 0;

        protected override void DoAfterPositionSet() => MaterialValue = GetMaterial().Where(piece => piece.Name != ChessPieceName.King).
            Select(piece => _evaluate(piece)).Sum();

        protected override void DoAfterMove()
        {
            var lastMove = GetLastMove();

            if (lastMove.IsCapture)
            {
                MaterialValue -= _evaluate(lastMove.CapturedPiece);
            }

            if (lastMove.IsPawnPromotion)
            {
                MaterialValue -= _evaluate(lastMove.MovingPiece);
                MaterialValue += _evaluate(lastMove.NewPiece);
            }
        }

        protected override void DoBeforeTakingBack()
        {
            var lastMove = GetLastMove();

            if (lastMove.IsCapture)
            {
                MaterialValue += _evaluate(lastMove.CapturedPiece);
            }

            if (lastMove.IsPawnPromotion)
            {
                MaterialValue += _evaluate(lastMove.MovingPiece);
                MaterialValue -= _evaluate(lastMove.NewPiece);
            }
        }

        public Func<ChessPiece, int> EvaluatePiece
        {
            get => _evaluate;

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                _evaluate = value;
            }
        }
    }
}
