
namespace Chess
{
    internal static class Program
    {
        public static bool ThinkingDisabled { get; internal set; } // Партия может быть прервана, пока программа думает, тогда ей нужно перестать думать. 

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new GameForm());
        }
    }
}

