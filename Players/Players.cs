using Chess.StrategicPart;

namespace Chess.Players
{
    public static class Players
    {
        private static VirtualPlayer[] GetAllPlayers() => new VirtualPlayer[]
        {
            new (Strategy.BuildTree, Strategy.Traverse, Strategy.EvaluatePiece, Strategy.Evaluate)
        };

        public static VirtualPlayer GetNewPlayer(int index) => new(GetAllPlayers()[index]);
    }
}
