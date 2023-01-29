
namespace Chess
{
    public partial class GamePanelSizeForm : Form
    {
        public GamePanelSizeForm()
        {
            InitializeComponent();
            MinimumSize = new Size(Width, Height);
            MaximumSize = new Size(Width, Height);
            cancelButton.Click += (sender, e) => Close();
        }

        private void SelectButton_Click(object sender, EventArgs e) => Close();

        private void GamePanelSizeForm_Load(object sender, EventArgs e)
        {

        }
    }
}
