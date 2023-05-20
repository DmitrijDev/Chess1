
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

            var colorsMenuItems = new ToolStripMenuItem[] { new ToolStripMenuItem("Стандартные"), new ToolStripMenuItem("Черно-белые поля, цветные фигуры"),
                new ToolStripMenuItem("Зима"), new ToolStripMenuItem("Весна"), new ToolStripMenuItem("Лето"), new ToolStripMenuItem("Осень")  };

            Array.ForEach(colorsMenuItems, item => item.CheckOnClick = true);
            colorsMenuItems[0].Checked = true;
            _colorsMenu.DropDownItems.AddRange(colorsMenuItems);
            Array.ForEach(colorsMenuItems, item => item.Click += ColorsMenuItem_Click);            

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
