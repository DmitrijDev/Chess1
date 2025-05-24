using Chess.LogicPart;
using Chess.VirtualPlayer;
using System.Text;

namespace Chess
{
    internal class GamePanel : Panel
    {
        private readonly Bitmap[][][] _images = new Bitmap[][][]
        {
          new Bitmap[][] { new Bitmap[6], new Bitmap[6] },
          new Bitmap[][] { new Bitmap[6], new Bitmap[6] },
          new Bitmap[][] { new Bitmap[6], new Bitmap[6] }
        };
        /*Три массива, в одном - фигуры на белых полях, в другом - на черных, в третьем - на подсвеченных.
         В каждом из этих массивов еще по два массива: один с белыми фигурами, другой - с черными.*/

        private readonly GamePanelSquare[,] _squares = new GamePanelSquare[8, 8];
        private ContextMenuStrip _newPieceMenu;
        private readonly int _minSquareSize;
        private readonly int _maxSquareSize;

        private Color _lightSquaresColor;
        private Color _darkSquaresColor;
        private Color _highlightColor;
        private Color _outlineColor;
        private SquareLocation? _highlightLocation;

        private readonly ChessBoard _gameBoard = new();
        private ChessRobot _whiteRobot; // == null, если за эту сторону играет пользователь.
        private ChessRobot _blackRobot; //Аналогично.
        private Task<Move> _thinkingTask;
        private bool _isExpectingMove;
        private bool _isExpectingRobotMove;
        private ulong _whitePlayerSwitchesCount;
        private ulong _blackPlayerSwitchesCount;

        public int SquareSize { get; private set; }

        public bool IsReversed { get; private set; }

        public GamePanel(GameForm gameForm)
        {
            BorderStyle = BorderStyle.None;
            _minSquareSize = gameForm.GetCaptionHeight();
            var maxWidth = Screen.PrimaryScreen.WorkingArea.Width - (gameForm.Width - gameForm.ClientRectangle.Width);

            var maxHeight = Screen.PrimaryScreen.WorkingArea.Height - (gameForm.Height - gameForm.ClientRectangle.Height) -
            gameForm.MenuStrip.Height - gameForm.TimePanel.Height;

            var boardMaxSize = Math.Min(maxWidth, maxHeight);
            var shift = Math.Min(_minSquareSize, boardMaxSize / 9 / 2);
            _maxSquareSize = (boardMaxSize - shift * 2) / 8;

            if (_maxSquareSize < _minSquareSize)
            {
                _minSquareSize = _maxSquareSize;
            }

            SquareSize = (_minSquareSize + _maxSquareSize) / 2;

            MouseClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    ShowContextMenu();
                }
            };

            _gameBoard.PositionSet += () =>
            {
                ShowPosition();
                _thinkingTask = null;
                PositionChanged?.Invoke();
                GameStartPositionSet?.Invoke();
                ExpectMoveAsync();
            };

            _gameBoard.MoveMade += () => MakeMoveAsync();

            CreateSquares();
            LocateSquares();
            _whiteRobot = gameForm.WhitePlayerMenu.GetSelectedSwitchItemIndex() == 1 ? RobotsCreator.GetRobot(0) : null;
            _blackRobot = gameForm.BlackPlayerMenu.GetSelectedSwitchItemIndex() == 1 ? RobotsCreator.GetRobot(0) : null;
        }

        private void CreateSquares()
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _squares[i, j] = new();
                    Controls.Add(_squares[i, j]);

                    var squareX = i;
                    var squareY = j;
                    _squares[i, j].MouseClick += (sender, e) => ClickAt(squareX, squareY, e.Button);
                }
            }
        }

        private void LocateSquares()
        {
            var shift = Math.Min(_minSquareSize, SquareSize / 2);
            Width = SquareSize * 8 + shift * 2;
            Height = Width;

            var squareX = !IsReversed ? shift : Width - shift - SquareSize;
            var squareY = !IsReversed ? Height - shift - SquareSize : shift;

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _squares[i, j].Size = new(SquareSize, SquareSize);
                    _squares[i, j].Location = new(squareX, squareY);
                    squareY += !IsReversed ? -SquareSize : SquareSize;
                }

                squareX += !IsReversed ? SquareSize : -SquareSize;
                squareY = !IsReversed ? Height - shift - SquareSize : shift;
            }
        }

        public void SetColors(Color boardColor, Color lightSquaresColor, Color darkSquaresColor, Color whitePiecesColor,
        Color blackPiecesColor, Color highlightColor, Color outlineColor)
        {
            BackColor = boardColor;
            _lightSquaresColor = lightSquaresColor;
            _darkSquaresColor = darkSquaresColor;
            _highlightColor = highlightColor;
            _outlineColor = outlineColor;
            SetImages(whitePiecesColor, blackPiecesColor);

            var lastMove = _gameBoard.LastMove;

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    var squareLocation = new SquareLocation(i, j);

                    var squareColor = _highlightLocation == squareLocation ? _highlightColor :
                    IsLightSquare(i, j) ? _lightSquaresColor : _darkSquaresColor;

                    _squares[i, j].SetColor(squareColor);
                    RenewImage(i, j);

                    if (lastMove?.Start == squareLocation || lastMove?.Destination == squareLocation)
                    {
                        _squares[i, j].Outline(_outlineColor);
                    }
                }
            }
        }

        private void SetImages(Color whitePiecesColor, Color blackPiecesColor)
        {
            var fileNames = new string[] { "Pawn", "Knight", "Bishop", "Rook", "Queen", "King" };
            var originalImages = fileNames.Select(name => new Bitmap("Images/" + name + ".jpg")).ToArray();

            for (var i = 0; i < originalImages.Length; ++i)
            {
                _images[0][0][i] = ChessPieceDrawing.GetColoredPicture(originalImages[i], whitePiecesColor, _lightSquaresColor);
                _images[0][1][i] = ChessPieceDrawing.GetColoredPicture(originalImages[i], blackPiecesColor, _lightSquaresColor);

                _images[1][0][i] = ChessPieceDrawing.GetColoredPicture(originalImages[i], whitePiecesColor, _darkSquaresColor);
                _images[1][1][i] = ChessPieceDrawing.GetColoredPicture(originalImages[i], blackPiecesColor, _darkSquaresColor);

                _images[2][0][i] = ChessPieceDrawing.GetColoredPicture(originalImages[i], whitePiecesColor, _highlightColor);
                _images[2][1][i] = ChessPieceDrawing.GetColoredPicture(originalImages[i], blackPiecesColor, _highlightColor);
            }
        }

        private static bool IsLightSquare(int x, int y) => x % 2 != y % 2;

        private void RenewImage(int x, int y)
        {
            if (_gameBoard[x, y].IsClear)
            {
                _squares[x, y].SetImage(null);
                return;
            }

            var piece = _gameBoard.GetPiece(x, y);
            Bitmap image;

            if (HighlightX == x && HighlightY == y)
            {
                image = _images[2][piece.Color == PieceColor.White ? 0 : 1][(int)piece.Name];
                _squares[x, y].SetImage(image);
                return;
            }

            image = _images[IsLightSquare(x, y) ? 0 : 1][piece.Color == PieceColor.White ? 0 : 1][(int)piece.Name];
            _squares[x, y].SetImage(image);
        }

        public void Rotate()
        {
            IsReversed = !IsReversed;
            LocateSquares();
        }

        public void ShowChangeSizeForm()
        {
            var form = new GamePanelSizeForm(SquareSize, _minSquareSize, _maxSquareSize);
            form.SizeSelected += SetSquareSize;
            form.ShowDialog();
        }

        public void SetSquareSize(int newSquareSize)
        {
            SquareSize = newSquareSize;
            LocateSquares();
        }

        private void ShowContextMenu()
        {
            var menuPosition = Cursor.Position;
            var menu = new ContextMenuStrip();
            var itemTexts = new string[] { "Новая партия", "Развернуть", "Изменить размер" };
            var clickActions = new Action[] { StartNewGame, Rotate, ShowChangeSizeForm };

            for (var i = 0; i < itemTexts.Length; ++i)
            {
                var item = new ToolStripMenuItem(itemTexts[i]);
                menu.Items.Add(item);
                var action = clickActions[i];
                item.Click += (sender, e) => action.Invoke();
            }

            menu.Show(menuPosition.X, menuPosition.Y);
        }

        public void StartNewGame()
        {
            StopExpectingMove();

            var pieceNames = new PieceName[] { PieceName.King, PieceName.Queen,PieceName.Rook, PieceName.Rook,
                PieceName.Knight, PieceName.Knight, PieceName.Bishop, PieceName.Bishop, PieceName.Pawn,
                PieceName.Pawn, PieceName.Pawn, PieceName.Pawn, PieceName.Pawn, PieceName.Pawn, PieceName.Pawn,
                PieceName.Pawn };

            var whitePositions = new string[] { "e1", "d1", "a1", "h1", "b1", "g1", "c1", "f1", "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2" };
            var blackPositions = new string[] { "e8", "d8", "a8", "h8", "b8", "g8", "c8", "f8", "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7" };

            _gameBoard.SetPosition(pieceNames, whitePositions, pieceNames, blackPositions, PieceColor.White);
        }

        private void ShowPosition()
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _squares[i, j].RemoveOutline();
                    RenewImage(i, j);
                }
            }

            var lastMove = _gameBoard.LastMove;

            if (lastMove != null)
            {
                _squares[lastMove.Start.X, lastMove.Start.Y].Outline(_outlineColor);
                _squares[lastMove.Destination.X, lastMove.Destination.Y].Outline(_outlineColor);
            }
        }

        private async Task ExpectMoveAsync()
        {
            if (_isExpectingMove || _gameBoard.Status != BoardStatus.GameIncomplete)
            {
                return;
            }

            var robot = MovingSideColor == PieceColor.White ? _whiteRobot : _blackRobot;

            if (robot == null)
            {
                _isExpectingMove = true;
                StartedExpectingMove?.Invoke();
                return;
            }

            var boardGamesCount = _gameBoard.GamesCount;
            var boardModCount = _gameBoard.ModCount;

            _thinkingTask ??= robot.GetMove(_gameBoard, () => _gameBoard.ModCount != boardModCount ||
            _gameBoard.GamesCount != boardGamesCount);

            var task = _thinkingTask;

            _isExpectingMove = true;
            _isExpectingRobotMove = true;
            StartedExpectingMove?.Invoke();

            await task;

            if (_gameBoard.ModCount == boardModCount && _gameBoard.GamesCount == boardGamesCount && _isExpectingRobotMove)
            {
                _gameBoard.MakeMove(task.Result);
            }
        }

        private void StopExpectingMove()
        {
            if (!_isExpectingMove)
            {
                return;
            }

            _isExpectingMove = false;
            _isExpectingRobotMove = false;
            CancelUserMoveParams();
            StoppedExpectingMove?.Invoke();
        }

        public void CancelUserMoveParams()
        {
            RemoveHighlight();
            _newPieceMenu?.Close();
        }

        private void RemoveHighlight()
        {
            if (_highlightLocation == null)
            {
                return;
            }

            var x = (int)HighlightX;
            var y = (int)HighlightY;
            var color = IsLightSquare(x, y) ? _lightSquaresColor : _darkSquaresColor;
            _squares[x, y].SetColor(color);
            RenewImage(x, y);
            _highlightLocation = null;
        }

        private void HighlightAt(int x, int y)
        {
            RemoveHighlight();
            _squares[x, y].SetColor(_highlightColor);
            RenewImage(x, y);
            _highlightLocation = new(x, y);
        }

        public void SwitchPlayer(PieceColor pieceColor)
        {
            if (pieceColor == PieceColor.White)
            {
                ++_whitePlayerSwitchesCount;
            }
            else
            {
                ++_blackPlayerSwitchesCount;
            }

            if (pieceColor == MovingSideColor)
            {
                StopExpectingMove();
            }

            if (pieceColor == PieceColor.White)
            {
                _whiteRobot = _whiteRobot == null ? RobotsCreator.GetRobot(0) : null;
            }
            else
            {
                _blackRobot = _blackRobot == null ? RobotsCreator.GetRobot(0) : null;
            }

            ExpectMoveAsync();
        }

        private ulong GetPlayerSwitchesCount(PieceColor pieceColor) => pieceColor == PieceColor.White ?
        _whitePlayerSwitchesCount : _blackPlayerSwitchesCount;

        private async Task MakeMoveAsync()
        {
            StopExpectingMove();
            ShowPosition();
            var thinkingTask = _thinkingTask;
            _thinkingTask = null;
            PositionChanged?.Invoke();

            if (_gameBoard.Status != BoardStatus.GameIncomplete)
            {
                ShowEndGameMessage();
                return;
            }

            if (thinkingTask == null)
            {
                ExpectMoveAsync();
                return;
            }

            var movingSideColor = MovingSideColor;
            var gameStartsCount = _gameBoard.GamesCount;
            var playerSwitchesCount = GetPlayerSwitchesCount(movingSideColor);

            var task = RobotPlaysFor(PieceColor.White) && RobotPlaysFor(PieceColor.Black) ?
            Task.WhenAll(Task.Delay(250), thinkingTask) : thinkingTask;

            await task;

            if (_gameBoard.GamesCount == gameStartsCount && GetPlayerSwitchesCount(movingSideColor) == playerSwitchesCount)
            {
                ExpectMoveAsync();
            }
        }

        private void ShowNewPieceMenu(int startX, int startY, int destinationX, int destinationY)
        {
            var menuPosition = Cursor.Position;
            var menu = new ContextMenuStrip();
            var itemTexts = new string[] { "Ферзь", "Ладья", "Конь", "Слон" };
            var pieceNames = new PieceName[] { PieceName.Queen, PieceName.Rook, PieceName.Knight, PieceName.Bishop };

            for (var i = 0; i < itemTexts.Length; ++i)
            {
                var item = new ToolStripMenuItem(itemTexts[i]);
                menu.Items.Add(item);
                var newPieceName = pieceNames[i];
                item.Click += (sender, e) => _gameBoard.MakeMove(startX, startY, destinationX, destinationY, newPieceName);
            }

            menu.Closed += (sender, e) => _newPieceMenu = null;
            _newPieceMenu = menu;
            _newPieceMenu.Show(menuPosition.X, menuPosition.Y);
        }

        public void BreakGame(BoardStatus gameResult, string message)
        {
            StopExpectingMove();
            _gameBoard.BreakGame(gameResult);
            MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void ShowEndGameMessage()
        {
            switch (_gameBoard.Status)
            {
                case BoardStatus.WhiteWon:
                    {
                        MessageBox.Show("Мат! Победа белых.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        break;
                    }

                case BoardStatus.BlackWon:
                    {
                        MessageBox.Show("Мат! Победа черных.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        break;
                    }

                case BoardStatus.Draw:
                    {
                        var message = _gameBoard.DrawReason switch
                        {
                            DrawReason.Stalemate => "Пат.",
                            DrawReason.MaterialLack => "Ничья. Недостаточно материала для мата.",
                            DrawReason.ThreeRepeatsRule => "Ничья. Трехкратное повторение позиции.",
                            _ => "Ничья по правилу 50 ходов."
                        };

                        MessageBox.Show(message, "", MessageBoxButtons.OK);
                        break;
                    }
            };
        }

        private bool RobotPlaysFor(PieceColor color) => color == PieceColor.White ? _whiteRobot != null : _blackRobot != null;

        public void SaveGame()
        {
            if (_gameBoard.MovesCount == 0)
            {
                MessageBox.Show("Игра еще не начата.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var date = DateTime.Now;

            var fileName = new StringBuilder(date.Year).Append(date.Month).Append(date.Day).Append(date.Hour).
            Append(date.Minute).Append(date.Second).Append(date.Millisecond).Append(".txt").ToString();

            using (var writer = new StreamWriter(fileName))
            {
                writer.Write(_gameBoard.GetGameText());
            }

            MessageBox.Show("Игра сохранена.", "", MessageBoxButtons.OK);
        }

        private void ClickAt(int x, int y, MouseButtons mouseButton)
        {
            if (!_isExpectingMove)
            {
                return;
            }

            if (_isExpectingRobotMove)
            {
                MessageBox.Show("Думаю! Не мешайте.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Выбор фигуры для хода.
            if (_highlightLocation == null)
            {
                if (_gameBoard[x, y].IsClear || mouseButton != MouseButtons.Left)
                {
                    return;
                }

                if (_gameBoard.GetPiece(x, y).Color != MovingSideColor)
                {
                    MessageBox.Show("Это не ваша фигура.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                HighlightAt(x, y);
                return; // Запомнили координаты выбранной фигуры, ждем щелчка по полю на которое нужно сходить.
            }

            // Отмена выбора.
            if (mouseButton != MouseButtons.Left)
            {
                if (mouseButton == MouseButtons.Right)
                {
                    CancelUserMoveParams();
                }

                return;
            }

            if (((SquareLocation)_highlightLocation).Corresponds(x, y))
            {
                CancelUserMoveParams();
                return;
            }

            //Замена выбранной фигуры на другую.
            if (_gameBoard.GetPiece(x, y)?.Color == MovingSideColor)
            {
                HighlightAt(x, y);
                return;
            }

            var startX = (int)HighlightX;
            var startY = (int)HighlightY;

            try
            {
                _gameBoard.MakeMove(startX, startY, x, y);
            }

            catch (IllegalMoveException exception)
            {
                if (exception is NewPieceNotSelectedException)
                {
                    ShowNewPieceMenu(startX, startY, x, y);
                }
                else
                {
                    var piece = _gameBoard.GetPiece(startX, startY);
                    var message = GetIllegalMoveMessage(piece, exception);
                    MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static string GetIllegalMoveMessage(ChessPiece piece, IllegalMoveException exception)
        {
            if (exception is KingUnsafetyException)
            {
                if (piece.Name == PieceName.King)
                {
                    if (piece.Square.IsMenacedBy(piece.EnemyColor))
                    {
                        return "Король не может оставаться под шахом.";
                    }

                    return "Король не может становиться под шах.";
                }

                if (exception is PawnPinnedException)
                {
                    return "Невозможный ход: пешка связана.";
                }

                if (exception is PiecePinnedException)
                {
                    return "Невозможный ход: фигура связана.";
                }

                return "Невозможный ход: ваш король под шахом.";
            }

            if (exception is IllegalCastlingException)
            {
                if (exception is KingHasMovedException)
                {
                    return "Рокирова невозможна: король уже сделал ход.";
                }

                if (exception is RookHasMovedException)
                {
                    return "Рокирова невозможна: ладья уже сделала ход.";
                }

                if (exception is CastlingKingCheckedException)
                {
                    return "Рокирова невозможна: король под шахом.";
                }

                return "При рокировке король не может пересекать угрожаемое поле.";
            }

            return "Невозможный ход";
        }

        public PieceColor MovingSideColor => _gameBoard.MoveTurn;

        public int? HighlightX => _highlightLocation?.X;

        public int? HighlightY => _highlightLocation?.Y;

        public event Action PositionChanged;
        public event Action GameStartPositionSet;
        public event Action StartedExpectingMove;
        public event Action StoppedExpectingMove;
    }
}
