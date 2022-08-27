using Chess.LogicPart;
using Chess.Players;
using Timer = System.Windows.Forms.Timer;

namespace Chess
{
    public partial class GameForm : Form
    {
        private readonly ChessBoard _gameBoard = new(); // ��� ������� �� ������� ������� �� ���������� ���������� �����.
        private readonly ChessBoard _whiteThinkingBoard;
        private readonly ChessBoard _blackThinkingBoard;

        private readonly SquareButton[,] _formButtons = new SquareButton[8, 8];
        private readonly Timer _timer = new() { Interval = 1000 };
        private readonly Panel _panel = new();

        private readonly int _initialButtonSize = Screen.PrimaryScreen.WorkingArea.Height / 16;

        private readonly List<int> _clickedButtons = new();
        private int[] _lastMove = new int[0];
        private int _movesCount;
        private PieceColor _movingSideColor = PieceColor.White;
        private bool _programMadeMove;

        public VirtualPlayer WhiteVirtualPlayer { get; private set; } //= new VirtualPlayer(Strategies.ChooseMoveForVirtualFool);
        // == null, ���� �� ��� ������� ������ ������������.

        public VirtualPlayer BlackVirtualPlayer { get; private set; } = new VirtualPlayer(Strategies.ChooseMoveForVirtualFool);
        //����������.

        public int ButtonSize { get; private set; }

        public Color LightSquaresColor { get; private set; } = Color.Yellow;

        public Color DarkSquaresColor { get; private set; } = Color.Chocolate;

        public Color HighlightColor { get; private set; } = Color.Blue;

        public GameForm()
        {
            InitializeComponent();
            Text = "";
            BackColor = Color.LightBlue;
            AutoSize = true;
            ButtonSize = _initialButtonSize;
            SetPanelSettings();
            MouseMove += new MouseEventHandler(MovePanel);
            CreateImages();

            var whiteMaterial = new string[3] { "King", "Rook", "Rook" };
            var whitePositions = new string[3] { "e1", "a1", "h1" };
            var blackMaterial = new string[3] { "King", "Rook", "Rook" };
            var blackPositions = new string[3] { "e8", "a8", "h8" };
            _gameBoard.SetPosition(whiteMaterial, whitePositions, blackMaterial, blackPositions, _movingSideColor);
            _whiteThinkingBoard = new ChessBoard(_gameBoard);
            _blackThinkingBoard = new ChessBoard(_gameBoard);

            SetControls();

            if (ProgramPlaysFor(PieceColor.White))
            {
                WhiteVirtualPlayer.SetBoard(_whiteThinkingBoard);
            }

            if (ProgramPlaysFor(PieceColor.Black))
            {
                BlackVirtualPlayer.SetBoard(_blackThinkingBoard);
            }

            _timer.Tick += new EventHandler(MakeProgramMove);
            _timer.Start();

            if (ProgramPlaysFor(_movingSideColor))
            {
                new Thread(Think).Start();
            }
        }

        public void SetControls()
        {
            Controls.Remove(_panel);
            MinimumSize = new Size(0, 0);
            MaximumSize = new Size(int.MaxValue, int.MaxValue);
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            var boardSize = _formButtons.GetLength(0);
            var buttonColor = LightSquaresColor;
            var buttonX = ButtonSize;
            var buttonY = _panel.Height + ButtonSize;

            for (var i = 0; i < boardSize; ++i)
            {
                for (var j = boardSize - 1; j >= 0; --j)
                {
                    var borderSize = 0;

                    // ���� ������ ����� ��� �������, � ������ �� ����� �������� ������ �����, �� ������ ������ ����� �������.
                    if (_formButtons[i, j] != null)
                    {
                        borderSize = _formButtons[i, j].FlatAppearance.BorderSize;
                        Controls.Remove(_formButtons[i, j]);
                    }

                    var newButton = new SquareButton(this, i, j)
                    {
                        BackColor = buttonColor,
                        Location = new Point(buttonX, buttonY),
                    };

                    newButton.FlatAppearance.BorderSize = borderSize;

                    _formButtons[i, j] = newButton;
                    Controls.Add(newButton);

                    buttonColor = buttonColor == LightSquaresColor ? DarkSquaresColor : LightSquaresColor;
                    buttonY += ButtonSize;
                }

                buttonColor = buttonColor == LightSquaresColor ? DarkSquaresColor : LightSquaresColor;
                buttonX += ButtonSize;
                buttonY = _panel.Height + ButtonSize;
            }

            AutoSizeMode = AutoSizeMode.GrowOnly;
            Width += ButtonSize;
            Height += ButtonSize;
            MinimumSize = new Size(Width, Height);
            MaximumSize = MinimumSize;
            _panel.Width = Width;
            Controls.Add(_panel);
            RenewPosition(RenewMode.FullRenew);
        }

        public void SetPanelSettings()
        {
            _panel.Height = _initialButtonSize / 2;
            _panel.Location = new Point(0, -_panel.Height);
            _panel.BackColor = Color.GhostWhite;
            _panel.BorderStyle = BorderStyle.FixedSingle;
            AddMenus();
        }

        public void AddMenus()
        {
            var menuStrip = new MenuStrip()
            {
                Height = _panel.Height,
                BackColor = _panel.BackColor,
            };


            var gameMenu = new ToolStripMenuItem("����");

            var escapeItem = new ToolStripMenuItem("�����");

            _panel.Controls.Add(menuStrip);

            menuStrip.Items.Add(gameMenu);

            gameMenu.DropDownItems.Add(escapeItem);

            escapeItem.Click += new EventHandler(Escape);
        }

        public void MovePanel(object sender, EventArgs e)
        {
            var titleHeight = RectangleToScreen(ClientRectangle).Top - Top;

            if (Cursor.Position.Y <= Location.Y + titleHeight + _panel.Height)
            {
                _panel.Location = new Point(0, 0);
            }
            else
            {
                _panel.Location = new Point(0, -_panel.Height);
            }
        }

        public void RenewPosition(RenewMode renewMode)
        {
            var currentPosition = _gameBoard.CurrentPosition;

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (renewMode == RenewMode.FullRenew)
                    {
                        var borderSize = _formButtons[i, j].FlatAppearance.BorderSize;
                        _formButtons[i, j].DisplayedPieceIndex = currentPosition[i, j];
                        _formButtons[i, j].RenewImage();
                        _formButtons[i, j].FlatAppearance.BorderSize = borderSize;
                        continue;
                    }

                    _formButtons[i, j].FlatAppearance.BorderSize = 0;

                    if (_formButtons[i, j].DisplayedPieceIndex != currentPosition[i, j])
                    {
                        _formButtons[i, j].DisplayedPieceIndex = currentPosition[i, j];
                        _formButtons[i, j].RenewImage();
                    }
                }
            }
        }

        public void SetSizeAndColors(int buttonSize, Color lightSquaresColor, Color darkSquaresColor)
        {
            ButtonSize = buttonSize;
            LightSquaresColor = lightSquaresColor;
            DarkSquaresColor = darkSquaresColor;
            SetControls();
        }

        public void HandleClickAt(int x, int y)
        {
            if (ProgramPlaysFor(_movingSideColor) || GameIsOver)
            {
                return;
            }

            if (_clickedButtons.Count == 0) // �.�. ����� ������ ��� ����.
            {
                if (_formButtons[x, y].DisplayedPieceIndex == 0)
                {
                    return;
                }

                if ((_movingSideColor == PieceColor.White && _formButtons[x, y].DisplayedPieceIndex > 6) ||
                    (_movingSideColor == PieceColor.Black && _formButtons[x, y].DisplayedPieceIndex <= 6))
                {
                    ShowMessage("��� �� ���� ������.");
                    return;
                }

                _clickedButtons.Add(x); // ��������� ���������� ��������� ������, ���� ������ �� ���� �� ������� ����� �������.
                _clickedButtons.Add(y);
                HighlightAt(x, y);
                return;
            }

            if (x == _clickedButtons[0] && y == _clickedButtons[1]) // ������ ������.
            {
                _clickedButtons.Clear();
                RemoveHighlightAt(x, y);
                return;
            }

            if ((_movingSideColor == PieceColor.White && _formButtons[x, y].DisplayedPieceIndex > 0 && _formButtons[x, y].DisplayedPieceIndex <= 6) ||
                (_movingSideColor == PieceColor.Black && _formButtons[x, y].DisplayedPieceIndex > 6)) //������ ��������� ������ �� ������.
            {
                RemoveHighlightAt(_clickedButtons[0], _clickedButtons[1]);
                _clickedButtons.Clear();
                _clickedButtons.Add(x);
                _clickedButtons.Add(y);
                HighlightAt(x, y);
                return;
            }

            RemoveHighlightAt(_clickedButtons[0], _clickedButtons[1]);
            _clickedButtons.Add(x);
            _clickedButtons.Add(y);
            var move = _clickedButtons.ToArray();
            _clickedButtons.Clear();
            MakeMove(move);
        }

        public void MakeMove(int[] move)
        {
            try
            {
                _gameBoard.MakeMove(move);
            }

            catch (Exception exception) // �� ������, ���� ��� �� �� ��������.
            {
                ShowMessage(exception.Message);
                return;
            }

            RenewPosition(RenewMode.RenewAfterMove);
            OutlineButton(move[0], move[1]);
            OutlineButton(move[2], move[3]);

            if (_movesCount < _gameBoard.MovesCount)
            {
                lock (_lastMove)
                {
                    _lastMove = new int[move.Length];
                    Array.Copy(move, 0, _lastMove, 0, move.Length);
                }

                _movesCount = _gameBoard.MovesCount;
                _movingSideColor = _gameBoard.MovingSideColor;
            }

            if (_gameBoard.Status == GameStatus.WhiteWin)
            {
                _timer.Stop();
                ShowMessage("��� ������.");
                return;
            }

            if (_gameBoard.Status == GameStatus.BlackWin)
            {
                _timer.Stop();
                ShowMessage("��� �����.");
                return;
            }

            if (_gameBoard.Status == GameStatus.Draw)
            {
                _timer.Stop();
                ShowMessage("�����.");
                return;
            }

            // ��������� ����� ���������� ��������� ����, ���� �����.
            if (ProgramPlaysFor(_movingSideColor))
            {
                new Thread(Think).Start();
            }
        }

        public void ShowMessage(string message) => MessageBox.Show(message, "", MessageBoxButtons.OK);

        public void Think()
        {
            var player = _movingSideColor == PieceColor.White ? WhiteVirtualPlayer : BlackVirtualPlayer;
            var board = player.Board;

            if (_movesCount > board.MovesCount)
            {
                int[] enemyMove;

                lock (_lastMove)
                {
                    enemyMove = new int[_lastMove.Length];
                    Array.Copy(_lastMove, 0, enemyMove, 0, _lastMove.Length);
                }

                board.MakeMove(enemyMove);
            }

            var replyMove = player.SelectMove();
            board.MakeMove(replyMove);

            lock (_lastMove)
            {
                _lastMove = new int[replyMove.Length];
                Array.Copy(replyMove, 0, _lastMove, 0, replyMove.Length);
            }

            _movesCount = board.MovesCount;
            _movingSideColor = board.MovingSideColor;
            _programMadeMove = true;
        }

        public void MakeProgramMove(object sender, EventArgs e)
        {
            if (!_programMadeMove || GameIsOver)
            {
                return;
            }

            _programMadeMove = false;

            int[] move;

            lock (_lastMove)
            {
                move = new int[_lastMove.Length];
                Array.Copy(_lastMove, 0, move, 0, _lastMove.Length);
            }

            MakeMove(move);
        }

        public void Escape(object sender, EventArgs e) => Close();

        public void CreateImages()
        {
            SquareButton.Images = new Bitmap[37];
            var initialImages = new Bitmap[7] {null, new Bitmap("King.jpg"), new Bitmap("Queen.jpg"), new Bitmap("Rook.jpg"), new Bitmap("Knight.jpg"),
                new Bitmap("Bishop.jpg"), new Bitmap("Pawn.jpg") };

            for (var i = 1; i < SquareButton.Images.Length; ++i)
            {
                SquareButton.Images[i] = i <= 6 ? new Bitmap(initialImages[i]) : new Bitmap(SquareButton.Images[i - 6]);
            }

            for (var i = 1; i < SquareButton.Images.Length; ++i)
            {
                var backColor = i <= 12 ? LightSquaresColor : i <= 24 ? DarkSquaresColor : HighlightColor;
                var imageColor = (i >= 1 && i <= 6) || (i >= 13 && i <= 18) || (i >= 25 && i <= 30) ? Color.White : Color.Black;
                SquareButton.Images[i] = Graphics.GetColoredPicture(SquareButton.Images[i], backColor, imageColor);
            }
        }

        public void HighlightAt(int x, int y) => _formButtons[x, y].Highlight();

        public void RemoveHighlightAt(int x, int y) => _formButtons[x, y].RemoveHighlight();

        public void OutlineButton(int x, int y) => _formButtons[x, y].FlatAppearance.BorderSize = 2;

        public bool ProgramPlaysFor(PieceColor color) => color == PieceColor.White ? WhiteVirtualPlayer != null : BlackVirtualPlayer != null;
        // ��������� ����� ������ � ���� � �����.

        public bool GameIsOver => _gameBoard.Status != GameStatus.GameCanContinue;
    }
}
