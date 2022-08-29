using Chess.LogicPart;
using Chess.Players;
using Timer = System.Windows.Forms.Timer;

namespace Chess
{
    public partial class GameForm : Form
    {
        private readonly MenuPanel _menuPanel;
        private readonly BoardPanel _boardPanel;

        public VirtualPlayer WhiteVirtualPlayer { get; private set; } //= new VirtualPlayer(Strategies.SelectMoveForVirtualFool);
        // == null, ���� �� ��� ������� ������ ������������.

        public VirtualPlayer BlackVirtualPlayer { get; private set; } = new VirtualPlayer(Strategies.SelectMoveForVirtualFool);
        //����������.

        public bool HidesMenus { get; set; }

        public Timer Timer { get; } = new() { Interval = 1000 };

        public GameForm()
        {
            InitializeComponent();
            Text = "";
            BackColor = Color.LightBlue;

            _boardPanel = new BoardPanel(this);
            _menuPanel = new MenuPanel(this);

            var _shift = Screen.PrimaryScreen.WorkingArea.Height / 16;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowOnly;

            _boardPanel.Location = new Point(_shift, _menuPanel.Height + _shift);
            Width = 0;
            Height = 0;            
            Controls.Add(_boardPanel);
            var minWidth = Width;
            var minHeight = Height;
            Width += _shift;
            Height += _shift;
            MinimumSize = new Size(Width, Height);
            MaximumSize = new Size(Width, Height);

            _menuPanel.Width = Width;
            Controls.Add(_menuPanel);

            AutoSize = false;
            MinimumSize = new Size(minWidth, minHeight);
            MaximumSize = new Size(int.MaxValue, int.MaxValue);
        }

        public void StartNewGame() => _boardPanel.StartNewGame();

        public void ShowMessage(string message) => MessageBox.Show(message, "", MessageBoxButtons.OK);

        public bool ProgramPlaysFor(PieceColor color) => color == PieceColor.White ? WhiteVirtualPlayer != null : BlackVirtualPlayer != null;
        // ��������� ����� ������ � ���� � �����.
    }
}
