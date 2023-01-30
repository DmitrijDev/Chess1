
namespace Chess
{
    public partial class GamePanelSizeForm : Form
    {
        private readonly GameForm _gameForm;//GamePanel _gamePanel;

        internal GamePanelSizeForm(/*GamePanel gamePanel*/ GameForm gameForm)
        {
            InitializeComponent();
            //_gamePanel = gamePanel;
            _gameForm = gameForm;

            MinimumSize = new Size(Width, Height);
            MaximumSize = new Size(Width, Height);

            TextBox.Text = _gameForm.ButtonSize.ToString();            
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            if (!int.TryParse(TextBox.Text, out var buttonSize))
            {
                TextBox.Text = "";
                return;
            }

            if (buttonSize < _gameForm.MinimumButtonSize)
            {
                TextBox.Text = _gameForm.MinimumButtonSize.ToString();
            }

            if (buttonSize > _gameForm.MaximumButtonSize)
            {
                TextBox.Text = _gameForm.MaximumButtonSize.ToString();
            }
        }

        private void PlusButton_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(TextBox.Text, out var buttonSize))
            {
                TextBox.Text = _gameForm.ButtonSize < _gameForm.MaximumButtonSize ? (_gameForm.ButtonSize + 1).ToString() : _gameForm.ButtonSize.ToString();
                return;
            }

            if (buttonSize == _gameForm.MaximumButtonSize)
            {
                return;
            }

            TextBox.Text = (buttonSize + 1).ToString();
        }

        private void MinusButton_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(TextBox.Text, out var buttonSize))
            {
                TextBox.Text = _gameForm.ButtonSize > _gameForm.MinimumButtonSize ? (_gameForm.ButtonSize - 1).ToString() : _gameForm.ButtonSize.ToString();
                return;
            }

            if (buttonSize == _gameForm.MinimumButtonSize)
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

            if (buttonSize < _gameForm.MinimumButtonSize)
            {
                TextBox.Text = _gameForm.MinimumButtonSize.ToString();
            }

            if (buttonSize > _gameForm.MaximumButtonSize)
            {
                TextBox.Text = _gameForm.MaximumButtonSize.ToString();
            }

            _gameForm.SetButtonSize(Convert.ToInt32(TextBox.Text));
        }
    }
}
