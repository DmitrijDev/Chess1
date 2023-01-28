
namespace Chess
{
    internal class ViewMenu : ToolStripMenuItem
    {
        private readonly GameForm _form;
        private readonly ToolStripMenuItem _allowGamePanelDragItem = new("Перетаскивание доски мышью");

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
        }
    }
}
