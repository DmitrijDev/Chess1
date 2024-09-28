
namespace Chess
{
    internal partial class GamePanelSizeForm : Form
    {
        private int _currentSquareSize;
        private readonly int _minSquareSize;
        private readonly int _maxSquareSize;

        public GamePanelSizeForm(int currentSquareSize, int minSquareSize, int maxSquareSize)
        {
            InitializeComponent();

            _currentSquareSize = currentSquareSize;
            _minSquareSize = minSquareSize;
            _maxSquareSize = maxSquareSize;

            MinimumSize = Size;
            MaximumSize = MinimumSize;

            TextBox.Text = _currentSquareSize.ToString();
        }

        private void PlusButton_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(TextBox.Text, out var squareSize))
            {
                TextBox.Text = _currentSquareSize < _maxSquareSize ? (_currentSquareSize + 1).ToString() :
                _currentSquareSize.ToString();

                return;
            }

            if (squareSize < _minSquareSize)
            {
                TextBox.Text = _minSquareSize.ToString();
                return;
            }

            if (squareSize >= _maxSquareSize)
            {
                TextBox.Text = _maxSquareSize.ToString();
                return;
            }

            TextBox.Text = (squareSize + 1).ToString();
        }

        private void MinusButton_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(TextBox.Text, out var squareSize))
            {
                TextBox.Text = _currentSquareSize > _minSquareSize ? (_currentSquareSize - 1).ToString() :
                _currentSquareSize.ToString();

                return;
            }

            if (squareSize <= _minSquareSize)
            {
                TextBox.Text = _minSquareSize.ToString();
                return;
            }

            if (squareSize > _maxSquareSize)
            {
                TextBox.Text = _maxSquareSize.ToString();
                return;
            }

            TextBox.Text = (squareSize - 1).ToString();
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(TextBox.Text, out var squareSize))
            {
                MessageBox.Show("Необходимо ввести новый размер поля.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (squareSize < _minSquareSize)
            {
                squareSize = _minSquareSize;
                TextBox.Text = squareSize.ToString();
            }

            if (squareSize > _maxSquareSize)
            {
                squareSize = _maxSquareSize;
                TextBox.Text = squareSize.ToString();
            }

            _currentSquareSize = squareSize;
            SizeSelected?.Invoke(_currentSquareSize);
        }

        private void GamePanelSizeForm_Load(object sender, EventArgs e)
        { }

        public event Action<int> SizeSelected;
    }
}
