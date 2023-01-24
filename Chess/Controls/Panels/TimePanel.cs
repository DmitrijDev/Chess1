
using Chess.LogicPart;
using System.Text;

namespace Chess
{
    internal class TimePanel : Panel
    {
        private readonly GameForm _form;

        private readonly Panel _whiteTimer = new();
        private readonly Panel _blackTimer = new();

        private readonly Label _whiteTimeLeft = new();
        private readonly Label _blackTimeLeft = new();

        public TimePanel(GameForm form)
        {
            _form = form;
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = _form.PanelColor;

            CreateTimer(PieceColor.White);
            CreateTimer(PieceColor.Black);
            var timerWidth = _whiteTimer.Width;
            var interval = timerWidth + timerWidth / 2;

            Width = Math.Max(_form.ClientRectangle.Width, timerWidth * 2 + interval);
            Height = _whiteTimer.Height;

            _whiteTimer.Location = new Point(Width / 2 - interval / 2 - timerWidth, 0);
            _blackTimer.Location = new Point(Width / 2 + interval / 2, 0);

            Controls.Add(_whiteTimer);
            Controls.Add(_blackTimer);

            MinimumSize = new Size(timerWidth * 2 + interval, Height);
            MaximumSize = new Size(int.MaxValue, Height);

            _form.SizeChanged += new EventHandler(ChangeWidth);
            SizeChanged += new EventHandler(MoveTimers);
        }

        private void CreateTimer(PieceColor color)
        {
            var panel = color == PieceColor.White ? _whiteTimer : _blackTimer;
            var label = color == PieceColor.White ? _whiteTimeLeft : _blackTimeLeft;

            panel.BackColor = Color.LightSlateGray;
            label.BackColor = panel.BackColor;
            label.ForeColor = Color.Black;
            panel.BorderStyle = BorderStyle.None;

            label.Font = new Font("TimesNewRoman", _form.TimeFontSize, FontStyle.Bold);
            label.AutoSize = true;
            ShowTime(label, 0);

            panel.Controls.Add(label);

            panel.Width = label.Width;
            panel.Height = label.Height;

            panel.MinimumSize = new Size(panel.Width, panel.Height);
            panel.MaximumSize = new Size(panel.Width, panel.Height);

            label.Text = "";
        }

        private static void ShowTime(Label timeLabel, int time)
        {
            var hours = time / 3600;
            var minutes = time % 3600 / 60;
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

        public void ShowTime(int whiteTimeLeft, int blackTimeLeft)
        {
            ShowTime(_whiteTimeLeft, whiteTimeLeft);
            ShowTime(_blackTimeLeft, blackTimeLeft);
        }

        public void ShowTime(PieceColor color, int time) => ShowTime(color == PieceColor.White ? _whiteTimeLeft : _blackTimeLeft, time);

        private void ChangeWidth(object sender, EventArgs e) => Width = _form.ClientRectangle.Width;

        private void MoveTimers(object sender, EventArgs e)
        {
            var timerWidth = _whiteTimer.Width;
            var interval = timerWidth + timerWidth / 2;
            _whiteTimer.Location = new Point(Width / 2 - interval / 2 - timerWidth, 0);
            _blackTimer.Location = new Point(Width / 2 + interval / 2, 0);
        }
    }
}
