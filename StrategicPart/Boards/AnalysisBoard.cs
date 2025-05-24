using Chess.LogicPart;

namespace Chess.StrategicPart
{
    public class AnalysisBoard : ChessBoard
    {
        private Func<AnalysisBoard, int> _evaluate = new(board => 0);

        public AnalysisBoard() { }

        public virtual int Evaluate() => _evaluate(this);

        public Func<AnalysisBoard, int> EvaluateFunc
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
