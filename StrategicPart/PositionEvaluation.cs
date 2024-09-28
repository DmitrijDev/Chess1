using Chess.LogicPart;

namespace Chess.StrategicPart
{
    public static class PositionEvaluation
    {
        public static int EvaluatePosition_1(this AnalysisBoard_Type1 board) => board.Status switch
        {
            BoardStatus.WhiteWon => int.MaxValue,
            BoardStatus.BlackWon => -int.MaxValue,
            BoardStatus.Draw => 0,
            _ => board.MaterialValue
        };
    }
}
