
namespace Chess
{
    internal class ViewMenu : ToolStripMenuItem
    {
        private readonly GameForm _form;
        private readonly ToolStripMenuItem _allowGamePanelDragItem = new("Перетаскивание доски мышью");
        private readonly ToolStripMenuItem _colorsMenu = new("Выбор цветов полей");

        public ViewMenu(GameForm form) : base("Вид")
        {
            _form = form;

            var menuItem = new ToolStripMenuItem("Доску по центру");
            DropDownItems.Add(menuItem);
            menuItem.Click += (sender, e) => _form.PutGamePanelToCenter();

            menuItem = new ToolStripMenuItem("Развернуть доску");
            DropDownItems.Add(menuItem);
            menuItem.Click += (sender, e) => _form.GamePanel.Rotate();

            DropDownItems.Add(_allowGamePanelDragItem);
            _allowGamePanelDragItem.CheckOnClick = true;
            _allowGamePanelDragItem.Checked = _form.DraggingGamePanelAllowed;
            _allowGamePanelDragItem.Click += (sender, e) => _form.DraggingGamePanelAllowed = _allowGamePanelDragItem.Checked;

            DropDownItems.Add(_colorsMenu);
            _colorsMenu.DropDownItems.Add(new ToolStripMenuItem("Желтые и коричневые") { CheckOnClick = true, Checked = true });
            _colorsMenu.DropDownItems.Add(new ToolStripMenuItem("Серые") { CheckOnClick = true, Checked = false });

            foreach (var obj in _colorsMenu.DropDownItems)
            {
                var item = (ToolStripMenuItem)obj;
                item.Click += ColorsMenuItem_Click;
            }
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
                    _form.GamePanel.SetColors(i);
                    return;
                }
            }
        }
    }
}
