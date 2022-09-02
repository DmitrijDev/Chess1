using Chess.LogicPart;
using Chess.Players;

namespace Chess
{
    internal class BoardPanel : Panel
    {
        private readonly GameForm _form;

        private VirtualPlayer _whiteVirtualPlayer; // == null, если за эту сторону играет пользователь.
        private VirtualPlayer _blackVirtualPlayer = new(Strategies.SelectMoveForVirtualFool); //Аналогично.

        private readonly ChessBoard _gameBoard = new(); // Для каждого из потоков создаем по отдельному экземпляру доски.
        private ChessBoard _programThinkingBoard;

        private Thread _thinkingThread;

        private readonly SquareButton[,] _buttons = new SquareButton[8, 8];
        private List<int> _clickedButtons = new();
        private int[] _userMove;
        private int[] _lastMove = new int[0];
        private int _movesCount;
        private PieceColor _movingSideColor = PieceColor.White;
        private bool _programMadeMove;

        public Color WhitePiecesColor { get; private set; } = Color.White;

        public Color BlackPiecesColor { get; private set; } = Color.Black;

        public Color LightSquaresColor { get; private set; } = Color.Goldenrod;

        public Color DarkSquaresColor { get; private set; } = Color.SaddleBrown;

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

            while (_thinkingThread != null && _thinkingThread.ThreadState == ThreadState.Running)
            { }

            _movesCount = 0;
            _movingSideColor = PieceColor.White;
            _programMadeMove = false;

            var whiteMaterial = new string[14] { "King", "Queen", "Rook", "Rook", "Bishop", "Bishop", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn" };
            var whitePositions = new string[14] { "e1", "d1", "a1", "h1", "c1", "f1", "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2" };
            var blackMaterial = new string[14] { "King", "Queen", "Rook", "Rook", "Bishop", "Bishop", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn" };
            var blackPositions = new string[14] { "e8", "d8", "a8", "h8", "c8", "f8", "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7" };
            _gameBoard.SetPosition(whiteMaterial, whitePositions, blackMaterial, blackPositions, _movingSideColor);
            _programThinkingBoard = new ChessBoard(_gameBoard);

            RenewButtonsView(RenewMode.RenewIfNeeded);

            if (ProgramPlaysFor(PieceColor.White))
            {
                _whiteVirtualPlayer.SetBoard(_programThinkingBoard);
            }

            if (ProgramPlaysFor(PieceColor.Black))
            {
                _blackVirtualPlayer.SetBoard(_programThinkingBoard);
            }

            if (ProgramPlaysFor(_movingSideColor))
            {
                _thinkingThread = new Thread(Think);
                _thinkingThread.Start();
            }

            _form.Timer.Start();
        }

        public void ChangePlayer(PieceColor pieceColor)
        {
            _form.Timer.Stop();

            while (_thinkingThread != null && _thinkingThread.ThreadState == ThreadState.Running)
            { }

            _programThinkingBoard = new ChessBoard(_gameBoard);
            _movesCount = _gameBoard.MovesCount;
            _movingSideColor = _gameBoard.MovingSideColor;
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

            if (_whiteVirtualPlayer != null)
            {
                _whiteVirtualPlayer.SetBoard(_programThinkingBoard);
            }

            if (_blackVirtualPlayer != null)
            {
                _blackVirtualPlayer.SetBoard(_programThinkingBoard);
            }

            _form.Timer.Start();

            if (ProgramPlaysFor(_movingSideColor))
            {
                _thinkingThread = new Thread(Think);
                _thinkingThread.Start();
            }            
        }

        public void HandleClickAt(int x, int y)
        {
            if (ProgramPlaysFor(_movingSideColor) || GameIsOver)
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
            _userMove = new int[5];
            Array.Copy(_clickedButtons.ToArray(), _userMove, 4);
            _clickedButtons.Clear();
            MakeMove(_userMove);
        }

        private void MakeMove(int[] move)
        {
            try
            {
                _gameBoard.MakeMove(move);
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
            if (ProgramPlaysFor(_movingSideColor))
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
            _userMove[4] = newPieceIndex;
            MakeMove(_userMove);
        }

        private void Think()
        {
            var player = _movingSideColor == PieceColor.White ? _whiteVirtualPlayer : _blackVirtualPlayer;
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

        private void MakeProgramMove(object sender, EventArgs e)
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

        private void OutlineButton(int x, int y) => _buttons[x, y].FlatAppearance.BorderSize = 2;

        private void HighlightAt(int x, int y) => _buttons[x, y].Highlight();

        private void RemoveHighlightAt(int x, int y) => _buttons[x, y].RemoveHighlight();

        private bool GameIsOver => _gameBoard.Status != GameStatus.GameCanContinue;

        public PieceColor MovingSideColor => _movingSideColor;

        private bool ProgramPlaysFor(PieceColor color) => color == PieceColor.White ? _whiteVirtualPlayer != null : _blackVirtualPlayer != null;
        // Программа может играть и сама с собой.
    }
}
