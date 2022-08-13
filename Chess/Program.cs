using System.Diagnostics;

namespace Chess
{
    internal static class Program
    {
        public static void EndWork(object sender, EventArgs e) => Process.GetCurrentProcess().Kill(); // Если пользователь закрыл форму.

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            var form = new GameForm();
            form.FormClosed += new FormClosedEventHandler(EndWork);
            Application.Run(form);
        }
    }
}

