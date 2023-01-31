
namespace Chess
{
    internal class ViewMenu : ToolStripMenuItem
    {
        private readonly GameForm _gameForm;
        private readonly ToolStripMenuItem _enableGamePanelDragItem = new("Перетаскивание доски мышью") { CheckOnClick = true };
        private readonly ToolStripMenuItem _colorsMenu = new("Выбор цветов");

        public ViewMenu(GameForm gameForm) : base("Вид")
        {
            _gameForm = gameForm;

            var menuItem = new ToolStripMenuItem("Доску по центру");
            DropDownItems.Add(menuItem);
            menuItem.Click += (sender, e) => _gameForm.PutGamePanelToCenter();

            menuItem = new ToolStripMenuItem("Развернуть доску");
            DropDownItems.Add(menuItem);
            menuItem.Click += (sender, e) => _gameForm.GamePanel.Rotate();

            // Перетаскивание доски.
            DropDownItems.Add(_enableGamePanelDragItem);
            _enableGamePanelDragItem.Checked = _gameForm.GamePanelDragEnabled;
            _enableGamePanelDragItem.Click += (sender, e) => _gameForm.GamePanelDragEnabled = _enableGamePanelDragItem.Checked;

            // Выбор цветов.
            DropDownItems.Add(_colorsMenu);
            var colorsMenuTexts = new string[6] { "Стандартные", "Черно-белые поля, цветные фигуры", "Зима", "Весна", "Лето", "Осень" };

            foreach (var text in colorsMenuTexts)
            {
                menuItem = new ToolStripMenuItem(text) { CheckOnClick = true };
                _colorsMenu.DropDownItems.Add(menuItem);
                menuItem.Click += ColorsMenuItem_Click;
            }

            var colorsMenuFirstItem = (ToolStripMenuItem)_colorsMenu.DropDownItems[0];
            colorsMenuFirstItem.Checked = true;

            // Изменение размера.
            menuItem = new ToolStripMenuItem("Изменить размер доски");
            DropDownItems.Add(menuItem);
            menuItem.Click += (sender, e) => new GamePanelSizeForm(_gameForm.GamePanel).ShowDialog();
        }

        private void ColorsMenuItem_Click(object sender, EventArgs e)
        {
            var clickedItem = (ToolStripMenuItem)sender;

            if (!clickedItem.Checked)
            {
                clickedItem.Checked = true;
                return;
            }

            foreach (var obj in _colorsMenu.DropDownItems)
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

            for (var i = 0; ; ++i)
            {
                if (_colorsMenu.DropDownItems[i] == sender)
                {
                    _gameForm.GamePanel.SetColors(i);
                    return;
                }
            }
        }
    }
}
