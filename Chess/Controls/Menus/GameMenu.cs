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

            var startGameItem = new ToolStripMenuItem("Новая партия");
            var escapeItem = new ToolStripMenuItem("Выход");

            DropDownItems.Add(startGameItem);
            DropDownItems.Add(_whitePlayerMenu);
            DropDownItems.Add(_blackPlayerMenu);
            DropDownItems.Add(_timeMenu);
            DropDownItems.Add(escapeItem);

            startGameItem.Click += (sender, e) => _form.GamePanel.StartNewGame();
            BuildChangePlayerMenus();
            BuildTimeMenu();
            escapeItem.Click += (sender, e) => _form.Close();
        }

        private void BuildChangePlayerMenus()
        {
            var userPlaysWhiteItem = new ToolStripMenuItem("Вы") { CheckOnClick = true, Checked = true };
            var programPlaysWhiteItem = new ToolStripMenuItem("Соперник") { CheckOnClick = true, Checked = false };
            var userPlaysBlackItem = new ToolStripMenuItem("Вы") { CheckOnClick = true, Checked = false };
            var programPlaysBlackItem = new ToolStripMenuItem("Соперник") { CheckOnClick = true, Checked = true };

            _whitePlayerMenu.DropDownItems.Add(userPlaysWhiteItem);
            _whitePlayerMenu.DropDownItems.Add(programPlaysWhiteItem);
            _blackPlayerMenu.DropDownItems.Add(userPlaysBlackItem);
            _blackPlayerMenu.DropDownItems.Add(programPlaysBlackItem);

            userPlaysWhiteItem.Click += WhitePlayerMenu_ItemClick;
            programPlaysWhiteItem.Click += WhitePlayerMenu_ItemClick;
            userPlaysBlackItem.Click += BlackPlayerMenu_ItemClick;
            programPlaysBlackItem.Click += BlackPlayerMenu_ItemClick;
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
            Array.ForEach(items, item => item.Click += TimeMenu_ItemClick);
        }

        private void WhitePlayerMenu_ItemClick(object sender, EventArgs e)
        {
            var clickedItem = (ToolStripMenuItem)sender;

            if (!clickedItem.Checked)
            {
                clickedItem.Checked = true;
                return;
            }

            foreach (var obj in _whitePlayerMenu.DropDownItems)
            {
                var item = (ToolStripMenuItem)obj;

                if (obj != sender)
                {
                    item.Checked = false;
                }
            }

            _form.GamePanel.ChangePlayer(PieceColor.White);
        }

        private void BlackPlayerMenu_ItemClick(object sender, EventArgs e)
        {
            var clickedItem = (ToolStripMenuItem)sender;

            if (!clickedItem.Checked)
            {
                clickedItem.Checked = true;
                return;
            }

            foreach (var obj in _blackPlayerMenu.DropDownItems)
            {
                var item = (ToolStripMenuItem)obj;

                if (obj != sender)
                {
                    item.Checked = false;
                }
            }

            _form.GamePanel.ChangePlayer(PieceColor.Black);
        }

        private void TimeMenu_ItemClick(object sender, EventArgs e)
        {
            var clickedItem = (ToolStripMenuItem)sender;

            if (!clickedItem.Checked)
            {
                clickedItem.Checked = true;
                return;
            }

            foreach (var obj in _timeMenu.DropDownItems)
            {
                var menuItem = (ToolStripMenuItem)obj;

                if (menuItem.Checked && obj != sender)
                {
                    menuItem.Checked = false;
                    break;
                }
            }

            var timeForGameValues = new int[7] { 300, 900, 1800, 3600, 5400, 7200, 10800 };

            for (var i = 0; ; ++i)
            {
                if (_timeMenu.DropDownItems[i] == sender)
                {
                    _form.GamePanel.SetTimeControl(timeForGameValues[i]);
                    return;
                }
            }            
        }
    }
}
