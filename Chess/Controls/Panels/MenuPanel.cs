
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
            _form.SizeChanged += new EventHandler(ChangeWidth);
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(menuStrip);

            menuStrip.Items.Add(new GameMenu(_form));
            menuStrip.Items.Add(new ViewMenu(_form));

            _form.MouseMove += new MouseEventHandler(MovePanel);
        }

        public void MovePanel(object sender, EventArgs e)
        {
            if (!_form.HidesMenus)
            {
                return;
            }

            var formTitleHeight = _form.RectangleToScreen(_form.ClientRectangle).Top - _form.Top;

            if (Cursor.Position.Y <= _form.Location.Y + formTitleHeight + Height)
            {
                Location = new Point(0, 0);
            }
            else
            {
                Location = new Point(0, -Height);
            }
        }

        public void ChangeWidth(object sender, EventArgs e) => Width = _form.Width;
    }
}
