
using System.Text;
using System.Windows.Forms;

namespace Chess
{
    internal class TimePanel : Panel
    {
        private readonly GameForm _form;

        private readonly Label _whiteTimeBox = new();
        private readonly Label _blackTimeBox = new();

        public TimePanel(GameForm form)
        {
            _form = form;
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = _form.PanelColor;

            SetTimeBoxProperties(_whiteTimeBox);
            SetTimeBoxProperties(_blackTimeBox);

            Height = _whiteTimeBox.Height;

            _whiteTimeBox.Location = new Point(_whiteTimeBox.Width, 0);
            _blackTimeBox.Location = new Point(Width - _blackTimeBox.Width * 2, 0);

            MinimumSize = new Size(Width, Height);
            MaximumSize = new Size(Width, Height);
            AutoSize = false;

            Controls.Add(_whiteTimeBox);
            Controls.Add(_blackTimeBox);

            _form.SizeChanged += new EventHandler(ChangeWidth);
        }

        private void SetTimeBoxProperties(Label timeBox)
        {
            timeBox.BackColor = Color.Cyan;
            timeBox.ForeColor = Color.Black;
            timeBox.BorderStyle = BorderStyle.None;

            timeBox.Font = new Font("TimesNewRoman", _form.TimeFontSize, FontStyle.Bold);

            timeBox.MinimumSize = new Size(0, timeBox.Font.Height);
            timeBox.MaximumSize = new Size(int.MaxValue, timeBox.MinimumSize.Height);
            timeBox.AutoSize = true;

            ShowTime(timeBox, 300);            
        }

        private static void ShowTime(Label timeBox, int time)
        {
            var hours = time / 3600;
            var minutes = (time % 3600) / 60;
            var seconds = time % 60;

            var newText = new StringBuilder(hours.ToString()).Append(':');

            if (minutes < 10)
            {
                newText.Append('0');
            }

            newText.Append(minutes).Append(':');

            if (seconds < 10)
            {
                newText.Append('0');
            }

            newText.Append(seconds);

            timeBox.Text = newText.ToString();
        }

        private void ChangeWidth(object sender, EventArgs e)
        {
            MinimumSize = new Size(0, Height);
            MaximumSize = new Size(int.MaxValue, Height);

            Width = _form.Width;

            MinimumSize = new Size(Width, Height);
            MaximumSize = new Size(Width, Height);

            _blackTimeBox.Location = new Point(Width - _blackTimeBox.Width * 2, 0);
        }
    }
}
