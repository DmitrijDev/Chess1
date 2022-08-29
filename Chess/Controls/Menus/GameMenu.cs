
namespace Chess
{
    internal class GameMenu: ToolStripMenuItem
    {
        private readonly GameForm _form;

        public GameMenu(GameForm form): base("Игра")
        {
            _form = form;

            var startGameItem = new ToolStripMenuItem("Новая игра");
            var escapeItem = new ToolStripMenuItem("Выход");

            DropDownItems.Add(startGameItem);
            DropDownItems.Add(escapeItem);

            startGameItem.Click += new EventHandler(StartNewGame);
            escapeItem.Click += new EventHandler(Escape);
        }

        public void StartNewGame(object sender, EventArgs e) => _form.StartNewGame();

        public void Escape(object sender, EventArgs e) => _form.Close();
    }
}
