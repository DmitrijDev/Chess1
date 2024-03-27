
namespace Chess.VirtualPlayer
{
    public static class RobotsConstructor
    {
        public static IChessRobot GetRobot(int strengthLevel)
        {
            if (strengthLevel < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var robots = new SourceRobotsProvider().GetSourceRobots();

            if (strengthLevel >= robots.Length)
            {
                throw new ArgumentOutOfRangeException($"Максимальный уровень силы - {robots.Length - 1}.");
            }

            return robots[strengthLevel];
        }
    }
}
