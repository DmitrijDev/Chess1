
namespace Chess
{
    public partial class GamePanelSizeForm : Form
    {
        public GamePanelSizeForm()
        {
            InitializeComponent();
            MinimumSize = new Size(Width, Height);
            MaximumSize = new Size(Width, Height);
        }

        private void SelectButton_Click(object sender, EventArgs e) => Close();        
    }
}
