using Chess.LogicPart;

namespace Chess.StrategicPart;

public class MaterialCheckingBoard : AnalysisBoard
{
    private Func<ChessPiece, int> _evaluatePieceFunc = new(piece => 0);
    private Func<MaterialCheckingBoard, int> _evaluatePositionFunc = new(board => 0);

    public int MaterialValue { get; private set; }

    public MaterialCheckingBoard() : base()
    { }

    protected override void DoAfterClear() => MaterialValue = 0;

    protected override void DoAfterPositionSet() => MaterialValue = GetMaterial().Where(piece => piece.Name != ChessPieceName.King).
        Select(piece => _evaluatePieceFunc(piece)).Sum();

    protected override void DoAfterMove()
    {
        var lastMove = GetLastMove();

        if (lastMove.IsCapture)
        {
            MaterialValue -= _evaluatePieceFunc(lastMove.CapturedPiece);
        }

        if (lastMove.IsPawnPromotion)
        {
            MaterialValue -= _evaluatePieceFunc(lastMove.MovingPiece);
            MaterialValue += _evaluatePieceFunc(lastMove.NewPiece);
        }
    }

    protected override void DoBeforeTakingBack()
    {
        var lastMove = GetLastMove();

        if (lastMove.IsCapture)
        {
            MaterialValue += _evaluatePieceFunc(lastMove.CapturedPiece);
        }

        if (lastMove.IsPawnPromotion)
        {
            MaterialValue += _evaluatePieceFunc(lastMove.MovingPiece);
            MaterialValue -= _evaluatePieceFunc(lastMove.NewPiece);
        }
    }

    public override int Evaluate() => _evaluatePositionFunc(this);

    public Func<ChessPiece, int> EvaluatePieceFunc
    {
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }

            _evaluatePieceFunc = value;
        }
    }

    public Func<MaterialCheckingBoard, int> EvaluatePositionFunc
    {
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }

            _evaluatePositionFunc = value;
        }
    }
}
