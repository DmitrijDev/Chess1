
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
            menuItem.Click += new EventHandler(PutGamePanelToCenter);

            menuItem = new ToolStripMenuItem("Развернуть доску");
            DropDownItems.Add(menuItem);
            menuItem.Click += new EventHandler(RotateGamePanel);
        }

        private void PutGamePanelToCenter(object sender, EventArgs e) => _form.PutGamePanelToCenter();

        private void RotateGamePanel(object sender, EventArgs e) => _form.GamePanel.Rotate();
    }
}
