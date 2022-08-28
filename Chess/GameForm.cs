using Chess.LogicPart;
using Chess.Players;
using Timer = System.Windows.Forms.Timer;

namespace Chess
{
    public partial class GameForm : Form
    {
        private readonly ChessBoard _gameBoard = new(); // Для каждого из потоков создаем по отдельному экземпляру доски.
        private ChessBoard _whiteThinkingBoard;
        private ChessBoard _blackThinkingBoard;
        private Thread _think;

        private readonly MenuPanel _menuPanel;
        private readonly SquareButton[,] _formButtons = new SquareButton[8, 8];
        private readonly Timer _timer = new() { Interval = 1000 };

        private int _startedGamesCount;
        private readonly List<int> _clickedButtons = new();
        private int[] _lastMove = new int[0];
        private int _movesCount;
        private PieceColor _movingSideColor = PieceColor.White;
        private bool _programMadeMove;

        public VirtualPlayer WhiteVirtualPlayer { get; private set; } //= new VirtualPlayer(Strategies.SelectMoveForVirtualFool);
        // == null, если за эту сторону играет пользователь.

        public VirtualPlayer BlackVirtualPlayer { get; private set; } = new VirtualPlayer(Strategies.SelectMoveForVirtualFool);
        //Аналогично.

        public int ButtonSize { get; private set; } = Screen.PrimaryScreen.WorkingArea.Height / 16;

        public Color LightSquaresColor { get; private set; } = Color.Yellow;

        public Color DarkSquaresColor { get; private set; } = Color.Chocolate;

        public Color HighlightColor { get; private set; } = Color.Blue;

        public bool HidesMenus { get; set; }

        public GameForm()
        {
            InitializeComponent();
            Text = "";
            BackColor = Color.LightBlue;
            AutoSize = true;
            _menuPanel = new MenuPanel(this);
            _timer.Tick += new EventHandler(MakeProgramMove);
            SetControls();
            StartNewGame();
        }

        public void SetControls()
        {
            Controls.Remove(_menuPanel);
            MinimumSize = new Size(0, 0);
            MaximumSize = new Size(int.MaxValue, int.MaxValue);
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            var buttonColor = LightSquaresColor;
            var buttonX = ButtonSize;
            var buttonY = _menuPanel.Height + ButtonSize;

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 7; j >= 0; --j)
                {
                    var borderSize = 0;

                    // Если кнопки ранее уже созданы, а теперь мы хотим изменить размер полей, то старые кнопки нужно удалить.
                    if (_formButtons[i, j] != null)
                    {
                        borderSize = _formButtons[i, j].FlatAppearance.BorderSize;
                        Controls.Remove(_formButtons[i, j]);
                    }

                    var newButton = new SquareButton(this, i, j)
                    {
                        BackColor = buttonColor,
                        Location = new Point(buttonX, buttonY)
                    };

                    newButton.FlatAppearance.BorderSize = borderSize;

                    _formButtons[i, j] = newButton;
                    Controls.Add(newButton);

                    buttonColor = buttonColor == LightSquaresColor ? DarkSquaresColor : LightSquaresColor;
                    buttonY += ButtonSize;
                }

                buttonColor = buttonColor == LightSquaresColor ? DarkSquaresColor : LightSquaresColor;
                buttonX += ButtonSize;
                buttonY = _menuPanel.Height + ButtonSize;
            }

            AutoSizeMode = AutoSizeMode.GrowOnly;
            Width += ButtonSize;
            Height += ButtonSize;
            MinimumSize = new Size(Width, Height);
            MaximumSize = MinimumSize;
            _menuPanel.Width = Width;
            Controls.Add(_menuPanel);
        }

        public void RenewButtonsView(RenewMode renewMode)
        {
            var currentPosition = _gameBoard.CurrentPosition;

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (renewMode == RenewMode.FullRenew)
                    {
                        _formButtons[i, j].DisplayedPieceIndex = currentPosition[i, j];
                        _formButtons[i, j].RenewImage();
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

        public void StartNewGame()
        {
            _timer.Stop();
            ++_startedGamesCount;

            while (_think != null && _think.ThreadState == ThreadState.Running)
            { }

            _movesCount = 0;
            _movingSideColor = PieceColor.White;
            _programMadeMove = false;

            var whiteMaterial = new string[3] { "King", "Rook", "Rook" };
            var whitePositions = new string[3] { "e1", "a1", "h1" };
            var blackMaterial = new string[3] { "King", "Rook", "Rook" };
            var blackPositions = new string[3] { "e8", "a8", "h8" };
            _gameBoard.SetPosition(whiteMaterial, whitePositions, blackMaterial, blackPositions, _movingSideColor);
            _whiteThinkingBoard = new ChessBoard(_gameBoard);
            _blackThinkingBoard = new ChessBoard(_gameBoard);            

            RenewButtonsView(RenewMode.RenewIfNeeded);

            if (ProgramPlaysFor(PieceColor.White))
            {
                WhiteVirtualPlayer.SetBoard(_whiteThinkingBoard);
            }

            if (ProgramPlaysFor(PieceColor.Black))
            {
                BlackVirtualPlayer.SetBoard(_blackThinkingBoard);
            }

            if (ProgramPlaysFor(_movingSideColor))
            {
                _think = new Thread(Think);
                _think.Start();
            }

            _timer.Start();
        }

        public void SetSizeAndColors(int buttonSize, Color lightSquaresColor, Color darkSquaresColor)
        {
            ButtonSize = buttonSize;
            LightSquaresColor = lightSquaresColor;
            DarkSquaresColor = darkSquaresColor;
            SetControls();
            RenewButtonsView(RenewMode.FullRenew);
        }

        public void HandleClickAt(int x, int y)
        {
            if (ProgramPlaysFor(_movingSideColor) || GameIsOver)
            {
                return;
            }

            if (_clickedButtons.Count == 0) // Т.е. выбор фигуры для хода.
            {
                if (_formButtons[x, y].DisplayedPieceIndex == 0)
                {
                    return;
                }

                if ((_movingSideColor == PieceColor.White && _formButtons[x, y].DisplayedPieceIndex > 6) ||
                    (_movingSideColor == PieceColor.Black && _formButtons[x, y].DisplayedPieceIndex <= 6))
                {
                    ShowMessage("Это не ваша фигура.");
                    return;
                }

                _clickedButtons.Add(x); // Запомнили координаты выбранной фигуры, ждем щелчка по полю на которое нужно сходить.
                _clickedButtons.Add(y);
                HighlightAt(x, y);
                return;
            }

            if (x == _clickedButtons[0] && y == _clickedButtons[1]) // Отмена выбора.
            {
                _clickedButtons.Clear();
                RemoveHighlightAt(x, y);
                return;
            }

            if ((_movingSideColor == PieceColor.White && _formButtons[x, y].DisplayedPieceIndex > 0 && _formButtons[x, y].DisplayedPieceIndex <= 6) ||
                (_movingSideColor == PieceColor.Black && _formButtons[x, y].DisplayedPieceIndex > 6)) //Замена выбранной фигуры на другую.
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

            catch (Exception exception) // На случай, если ход не по правилам.
            {
                ShowMessage(exception.Message);
                return;
            }

            RenewButtonsView(RenewMode.RenewIfNeeded);
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
                ShowMessage("Мат черным.");
                return;
            }

            if (_gameBoard.Status == GameStatus.BlackWin)
            {
                _timer.Stop();
                ShowMessage("Мат белым.");
                return;
            }

            if (_gameBoard.Status == GameStatus.Draw)
            {
                _timer.Stop();
                ShowMessage("Ничья.");
                return;
            }

            // Запускаем выбор программой ответного хода, если нужно.
            if (ProgramPlaysFor(_movingSideColor))
            {
                _think = new Thread(Think);
                _think.Start();
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

        public void HighlightAt(int x, int y) => _formButtons[x, y].Highlight();

        public void RemoveHighlightAt(int x, int y) => _formButtons[x, y].RemoveHighlight();

        public void OutlineButton(int x, int y) => _formButtons[x, y].FlatAppearance.BorderSize = 2;

        public bool ProgramPlaysFor(PieceColor color) => color == PieceColor.White ? WhiteVirtualPlayer != null : BlackVirtualPlayer != null;
        // Программа может играть и сама с собой.

        public bool GameIsOver => _gameBoard.Status != GameStatus.GameCanContinue;
    }
}
