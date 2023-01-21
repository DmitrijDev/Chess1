using Chess.LogicPart;

namespace Chess
{
    public partial class GameForm : Form
    {
        private MenuPanel _menuPanel;
        private TimePanel _timePanel;
        private BoardPanel _boardPanel;        

        public GameForm()
        {
            InitializeComponent();
            Text = "";
            BackColor = Color.LightBlue;
            SetControls();
            StartNewGame();
        }

        private void SetControls()
        {
            _menuPanel = new MenuPanel(this);
            _timePanel = new TimePanel(this);
            _boardPanel = new BoardPanel(this);

            var shift = Screen.PrimaryScreen.WorkingArea.Height / 16;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowOnly;

            _timePanel.Location = new Point(0, _menuPanel.Height);
            _boardPanel.Location = new Point(shift, _timePanel.Height + _menuPanel.Height + shift);

            Width = 0;
            Height = 0;
            Controls.Add(_boardPanel);
            var minWidth = Width;
            var minHeight = Height;
            Width += shift;
            Height += shift;
            MinimumSize = new Size(Width, Height);
            MaximumSize = new Size(Width, Height);

            Controls.Add(_timePanel);
            Controls.Add(_menuPanel);

            AutoSize = false;
            MinimumSize = new Size(minWidth, minHeight);
            MaximumSize = new Size(int.MaxValue, int.MaxValue);
        }

        internal void ShowTime(int whiteTimeLeft, int blackTimeLeft) => _timePanel.ShowTime(whiteTimeLeft, blackTimeLeft);

        internal void ShowTime(PieceColor color, int time) => _timePanel.ShowTime(color, time);

        internal void StartNewGame() => _boardPanel.StartNewGame();

        internal void ChangePlayer(PieceColor pieceColor) => _boardPanel.ChangePlayer(pieceColor);

        internal void ShowMessage(string message) => MessageBox.Show(message, "", MessageBoxButtons.OK);

        public Color PanelColor => DefaultBackColor;

        public int TimeFontSize => _menuPanel.Font.Height;
    }
}
