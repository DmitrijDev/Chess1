
namespace Chess
{
    internal partial class GamePanelSizeForm : Form
    {
        private readonly GamePanel _gamePanel;

        internal GamePanelSizeForm(GamePanel gamePanel)
        {
            InitializeComponent();
            _gamePanel = gamePanel;

            MinimumSize = new Size(Width, Height);
            MaximumSize = new Size(Width, Height);

            TextBox.Text = _gamePanel.ButtonSize.ToString();
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            if (!int.TryParse(TextBox.Text, out var buttonSize))
            {
                TextBox.Text = "";
                return;
            }

            if (buttonSize < _gamePanel.MinimumButtonSize)
            {
                TextBox.Text = _gamePanel.MinimumButtonSize.ToString();
            }

            if (buttonSize > _gamePanel.MaximumButtonSize)
            {
                TextBox.Text = _gamePanel.MaximumButtonSize.ToString();
            }
        }

        private void PlusButton_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(TextBox.Text, out var buttonSize))
            {
                TextBox.Text = _gamePanel.ButtonSize < _gamePanel.MaximumButtonSize ? (_gamePanel.ButtonSize + 1).ToString() : _gamePanel.ButtonSize.ToString();
                return;
            }

            if (buttonSize == _gamePanel.MaximumButtonSize)
            {
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

            if (buttonSize == _gamePanel.MinimumButtonSize)
            {
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
    }
}
