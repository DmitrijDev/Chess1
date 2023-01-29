
namespace Chess
{
    public partial class GameForm : Form
    {
        private int _fromDragCursorToGamePanelLeft;
        private int _fromDragCursorToGamePanelTop;

        internal MenuPanel MenuPanel { get; private set; }

        internal TimePanel TimePanel { get; private set; }

        internal GamePanel GamePanel { get; private set; }

        public bool GamePanelDragEnabled { get; internal set; } = true;

        public GameForm()
        {
            InitializeComponent();
            Text = "";
            BackColor = Color.Olive;            
            SetPanels();

            SizeChanged += Form_SizeChanged;
            GamePanel.MouseDown += GamePanel_MouseDown;
            QueryContinueDrag += DragGamePanel;

            GamePanel.StartNewGame();
        }

        private void SetPanels()
        {
            MenuPanel = new MenuPanel(this);
            TimePanel = new TimePanel(this);
            GamePanel = new GamePanel(this);

            var shift = Screen.PrimaryScreen.WorkingArea.Height / 16;
            Width = Math.Max(shift * 2 + GamePanel.Width, TimePanel.MinimumSize.Width);
            Width += Width - ClientRectangle.Width;
            Height = shift * 2 + MenuPanel.Height + TimePanel.Height + GamePanel.Height;
            Height += Height - ClientRectangle.Height;

            TimePanel.Location = new Point(0, MenuPanel.Height);
            PutGamePanelToCenter();

            Controls.Add(MenuPanel);
            Controls.Add(TimePanel);
            Controls.Add(GamePanel);

            var minWidth = Math.Max(Width - shift * 2, TimePanel.MinimumSize.Width);
            var minHeight = Height - shift * 2;
            MinimumSize = new Size(minWidth, minHeight);
        }

        public int GetCaptionHeight()
        {
            var clientRectangleToScreen = RectangleToScreen(ClientRectangle);
            return clientRectangleToScreen.Top - Top;
        }

        internal void PutGamePanelToCenter()
        {
            var gamePanelX = (ClientRectangle.Width - GamePanel.Width) / 2;
            var gamePanelY = MenuPanel.Height + TimePanel.Height + (ClientRectangle.Height - MenuPanel.Height - TimePanel.Height - GamePanel.Height) / 2;
            GamePanel.Location = new Point(gamePanelX, gamePanelY);
        }

        internal void ShowMessage(string message) => MessageBox.Show(message, "", MessageBoxButtons.OK);

        private void GamePanel_MouseDown(object sender, EventArgs e)
        {
            if (!GamePanelDragEnabled)
            {
                return;
            }

            var captionHeight = GetCaptionHeight();
            _fromDragCursorToGamePanelLeft = (Cursor.Position.X - Location.X) - (ClientRectangle.Left + GamePanel.Location.X);
            _fromDragCursorToGamePanelTop = (Cursor.Position.Y - Location.Y) - (ClientRectangle.Top + captionHeight + GamePanel.Location.Y);
            DoDragDrop(GamePanel, DragDropEffects.None);
        }

        private void DragGamePanel(object sender, EventArgs e)
        {
            var captionHeight = GetCaptionHeight();
            var newGamePanelX = (Cursor.Position.X - (Location.X + ClientRectangle.Left)) - _fromDragCursorToGamePanelLeft;
            var newGamePanelY = (Cursor.Position.Y - (Location.Y + ClientRectangle.Top + captionHeight)) - _fromDragCursorToGamePanelTop;

            if (newGamePanelX < 0)
            {
                newGamePanelX = 0;
                _fromDragCursorToGamePanelLeft = Math.Max(Cursor.Position.X - (Location.X + ClientRectangle.Left), 0);
            }

            if (newGamePanelX > ClientRectangle.Width - GamePanel.Width)
            {
                newGamePanelX = ClientRectangle.Width - GamePanel.Width;
                _fromDragCursorToGamePanelLeft = Math.Min(Cursor.Position.X - (Location.X + ClientRectangle.Left + ClientRectangle.Width - GamePanel.Width), GamePanel.Width);
            }

            if (newGamePanelY < MenuPanel.Height + TimePanel.Height)
            {
                newGamePanelY = MenuPanel.Height + TimePanel.Height;
                _fromDragCursorToGamePanelTop = Math.Max((Cursor.Position.Y - Location.Y) - (ClientRectangle.Top + captionHeight + MenuPanel.Height + TimePanel.Height), 0);
            }

            if (newGamePanelY > ClientRectangle.Height - GamePanel.Height)
            {
                newGamePanelY = ClientRectangle.Height - GamePanel.Height;
                _fromDragCursorToGamePanelTop = Math.Min(Cursor.Position.Y - (Location.Y + ClientRectangle.Top + captionHeight + GamePanel.Location.Y), GamePanel.Height);
            }

            GamePanel.Location = new Point(newGamePanelX, newGamePanelY);
        }

        private void Form_SizeChanged(object sender, EventArgs e)
        {
            var newGamePanelX = GamePanel.Location.X;
            var newGamePanelY = GamePanel.Location.Y;

            if (ClientRectangle.Width < GamePanel.Location.X + GamePanel.Width)
            {
                newGamePanelX -= GamePanel.Location.X + GamePanel.Width - ClientRectangle.Width;
            }

            if (ClientRectangle.Height < GamePanel.Location.Y + GamePanel.Height)
            {
                newGamePanelY -= GamePanel.Location.Y + GamePanel.Height - ClientRectangle.Height;
            }

            GamePanel.Location = new Point(newGamePanelX, newGamePanelY);
        }

        public Color PanelColor => DefaultBackColor;

        public int TimeFontSize => MenuPanel.Font.Height;

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
