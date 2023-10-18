using Chess.LogicPart;

namespace Chess
{
    internal class GameMenu : ToolStripMenuItem
    {
        private readonly GameForm _form;

        private readonly ToolStripMenuItem _whitePlayerMenu;
        private readonly ToolStripMenuItem _blackPlayerMenu;
        private readonly ToolStripMenuItem _timeMenu;

        public GameMenu(GameForm form) : base("Игра")
        {
            _form = form;

            // Начало новой партии.
            var item = new ToolStripMenuItem("Новая партия");
            DropDownItems.Add(item);
            item.Click += (sender, e) => _form.GamePanel.StartNewGame();

            // Смена игроков.
            _whitePlayerMenu = GetChangePlayerMenu(ChessPieceColor.White);
            DropDownItems.Add(_whitePlayerMenu);

            _blackPlayerMenu = GetChangePlayerMenu(ChessPieceColor.Black);
            DropDownItems.Add(_blackPlayerMenu);

            // Смена контроля времени.
            _timeMenu = GetTimeMenu();
            DropDownItems.Add(_timeMenu);

            // Выход.
            item = new("Выход");
            DropDownItems.Add(item);
            item.Click += (sender, e) => _form.Close();
        }

        private ToolStripMenuItem GetChangePlayerMenu(ChessPieceColor color)
        {
            var menu = new ToolStripMenuItem(color == ChessPieceColor.White ? "Белые" : "Черные");

            var item1 = new ToolStripMenuItem("Вы") { CheckOnClick = true, Checked = color == ChessPieceColor.White };
            var item2 = new ToolStripMenuItem("Соперник") { CheckOnClick = true, Checked = color == ChessPieceColor.Black };

            menu.DropDownItems.Add(item1);
            menu.DropDownItems.Add(item2);

            item1.Click += ChangePlayerMenu_ItemClick;
            item2.Click += ChangePlayerMenu_ItemClick;

            return menu;
        }

        private ToolStripMenuItem GetTimeMenu()
        {
            var menu = new ToolStripMenuItem("Время на партию");
            var itemTexts = new string[] { "5 минут", "15 минут", "30 минут", "1 час", "1,5 часа", "2 часа", "3 часа" };

            foreach (var text in itemTexts)
            {
                var item = new ToolStripMenuItem(text) { CheckOnClick = true };
                menu.DropDownItems.Add(item);
                item.Click += TimeMenu_ItemClick;
            }

            var firstItem = (ToolStripMenuItem)menu.DropDownItems[0];
            firstItem.Checked = true;
            return menu;
        }

        private void ChangePlayerMenu_ItemClick(object sender, EventArgs e)
        {
            var clickedItem = (ToolStripMenuItem)sender;

            if (!clickedItem.Checked)
            {
                clickedItem.Checked = true;
                return;
            }

            var color = _whitePlayerMenu.DropDownItems.Contains(clickedItem) ? ChessPieceColor.White : ChessPieceColor.Black;
            var menu = color == ChessPieceColor.White ? _whitePlayerMenu : _blackPlayerMenu;

            foreach (var obj in menu.DropDownItems)
            {
                if (obj != sender)
                {
                    var item = (ToolStripMenuItem)obj;
                    item.Checked = false;
                    break;
                }
            }

            _form.GamePanel.ChangePlayer(color);
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
                if (obj == sender)
                {
                    continue;
                }

                var menuItem = (ToolStripMenuItem)obj;

                if (menuItem.Checked)
                {
                    menuItem.Checked = false;
                    break;
                }
            }

            var timeForGameValues = new int[] { 300, 900, 1800, 3600, 5400, 7200, 10800 };

            for (var i = 0; ; ++i)
            {
                if (_timeMenu.DropDownItems[i] == sender)
                {
                    _form.TimePanel.ResetTime(timeForGameValues[i]);
                    return;
                }
            }
        }
    }
}
