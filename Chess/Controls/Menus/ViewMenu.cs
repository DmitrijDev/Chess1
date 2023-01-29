
namespace Chess
{
    internal class ViewMenu : ToolStripMenuItem
    {
        private readonly GameForm _gameForm;
        private readonly ToolStripMenuItem _enableGamePanelDragItem = new("Перетаскивание доски мышью");
        private readonly ToolStripMenuItem _colorsMenu = new("Выбор цветов");

        public ViewMenu(GameForm form) : base("Вид")
        {
            _gameForm = form;

            var menuItem = new ToolStripMenuItem("Доску по центру");
            DropDownItems.Add(menuItem);
            menuItem.Click += (sender, e) => _gameForm.PutGamePanelToCenter();

            menuItem = new ToolStripMenuItem("Развернуть доску");
            DropDownItems.Add(menuItem);
            menuItem.Click += (sender, e) => _gameForm.GamePanel.Rotate();

            DropDownItems.Add(_enableGamePanelDragItem);
            _enableGamePanelDragItem.CheckOnClick = true;
            _enableGamePanelDragItem.Checked = _gameForm.GamePanelDragEnabled;
            _enableGamePanelDragItem.Click += (sender, e) => _gameForm.GamePanelDragEnabled = _enableGamePanelDragItem.Checked;

            DropDownItems.Add(_colorsMenu);
            _colorsMenu.DropDownItems.Add(new ToolStripMenuItem("Осень") { CheckOnClick = true, Checked = true });
            _colorsMenu.DropDownItems.Add(new ToolStripMenuItem("Зима") { CheckOnClick = true, Checked = false });
            _colorsMenu.DropDownItems.Add(new ToolStripMenuItem("Весна") { CheckOnClick = true, Checked = false });
            _colorsMenu.DropDownItems.Add(new ToolStripMenuItem("Лето") { CheckOnClick = true, Checked = false });
            _colorsMenu.DropDownItems.Add(new ToolStripMenuItem("Черно-белые поля, цветные фигуры") { CheckOnClick = true, Checked = false });

            foreach (var obj in _colorsMenu.DropDownItems)
            {
                var item = (ToolStripMenuItem)obj;
                item.Click += ColorsMenuItem_Click;
            }

            menuItem = new ToolStripMenuItem("Изменить размер доски");
            DropDownItems.Add(menuItem);
            menuItem.Click += (sender, e) => new GamePanelSizeForm().ShowDialog();
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
