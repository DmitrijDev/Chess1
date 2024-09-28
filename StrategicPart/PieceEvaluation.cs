using Chess.LogicPart;

namespace Chess.StrategicPart
{
    public static class PieceEvaluation
    {       
        public static int GetBasicValue(this ChessPiece piece)
        {
            var result = piece.Name switch
            {
                PieceName.Pawn => 100,
                PieceName.Knight => 300,
                PieceName.Bishop => 300,
                PieceName.Rook => 500,
                PieceName.Queen => 900,
                _ => throw new InvalidOperationException("Короля невозможно оценить в баллах."),
            };

            if (piece.Color == PieceColor.Black)
            {
                result = -result;
            }

            return result;
        }
    }
}
