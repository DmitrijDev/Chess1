
namespace Chess
{
    internal static class Program
    {
        private static GameForm GetGameForm()
        {
            try
            {
                return new GameForm(true);
            }

            catch
            {
                return new GameForm(false);
            }
        }

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(GetGameForm());
        }
    }
}
