using Chess.LogicPart;

namespace Chess.StrategicPart;

public class AnalysisBoard_Type1 : AnalysisBoard
{
    private Func<ChessPiece, int> _evaluatePieceFunc = new(piece => 0);
    private Func<AnalysisBoard_Type1, int> _evaluatePositionFunc = new(board => 0);

    public int MaterialValue { get; private set; }

    public AnalysisBoard_Type1() 
    { 
        PositionSet += () => MaterialValue = GetMaterial().Where(piece => piece.Name != PieceName.King).
        Select(piece => _evaluatePieceFunc(piece)).Sum();

        MakingMove += (move) =>
        {
            if (move.IsCapture)
            {
                var capturedPiece = !move.IsEnPassantCapture ? GetPiece(move.Destination) :
                GetPiece(move.Destination.X, move.Start.Y);

                MaterialValue -= _evaluatePieceFunc(capturedPiece);
            }

            if (move.IsPawnPromotion)
            {
                var pawn = GetPiece(move.Start);
                MaterialValue -= _evaluatePieceFunc(pawn);
            }
        };

        MoveMade += () =>
        {
            if (LastMove.IsPawnPromotion)
            {
                var newPiece = GetPiece(LastMove.Destination);
                MaterialValue += _evaluatePieceFunc(newPiece);
            }
        };

        CancellingMove += () =>
        {
            if (LastMove.IsPawnPromotion)
            {
                var newPiece = GetPiece(LastMove.Destination);
                MaterialValue -= _evaluatePieceFunc(newPiece);
            }
        };

        MoveCancelled += (move) =>
        {
            if (move.IsCapture)
            {
                var capturedPiece = !move.IsEnPassantCapture ? GetPiece(move.Destination) :
                GetPiece(move.Destination.X, move.Start.Y);

                MaterialValue += _evaluatePieceFunc(capturedPiece);
            }

            if (move.IsPawnPromotion)
            {
                var pawn = GetPiece(move.Start);
                MaterialValue += _evaluatePieceFunc(pawn);
            }
        };
    }  
    
    public override int Evaluate() => _evaluatePositionFunc(this);

    public Func<ChessPiece, int> EvaluatePieceFunc
    {
        get => _evaluatePieceFunc;

        set
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }

            _evaluatePieceFunc = value;
        }
    }

    public Func<AnalysisBoard_Type1, int> EvaluatePositionFunc
    {
        get => _evaluatePositionFunc;

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
