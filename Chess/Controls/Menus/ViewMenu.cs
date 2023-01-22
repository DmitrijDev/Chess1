
namespace Chess
{
    internal class ViewMenu : ToolStripMenuItem
    {
        private readonly GameForm _form;

        public ViewMenu(GameForm form) : base("Вид")
        {
            _form = form;
            var menuItem = new ToolStripMenuItem("Доску по центру");
            DropDownItems.Add(menuItem);
            menuItem.Click += new EventHandler(_form.PutGamePanelToCenter);
        }
    }
}
