
namespace Chess
{
    internal class MenuPanel : Panel
    {
        public MenuPanel(GameForm form)
        {
            var menuStrip = new MenuStrip();
            Height = menuStrip.Height;
            Width = form.ClientRectangle.Width;
            form.SizeChanged += (sender, e) => Width = form.ClientRectangle.Width;
            BorderStyle = BorderStyle.None;
            BackColor = form.PanelColor;
            Controls.Add(menuStrip);

            menuStrip.Items.Add(new GameMenu(form));
            menuStrip.Items.Add(new ViewMenu(form));
        }
    }
}
