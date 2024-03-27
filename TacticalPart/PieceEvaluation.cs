using Chess.LogicPart;

namespace Chess.TacticalPart
{
    public static class PieceEvaluation
    {       
        public static int GetBasicValue(this ChessPiece piece)
        {
            var result = piece.Name switch
            {
                ChessPieceName.Pawn => 100,
                ChessPieceName.Knight => 300,
                ChessPieceName.Bishop => 300,
                ChessPieceName.Rook => 500,
                ChessPieceName.Queen => 900,
                _ => throw new ApplicationException("Короля невозможно оценить в баллах."),
            };

            if (piece.Color == ChessPieceColor.Black)
            {
                result = -result;
            }

            return result;
        }
    }
}
