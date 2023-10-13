using Chess.LogicPart;
using System.Text;

namespace Chess
{
    internal class TimePanel : Panel
    {
        private readonly GameForm _form;

        private readonly Label _whiteTimer;
        private readonly Label _blackTimer;

        public TimePanel(GameForm form)
        {
            _form = form;
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = DefaultBackColor;
            Font = GetNewFont();

            _whiteTimer = GetNewTimer();
            _blackTimer = GetNewTimer();

            var timerWidth = _whiteTimer.Width;
            var interval = timerWidth + timerWidth / 2;

            MinimumSize = new(timerWidth * 2 + interval, _whiteTimer.Height);
            Height = _whiteTimer.Height;

            _whiteTimer.Location = new(Width / 2 - interval / 2 - timerWidth, 0);
            _blackTimer.Location = new(Width / 2 + interval / 2, 0);

            Controls.Add(_whiteTimer);
            Controls.Add(_blackTimer);

            _form.SizeChanged += (sender, e) => Width = _form.ClientRectangle.Width;
            SizeChanged += Size_Changed;
        }

        private Font GetNewFont()
        {
            var result = new Font("TimesNewRoman", 1, FontStyle.Bold, GraphicsUnit.Pixel);

            for (; ; )
            {
                var newFont = new Font("TimesNewRoman", result.Size + 1, FontStyle.Bold, GraphicsUnit.Pixel);

                if (newFont.Height <= _form.MenuStrip.Height)
                {
                    result = newFont;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private Label GetNewTimer() => new()
        {
            BackColor = Color.LightSlateGray,
            ForeColor = Color.Black,
            TextAlign = ContentAlignment.TopCenter,
            Height = Font.Height,
            Width = Font.Height * 4
        };

        private static string GetTimeText(int time)
        {
            var hours = time / 3600;
            var minutes = time % 3600 / 60;
            var seconds = time % 60;

            var text = new StringBuilder().Append(hours).Append(':');

            if (minutes < 10)
            {
                text.Append('0');
            }

            text.Append(minutes).Append(':');

            if (seconds < 10)
            {
                text.Append('0');
            }

            text.Append(seconds);
            return text.ToString();
        }

        public void ShowTime(ChessPieceColor color, int time)
        {
            var timer = color == ChessPieceColor.White ? _whiteTimer : _blackTimer;
            timer.Text = GetTimeText(time);
        }

        public void ShowTime(int whiteTimeLeft, int blackTimeLeft)
        {
            _whiteTimer.Text = GetTimeText(whiteTimeLeft);
            _blackTimer.Text = GetTimeText(blackTimeLeft);
        }

        private void Size_Changed(object sender, EventArgs e)
        {
            var timerWidth = _whiteTimer.Width;
            var interval = timerWidth + timerWidth / 2;
            _whiteTimer.Location = new(Width / 2 - interval / 2 - timerWidth, 0);
            _blackTimer.Location = new(Width / 2 + interval / 2, 0);
        }
    }
}
