using Chess.LogicPart;

namespace Chess
{
    public partial class GameForm : Form
    {
        private MenuPanel _menuPanel;
        private TimePanel _timePanel;
        private GamePanel _gamePanel;
        private Point _oldGamePanelLocation;
        private bool _wasMaximized;

        public GameForm()
        {
            InitializeComponent();
            Text = "";
            BackColor = Color.Silver;
            SetPanels();
            SizeChanged += new EventHandler(MoveGamePanel);
            _gamePanel.LocationChanged += new EventHandler(SaveGamePanelLocation);
            StartNewGame();
        }

        private void SetPanels()
        {
            _menuPanel = new MenuPanel(this);
            _timePanel = new TimePanel(this);
            _gamePanel = new GamePanel(this);

            var shift = Screen.PrimaryScreen.WorkingArea.Height / 16;
            Width = Math.Max(shift * 2 + _gamePanel.Width, _timePanel.MinimumSize.Width);
            Width += Width - ClientRectangle.Width;
            Height = shift * 2 + _menuPanel.Height + _timePanel.Height + _gamePanel.Height;
            Height += Height - ClientRectangle.Height;

            _timePanel.Location = new Point(0, _menuPanel.Height);
            PutGamePanelToCenter();

            Controls.Add(_menuPanel);
            Controls.Add(_timePanel);
            Controls.Add(_gamePanel);
            _oldGamePanelLocation = _gamePanel.Location;

            var minWidth = Math.Max(Width - shift * 2, _timePanel.MinimumSize.Width);
            var minHeight = Height - shift * 2;
            MinimumSize = new Size(minWidth, minHeight);
        }

        private void MoveGamePanel(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                PutGamePanelToCenter();
                _wasMaximized = true;
                return;
            }

            if (_wasMaximized)
            {
                _gamePanel.Location = _oldGamePanelLocation;
                _wasMaximized = false;
                return;
            }

            var newGamePanelX = _gamePanel.Location.X;
            var newGamePanelY = _gamePanel.Location.Y;

            if (ClientRectangle.Width < _gamePanel.Location.X + _gamePanel.Width)
            {
                newGamePanelX -= _gamePanel.Location.X + _gamePanel.Width - ClientRectangle.Width;
            }

            if (ClientRectangle.Height < _gamePanel.Location.Y + _gamePanel.Height)
            {
                newGamePanelY -= _gamePanel.Location.Y + _gamePanel.Height - ClientRectangle.Height;
            }

            _gamePanel.Location = new Point(newGamePanelX, newGamePanelY);
        }

        internal void PutGamePanelToCenter(object sender, EventArgs e) => PutGamePanelToCenter();

        private void PutGamePanelToCenter()
        {
            var gamePanelX = (ClientRectangle.Width - _gamePanel.Width) / 2;
            var gamePanelY = _menuPanel.Height + _timePanel.Height + (ClientRectangle.Height - _menuPanel.Height - _timePanel.Height - _gamePanel.Height) / 2;
            _gamePanel.Location = new Point(gamePanelX, gamePanelY);
        }

        private void SaveGamePanelLocation(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Maximized)
            {
                _oldGamePanelLocation = _gamePanel.Location;
            }
        }

        internal void ShowTime(int whiteTimeLeft, int blackTimeLeft) => _timePanel.ShowTime(whiteTimeLeft, blackTimeLeft);

        internal void ShowTime(PieceColor color, int time) => _timePanel.ShowTime(color, time);

        internal void StartNewGame() => _gamePanel.StartNewGame();

        internal void ChangePlayer(PieceColor pieceColor) => _gamePanel.ChangePlayer(pieceColor);

        internal void ShowMessage(string message) => MessageBox.Show(message, "", MessageBoxButtons.OK);

        public Color PanelColor => DefaultBackColor;

        public int TimeFontSize => _menuPanel.Font.Height;
    }
}
