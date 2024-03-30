
namespace Chess
{
    internal partial class GamePanelSizeForm : Form
    {
        private readonly GamePanel _gamePanel;

        public GamePanelSizeForm(GamePanel gamePanel)
        {
            InitializeComponent();
            _gamePanel = gamePanel;

            MinimumSize = Size;
            MaximumSize = MinimumSize;

            TextBox.Text = _gamePanel.ButtonSize.ToString();
        }        

        private void PlusButton_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(TextBox.Text, out var buttonSize))
            {
                TextBox.Text = _gamePanel.ButtonSize < _gamePanel.MaximumButtonSize ? (_gamePanel.ButtonSize + 1).ToString() : _gamePanel.ButtonSize.ToString();
                return;
            }

            if (buttonSize < _gamePanel.MinimumButtonSize)
            {
                TextBox.Text = _gamePanel.MinimumButtonSize.ToString();
                return;
            }

            if (buttonSize >= _gamePanel.MaximumButtonSize)
            {
                TextBox.Text = _gamePanel.MaximumButtonSize.ToString();
                return;
            }

            TextBox.Text = (buttonSize + 1).ToString();
        }

        private void MinusButton_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(TextBox.Text, out var buttonSize))
            {
                TextBox.Text = _gamePanel.ButtonSize > _gamePanel.MinimumButtonSize ? (_gamePanel.ButtonSize - 1).ToString() : _gamePanel.ButtonSize.ToString();
                return;
            }

            if (buttonSize <= _gamePanel.MinimumButtonSize)
            {
                TextBox.Text = _gamePanel.MinimumButtonSize.ToString();
                return;
            }

            if (buttonSize > _gamePanel.MaximumButtonSize)
            {
                TextBox.Text = _gamePanel.MaximumButtonSize.ToString();
                return;
            }

            TextBox.Text = (buttonSize - 1).ToString();
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(TextBox.Text, out var buttonSize))
            {
                MessageBox.Show("Необходимо ввести новый размер поля.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (buttonSize < _gamePanel.MinimumButtonSize)
            {
                buttonSize = _gamePanel.MinimumButtonSize;
                TextBox.Text = buttonSize.ToString();
            }

            if (buttonSize > _gamePanel.MaximumButtonSize)
            {
                buttonSize = _gamePanel.MaximumButtonSize;
                TextBox.Text = buttonSize.ToString();
            }

            _gamePanel.SetButtonSize(buttonSize);
        }

        private void GamePanelSizeForm_Load(object sender, EventArgs e)
        {

        }
    }
}
