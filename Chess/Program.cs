
namespace Chess
{
    internal static class Program
    {
        public static bool ThinkingDisabled { get; internal set; } // ������ ����� ���� ��������, ���� ��������� ������, ����� �� ����� ��������� ������. 

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new GameForm());
        }
    }
}

