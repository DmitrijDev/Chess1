using Chess.LogicPart;

namespace Chess
{
    internal class GameMenu : ToolStripMenuItem
    {
        private readonly GameForm _form;

        private readonly ToolStripMenuItem _whitePlayerMenu = new("Белые");
        private readonly ToolStripMenuItem _blackPlayerMenu = new("Черные");
        private readonly ToolStripMenuItem _timeMenu = new("Время на партию");

        public GameMenu(GameForm form) : base("Игра")
        {
            _form = form;

            var startGameItem = new ToolStripMenuItem("Новая игра");
            var escapeItem = new ToolStripMenuItem("Выход");

            DropDownItems.Add(startGameItem);
            DropDownItems.Add(_whitePlayerMenu);
            DropDownItems.Add(_blackPlayerMenu);
            DropDownItems.Add(_timeMenu);
            DropDownItems.Add(escapeItem);

            startGameItem.Click += new EventHandler(StartNewGame);
            BuildChangePlayerMenus();
            BuildTimeMenu();
            escapeItem.Click += new EventHandler(Escape);
        }

        private void BuildChangePlayerMenus()
        {
            var userPlaysWhiteItem = new ToolStripMenuItem("Вы") { CheckOnClick = true, Checked = true };
            var programPlaysWhiteItem = new ToolStripMenuItem("Программа") { CheckOnClick = true, Checked = false };
            var userPlaysBlackItem = new ToolStripMenuItem("Вы") { CheckOnClick = true, Checked = false };
            var programPlaysBlackItem = new ToolStripMenuItem("Программа") { CheckOnClick = true, Checked = true };

            _whitePlayerMenu.DropDownItems.Add(userPlaysWhiteItem);
            _whitePlayerMenu.DropDownItems.Add(programPlaysWhiteItem);
            _blackPlayerMenu.DropDownItems.Add(userPlaysBlackItem);
            _blackPlayerMenu.DropDownItems.Add(programPlaysBlackItem);

            userPlaysWhiteItem.Click += new EventHandler(ChangeWhitePlayer);
            programPlaysWhiteItem.Click += new EventHandler(ChangeWhitePlayer);
            userPlaysBlackItem.Click += new EventHandler(ChangeBlackPlayer);
            programPlaysBlackItem.Click += new EventHandler(ChangeBlackPlayer);
        }

        private void BuildTimeMenu()
        {
            var items = new ToolStripMenuItem[7]
            {
                new ToolStripMenuItem("5 минут"), new ToolStripMenuItem("15 минут"), new ToolStripMenuItem("30 минут"), new ToolStripMenuItem("1 час"),
                    new ToolStripMenuItem("1,5 часа"), new ToolStripMenuItem("2 часа"), new ToolStripMenuItem("3 часа")
            };

            _timeMenu.DropDownItems.AddRange(items);
            Array.ForEach(items, item => item.CheckOnClick = true);
            items[0].Checked = true;
            Array.ForEach(items, item => item.Click += new EventHandler(SelectTimeControl));
        }

        private void StartNewGame(object sender, EventArgs e) => _form.GamePanel.StartNewGame();

        private void ChangeWhitePlayer(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;

            if (!menuItem.Checked)
            {
                menuItem.Checked = true;
                return;
            }

            foreach (var obj in _whitePlayerMenu.DropDownItems)
            {
                var item = (ToolStripMenuItem)obj;
                item.Checked = false;
            }

            menuItem.Checked = true;
            _form.GamePanel.ChangePlayer(PieceColor.White);
        }

        private void ChangeBlackPlayer(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;

            if (!menuItem.Checked)
            {
                menuItem.Checked = true;
                return;
            }

            foreach (var obj in _blackPlayerMenu.DropDownItems)
            {
                var item = (ToolStripMenuItem)obj;
                item.Checked = false;
            }

            menuItem.Checked = true;
            _form.GamePanel.ChangePlayer(PieceColor.Black);
        }

        private void SelectTimeControl(object sender, EventArgs e)
        {
            var clickedItem = (ToolStripMenuItem)sender;

            if (!clickedItem.Checked)
            {
                clickedItem.Checked = true;
                return;
            }

            foreach (var obj in _timeMenu.DropDownItems)
            {
                var item = (ToolStripMenuItem)obj;
                item.Checked = false;
            }

            clickedItem.Checked = true;

            var texts = new string[7] { "5 минут", "15 минут", "30 минут", "1 час", "1,5 часа", "2 часа", "3 часа" };
            var timeForGameValues = new int[7] { 300, 900, 1800, 3600, 5400, 7200, 10800 };

            for (var i = 0; ; ++i)
            {
                if (texts[i] == clickedItem.Text)
                {
                    _form.GamePanel.SetTimeControl(timeForGameValues[i]);
                    return;
                }
            }
        }

        private void Escape(object sender, EventArgs e) => _form.Close();
    }
}
