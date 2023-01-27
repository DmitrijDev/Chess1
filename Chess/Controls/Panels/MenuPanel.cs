
namespace Chess
{
    internal class MenuPanel : Panel
    {
        private readonly GameForm _form;

        public MenuPanel(GameForm form)
        {
            _form = form;
            var menuStrip = new MenuStrip();
            Height = menuStrip.Height;
            Width = _form.ClientRectangle.Width;
            _form.SizeChanged += (sender, e) => Width = _form.ClientRectangle.Width;
            BorderStyle = BorderStyle.None;
            BackColor = _form.PanelColor;
            Controls.Add(menuStrip);

            menuStrip.Items.Add(new GameMenu(_form));
            menuStrip.Items.Add(new ViewMenu(_form));
        }
    }
}
