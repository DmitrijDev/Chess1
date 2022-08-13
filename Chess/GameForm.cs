using Chess.LogicPart;
using Chess.Players;
using Timer = System.Windows.Forms.Timer;

namespace Chess
{
    public partial class GameForm : Form
    {
        private readonly ChessBoard _whiteThinkingBoard;
        private readonly ChessBoard _blackThinkingBoard;
        private readonly SquareButton[,] _formButtons = new SquareButton[8, 8];
        private readonly Timer _timer = new() { Interval = 1000 };
        private int[] _lastMove = new int[0];
        private int _movesCount;
        private bool _programMadeMove;

        public ChessBoard GameBoard { get; private set; } = new ChessBoard();

        public VirtualPlayer WhiteVirtualPlayer { get; private set; } //= new VirtualPlayer(Strategies.ChooseMoveForVirtualFool);
        // == null, если за эту сторону играет пользователь.

        public VirtualPlayer BlackVirtualPlayer { get; private set; } = new VirtualPlayer(Strategies.ChooseMoveForVirtualFool);
        //Аналогично.

        public List<int> ClickedButtons { get; } = new();

        public int ButtonSize { get; private set; } = Screen.PrimaryScreen.WorkingArea.Height / 16;

        public Color LightSquaresColor { get; private set; } = Color.Gold;

        public Color DarkSquaresColor { get; private set; } = Color.Chocolate;

        public GameForm()
        {
            InitializeComponent();
            AutoSize = true;
            SetButtons();
            var whiteMaterial = new string[3] { "King", "Rook", "Rook" };
            var whitePositions = new string[3] { "e1", "a1", "h1" };
            var blackMaterial = new string[3] { "King", "Rook", "Rook" };
            var blackPositions = new string[3] { "e8", "a8", "h8" };
            GameBoard.SetPosition(whiteMaterial, whitePositions, blackMaterial, blackPositions, PieceColor.White);
            _whiteThinkingBoard = new ChessBoard(GameBoard);
            _blackThinkingBoard = new ChessBoard(GameBoard);

            if (WhiteVirtualPlayer != null)
            {
                WhiteVirtualPlayer.SetBoard(_whiteThinkingBoard);
            }

            if (BlackVirtualPlayer != null)
            {
                BlackVirtualPlayer.SetBoard(_blackThinkingBoard);
            }

            RenewPosition();
            _timer.Tick += new EventHandler(MakeProgramMove);
            _timer.Start();

            if (WhiteVirtualPlayer != null)
            {
                new Thread(Think).Start();
            }
        }

        /*private void ChessGameForm_Load(object sender, EventArgs e)
        {
        }*/

        public void SetButtons()
        {
            MinimumSize = new Size(0, 0);
            MaximumSize = new Size(int.MaxValue, int.MaxValue);
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            var boardSize = _formButtons.GetLength(0);
            var shift = ButtonSize / 2;
            var buttonColor = LightSquaresColor;
            var buttonX = shift;
            var buttonY = shift;

            for (var i = 0; i < boardSize; ++i)
            {
                for (var j = boardSize - 1; j >= 0; --j)
                {
                    // Если кнопки ранее уже созданы, а теперь мы хотим изменить размер полей, то старые кнопки нужно удалить.
                    if (_formButtons[i, j] != null)
                    {
                        Controls.Remove(_formButtons[i, j]);
                    }

                    var newButton = new SquareButton(this, i, j)
                    {
                        BackColor = buttonColor,
                        Location = new Point(buttonX, buttonY)
                    };

                    _formButtons[i, j] = newButton;
                    Controls.Add(newButton);

                    buttonColor = buttonColor == LightSquaresColor ? DarkSquaresColor : LightSquaresColor;
                    buttonY += ButtonSize;
                }

                buttonColor = buttonColor == LightSquaresColor ? DarkSquaresColor : LightSquaresColor;
                buttonX += ButtonSize;
                buttonY = shift;
            }

            AutoSizeMode = AutoSizeMode.GrowOnly;
            Height += shift;
            Width += shift;
            MinimumSize = new Size(Width, Height);
            MaximumSize = MinimumSize;
        }

        public void SetSizeAndColors(int buttonSize, Color lightSquaresColor, Color darkSquaresColor)
        {
            ButtonSize = buttonSize;
            LightSquaresColor = lightSquaresColor;
            DarkSquaresColor = darkSquaresColor;
            SetButtons();
        }

        public void RenewPosition()
        {
            var currentPosition = GameBoard.CurrentPosition;

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _formButtons[i, j].ContainedPieceIndex = currentPosition[i, j];
                    _formButtons[i, j].RenewText();
                }
            }
        }

        public void MakeMove(int[] move)
        {
            try
            {
                GameBoard.MakeMove(move);
            }

            catch (Exception exception) // На случай, если ход не по правилам.
            {
                ShowMessage(exception.Message);
                return;
            }

            RenewPosition();

            lock (_lastMove)
            {
                _lastMove = new int[move.Length];
                Array.Copy(move, 0, _lastMove, 0, move.Length);
            }

            _movesCount = GameBoard.MovesCount;

            if (GameBoard.Status == GameStatus.WhiteWin)
            {
                _timer.Stop();
                ShowMessage("Мат черным.");
                return;
            }

            if (GameBoard.Status == GameStatus.BlackWin)
            {
                _timer.Stop();
                ShowMessage("Мат белым.");
                return;
            }

            if (GameBoard.Status == GameStatus.Draw)
            {
                _timer.Stop();
                ShowMessage("Ничья.");
                return;
            }

            // Запускаем выбор программой ответного хода, если нужно.
            if ((MovingSideColor == PieceColor.White && ProgramPlaysForWhite) || (MovingSideColor == PieceColor.Black && ProgramPlaysForBlack))
            {
                new Thread(Think).Start();
            }
        }

        public void ShowMessage(string message) => MessageBox.Show(message, "", MessageBoxButtons.OK);
        
        public void Think()
        {
            var board = MovingSideColor == PieceColor.White ? _whiteThinkingBoard : _blackThinkingBoard;
            var player = MovingSideColor == PieceColor.White ? WhiteVirtualPlayer : BlackVirtualPlayer;

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

            //Thread.Sleep(5000); // Можно проверить будет ли работать интерфейс пока программа думает.
            var replyMove = player.ChooseMove();
            board.MakeMove(replyMove);

            lock (_lastMove)
            {
                _lastMove = new int[replyMove.Length];
                Array.Copy(replyMove, 0, _lastMove, 0, replyMove.Length);
            }

            _movesCount = board.MovesCount;
            _programMadeMove = true;
        }

        public void MakeProgramMove(object sender, EventArgs e)
        {
            if (!_programMadeMove || GameBoard.Status != GameStatus.GameCanContinue)
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

        public PieceColor MovingSideColor => GameBoard.MovingSideColor;

        public bool ProgramPlaysForWhite => WhiteVirtualPlayer != null; // Программа может играть и сама с собой.

        public bool ProgramPlaysForBlack => BlackVirtualPlayer != null;
    }
}
