
namespace Chess
{
    internal partial class GameForm : Form
    {
        private int _fromDragCursorToGamePanelLeft;
        private int _fromDragCursorToGamePanelTop;

        public MenuStrip MenuStrip { get; } = new();

        public TimePanel TimePanel { get; private set; }

        public GamePanel GamePanel { get; private set; }

        public GameForm()
        {
            InitializeComponent();
            Text = "Шахматы";
            Icon = new("Images/Icon.ico");
            SetControls();

            SizeChanged += Size_Changed;
            GamePanel.SizeChanged += GamePanel_SizeChanged;
            GamePanel.MouseDown += GamePanel_MouseDown;
            QueryContinueDrag += GamePanel_Drag;

            GamePanel.StartNewGame();
        }

        private void SetControls()
        {
            MenuStrip.Items.Add(new GameMenu(this));
            MenuStrip.Items.Add(new ViewMenu(this));

            TimePanel = new(this);
            GamePanel = new(this);

            var shift = Screen.PrimaryScreen.WorkingArea.Height / 16;
            Width = Math.Max(shift * 2 + GamePanel.Width, TimePanel.MinimumSize.Width);
            Width += Width - ClientRectangle.Width;
            Height = shift * 2 + MenuStrip.Height + TimePanel.Height + GamePanel.Height;
            Height += Height - ClientRectangle.Height;

            TimePanel.Location = new(0, MenuStrip.Height);
            TimePanel.Width = ClientRectangle.Width;
            PutGamePanelToCenter();

            Controls.Add(MenuStrip);
            Controls.Add(TimePanel);
            Controls.Add(GamePanel);

            var minWidth = Math.Max(GamePanel.Width, TimePanel.MinimumSize.Width) + (Width - ClientRectangle.Width);
            var minHeight = MenuStrip.Height + TimePanel.Height + GamePanel.Height + (Height - ClientRectangle.Height);
            MinimumSize = new(minWidth, minHeight);
        }

        public int GetCaptionHeight()
        {
            var clientRectangle = RectangleToScreen(ClientRectangle);
            return clientRectangle.Top - Top;
        }

        private void PutGamePanelToCenter()
        {
            var gamePanelX = (ClientRectangle.Width - GamePanel.Width) / 2;
            var gamePanelY = MenuStrip.Height + TimePanel.Height + (ClientRectangle.Height - MenuStrip.Height - TimePanel.Height - GamePanel.Height) / 2;
            GamePanel.Location = new(gamePanelX, gamePanelY);
        }

        private void GamePanel_MouseDown(object sender, EventArgs e)
        {
            var clientRectangle = RectangleToScreen(ClientRectangle);
            _fromDragCursorToGamePanelLeft = Cursor.Position.X - clientRectangle.Left - GamePanel.Location.X;
            _fromDragCursorToGamePanelTop = Cursor.Position.Y - clientRectangle.Top - GamePanel.Location.Y;
            DoDragDrop(GamePanel, DragDropEffects.None);
        }

        private void GamePanel_Drag(object sender, EventArgs e)
        {
            var clientRectangle = RectangleToScreen(ClientRectangle);
            var gamePanelX = Cursor.Position.X - clientRectangle.Left - _fromDragCursorToGamePanelLeft;
            var gamePanelY = Cursor.Position.Y - clientRectangle.Top - _fromDragCursorToGamePanelTop;

            if (gamePanelX < 0)
            {
                gamePanelX = 0;
                _fromDragCursorToGamePanelLeft = Math.Max(Cursor.Position.X - clientRectangle.Left, 0);
            }

            if (gamePanelX > clientRectangle.Width - GamePanel.Width)
            {
                gamePanelX = clientRectangle.Width - GamePanel.Width;
                _fromDragCursorToGamePanelLeft = Math.Min(Cursor.Position.X - clientRectangle.Left - gamePanelX, GamePanel.Width);
            }

            if (gamePanelY < MenuStrip.Height + TimePanel.Height)
            {
                gamePanelY = MenuStrip.Height + TimePanel.Height;
                _fromDragCursorToGamePanelTop = Math.Max(Cursor.Position.Y - clientRectangle.Top - gamePanelY, 0);
            }

            if (gamePanelY > clientRectangle.Height - GamePanel.Height)
            {
                gamePanelY = clientRectangle.Height - GamePanel.Height;
                _fromDragCursorToGamePanelTop = Math.Min(Cursor.Position.Y - clientRectangle.Top - gamePanelY, GamePanel.Height);
            }

            GamePanel.Location = new(gamePanelX, gamePanelY);
        }

        private void Size_Changed(object sender, EventArgs e)
        {
            var gamePanelX = GamePanel.Location.X;
            var gamePanelY = GamePanel.Location.Y;

            if (gamePanelX < 0)
            {
                gamePanelX = 0;
            }

            if (gamePanelX > ClientRectangle.Width - GamePanel.Width)
            {
                gamePanelX = ClientRectangle.Width - GamePanel.Width;
            }

            if (gamePanelY < MenuStrip.Height + TimePanel.Height)
            {
                gamePanelY = MenuStrip.Height + TimePanel.Height;
            }

            if (gamePanelY > ClientRectangle.Height - GamePanel.Height)
            {
                gamePanelY = ClientRectangle.Height - GamePanel.Height;
            }

            GamePanel.Location = new(gamePanelX, gamePanelY);
        }

        private void GamePanel_SizeChanged(object sender, EventArgs e)
        {
            var minWidth = Math.Max(GamePanel.Width, TimePanel.MinimumSize.Width) + (Width - ClientRectangle.Width);
            var minHeight = MenuStrip.Height + TimePanel.Height + GamePanel.Height + (Height - ClientRectangle.Height);
            MinimumSize = new(minWidth, minHeight);

            var gamePanelX = GamePanel.Location.X;
            var gamePanelY = GamePanel.Location.Y;

            if (gamePanelX > ClientRectangle.Width - GamePanel.Width)
            {
                gamePanelX = ClientRectangle.Width - GamePanel.Width;
            }

            if (gamePanelY > ClientRectangle.Height - GamePanel.Height)
            {
                gamePanelY = ClientRectangle.Height - GamePanel.Height;
            }

            GamePanel.Location = new(gamePanelX, gamePanelY);
        }
    }
}
