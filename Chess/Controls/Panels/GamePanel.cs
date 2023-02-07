using Chess.LogicPart;
using Chess.Players;
using Timer = System.Windows.Forms.Timer;

namespace Chess
{
    internal class GamePanel : Panel
    {
        private readonly GameForm _form;

        private VirtualPlayer _whiteVirtualPlayer; // == null, если за эту сторону играет пользователь.
        private VirtualPlayer _blackVirtualPlayer = new(Strategies.SelectMoveForVirtualFool); //Аналогично.

        private readonly ChessBoard _gameBoard = new();
        private Thread _thinkingThread;

        private readonly GamePanelButton[,] _buttons = new GamePanelButton[8, 8];
        private readonly int _defaultButtonSize;

        private Orientation _orientation = Orientation.Standart;

        private string _highlightedButtonName;
        private string[] _selectedMove;

        private int _whiteTimeLeft;
        private int _blackTimeLeft;
        private readonly Timer _timer = new() { Interval = 1000 };
        private int _timeForGame = 300; // В секундах.

        public Color WhitePiecesColor { get; private set; }

        public Color BlackPiecesColor { get; private set; }

        public Color LightSquaresColor { get; private set; }

        public Color DarkSquaresColor { get; private set; }

        public Color HighlightColor { get; private set; }

        public int ButtonSize { get; private set; }

        public int MinimumButtonSize { get; private set; }

        public int MaximumButtonSize { get; private set; }

        public GamePanel(GameForm form)
        {
            _form = form;
            BorderStyle = BorderStyle.FixedSingle;

            _defaultButtonSize = (Screen.PrimaryScreen.WorkingArea.Height - _form.GetCaptionHeight() - _form.MenuPanel.Height - _form.TimePanel.Height) / 16;
            MinimumButtonSize = _form.GetCaptionHeight();
            MaximumButtonSize = (Screen.PrimaryScreen.WorkingArea.Height - _form.GetCaptionHeight() - _form.MenuPanel.Height - _form.TimePanel.Height) / 9;
            ButtonSize = _defaultButtonSize;

            _timer.Tick += Timer_Tick;
            _form.FormClosing += (sender, e) => StopThinking();

            SetButtons();
            SetColors(0);
        }

        private void SetButtons()
        {
            var shift = Math.Min(_defaultButtonSize / 2, ButtonSize / 2);
            Width = ButtonSize * 8 + shift * 2;
            Height = Width;

            var buttonX = _orientation == Orientation.Standart ? shift : Width - shift - ButtonSize;
            var buttonY = _orientation == Orientation.Standart ? shift : Height - shift - ButtonSize;

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 7; j >= 0; --j)
                {
                    // Возможно, кнопки уже созданы, и теперь нужно только изменить их размер.
                    if (_buttons[i, j] == null)
                    {
                        _buttons[i, j] = new GamePanelButton(this, i, j);
                        Controls.Add(_buttons[i, j]);
                        _buttons[i, j].Click += ClickButton;
                    }

                    _buttons[i, j].Width = ButtonSize;
                    _buttons[i, j].Height = ButtonSize;
                    _buttons[i, j].Location = new Point(buttonX, buttonY);

                    buttonY += _orientation == Orientation.Standart ? ButtonSize : -ButtonSize;
                }

                buttonX += _orientation == Orientation.Standart ? ButtonSize : -ButtonSize;
                buttonY = _orientation == Orientation.Standart ? shift : Height - shift - ButtonSize;
            }
        }

        public void Rotate()
        {
            _orientation = _orientation == Orientation.Standart ? Orientation.Reversed : Orientation.Standart;
            SetButtons();
        }

        public void SetButtonSize(int buttonSize)
        {
            if (buttonSize < MinimumButtonSize || buttonSize > MaximumButtonSize)
            {
                return;
            }

            ButtonSize = buttonSize;
            SetButtons();
        }

        public void SetColors(int colorsArrayIndex)
        {
            var colors = new Color[6][]
            {
              new Color[7] {Color.White, Color.Black, Color.SandyBrown, Color.Sienna, Color.Blue, Color.SaddleBrown, Color.Wheat},
              new Color[7] {Color.Goldenrod, Color.DarkRed, Color.White, Color.Black, Color.LawnGreen, Color.Black, Color.Khaki},
              new Color[7] {Color.White, Color.Black, Color.DarkGray, Color.Gray, Color.LightGreen, Color.Black, Color.LightGray},
              new Color[7] {Color.White, Color.Black, Color.Gray, Color.SeaGreen, Color.GreenYellow, Color.DimGray, Color.LightSkyBlue},
              new Color[7] {Color.White, Color.Black, Color.DarkKhaki, Color.Chocolate, Color.DarkBlue, Color.SaddleBrown, Color.SandyBrown},
              new Color[7] {Color.White, Color.Black, Color.Goldenrod, Color.SaddleBrown, Color.Blue, Color.Maroon, Color.Olive}
            };

            var colorsArray = colors[colorsArrayIndex];

            WhitePiecesColor = colorsArray[0];
            BlackPiecesColor = colorsArray[1];
            LightSquaresColor = colorsArray[2];
            DarkSquaresColor = colorsArray[3];
            HighlightColor = colorsArray[4];
            BackColor = colorsArray[5];
            _form.BackColor = colorsArray[6];

            GamePanelButton.SetNewImagesFor(this);

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _buttons[i, j].BackColor = i % 2 == j % 2 ? DarkSquaresColor : LightSquaresColor;
                    _buttons[i, j].FlatAppearance.BorderColor = _buttons[i, j].IsHighlighted || _buttons[i, j].IsOutlined ? HighlightColor : _buttons[i, j].BackColor;
                    _buttons[i, j].RenewImage();
                }
            }
        }

        public void StartNewGame()
        {
            _timer.Stop();
            StopThinking();
            CancelPieceChoice();
            _selectedMove = null;

            var whiteMaterial = new PieceName[16] { PieceName.King, PieceName.Queen,PieceName.Rook, PieceName.Rook, PieceName.Knight, PieceName.Knight, PieceName.Bishop,
                PieceName.Bishop, PieceName.Pawn, PieceName.Pawn, PieceName.Pawn, PieceName.Pawn, PieceName.Pawn, PieceName.Pawn, PieceName.Pawn, PieceName.Pawn };
            var whitePositions = new string[16] { "e1", "d1", "a1", "h1", "b1", "g1", "c1", "f1", "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2" };
            var blackMaterial = new PieceName[16] { PieceName.King, PieceName.Queen,PieceName.Rook, PieceName.Rook, PieceName.Knight, PieceName.Knight, PieceName.Bishop,
                PieceName.Bishop, PieceName.Pawn, PieceName.Pawn, PieceName.Pawn, PieceName.Pawn, PieceName.Pawn, PieceName.Pawn, PieceName.Pawn, PieceName.Pawn };
            var blackPositions = new string[16] { "e8", "d8", "a8", "h8", "b8", "g8", "c8", "f8", "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7" };

            lock (_gameBoard)
            {
                _gameBoard.SetPosition(whiteMaterial, whitePositions, blackMaterial, blackPositions, PieceColor.White);
            }

            RenewButtonsView();
            SetTimeLeft(_timeForGame);

            if (ProgramPlaysFor(PieceColor.White))
            {
                _thinkingThread = new Thread(Think);
                _thinkingThread.Start();
            }

            _timer.Start();
        }

        private void SetTimeLeft(int timeLeft)
        {
            _whiteTimeLeft = timeLeft;
            _blackTimeLeft = timeLeft;
            _form.TimePanel.ShowTime(_whiteTimeLeft, _blackTimeLeft);
        }

        public void SetTimeControl(int timeForGame)
        {
            _timeForGame = timeForGame;
            SetTimeLeft(timeForGame);
        }

        public void ChangePlayer(PieceColor pieceColor)
        {
            _timer.Stop();

            if (pieceColor == PieceColor.White)
            {
                _whiteVirtualPlayer = _whiteVirtualPlayer == null ? new(Strategies.SelectMoveForVirtualFool) : null;
            }
            else
            {
                _blackVirtualPlayer = _blackVirtualPlayer == null ? new(Strategies.SelectMoveForVirtualFool) : null;
            }

            if (pieceColor == _gameBoard.MovingSideColor)
            {
                StopThinking();
                CancelPieceChoice();
                _selectedMove = null;
            }

            if (GameIsOver)
            {
                return;
            }

            if (ProgramPlaysFor(_gameBoard.MovingSideColor))
            {
                _thinkingThread = new Thread(Think);
                _thinkingThread.Start();
            }

            _timer.Start();
        }

        private void CancelPieceChoice()
        {
            if (_highlightedButtonName != null)
            {
                var highlightedButtonCoordinates = SharedItems.GetChessSquareCoordinates(_highlightedButtonName);
                _buttons[highlightedButtonCoordinates[0], highlightedButtonCoordinates[1]].RemoveHighlight();
                _highlightedButtonName = null;
            }
        }

        private void MakeMove(string startSquareName, string destinationSquareName, PieceName? newPieceName)
        {
            try
            {
                lock (_gameBoard)
                {
                    _gameBoard.MakeMove(startSquareName, destinationSquareName, newPieceName);
                }
            }

            catch (IllegalMoveException exception)
            {
                MessageBox.Show(exception.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            catch (NewPieceNotSelectedException)
            {
                new NewPieceMenu(this).Show(Cursor.Position.X, Cursor.Position.Y);
                return;
            }

            _timer.Stop();

            RenewButtonsView();
            var startSquareCoordinates = SharedItems.GetChessSquareCoordinates(startSquareName);
            var destinationSquareCoordinates = SharedItems.GetChessSquareCoordinates(destinationSquareName);
            _buttons[startSquareCoordinates[0], startSquareCoordinates[1]].Outline();
            _buttons[destinationSquareCoordinates[0], destinationSquareCoordinates[1]].Outline();
            _selectedMove = null;

            if (GameIsOver)
            {
                ShowEndGameMessage();
                return;
            }

            // Запускаем выбор программой ответного хода, если нужно.
            if (ProgramPlaysFor(_gameBoard.MovingSideColor))
            {
                _thinkingThread = new Thread(Think);
                _thinkingThread.Start();
            }

            _timer.Start();
        }

        private void MakeSelectedMove() => MakeMove(_selectedMove[0], _selectedMove[1], _selectedMove[2] != null ? Enum.Parse<PieceName>(_selectedMove[2]) : null);

        private void RenewButtonsView()
        {
            var currentPosition = _gameBoard.GetCurrentPosition();

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (_buttons[i, j].IsOutlined)
                    {
                        _buttons[i, j].RemoveOutline();
                    }

                    _buttons[i, j].FlatAppearance.BorderSize = 2;
                    _buttons[i, j].SetDisplayedPiece(currentPosition.GetPieceName(i, j), currentPosition.GetPieceColor(i, j));
                }
            }
        }

        internal void PromotePawn(PieceName newPieceName) => MakeMove(_selectedMove[0], _selectedMove[1], newPieceName);

        private void Think()
        {
            var player = _gameBoard.MovingSideColor == PieceColor.White ? _whiteVirtualPlayer : _blackVirtualPlayer;

            lock (_gameBoard)
            {
                _selectedMove = player.SelectMove(_gameBoard);
            }
        }

        private void StopThinking()
        {
            Program.ThinkingDisabled = true;

            while (_thinkingThread != null && _thinkingThread.ThreadState == ThreadState.Running)
            { }

            Program.ThinkingDisabled = false;
        }

        private void ShowEndGameMessage()
        {
            if (_gameBoard.Status == GameStatus.WhiteWin)
            {
                var message = _blackTimeLeft > 0 ? "Мат черным." : "Время истекло. Победа белых.";
                MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (_gameBoard.Status == GameStatus.BlackWin)
            {
                var message = _whiteTimeLeft > 0 ? "Мат белым." : "Время истекло. Победа черных.";
                MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (_gameBoard.DrawReason == DrawReason.Stalemate)
            {
                MessageBox.Show("Пат.", "", MessageBoxButtons.OK);
                return;
            }

            if (_gameBoard.DrawReason == DrawReason.NotEnoughMaterial)
            {
                MessageBox.Show("Ничья. Недостаточно материала для мата.", "", MessageBoxButtons.OK);
                return;
            }

            if (_gameBoard.DrawReason == DrawReason.ThreeRepeatsRule)
            {
                MessageBox.Show("Ничья. Трехкратное повторение позиции.", "", MessageBoxButtons.OK);
                return;
            }

            MessageBox.Show("Ничья по правилу 50 ходов.", "", MessageBoxButtons.OK);
        }

        public bool ProgramPlaysFor(PieceColor color) => color == PieceColor.White ? _whiteVirtualPlayer != null : _blackVirtualPlayer != null;
        // Программа может играть и сама с собой.

        private void ClickButton(object sender, EventArgs e)
        {
            if (ProgramPlaysFor(_gameBoard.MovingSideColor) || GameIsOver)
            {
                return;
            }

            var button = (GamePanelButton)sender;
            
            // Выбор фигуры для хода.
            if (_highlightedButtonName == null)
            {
                if (button.IsClear)
                {
                    return;
                }

                if (button.DisplayedPieceColor != _gameBoard.MovingSideColor)
                {
                    MessageBox.Show("Это не ваша фигура.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _highlightedButtonName = button.ChessName;
                button.Highlight();
                return; // Запомнили координаты выбранной фигуры, ждем щелчка по полю на которое нужно сходить.
            }

            // Отмена выбора.
            if (button.ChessName == _highlightedButtonName)
            {
                CancelPieceChoice();
                return;
            }

            var highlightedButtonCoordinates = SharedItems.GetChessSquareCoordinates(_highlightedButtonName);
            _buttons[highlightedButtonCoordinates[0], highlightedButtonCoordinates[1]].RemoveHighlight();

            //Замена выбранной фигуры на другую.
            if (button.DisplayedPieceColor == _gameBoard.MovingSideColor)
            {
                _highlightedButtonName = button.ChessName;
                button.Highlight();
                return;
            }

            _selectedMove = new string[3] { _highlightedButtonName, button.ChessName, null };
            _highlightedButtonName = null;
            MakeSelectedMove();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_selectedMove != null && ProgramPlaysFor(_gameBoard.MovingSideColor))
            {
                MakeSelectedMove();
                return;
            }

            if (_gameBoard.MovingSideColor == PieceColor.White)
            {
                --_whiteTimeLeft;
                _form.TimePanel.ShowTime(PieceColor.White, _whiteTimeLeft);

                if (_whiteTimeLeft == 0)
                {
                    _timer.Stop();
                    StopThinking();
                    CancelPieceChoice();
                    _gameBoard.Status = GameStatus.BlackWin;
                    ShowEndGameMessage();
                }
            }
            else
            {
                --_blackTimeLeft;
                _form.TimePanel.ShowTime(PieceColor.Black, _blackTimeLeft);

                if (_blackTimeLeft == 0)
                {
                    _timer.Stop();
                    StopThinking();
                    CancelPieceChoice();
                    _gameBoard.Status = GameStatus.WhiteWin;
                    ShowEndGameMessage();
                }
            }
        }

        public bool GameIsOver => _gameBoard.Status != GameStatus.GameCanContinue;
    }
}
