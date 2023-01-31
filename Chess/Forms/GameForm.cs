
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
            SetPanels();

            SizeChanged += Form_SizeChanged;
            GamePanel.SizeChanged += GamePanel_SizeChanged;
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

            var minWidth = Math.Max(GamePanel.Width, TimePanel.MinimumSize.Width) + (Width - ClientRectangle.Width);
            var minHeight = MenuPanel.Height + TimePanel.Height + GamePanel.Height + (Height - ClientRectangle.Height);
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

        private void GamePanel_MouseDown(object sender, EventArgs e)
        {
            if (!GamePanelDragEnabled)
            {
                return;
            }

            var captionHeight = GetCaptionHeight();
            _fromDragCursorToGamePanelLeft = (Cursor.Position.X - Location.X) - (ClientRectangle.Left + GamePanel.Location.X);
            _fromDragCursorToGamePanelTop = (Cursor.Position.Y - Location.Y) - (captionHeight + ClientRectangle.Top + GamePanel.Location.Y);
            DoDragDrop(GamePanel, DragDropEffects.None);
        }

        private void DragGamePanel(object sender, EventArgs e)
        {
            var captionHeight = GetCaptionHeight();
            var newGamePanelX = (Cursor.Position.X - (Location.X + ClientRectangle.Left)) - _fromDragCursorToGamePanelLeft;
            var newGamePanelY = (Cursor.Position.Y - (Location.Y + captionHeight + ClientRectangle.Top )) - _fromDragCursorToGamePanelTop;

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
                _fromDragCursorToGamePanelTop = Math.Max((Cursor.Position.Y - Location.Y) - (captionHeight + ClientRectangle.Top +  MenuPanel.Height + TimePanel.Height), 0);
            }

            if (newGamePanelY > ClientRectangle.Height - GamePanel.Height)
            {
                newGamePanelY = ClientRectangle.Height - GamePanel.Height;
                _fromDragCursorToGamePanelTop = Math.Min(Cursor.Position.Y - (Location.Y + captionHeight + ClientRectangle.Top  + GamePanel.Location.Y), GamePanel.Height);
            }

            GamePanel.Location = new Point(newGamePanelX, newGamePanelY);
        }

        private void Form_SizeChanged(object sender, EventArgs e)
        {
            var newGamePanelX = GamePanel.Location.X;
            var newGamePanelY = GamePanel.Location.Y;

            if (newGamePanelX < 0)
            {
                newGamePanelX = 0;
            }

            if (newGamePanelY < MenuPanel.Height + TimePanel.Height)
            {
                newGamePanelY = MenuPanel.Height + TimePanel.Height;
            }

            if (ClientRectangle.Width < newGamePanelX + GamePanel.Width)
            {
                newGamePanelX -= newGamePanelX + GamePanel.Width - ClientRectangle.Width;
            }

            if (ClientRectangle.Height < newGamePanelY + GamePanel.Height)
            {
                newGamePanelY -= newGamePanelY + GamePanel.Height - ClientRectangle.Height;
            }

            GamePanel.Location = new Point(newGamePanelX, newGamePanelY);
        }

        private void GamePanel_SizeChanged(object sender, EventArgs e)
        {
            var minWidth = Math.Max(GamePanel.Width, TimePanel.MinimumSize.Width) + (Width - ClientRectangle.Width);
            var minHeight = MenuPanel.Height + TimePanel.Height + GamePanel.Height + (Height - ClientRectangle.Height);
            MinimumSize = new Size(minWidth, minHeight);

            if (GamePanel.Location.X + GamePanel.Width > ClientRectangle.Width)
            {
                GamePanel.Location = new Point(ClientRectangle.Width - GamePanel.Width, GamePanel.Location.Y);
            }

            if (GamePanel.Location.Y + GamePanel.Height > ClientRectangle.Height)
            {
                GamePanel.Location = new Point(GamePanel.Location.X, ClientRectangle.Height - GamePanel.Height);
            }
        }

        public Color PanelColor => DefaultBackColor;

        public int TimeFontSize => MenuPanel.Font.Height;
    }
}
