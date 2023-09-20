using Chess.LogicPart;
using Chess.Players;
using Timer = System.Windows.Forms.Timer;

namespace Chess
{
    internal class GamePanel : Panel
    {
        private readonly GameForm _form;

        private VirtualPlayer _whiteVirtualPlayer; // == null, если за эту сторону играет пользователь.
        private VirtualPlayer _blackVirtualPlayer = new Player1(); //Аналогично.

        private readonly ChessBoard _gameBoard = new();
        private Thread _thinkingThread;

        private readonly GamePanelButton[,] _buttons = new GamePanelButton[8, 8];
        private readonly int _defaultButtonSize;

        private Orientation _orientation = Orientation.Standart;

        private int? _highlightedButtonX;
        private int? _highlightedButtonY;

        private readonly NewPieceMenu _newPieceMenu;

        private int _whiteTimeLeft;
        private int _blackTimeLeft;
        private readonly Timer _timer = new() { Interval = 1000 };
        private int _timeForGame = 300; // В секундах.

        private Move _selectedMove;

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

            _newPieceMenu = new(this);
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
                        _buttons[i, j].Click += Button_Click;
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
            CancelMoveChoice();

            var whiteMaterial = new ChessPieceName[] { ChessPieceName.King, ChessPieceName.Queen,ChessPieceName.Rook, ChessPieceName.Rook, ChessPieceName.Knight, ChessPieceName.Knight, ChessPieceName.Bishop,
                ChessPieceName.Bishop, ChessPieceName.Pawn, ChessPieceName.Pawn, ChessPieceName.Pawn, ChessPieceName.Pawn, ChessPieceName.Pawn, ChessPieceName.Pawn, ChessPieceName.Pawn, ChessPieceName.Pawn };
            var whitePositions = new string[] { "e1", "d1", "a1", "h1", "b1", "g1", "c1", "f1", "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2" };
            var blackMaterial = new ChessPieceName[] { ChessPieceName.King, ChessPieceName.Queen,ChessPieceName.Rook, ChessPieceName.Rook, ChessPieceName.Knight, ChessPieceName.Knight, ChessPieceName.Bishop,
                ChessPieceName.Bishop, ChessPieceName.Pawn, ChessPieceName.Pawn, ChessPieceName.Pawn, ChessPieceName.Pawn, ChessPieceName.Pawn, ChessPieceName.Pawn, ChessPieceName.Pawn, ChessPieceName.Pawn };
            var blackPositions = new string[] { "e8", "d8", "a8", "h8", "b8", "g8", "c8", "f8", "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7" };

            _gameBoard.SetPosition(whiteMaterial, whitePositions, blackMaterial, blackPositions, ChessPieceColor.White);

            RenewButtonsView();
            SetTimeLeft(_timeForGame);

            if (ProgramPlaysFor(ChessPieceColor.White))
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

        public void ChangePlayer(ChessPieceColor pieceColor)
        {
            _timer.Stop();

            if (pieceColor == ChessPieceColor.White)
            {
                _whiteVirtualPlayer = _whiteVirtualPlayer == null ? new Player1() : null;
            }
            else
            {
                _blackVirtualPlayer = _blackVirtualPlayer == null ? new Player1() : null;
            }

            if (pieceColor == _gameBoard.MovingSideColor)
            {
                StopThinking();
                CancelMoveChoice();
            }

            if (GameIsOver)
            {
                return;
            }

            if (ProgramPlaysFor(_gameBoard.MovingSideColor) && (_thinkingThread == null || _thinkingThread.ThreadState == ThreadState.Stopped))
            {
                _thinkingThread = new Thread(Think);
                _thinkingThread.Start();
            }

            _timer.Start();
        }

        private void CancelMoveChoice()
        {
            if (_highlightedButtonX != null && _highlightedButtonY != null)
            {
                _buttons[(int)_highlightedButtonX, (int)_highlightedButtonY].RemoveHighlight();
            }

            _highlightedButtonX = null;
            _highlightedButtonY = null;
            _selectedMove = null;
            _newPieceMenu.Close();
        }

        private void MakeMove(Move move)
        {
            try
            {
                _gameBoard.MakeMove(move);
            }

            catch (IllegalMoveException exception)
            {
                if (exception is not NewPieceNotSelectedException)
                {
                    MessageBox.Show(exception.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    _selectedMove = move;
                    _newPieceMenu.Show(Cursor.Position.X, Cursor.Position.Y);
                }

                return;
            }

            _timer.Stop();
            CancelMoveChoice();
            RenewButtonsView();
            _buttons[move.StartSquare.Vertical, move.StartSquare.Horizontal].Outline();
            _buttons[move.MoveSquare.Vertical, move.MoveSquare.Horizontal].Outline();

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
                    _buttons[i, j].DisplayPiece(currentPosition.GetPieceName(i, j), currentPosition.GetPieceColor(i, j));
                }
            }
        }

        public void PromotePawnTo(ChessPieceName newPieceName)
        {
            var move = new Move(_selectedMove.MovingPiece, _selectedMove.MoveSquare, newPieceName);
            MakeMove(move);
        }

        private void Think()
        {
            var player = _gameBoard.MovingSideColor == ChessPieceColor.White ? _whiteVirtualPlayer : _blackVirtualPlayer;

            try
            {
                _selectedMove = player.SelectMove(_gameBoard);
            }

            catch (ApplicationException)
            { }
        }

        private void StopThinking()
        {
            if (!ProgramPlaysFor(_gameBoard.MovingSideColor))
            {
                return;
            }

            var player = _gameBoard.MovingSideColor == ChessPieceColor.White ? _whiteVirtualPlayer : _blackVirtualPlayer;
            player.DisableThinking();

            while (_thinkingThread != null && _thinkingThread.ThreadState == ThreadState.Running)
            { }

            player.EnableThinking();
        }

        private void ShowEndGameMessage()
        {
            switch (_gameBoard.Status)
            {
                case BoardStatus.WhiteWin:
                    {
                        var message = _blackTimeLeft > 0 ? "Мат черным." : "Время истекло. Победа белых.";
                        MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        break;
                    }

                case BoardStatus.BlackWin:
                    {
                        var message = _whiteTimeLeft > 0 ? "Мат белым." : "Время истекло. Победа черных.";
                        MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        break;
                    }

                case BoardStatus.Draw:
                    {
                        var message = _gameBoard.DrawReason switch
                        {
                            DrawReason.Stalemate => "Пат.",
                            DrawReason.NotEnoughMaterial => "Ничья. Недостаточно материала для мата.",
                            DrawReason.ThreeRepeatsRule => "Ничья. Трехкратное повторение позиции.",
                            _ => "Ничья по правилу 50 ходов."
                        };

                        MessageBox.Show(message, "", MessageBoxButtons.OK);
                        break;
                    }
            };
        }

        private bool ProgramPlaysFor(ChessPieceColor color) => color == ChessPieceColor.White ? _whiteVirtualPlayer != null : _blackVirtualPlayer != null;
        // Программа может играть и сама с собой.

        private void Button_Click(object sender, EventArgs e)
        {
            if (ProgramPlaysFor(_gameBoard.MovingSideColor) || !_timer.Enabled || _gameBoard.Status != BoardStatus.GameIsIncomplete)
            {
                return;
            }

            var button = (GamePanelButton)sender;

            // Выбор фигуры для хода.
            if (_highlightedButtonX == null)
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

                _highlightedButtonX = button.X;
                _highlightedButtonY = button.Y;
                button.Highlight();
                return; // Запомнили координаты выбранной фигуры, ждем щелчка по полю на которое нужно сходить.
            }

            // Отмена выбора.
            if (button.X == _highlightedButtonX && button.Y == _highlightedButtonY)
            {
                CancelMoveChoice();
                return;
            }

            //Замена выбранной фигуры на другую.
            if (button.DisplayedPieceColor == _gameBoard.MovingSideColor)
            {
                CancelMoveChoice();
                _highlightedButtonX = button.X;
                _highlightedButtonY = button.Y;
                button.Highlight();
                return;
            }

            var piece = _gameBoard[(int)_highlightedButtonX, (int)_highlightedButtonY].ContainedPiece;
            var square = _gameBoard[button.X, button.Y];
            var move = new Move(piece, square);
            MakeMove(move);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (ProgramPlaysFor(_gameBoard.MovingSideColor) && _selectedMove != null)
            {
                MakeMove(_selectedMove);
                return;
            }

            if (_gameBoard.MovingSideColor == ChessPieceColor.White)
            {
                --_whiteTimeLeft;
                _form.TimePanel.ShowTime(ChessPieceColor.White, _whiteTimeLeft);
            }
            else
            {
                --_blackTimeLeft;
                _form.TimePanel.ShowTime(ChessPieceColor.Black, _blackTimeLeft);
            }

            if (_whiteTimeLeft == 0 || _blackTimeLeft == 0)
            {
                _timer.Stop();
                StopThinking();
                CancelMoveChoice();
                _gameBoard.SetStatus(_whiteTimeLeft == 0 ? BoardStatus.BlackWin : BoardStatus.WhiteWin);
                ShowEndGameMessage();
            }
        }

        public bool GameIsOver => _gameBoard.Status == BoardStatus.WhiteWin || _gameBoard.Status == BoardStatus.BlackWin || _gameBoard.Status == BoardStatus.Draw;
    }
}
