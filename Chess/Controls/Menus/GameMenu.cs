using Chess.LogicPart;

namespace Chess
{
    internal class GameMenu : ToolStripMenuItem
    {
        private readonly GameForm _form;

        private readonly ToolStripMenuItem _userPlaysWhiteItem;
        private readonly ToolStripMenuItem _programPlaysWhiteItem;
        private readonly ToolStripMenuItem _userPlaysBlackItem;
        private readonly ToolStripMenuItem _programPlaysBlackItem;

        public GameMenu(GameForm form) : base("Игра")
        {
            _form = form;

            var startGameItem = new ToolStripMenuItem("Новая игра");
            var whitePlayerItem = new ToolStripMenuItem("Белые");
            var blackPlayerItem = new ToolStripMenuItem("Черные");
            var escapeItem = new ToolStripMenuItem("Выход");

            DropDownItems.Add(startGameItem);
            DropDownItems.Add(whitePlayerItem);
            DropDownItems.Add(blackPlayerItem);
            DropDownItems.Add(escapeItem);

            _userPlaysWhiteItem = new ToolStripMenuItem("Вы")
            {
                CheckOnClick = true,
                Checked = true
            };

            _programPlaysWhiteItem = new ToolStripMenuItem("Программа")
            {
                CheckOnClick = true,
                Checked = false
            };

            _userPlaysBlackItem = new ToolStripMenuItem("Вы")
            {
                CheckOnClick = true,
                Checked = false
            };

            _programPlaysBlackItem = new ToolStripMenuItem("Программа")
            {
                CheckOnClick = true,
                Checked = true
            };

            whitePlayerItem.DropDownItems.Add(_userPlaysWhiteItem);
            whitePlayerItem.DropDownItems.Add(_programPlaysWhiteItem);
            blackPlayerItem.DropDownItems.Add(_userPlaysBlackItem);
            blackPlayerItem.DropDownItems.Add(_programPlaysBlackItem);

            startGameItem.Click += new EventHandler(StartNewGame);
            escapeItem.Click += new EventHandler(Escape);

            _userPlaysWhiteItem.Click += new EventHandler(ChangeWhitePlayer);
            _programPlaysWhiteItem.Click += new EventHandler(ChangeWhitePlayer);
            _userPlaysBlackItem.Click += new EventHandler(ChangeBlackPlayer);
            _programPlaysBlackItem.Click += new EventHandler(ChangeBlackPlayer);
        }

        private void StartNewGame(object sender, EventArgs e) => _form.StartNewGame();

        private void ChangeWhitePlayer(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;

            if (!menuItem.Checked)
            {
                menuItem.Checked = true;
                return;
            }

            _userPlaysWhiteItem.Checked = false;
            _programPlaysWhiteItem.Checked = false;
            menuItem.Checked = true;
            _form.ChangePlayer(PieceColor.White);
        }

        private void ChangeBlackPlayer(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;

            if (!menuItem.Checked)
            {
                menuItem.Checked = true;
                return;
            }

            _userPlaysBlackItem.Checked = false;
            _programPlaysBlackItem.Checked = false;
            menuItem.Checked = true;
            _form.ChangePlayer(PieceColor.Black);
        }

        private void Escape(object sender, EventArgs e) => _form.Close();
    }
}
