using Chess.LogicPart;
using Timer = System.Windows.Forms.Timer;

namespace Chess
{
    public partial class GameForm : Form
    {
        private MenuPanel _menuPanel;
        private BoardPanel _boardPanel;

        public bool HidesMenus { get; set; }

        public Timer Timer { get; } = new() { Interval = 1000 };

        public GameForm()
        {
            InitializeComponent();
            Text = "";
            BackColor = Color.LightBlue;
            SetControls();
        }

        public void SetControls()
        {
            _boardPanel = new BoardPanel(this);
            _menuPanel = new MenuPanel(this);

            var shift = Screen.PrimaryScreen.WorkingArea.Height / 16;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowOnly;

            _boardPanel.Location = new Point(shift, _menuPanel.Height + shift);
            Width = 0;
            Height = 0;
            Controls.Add(_boardPanel);
            var minWidth = Width;
            var minHeight = Height;
            Width += shift;
            Height += shift;
            MinimumSize = new Size(Width, Height);
            MaximumSize = new Size(Width, Height);

            _menuPanel.Width = Width;
            Controls.Add(_menuPanel);

            AutoSize = false;
            MinimumSize = new Size(minWidth, minHeight);
            MaximumSize = new Size(int.MaxValue, int.MaxValue);
        }

        public void StartNewGame() => _boardPanel.StartNewGame();

        public void ChangePlayer(PieceColor pieceColor) => _boardPanel.ChangePlayer(pieceColor);

        public void ShowMessage(string message) => MessageBox.Show(message, "", MessageBoxButtons.OK);        
    }
}
