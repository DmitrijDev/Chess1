using Chess.LogicPart;
using Chess.VirtualPlayer;
using Timer = System.Windows.Forms.Timer;

namespace Chess
{
    internal class GamePanel : Panel
    {
        private readonly GameForm _form;

        private readonly ChessBoard _gameBoard = new();
        private Player _whitePlayer; // == null, если за эту сторону играет пользователь.
        private Player _blackPlayer = Player.GetNewPlayer(0); //Аналогично.
        private Thread _thinkingThread;
        private Move _selectedMove;
        private readonly Timer _moveChecker = new() { Interval = 100 };

        private readonly GamePanelSquare[,] _squares = new GamePanelSquare[8, 8];
        private readonly int _defaultButtonSize;
        private Orientation _orientation = Orientation.Standart;
        private int? _highlightedButtonX;
        private int? _highlightedButtonY;
        private readonly NewPieceMenu _newPieceMenu;

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

            MinimumButtonSize = _form.GetCaptionHeight();
            MaximumButtonSize = (Screen.PrimaryScreen.WorkingArea.Height - _form.GetCaptionHeight() - _form.MenuStrip.Height - _form.TimePanel.Height) / 9;

            if (MaximumButtonSize < MinimumButtonSize)
            {
                MaximumButtonSize = MinimumButtonSize;
            }

            _defaultButtonSize = (Screen.PrimaryScreen.WorkingArea.Height - _form.GetCaptionHeight() - _form.MenuStrip.Height - _form.TimePanel.Height) / 16;

            if (_defaultButtonSize < MinimumButtonSize)
            {
                _defaultButtonSize = MinimumButtonSize;
            }

            ButtonSize = _defaultButtonSize;

            _moveChecker.Tick += MoveChecker_Tick;
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
                    if (_squares[i, j] == null)
                    {
                        _squares[i, j] = new(this, i, j);
                        Controls.Add(_squares[i, j]);
                        _squares[i, j].MouseClick += Square_MouseClick;
                    }

                    _squares[i, j].Width = ButtonSize;
                    _squares[i, j].Height = ButtonSize;
                    _squares[i, j].Location = new(buttonX, buttonY);

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

            GamePanelSquare.SetNewImagesFor(this);

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _squares[i, j].SetColors();
                    _squares[i, j].RenewImage();
                }
            }
        }

        public void StartNewGame()
        {
            StopThinking();

            var pieceNames = new ChessPieceName[] { ChessPieceName.King, ChessPieceName.Queen,ChessPieceName.Rook, ChessPieceName.Rook,
                ChessPieceName.Knight, ChessPieceName.Knight, ChessPieceName.Bishop, ChessPieceName.Bishop, ChessPieceName.Pawn,
                ChessPieceName.Pawn, ChessPieceName.Pawn, ChessPieceName.Pawn, ChessPieceName.Pawn, ChessPieceName.Pawn, ChessPieceName.Pawn,
                ChessPieceName.Pawn };

            var whitePositions = new string[] { "e1", "d1", "a1", "h1", "b1", "g1", "c1", "f1", "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2" };
            var blackPositions = new string[] { "e8", "d8", "a8", "h8", "b8", "g8", "c8", "f8", "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7" };

            _gameBoard.SetPosition(pieceNames, whitePositions, pieceNames, blackPositions, ChessPieceColor.White);
            RenewButtonsView();
            _form.TimePanel.ResetTime();
            StartThinking();
        }

        public void ChangePlayer(ChessPieceColor pieceColor)
        {
            if (pieceColor == _gameBoard.MovingSideColor)
            {
                StopThinking();
            }

            if (pieceColor == ChessPieceColor.White)
            {
                _whitePlayer = _whitePlayer == null ? Player.GetNewPlayer(0) : null;
            }
            else
            {
                _blackPlayer = _blackPlayer == null ? Player.GetNewPlayer(0) : null;
            }

            StartThinking();
        }

        private void CancelMoveChoice()
        {
            if (_highlightedButtonX != null && _highlightedButtonY != null)
            {
                _squares[(int)_highlightedButtonX, (int)_highlightedButtonY].RemoveHighlight();
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

            _form.TimePanel.StopTimer();
            CancelMoveChoice();
            RenewButtonsView();
            _squares[move.StartSquare.Vertical, move.StartSquare.Horizontal].Outline();
            _squares[move.MoveSquare.Vertical, move.MoveSquare.Horizontal].Outline();

            if (_gameBoard.Status != BoardStatus.GameIsIncomplete)
            {
                ShowEndGameMessage();
                return;
            }

            StartThinking();
        }

        private void RenewButtonsView()
        {
            var currentPosition = _gameBoard.GetCurrentPosition();

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (_squares[i, j].IsOutlined)
                    {
                        _squares[i, j].RemoveOutline();
                    }

                    _squares[i, j].DisplayPiece(currentPosition.GetPieceName(i, j), currentPosition.GetPieceColor(i, j));
                }
            }
        }

        public void PromotePawnTo(ChessPieceName newPieceName)
        {
            var move = new Move(_selectedMove.MovingPiece, _selectedMove.MoveSquare, newPieceName);
            MakeMove(move);
        }

        private void StartThinking()
        {
            if (_gameBoard.Status != BoardStatus.GameIsIncomplete)
            {
                return;
            }

            var player = _gameBoard.MovingSideColor == ChessPieceColor.White ? _whitePlayer : _blackPlayer;

            if (player == null)
            {
                _form.TimePanel.StartTimer();
                return;
            }

            if (_thinkingThread != null)
            {
                return;
            }

            _thinkingThread = new(() =>
             {
                 try
                 {
                     _selectedMove = player.SelectMove(_gameBoard);
                 }

                 catch (GameInterruptedException)
                 { }
             });

            _thinkingThread.Start();
            _moveChecker.Start();
            _form.TimePanel.StartTimer();
        }

        private void StopThinking()
        {
            _moveChecker.Stop();
            _form.TimePanel.StopTimer();

            var player = _gameBoard.MovingSideColor == ChessPieceColor.White ? _whitePlayer : _blackPlayer;

            if (player == null)
            {
                CancelMoveChoice();
                return;
            }

            if (_thinkingThread == null)
            {
                return;
            }

            player.ThinkingDisabled = true;

            while (_thinkingThread.ThreadState != ThreadState.Stopped)
            { }

            player.ThinkingDisabled = false;
            _thinkingThread = null;
            _selectedMove = null;
        }

        public void EndGame(BoardStatus gameResult)
        {
            StopThinking();
            _gameBoard.SetStatus(gameResult);
            ShowEndGameMessage();
        }

        private void ShowEndGameMessage()
        {
            switch (_gameBoard.Status)
            {
                case BoardStatus.WhiteWin:
                    {
                        var message = _form.TimePanel.BlackTimeLeft > 0 ? "Мат черным." : "Время истекло. Победа белых.";
                        MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        break;
                    }

                case BoardStatus.BlackWin:
                    {
                        var message = _form.TimePanel.WhiteTimeLeft > 0 ? "Мат белым." : "Время истекло. Победа черных.";
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

        private bool ProgramPlaysFor(ChessPieceColor color) => color == ChessPieceColor.White ? _whitePlayer != null : _blackPlayer != null;
        // Программа может играть и сама с собой.        

        private void Square_MouseClick(object sender, MouseEventArgs e)
        {
            if (ProgramPlaysFor(_gameBoard.MovingSideColor) || _gameBoard.Status != BoardStatus.GameIsIncomplete)
            {
                return;
            }

            var button = (GamePanelSquare)sender;

            // Выбор фигуры для хода.
            if (_highlightedButtonX == null)
            {
                if (button.IsClear || e.Button != MouseButtons.Left)
                {
                    return;
                }

                if (button.DisplayedPieceColor != _gameBoard.MovingSideColor)
                {
                    MessageBox.Show("Это не ваша фигура.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _highlightedButtonX = button.Vertical;
                _highlightedButtonY = button.Horizontal;
                button.Highlight();
                return; // Запомнили координаты выбранной фигуры, ждем щелчка по полю на которое нужно сходить.
            }

            // Отмена выбора.
            if (e.Button != MouseButtons.Left)
            {
                CancelMoveChoice();
                return;
            }

            if (button.Vertical == _highlightedButtonX && button.Horizontal == _highlightedButtonY)
            {
                CancelMoveChoice();
                return;
            }

            //Замена выбранной фигуры на другую.
            if (button.DisplayedPieceColor == _gameBoard.MovingSideColor)
            {
                CancelMoveChoice();
                _highlightedButtonX = button.Vertical;
                _highlightedButtonY = button.Horizontal;
                button.Highlight();
                return;
            }

            var piece = _gameBoard[(int)_highlightedButtonX, (int)_highlightedButtonY].ContainedPiece;
            var square = _gameBoard[button.Vertical, button.Horizontal];
            var move = new Move(piece, square);
            MakeMove(move);
        }

        private void MoveChecker_Tick(object sender, EventArgs e)
        {
            if (_selectedMove != null)
            {
                _moveChecker.Stop();
                _thinkingThread = null;
                MakeMove(_selectedMove);
            }
        }

        public ChessPieceColor MovingSideColor => _gameBoard.MovingSideColor;
    }
}
