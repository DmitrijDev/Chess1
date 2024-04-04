using Chess.LogicPart;

namespace Chess.StrategicPart
{
    public class AnalysisBoard : ChessBoard
    {
        private Func<AnalysisBoard, int> _evaluateFunc = new(board => 0);

        public AnalysisBoard() : base()
        { }

        public virtual int Evaluate() => _evaluateFunc(this);

        public Func<AnalysisBoard, int> EvaluateFunc
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                _evaluateFunc = value;
            }
        }
    }
}
