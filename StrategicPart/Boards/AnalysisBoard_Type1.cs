using Chess.LogicPart;

namespace Chess.StrategicPart;

public class AnalysisBoard_Type1 : AnalysisBoard
{
    private Func<ChessPiece, int> _evaluatePiece = new(piece => 0);
    private Func<AnalysisBoard_Type1, int> _evaluatePosition = new(board => 0);

    public int MaterialValue { get; private set; }

    public AnalysisBoard_Type1()
    {
        PositionSet += () => MaterialValue = GetMaterial().Where(piece => piece.Name != PieceName.King).
        Select(piece => _evaluatePiece(piece)).Sum();

        MakingMove += (move) =>
        {
            if (move.IsCapture)
            {
                var capturedPiece = move.IsEnPassantCapture ?
                GetPiece(move.Destination.X, move.Start.Y) : GetPiece(move.Destination);

                MaterialValue -= _evaluatePiece(capturedPiece);
            }

            if (move.IsPawnPromotion)
            {
                var pawn = GetPiece(move.Start);
                MaterialValue -= _evaluatePiece(pawn);
            }
        };

        MoveMade += () =>
        {
            if (LastMove.IsPawnPromotion)
            {
                var newPiece = GetPiece(LastMove.Destination);
                MaterialValue += _evaluatePiece(newPiece);
            }
        };

        CancellingMove += () =>
        {
            if (LastMove.IsPawnPromotion)
            {
                var newPiece = GetPiece(LastMove.Destination);
                MaterialValue -= _evaluatePiece(newPiece);
            }
        };

        MoveCancelled += (move) =>
        {
            if (move.IsCapture)
            {
                var capturedPiece = move.IsEnPassantCapture ? GetPiece(move.Destination.X, move.Start.Y) :
                GetPiece(move.Destination);

                MaterialValue += _evaluatePiece(capturedPiece);
            }

            if (move.IsPawnPromotion)
            {
                var pawn = GetPiece(move.Start);
                MaterialValue += _evaluatePiece(pawn);
            }
        };
    }

    public override int Evaluate() => _evaluatePosition(this);

    public Func<ChessPiece, int> EvaluatePieceFunc
    {
        get => _evaluatePiece;

        set
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }

            _evaluatePiece = value;
        }
    }

    public Func<AnalysisBoard_Type1, int> EvaluatePositionFunc
    {
        get => _evaluatePosition;

        set
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }

            _evaluatePosition = value;
        }
    }
}
