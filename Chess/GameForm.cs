using Chess.LogicPart;
using Board = Chess.GameBoard;

namespace Chess
{
    public partial class GameForm : Form
    {
        public int ButtonSize { get; private set; } = Screen.PrimaryScreen.WorkingArea.Height / 16;

        private SquareButton[,] FormButtons { get; } = new SquareButton[8, 8];

        public Color LightSquaresColor { get; private set; } = Color.Gold;

        public Color DarkSquaresColor { get; private set; } = Color.Chocolate;

        public GameForm()
        {
            InitializeComponent();
            AutoSize = true;
            SetButtons();
            RenewPosition();
        }

        private void ChessGameForm_Load(object sender, EventArgs e)
        {
        }

        public void SetButtons()
        {
            MinimumSize = new Size(0, 0);
            MaximumSize = new Size(int.MaxValue, int.MaxValue);
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            var boardSize = FormButtons.GetLength(0);
            var shift = ButtonSize / 2;
            var buttonColor = LightSquaresColor;
            var buttonX = shift;
            var buttonY = shift;

            for (var i = 0; i < boardSize; ++i)
            {
                for (var j = boardSize - 1; j >= 0; --j)
                {
                    // ≈сли кнопки ранее уже созданы, а теперь мы хотим изменить размер полей, то старые кнопки нужно удалить.
                    if (FormButtons[i, j] != null)
                    {
                        Controls.Remove(FormButtons[i, j]);
                    }

                    var newButton = new SquareButton(this, i, j)
                    {
                        BackColor = buttonColor,
                        Location = new Point(buttonX, buttonY)
                    };

                    FormButtons[i, j] = newButton;
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
            GamePosition currentPosition;

            lock (Board.Board)
            {
                currentPosition = Board.Board.CurrentPosition;
            }

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    FormButtons[i, j].ContainedPieceIndex = currentPosition[i, j];
                    FormButtons[i, j].RenewText();
                }
            }
        }

        public void ShowMessage(string message)
        {
            string caption = "";
            MessageBoxButtons okButton = MessageBoxButtons.OK;
            MessageBox.Show(message, caption, okButton);
        }

        public PieceColor MovingSideColor => Board.Board.MovingSideColor; // == 0 - ход белых, == 1 - ход черных.        
    }
}