using Chess.LogicPart;

namespace Chess
{
    internal class BoardPanel : Panel
    {
        private readonly GameForm _form;

        private readonly ChessBoard _gameBoard = new(); // Для каждого из потоков создаем по отдельному экземпляру доски.
        private ChessBoard _whiteThinkingBoard;
        private ChessBoard _blackThinkingBoard;
        private Thread _think;

        private readonly SquareButton[,] _buttons = new SquareButton[8, 8];

        private readonly List<int> _clickedButtons = new();
        private int[] _lastMove = new int[0];
        private int _movesCount;
        private PieceColor _movingSideColor = PieceColor.White;
        private bool _programMadeMove;

        public Color LightSquaresColor { get; private set; } = Color.Yellow;

        public Color DarkSquaresColor { get; private set; } = Color.Chocolate;

        public Color HighlightColor { get; private set; } = Color.Blue;

        public int InitialButtonSize { get; } = Screen.PrimaryScreen.WorkingArea.Height / 16;

        public int ButtonSize { get; private set; }

        public BoardPanel(GameForm form)
        {
            _form = form;

            BackColor = Color.Maroon;
            BorderStyle = BorderStyle.FixedSingle;
            ButtonSize = InitialButtonSize;
            _form.Timer.Tick += new EventHandler(MakeProgramMove);

            SetButtons();
            StartNewGame();
        }

        private void SetButtons()
        {
            MinimumSize = new Size(0, 0);
            MaximumSize = new Size(int.MaxValue, int.MaxValue);
            var shift = InitialButtonSize / 2;
            Width = ButtonSize * 8 + shift * 2;
            Height = Width;
            MinimumSize = new Size(Width, Height);
            MaximumSize = MinimumSize;
           
            var buttonColor = LightSquaresColor;
            var buttonX = shift;
            var buttonY = shift;

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 7; j >= 0; --j)
                {
                    var borderSize = 0;

                    // Если кнопки ранее уже созданы, а теперь мы хотим изменить размер полей, то старые кнопки нужно удалить.
                    if (_buttons[i, j] != null)
                    {
                        borderSize = _buttons[i, j].FlatAppearance.BorderSize;
                        Controls.Remove(_buttons[i, j]);
                    }

                    var newButton = new SquareButton(this, i, j)
                    {
                        BackColor = buttonColor,
                        Location = new Point(buttonX, buttonY)
                    };

                    newButton.FlatAppearance.BorderSize = borderSize;

                    _buttons[i, j] = newButton;
                    Controls.Add(newButton);

                    buttonColor = buttonColor == LightSquaresColor ? DarkSquaresColor : LightSquaresColor;
                    buttonY += ButtonSize;
                }

                buttonColor = buttonColor == LightSquaresColor ? DarkSquaresColor : LightSquaresColor;
                buttonX += ButtonSize;
                buttonY = shift;
            }      
        }

        public void SetSizeAndColors(int buttonSize, Color lightSquaresColor, Color darkSquaresColor)
        {
            ButtonSize = buttonSize;
            LightSquaresColor = lightSquaresColor;
            DarkSquaresColor = darkSquaresColor;
            SetButtons();
            RenewButtonsView(RenewMode.FullRenew);
        }

        public void StartNewGame()
        {
            _form.Timer.Stop();

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

            if (_form.ProgramPlaysFor(PieceColor.White))
            {
                _form.WhiteVirtualPlayer.SetBoard(_whiteThinkingBoard);
            }

            if (_form.ProgramPlaysFor(PieceColor.Black))
            {
                _form.BlackVirtualPlayer.SetBoard(_blackThinkingBoard);
            }

            if (_form.ProgramPlaysFor(_movingSideColor))
            {
                _think = new Thread(Think);
                _think.Start();
            }

            _form.Timer.Start();
        }

        public void HandleClickAt(int x, int y)
        {
            if (_form.ProgramPlaysFor(_movingSideColor) || GameIsOver)
            {
                return;
            }

            if (_clickedButtons.Count == 0) // Т.е. выбор фигуры для хода.
            {
                if (_buttons[x, y].DisplayedPieceIndex == 0)
                {
                    return;
                }

                if ((_movingSideColor == PieceColor.White && _buttons[x, y].DisplayedPieceIndex > 6) ||
                    (_movingSideColor == PieceColor.Black && _buttons[x, y].DisplayedPieceIndex <= 6))
                {
                    _form.ShowMessage("Это не ваша фигура.");
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

            if ((_movingSideColor == PieceColor.White && _buttons[x, y].DisplayedPieceIndex > 0 && _buttons[x, y].DisplayedPieceIndex <= 6) ||
                (_movingSideColor == PieceColor.Black && _buttons[x, y].DisplayedPieceIndex > 6)) //Замена выбранной фигуры на другую.
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
                _form.ShowMessage(exception.Message);
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
                _form.Timer.Stop();
                _form.ShowMessage("Мат черным.");
                return;
            }

            if (_gameBoard.Status == GameStatus.BlackWin)
            {
                _form.Timer.Stop();
                _form.ShowMessage("Мат белым.");
                return;
            }

            if (_gameBoard.Status == GameStatus.Draw)
            {
                _form.Timer.Stop();
                _form.ShowMessage("Ничья.");
                return;
            }

            // Запускаем выбор программой ответного хода, если нужно.
            if (_form.ProgramPlaysFor(_movingSideColor))
            {
                _think = new Thread(Think);
                _think.Start();
            }
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
                        _buttons[i, j].DisplayedPieceIndex = currentPosition[i, j];
                        _buttons[i, j].RenewImage();
                        continue;
                    }

                    _buttons[i, j].FlatAppearance.BorderSize = 0;

                    if (_buttons[i, j].DisplayedPieceIndex != currentPosition[i, j])
                    {
                        _buttons[i, j].DisplayedPieceIndex = currentPosition[i, j];
                        _buttons[i, j].RenewImage();
                    }
                }
            }
        }

        public void Think()
        {
            var player = _movingSideColor == PieceColor.White ? _form.WhiteVirtualPlayer : _form.BlackVirtualPlayer;
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

        public void OutlineButton(int x, int y) => _buttons[x, y].FlatAppearance.BorderSize = 2;

        public void HighlightAt(int x, int y) => _buttons[x, y].Highlight();

        public void RemoveHighlightAt(int x, int y) => _buttons[x, y].RemoveHighlight();

        public bool GameIsOver => _gameBoard.Status != GameStatus.GameCanContinue;
    }
}
