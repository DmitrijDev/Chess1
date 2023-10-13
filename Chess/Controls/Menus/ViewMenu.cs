
namespace Chess
{
    internal class ViewMenu : ToolStripMenuItem
    {
        private readonly GameForm _gameForm;
        private readonly ToolStripMenuItem _colorsMenu;

        public ViewMenu(GameForm gameForm) : base("Вид")
        {
            _gameForm = gameForm;

            // Разворот доски.
            var menuItem = new ToolStripMenuItem("Развернуть доску");
            DropDownItems.Add(menuItem);
            menuItem.Click += (sender, e) => _gameForm.GamePanel.Rotate();

            // Выбор цветов.
            _colorsMenu = GetColorsMenu();
            DropDownItems.Add(_colorsMenu);

            // Изменение размера.
            menuItem = new ToolStripMenuItem("Изменить размер доски");
            DropDownItems.Add(menuItem);
            menuItem.Click += (sender, e) => new GamePanelSizeForm(_gameForm.GamePanel).ShowDialog();
        }

        private ToolStripMenuItem GetColorsMenu()
        {
            var menu = new ToolStripMenuItem("Выбор цветов");
            var itemTexts = new string[] { "Стандартные", "Черно-белые поля, цветные фигуры", "Зима", "Весна", "Лето", "Осень" };

            foreach (var text in itemTexts)
            {
                var item = new ToolStripMenuItem(text) { CheckOnClick = true };
                menu.DropDownItems.Add(item);
                item.Click += ColorsMenuItem_Click;
            }

            var firstItem = (ToolStripMenuItem)menu.DropDownItems[0];
            firstItem.Checked = true;
            return menu;
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
