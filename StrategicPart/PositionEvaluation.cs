using Chess.LogicPart;

namespace Chess.StrategicPart
{
    public static class PositionEvaluation
    {
        public static int EvaluatePosition_1(this MaterialCheckingBoard board) => board.Status switch
        {
            BoardStatus.WhiteWin => int.MaxValue,
            BoardStatus.BlackWin => -int.MaxValue,
            BoardStatus.Draw => 0,
            _ => board.MaterialValue
        };
    }
}
