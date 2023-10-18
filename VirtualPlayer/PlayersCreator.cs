using Chess.StrategicPart;

namespace Chess.VirtualPlayer
{
    internal static class PlayersCreator
    {
        public static Player[] SamplePlayers { get; } = new Player[]
        {
            new (Strategy.BuildTree, Strategy.Traverse, Strategy.EvaluatePiece, Strategy.Evaluate)
        };
    }
}
