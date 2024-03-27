using Chess.LogicPart;

namespace Chess.TacticalPart
{
    public static class PositionEvaluation
    {
        public static int EvaluatePosition_1(MaterialEvaluatingBoard board) => board.Status switch
        {
            BoardStatus.WhiteWin => int.MaxValue,
            BoardStatus.BlackWin => -int.MaxValue,
            BoardStatus.Draw => 0,
            _ => board.MaterialValue
        };
    }
}
