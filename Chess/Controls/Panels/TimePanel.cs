
using Chess.LogicPart;
using System.Text;

namespace Chess
{
    internal class TimePanel : Panel
    {
        private readonly GameForm _form;

        private readonly Panel _whiteTimer = new();
        private readonly Panel _blackTimer = new();

        private readonly Label _whiteTimeLabel = new();
        private readonly Label _blackTimeLabel = new();

        private readonly int _timerWidth;

        public TimePanel(GameForm form)
        {
            _form = form;
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = _form.PanelColor;

            BuildTimer(PieceColor.White);
            BuildTimer(PieceColor.Black);
            _timerWidth = _whiteTimer.Width;

            AutoSize = false;
            Width = _timerWidth * 5;
            Height = _whiteTimer.Height;

            _whiteTimer.Location = new Point(_timerWidth, 0);
            _blackTimer.Location = new Point(Width - _timerWidth * 2, 0);

            Controls.Add(_whiteTimer);
            Controls.Add(_blackTimer);

            MinimumSize = new Size(Width, Height);
            MaximumSize = new Size(int.MaxValue, Height);

            _form.SizeChanged += new EventHandler(ChangeWidth);
            SizeChanged += new EventHandler(MoveBlackTimer);
        }

        private void BuildTimer(PieceColor color)
        {
            var panel = color == PieceColor.White ? _whiteTimer : _blackTimer;
            var label = color == PieceColor.White ? _whiteTimeLabel : _blackTimeLabel;

            panel.BackColor = Color.Cyan;
            label.BackColor = panel.BackColor;
            label.ForeColor = Color.Black;
            panel.BorderStyle = BorderStyle.None;

            label.Font = new Font("TimesNewRoman", _form.TimeFontSize, FontStyle.Bold);
            label.AutoSize = true;
            ShowTime(label, 300);

            label.Location = new Point(0, 0);
            panel.Controls.Add(label);

            panel.Width = label.Width;
            panel.Height = label.Height;

            panel.MinimumSize = new Size(panel.Width, panel.Height);
            panel.MaximumSize = new Size(panel.Width, panel.Height);
        }

        private static void ShowTime(Label timeLabel, int time)
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

            timeLabel.Text = newText.ToString();
        }

        private void ChangeWidth(object sender, EventArgs e) => Width = _form.Width;

        private void MoveBlackTimer(object sender, EventArgs e) => _blackTimer.Location = new Point(_form.ClientRectangle.Width - _timerWidth * 2, 0);
    }
}
