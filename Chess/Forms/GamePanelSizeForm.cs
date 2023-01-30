
using System.Windows.Forms;

namespace Chess
{
    public partial class GamePanelSizeForm : Form
    {
        private readonly GamePanel _gamePanel;

        internal GamePanelSizeForm(GamePanel gamePanel)
        {
            InitializeComponent();
            _gamePanel = gamePanel;

            MinimumSize = new Size(Width, Height);
            MaximumSize = new Size(Width, Height);

            TextBox.Text = _gamePanel.ButtonSize.ToString();

            CancelButton.Click += (sender, e) => Close();
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            if (!int.TryParse(TextBox.Text, out var buttonSize))
            {
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

        private void SelectButton_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(TextBox.Text, out var buttonSize))
            {
                MessageBox.Show("Необходимо ввести новый размер поля.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            _gamePanel.SetButtonSize(Convert.ToInt32(TextBox.Text));
            Close();
        }
    }
}
