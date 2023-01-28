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

        private readonly SquareButton[,] _buttons = new SquareButton[8, 8];
        private readonly int _defaultButtonSize = Screen.PrimaryScreen.WorkingArea.Height / 16;
        private Orientation _orientation = Orientation.Standart;

        private List<int> _clicksCoordinates;
        private int[] _selectedMove;
        private bool _programSelectedMove;

        private int _whiteTimeLeft;
        private int _blackTimeLeft;
        private readonly Timer _timer = new() { Interval = 1000 };
        private int _timeForGame = 300; // В секундах.

        public Color WhitePiecesColor { get; private set; } = Color.White;

        public Color BlackPiecesColor { get; private set; } = Color.Black;

        public Color LightSquaresColor { get; private set; } = Color.Goldenrod;

        public Color DarkSquaresColor { get; private set; } = Color.SaddleBrown;

        public Color HighlightColor { get; private set; } = Color.Blue;

        public int ButtonSize { get; private set; }

        public GamePanel(GameForm form)
        {
            _form = form;
            BackColor = Color.Maroon;
            BorderStyle = BorderStyle.FixedSingle;
            ButtonSize = _defaultButtonSize;
            _timer.Tick += Timer_Tick;
            _form.FormClosing += (sender, e) => StopThinking();
            SetButtons();
        }

        private void SetButtons()
        {
            var shift = Math.Min(_defaultButtonSize / 2, ButtonSize / 2);
            Width = ButtonSize * 8 + shift * 2;
            Height = Width;

            var buttonColor = LightSquaresColor;
            var buttonX = _orientation == Orientation.Standart ? shift : Width - shift - ButtonSize;
            var buttonY = _orientation == Orientation.Standart ? shift : Height - shift - ButtonSize;

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 7; j >= 0; --j)
                {
                    // Возможно, кнопки уже созданы, и теперь нужно только изменить их размер.
                    if (_buttons[i, j] == null)
                    {
                        _buttons[i, j] = new SquareButton(this, i, j)
                        {
                            BackColor = buttonColor
                        };

                        Controls.Add(_buttons[i, j]);
                        _buttons[i, j].Click += ClickButton;
                    }

                    _buttons[i, j].Width = ButtonSize;
                    _buttons[i, j].Height = ButtonSize;
                    _buttons[i, j].Location = new Point(buttonX, buttonY);

                    buttonColor = buttonColor == LightSquaresColor ? DarkSquaresColor : LightSquaresColor;
                    buttonY += _orientation == Orientation.Standart ? ButtonSize : -ButtonSize;
                }

                buttonColor = buttonColor == LightSquaresColor ? DarkSquaresColor : LightSquaresColor;
                buttonX += _orientation == Orientation.Standart ? ButtonSize : -ButtonSize;
                buttonY = _orientation == Orientation.Standart ? shift : Height - shift - ButtonSize;
            }
        }

        public void Rotate()
        {
            _orientation = _orientation == Orientation.Standart ? Orientation.Reversed : Orientation.Standart;
            SetButtons();
        }

        /*public void SetSizeAndColors(int buttonSize, Color lightSquaresColor, Color darkSquaresColor)
        {
            ButtonSize = buttonSize;
            LightSquaresColor = lightSquaresColor;
            DarkSquaresColor = darkSquaresColor;
            SetButtons();
            RenewButtonsView(RenewMode.FullRenew);
        }*/

        public void StartNewGame()
        {
            _timer.Stop();
            StopThinking();
            CancelPieceChoice();
            _selectedMove = null;
            _programSelectedMove = false;

            var whiteMaterial = new string[16] { "King", "Queen", "Rook", "Rook", "Knight", "Knight", "Bishop", "Bishop",
                "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn" };
            var whitePositions = new string[16] { "e1", "d1", "a1", "h1", "b1", "g1", "c1", "f1", "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2" };
            var blackMaterial = new string[16] { "King", "Queen", "Rook", "Rook", "Knight", "Knight", "Bishop", "Bishop",
                "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn" };
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

            if (ProgramPlaysFor(_gameBoard.MovingSideColor))
            {
                StopThinking();
            }

            _programSelectedMove = false;

            if (pieceColor == PieceColor.White)
            {
                if (_whiteVirtualPlayer != null)
                {
                    _whiteVirtualPlayer = null;
                }
                else
                {
                    _whiteVirtualPlayer = new(Strategies.SelectMoveForVirtualFool);
                }
            }
            else
            {
                if (_blackVirtualPlayer != null)
                {
                    _blackVirtualPlayer = null;
                }
                else
                {
                    _blackVirtualPlayer = new(Strategies.SelectMoveForVirtualFool);
                }
            }

            if (ProgramPlaysFor(_gameBoard.MovingSideColor))
            {
                CancelPieceChoice();
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
            if (_clicksCoordinates != null)
            {
                _buttons[_clicksCoordinates[0], _clicksCoordinates[1]].RemoveHighlight();
                _clicksCoordinates = null;
            }
        }

        private void MakeSelectedMove()
        {
            try
            {
                lock (_gameBoard)
                {
                    _gameBoard.MakeMove(_selectedMove);
                }
            }

            catch (IllegalMoveException exception) // На случай, если ход не по правилам.
            {
                _form.ShowMessage(exception.Message);
                return;
            }

            catch (NewPieceNotSelectedException)
            {
                new NewPieceMenu(this).Show(Cursor.Position.X, Cursor.Position.Y);
                return;
            }

            RenewButtonsView();
            _buttons[_selectedMove[0], _selectedMove[1]].Outline();
            _buttons[_selectedMove[2], _selectedMove[3]].Outline();
            _selectedMove = null;

            if (GameIsOver)
            {
                _timer.Stop();
                ShowEndGameMessage();
                return;
            }

            // Запускаем выбор программой ответного хода, если нужно.
            if (ProgramPlaysFor(_gameBoard.MovingSideColor))
            {
                _thinkingThread = new Thread(Think);
                _thinkingThread.Start();
            }
        }

        private void RenewButtonsView()
        {
            var currentPosition = _gameBoard.GetCurrentPosition();

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _buttons[i, j].RemoveOutline();

                    if (_buttons[i, j].DisplayedPieceIndex != currentPosition[i, j])
                    {
                        _buttons[i, j].DisplayedPieceIndex = currentPosition[i, j];
                        _buttons[i, j].RenewImage();
                    }
                }
            }
        }

        public void PromotePawn(int newPieceIndex)
        {
            _selectedMove[4] = newPieceIndex;
            MakeSelectedMove();
        }

        private void Think()
        {
            var player = _gameBoard.MovingSideColor == PieceColor.White ? _whiteVirtualPlayer : _blackVirtualPlayer;

            lock (_gameBoard)
            {
                _selectedMove = player.SelectMove(_gameBoard);
            }

            _programSelectedMove = true;
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
                _form.ShowMessage(_blackTimeLeft > 0 ? "Мат черным." : "Время истекло. Победа белых.");
                return;
            }

            if (_gameBoard.Status == GameStatus.BlackWin)
            {
                _form.ShowMessage(_whiteTimeLeft > 0 ? "Мат белым." : "Время истекло. Победа черных.");
                return;
            }

            if (_gameBoard.DrawReason == DrawReason.Stalemate)
            {
                _form.ShowMessage("Пат.");
                return;
            }

            if (_gameBoard.DrawReason == DrawReason.NotEnoughMaterial)
            {
                _form.ShowMessage("Ничья. Недостаточно материала для мата.");
                return;
            }

            if (_gameBoard.DrawReason == DrawReason.ThreeRepeatsRule)
            {
                _form.ShowMessage("Ничья. Трехкратное повторение позиции.");
                return;
            }

            _form.ShowMessage("Ничья по правилу 50 ходов.");
        }

        public bool ProgramPlaysFor(PieceColor color) => color == PieceColor.White ? _whiteVirtualPlayer != null : _blackVirtualPlayer != null;
        // Программа может играть и сама с собой.

        private void ClickButton(object sender, EventArgs e)
        {
            if (ProgramPlaysFor(_gameBoard.MovingSideColor) || GameIsOver)
            {
                return;
            }

            var button = (SquareButton)sender;

            if (_clicksCoordinates == null) // Т.е. выбор фигуры для хода.
            {
                if (button.DisplayedPieceIndex == 0)
                {
                    return;
                }

                if ((_gameBoard.MovingSideColor == PieceColor.White && button.DisplayedPieceIndex > 6) ||
                    (_gameBoard.MovingSideColor == PieceColor.Black && button.DisplayedPieceIndex <= 6))
                {
                    _form.ShowMessage("Это не ваша фигура.");
                    return;
                }

                _clicksCoordinates = new();
                _clicksCoordinates.Add(button.X);
                _clicksCoordinates.Add(button.Y);
                button.Highlight();
                return; // Запомнили координаты выбранной фигуры, ждем щелчка по полю на которое нужно сходить.
            }

            if (button.X == _clicksCoordinates[0] && button.Y == _clicksCoordinates[1]) // Отмена выбора.
            {
                CancelPieceChoice();
                return;
            }

            if ((_gameBoard.MovingSideColor == PieceColor.White && button.DisplayedPieceIndex > 0 && button.DisplayedPieceIndex <= 6) ||
                (_gameBoard.MovingSideColor == PieceColor.Black && button.DisplayedPieceIndex > 6)) //Замена выбранной фигуры на другую.
            {
                _buttons[_clicksCoordinates[0], _clicksCoordinates[1]].RemoveHighlight();
                _clicksCoordinates.Clear();
                _clicksCoordinates.Add(button.X);
                _clicksCoordinates.Add(button.Y);
                button.Highlight();
                return;
            }

            _buttons[_clicksCoordinates[0], _clicksCoordinates[1]].RemoveHighlight();
            _clicksCoordinates.Add(button.X);
            _clicksCoordinates.Add(button.Y);
            _selectedMove = new int[5];
            Array.Copy(_clicksCoordinates.ToArray(), _selectedMove, 4);
            _clicksCoordinates = null;
            MakeSelectedMove();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_programSelectedMove)
            {
                _programSelectedMove = false;
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
                    _gameBoard.Status = GameStatus.BlackWin;
                    CancelPieceChoice();
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
                    _gameBoard.Status = GameStatus.WhiteWin;
                    CancelPieceChoice();
                    ShowEndGameMessage();
                }
            }
        }                

        public bool GameIsOver => _gameBoard.Status != GameStatus.GameCanContinue;

        public PieceColor MovingSideColor => _gameBoard.MovingSideColor;
    }
}
