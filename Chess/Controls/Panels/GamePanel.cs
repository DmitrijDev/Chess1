using Chess.LogicPart;
using Chess.VirtualPlayer;
using System.Text;
using Timer = System.Windows.Forms.Timer;

namespace Chess
{
    internal class GamePanel : Panel
    {
        private readonly GameForm _form;

        private readonly ChessBoard _gameBoard = new();
        private IChessRobot _whiteRobot; // == null, если за эту сторону играет пользователь.
        private IChessRobot _blackRobot; //Аналогично.
        private Thread _thinkingThread;
        private Move _selectedMove;
        private readonly Timer _moveChecker = new() { Interval = 100 };

        private readonly GamePanelSquare[,] _squares = new GamePanelSquare[8, 8];
        private readonly NewPieceMenu _newPieceMenu;

        public int ButtonSize { get; private set; }

        public int MinimumButtonSize { get; private set; }

        public int MaximumButtonSize { get; private set; }

        public int DefaultButtonSize { get; }

        public bool IsReversed { get; private set; }

        public int? HighlightedButtonX { get; private set; }

        public int? HighlightedButtonY { get; private set; }

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

            DefaultButtonSize = (Screen.PrimaryScreen.WorkingArea.Height - _form.GetCaptionHeight() - _form.MenuStrip.Height - _form.TimePanel.Height) / 16;

            if (DefaultButtonSize < MinimumButtonSize)
            {
                DefaultButtonSize = MinimumButtonSize;
            }

            ButtonSize = DefaultButtonSize;

            MouseClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    CancelMoveChoice();
                }
            };

            _moveChecker.Tick += MoveChecker_Tick;
            _form.FormClosing += (sender, e) => StopThinking();

            SetButtons();
            _newPieceMenu = new(this);
            _whiteRobot = _form.WhitePlayerMenu.SelectedItemIndex == 1 ? RobotsConstructor.GetRobot(0) : null;
            _blackRobot = _form.BlackPlayerMenu.SelectedItemIndex == 1 ? RobotsConstructor.GetRobot(0) : null;
        }

        private void SetButtons()
        {
            var shift = Math.Min(DefaultButtonSize / 2, ButtonSize / 2);
            Width = ButtonSize * 8 + shift * 2;
            Height = Width;

            var buttonX = !IsReversed ? shift : Width - shift - ButtonSize;
            var buttonY = !IsReversed ? shift : Height - shift - ButtonSize;

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

                    buttonY += !IsReversed ? ButtonSize : -ButtonSize;
                }

                buttonX += !IsReversed ? ButtonSize : -ButtonSize;
                buttonY = !IsReversed ? shift : Height - shift - ButtonSize;
            }
        }

        public void Rotate()
        {
            IsReversed = !IsReversed;
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

        public void SetColors()
        {
            BackColor = _form.ColorSet.BoardColor;
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
                _whiteRobot = _whiteRobot == null ? RobotsConstructor.GetRobot(0) : null;
            }
            else
            {
                _blackRobot = _blackRobot == null ? RobotsConstructor.GetRobot(0) : null;
            }

            StartThinking();
        }

        public void CancelMoveChoice()
        {
            if (HighlightedButtonX != null && HighlightedButtonY != null)
            {
                _squares[(int)HighlightedButtonX, (int)HighlightedButtonY].RemoveHighlight();
            }

            HighlightedButtonX = null;
            HighlightedButtonY = null;
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
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (_squares[i, j].IsOutlined)
                    {
                        _squares[i, j].RemoveOutline();
                    }

                    var piece = _gameBoard[i, j].ContainedPiece;

                    if (piece != null)
                    {
                        _squares[i, j].DisplayPiece(piece.Name, piece.Color);
                    }
                    else
                    {
                        _squares[i, j].DisplayPiece(null, null);
                    }
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

            var robot = _gameBoard.MovingSideColor == ChessPieceColor.White ? _whiteRobot : _blackRobot;

            if (robot == null)
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
                     _selectedMove = robot.SelectMove(_gameBoard);
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

            var robot = _gameBoard.MovingSideColor == ChessPieceColor.White ? _whiteRobot : _blackRobot;

            if (robot == null)
            {
                CancelMoveChoice();
                return;
            }

            if (_thinkingThread == null)
            {
                return;
            }

            robot.ThinkingDisabled = true;

            while (_thinkingThread.ThreadState == ThreadState.Running)
            { }

            robot.ThinkingDisabled = false;
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

        private bool ProgramPlaysFor(ChessPieceColor color) => color == ChessPieceColor.White ? _whiteRobot != null : _blackRobot != null;
        // Программа может играть и сама с собой.
         
        public void SaveGame()
        {
            var date = DateTime.Now;

            var fileName = new StringBuilder(date.Year).Append(date.Month).Append(date.Day).Append(date.Hour).
            Append(date.Minute).Append(date.Second).Append(date.Millisecond).Append(".txt").ToString();

            using (var writer = new StreamWriter(fileName))
            {
                writer.Write(_gameBoard.GetGameText());
            }

            MessageBox.Show("Игра сохранена.", "", MessageBoxButtons.OK);
        }

        private void Square_MouseClick(object sender, MouseEventArgs e)
        {
            if (ProgramPlaysFor(_gameBoard.MovingSideColor) || _gameBoard.Status != BoardStatus.GameIsIncomplete)
            {
                return;
            }

            var squareControl = (GamePanelSquare)sender;

            // Выбор фигуры для хода.
            if (HighlightedButtonX == null)
            {
                if (squareControl.IsClear || e.Button != MouseButtons.Left)
                {
                    return;
                }

                if (squareControl.DisplayedPieceColor != _gameBoard.MovingSideColor)
                {
                    MessageBox.Show("Это не ваша фигура.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                HighlightedButtonX = squareControl.Vertical;
                HighlightedButtonY = squareControl.Horizontal;
                squareControl.Highlight();
                return; // Запомнили координаты выбранной фигуры, ждем щелчка по полю на которое нужно сходить.
            }

            // Отмена выбора.
            if (e.Button != MouseButtons.Left)
            {
                if (e.Button == MouseButtons.Right)
                {
                    CancelMoveChoice();
                }

                return;
            }

            if (squareControl.Vertical == HighlightedButtonX && squareControl.Horizontal == HighlightedButtonY)
            {
                return;
            }

            //Замена выбранной фигуры на другую.
            if (squareControl.DisplayedPieceColor == _gameBoard.MovingSideColor)
            {
                CancelMoveChoice();
                HighlightedButtonX = squareControl.Vertical;
                HighlightedButtonY = squareControl.Horizontal;
                squareControl.Highlight();
                return;
            }

            var piece = _gameBoard[(int)HighlightedButtonX, (int)HighlightedButtonY].ContainedPiece;
            var square = _gameBoard[squareControl.Vertical, squareControl.Horizontal];
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

        public Color WhitePiecesColor => _form.ColorSet.WhitePiecesColor;

        public Color BlackPiecesColor => _form.ColorSet.BlackPiecesColor;

        public Color LightSquaresColor => _form.ColorSet.LightSquaresColor;

        public Color DarkSquaresColor => _form.ColorSet.DarkSquaresColor;

        public Color HighlightColor => _form.ColorSet.HighlightColor;

        public Color OutlineColor => _form.ColorSet.OutlineColor;
    }
}
