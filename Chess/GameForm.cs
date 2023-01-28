using System.Windows;

namespace Chess
{
    public partial class GameForm : Form
    {
        private Point _oldGamePanelLocation;
        private bool _wasMaximized;
        private int _fromCursorToGamePanelLeft;
        private int _fromCursorToGamePanelTop;

        internal MenuPanel MenuPanel { get; private set; }

        internal TimePanel TimePanel { get; private set; }

        internal GamePanel GamePanel { get; private set; }

        public bool DraggingGamePanelAllowed { get; internal set; } = true;

        public GameForm()
        {
            InitializeComponent();
            Text = "";
            BackColor = Color.Olive;
            SetPanels();

            SizeChanged += Form_SizeChanged;
            GamePanel.LocationChanged += GamePanel_LocationChanged;
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
            _oldGamePanelLocation = GamePanel.Location;

            var minWidth = Math.Max(Width - shift * 2, TimePanel.MinimumSize.Width);
            var minHeight = Height - shift * 2;
            MinimumSize = new Size(minWidth, minHeight);
        }

        public int GetTitleHeight()
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
            if (!DraggingGamePanelAllowed)
            {
                return;
            }

            var titleHeight = GetTitleHeight();
            _fromCursorToGamePanelLeft = Cursor.Position.X - Location.X - GamePanel.Location.X;
            _fromCursorToGamePanelTop = (Cursor.Position.Y - Location.Y) - (titleHeight + GamePanel.Location.Y);
            DoDragDrop(GamePanel, DragDropEffects.None);
        }

        private void DragGamePanel(object sender, EventArgs e)
        {
            var titleHeight = GetTitleHeight();
            var newGamePanelX = Cursor.Position.X - Location.X - _fromCursorToGamePanelLeft;
            var newGamePanelY = Cursor.Position.Y - (Location.Y + titleHeight) - _fromCursorToGamePanelTop;

            if (newGamePanelX < 0)
            {
                newGamePanelX = 0;
                _fromCursorToGamePanelLeft = Math.Max(Cursor.Position.X - Location.X, 0);
            }

            if (newGamePanelX > ClientRectangle.Width - GamePanel.Width)
            {
                newGamePanelX = ClientRectangle.Width - GamePanel.Width;
                _fromCursorToGamePanelLeft = Math.Min(Cursor.Position.X - (Location.X + ClientRectangle.Width - GamePanel.Width), GamePanel.Width);
            }

            if (newGamePanelY < MenuPanel.Height + TimePanel.Height)
            {
                newGamePanelY = MenuPanel.Height + TimePanel.Height;
                _fromCursorToGamePanelTop = Math.Max((Cursor.Position.Y - Location.Y) - (titleHeight + MenuPanel.Height + TimePanel.Height), 0);
            }

            if (newGamePanelY > ClientRectangle.Height - GamePanel.Height)
            {
                newGamePanelY = ClientRectangle.Height - GamePanel.Height;
                _fromCursorToGamePanelTop = Math.Min(Cursor.Position.Y - (Location.Y + titleHeight + GamePanel.Location.Y), GamePanel.Height);
            }

            GamePanel.Location = new Point(newGamePanelX, newGamePanelY);
        }

        private void Form_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                PutGamePanelToCenter();
                _wasMaximized = true;
                return;
            }

            if (_wasMaximized)
            {
                GamePanel.Location = _oldGamePanelLocation;
                _wasMaximized = false;
                return;
            }

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

        private void GamePanel_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Maximized)
            {
                _oldGamePanelLocation = GamePanel.Location;
            }
        }

        public Color PanelColor => DefaultBackColor;

        public int TimeFontSize => MenuPanel.Font.Height;
    }
}
