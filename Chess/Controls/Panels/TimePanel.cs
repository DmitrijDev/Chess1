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

            CreateTimer(ChessPieceColor.White);
            CreateTimer(ChessPieceColor.Black);
            var timerWidth = _whiteTimer.Width;
            var interval = timerWidth + timerWidth / 2;

            Width = Math.Max(_form.ClientRectangle.Width, timerWidth * 2 + interval);
            Height = _whiteTimer.Height;

            _whiteTimer.Location = new Point(Width / 2 - interval / 2 - timerWidth, 0);
            _blackTimer.Location = new Point(Width / 2 + interval / 2, 0);

            Controls.Add(_whiteTimer);
            Controls.Add(_blackTimer);

            MinimumSize = new Size(timerWidth * 2 + interval, Height);

            _form.SizeChanged += (sender, e) => Width = _form.ClientRectangle.Width;
            SizeChanged += Size_Changed;
        }

        private void CreateTimer(ChessPieceColor color)
        {
            var panel = color == ChessPieceColor.White ? _whiteTimer : _blackTimer;
            var label = color == ChessPieceColor.White ? _whiteTimeLeft : _blackTimeLeft;

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

            label.Text = "";
        }

        private static void ShowTime(Label timeLabel, int time)
        {
            var hours = time / 3600;
            var minutes = time % 3600 / 60;
            var seconds = time % 60;

            var newText = new StringBuilder().Append(hours).Append(':');

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

        public void ShowTime(ChessPieceColor color, int time) => ShowTime(color == ChessPieceColor.White ? _whiteTimeLeft : _blackTimeLeft, time);

        private void Size_Changed(object sender, EventArgs e)
        {
            var timerWidth = _whiteTimer.Width;
            var interval = timerWidth + timerWidth / 2;
            _whiteTimer.Location = new Point(Width / 2 - interval / 2 - timerWidth, 0);
            _blackTimer.Location = new Point(Width / 2 + interval / 2, 0);
        }
    }
}
