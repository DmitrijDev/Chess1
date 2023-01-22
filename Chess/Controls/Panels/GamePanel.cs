﻿using Chess.LogicPart;
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
        private List<int> _clicksCoordinates;
        private int[] _lastMove;
        private bool _programMadeMove;

        private int _whiteTimeLeft;
        private int _blackTimeLeft;
        private readonly Timer _timer = new() { Interval = 1000 };

        public Color WhitePiecesColor { get; private set; } = Color.White;

        public Color BlackPiecesColor { get; private set; } = Color.Black;

        public Color LightSquaresColor { get; private set; } = Color.Goldenrod;

        public Color DarkSquaresColor { get; private set; } = Color.SaddleBrown;

        public Color HighlightColor { get; private set; } = Color.Blue;

        public int InitialButtonSize { get; } = Screen.PrimaryScreen.WorkingArea.Height / 16;

        public int ButtonSize { get; private set; }

        public bool ThinkingDisabled { get; private set; }

        public GamePanel(GameForm form)
        {
            _form = form;
            BackColor = Color.Maroon;
            BorderStyle = BorderStyle.FixedSingle;
            ButtonSize = InitialButtonSize;
            _timer.Tick += new EventHandler(HandleTimerTick);
            SetButtons();
        }

        private void SetButtons()
        {
            var shift = InitialButtonSize / 2;
            Width = ButtonSize * 8 + shift * 2;
            Height = Width;

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
            _timer.Stop();
            StopThinking();
            CancelPieceChoice();
            _lastMove = null;
            _programMadeMove = false;

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

            RenewButtonsView(RenewMode.RenewIfNeeded);

            if (ProgramPlaysFor(PieceColor.White))
            {
                _thinkingThread = new Thread(Think);
                _thinkingThread.Start();
            }

            _whiteTimeLeft = 300;
            _blackTimeLeft = 300;
            _form.ShowTime(_whiteTimeLeft, _blackTimeLeft);
            _timer.Start();
        }

        public void ChangePlayer(PieceColor pieceColor)
        {
            _timer.Stop();
            StopThinking();
            CancelPieceChoice();
            _programMadeMove = false;

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

            if (GameIsOver)
            {
                return;
            }

            _timer.Start();

            if (ProgramPlaysFor(_gameBoard.MovingSideColor))
            {
                _thinkingThread = new Thread(Think);
                _thinkingThread.Start();
            }
        }

        public void HandleClickAt(int x, int y)
        {
            if (ProgramPlaysFor(_gameBoard.MovingSideColor) || GameIsOver)
            {
                return;
            }

            if (_clicksCoordinates == null) // Т.е. выбор фигуры для хода.
            {
                if (_buttons[x, y].DisplayedPieceIndex == 0)
                {
                    return;
                }

                if ((_gameBoard.MovingSideColor == PieceColor.White && _buttons[x, y].DisplayedPieceIndex > 6) ||
                    (_gameBoard.MovingSideColor == PieceColor.Black && _buttons[x, y].DisplayedPieceIndex <= 6))
                {
                    _form.ShowMessage("Это не ваша фигура.");
                    return;
                }

                _clicksCoordinates = new List<int>();
                _clicksCoordinates.Add(x);
                _clicksCoordinates.Add(y);
                HighlightAt(x, y);
                return; // Запомнили координаты выбранной фигуры, ждем щелчка по полю на которое нужно сходить.
            }

            if (x == _clicksCoordinates[0] && y == _clicksCoordinates[1]) // Отмена выбора.
            {
                CancelPieceChoice();
                return;
            }

            if ((_gameBoard.MovingSideColor == PieceColor.White && _buttons[x, y].DisplayedPieceIndex > 0 && _buttons[x, y].DisplayedPieceIndex <= 6) ||
                (_gameBoard.MovingSideColor == PieceColor.Black && _buttons[x, y].DisplayedPieceIndex > 6)) //Замена выбранной фигуры на другую.
            {
                RemoveHighlightAt(_clicksCoordinates[0], _clicksCoordinates[1]);
                _clicksCoordinates.Clear();
                _clicksCoordinates.Add(x);
                _clicksCoordinates.Add(y);
                HighlightAt(x, y);
                return;
            }

            RemoveHighlightAt(_clicksCoordinates[0], _clicksCoordinates[1]);
            _clicksCoordinates.Add(x);
            _clicksCoordinates.Add(y);
            _lastMove = new int[5];
            Array.Copy(_clicksCoordinates.ToArray(), _lastMove, 4);
            _clicksCoordinates = null;
            MakeMove();
        }

        private void MakeMove()
        {
            try
            {
                lock (_gameBoard)
                {
                    _gameBoard.MakeMove(_lastMove);
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

            RenewButtonsView(RenewMode.RenewIfNeeded);
            OutlineButton(_lastMove[0], _lastMove[1]);
            OutlineButton(_lastMove[2], _lastMove[3]);
            _lastMove = null;

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

        private void RenewButtonsView(RenewMode renewMode)
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

        public void PromotePawn(int newPieceIndex)
        {
            _lastMove[4] = newPieceIndex;
            MakeMove();
        }

        private void Think()
        {
            var player = _gameBoard.MovingSideColor == PieceColor.White ? _whiteVirtualPlayer : _blackVirtualPlayer;

            int[] programMove;

            lock (_gameBoard)
            {
                programMove = player.SelectMove(_gameBoard);
            }

            _lastMove = new int[programMove.Length];
            Array.Copy(programMove, _lastMove, programMove.Length);
            _programMadeMove = true;
        }

        private void StopThinking()
        {
            ThinkingDisabled = true;

            while (_thinkingThread != null && _thinkingThread.ThreadState == ThreadState.Running)
            { }

            ThinkingDisabled = false;
        }

        private void HandleTimerTick(object sender, EventArgs e)
        {
            if (_programMadeMove)
            {
                _programMadeMove = false;
                MakeMove();
                return;
            }

            if (_gameBoard.MovingSideColor == PieceColor.White)
            {
                --_whiteTimeLeft;
                _form.ShowTime(PieceColor.White, _whiteTimeLeft);

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
                _form.ShowTime(PieceColor.Black, _blackTimeLeft);

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

        private void OutlineButton(int x, int y) => _buttons[x, y].FlatAppearance.BorderSize = 2;

        private void HighlightAt(int x, int y) => _buttons[x, y].Highlight();

        private void RemoveHighlightAt(int x, int y) => _buttons[x, y].RemoveHighlight();

        private void CancelPieceChoice()
        {
            if (_clicksCoordinates != null)
            {
                RemoveHighlightAt(_clicksCoordinates[0], _clicksCoordinates[1]);
                _clicksCoordinates = null;
            }
        }

        private bool GameIsOver => _gameBoard.Status != GameStatus.GameCanContinue;

        public PieceColor MovingSideColor => _gameBoard.MovingSideColor;

        private bool ProgramPlaysFor(PieceColor color) => color == PieceColor.White ? _whiteVirtualPlayer != null : _blackVirtualPlayer != null;
        // Программа может играть и сама с собой.
    }
}