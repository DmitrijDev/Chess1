using Chess.LogicPart;
using System.Text;
using Timer = System.Windows.Forms.Timer;

namespace Chess
{
    internal class TimePanel : Panel
    {
        private readonly GameForm _form;
        private readonly Timer _timer = new() { Interval = 1000 };

        private readonly Label _whiteTimeLabel;
        private readonly Label _blackTimeLabel;

        public int TimeForGame { get; private set; } = 300;

        public int WhiteTimeLeft { get; private set; } = 300;

        public int BlackTimeLeft { get; private set; } = 300;

        public TimePanel(GameForm form)
        {
            _form = form;
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = DefaultBackColor;
            Font = GetNewFont();

            _whiteTimeLabel = GetNewLabel();
            _blackTimeLabel = GetNewLabel();

            var labelWidth = _whiteTimeLabel.Width;
            var interval = labelWidth + labelWidth / 2;

            MinimumSize = new(labelWidth * 2 + interval, _whiteTimeLabel.Height);
            Width = _form.ClientRectangle.Width;
            Height = _whiteTimeLabel.Height;

            _whiteTimeLabel.Location = new(Width / 2 - interval / 2 - labelWidth, 0);
            _blackTimeLabel.Location = new(Width / 2 + interval / 2, 0);

            Controls.Add(_whiteTimeLabel);
            Controls.Add(_blackTimeLabel);

            ShowTime();            

            _form.SizeChanged += (sender, e) => Width = _form.ClientRectangle.Width;
            SizeChanged += Size_Changed;
            _timer.Tick += Timer_Tick;
        }

        private Font GetNewFont()
        {
            var font = new Font("TimesNewRoman", 1, FontStyle.Bold, GraphicsUnit.Pixel);

            for (; ; )
            {
                var newFont = new Font("TimesNewRoman", font.Size + 1, FontStyle.Bold, GraphicsUnit.Pixel);

                if (newFont.Height <= _form.MenuStrip.Height)
                {
                    font = newFont;
                }
                else
                {
                    break;
                }
            }

            return font;
        }

        private Label GetNewLabel() => new()
        {
            BackColor = Color.LightSlateGray,
            ForeColor = Color.Black,
            TextAlign = ContentAlignment.MiddleCenter,
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

        private void ShowTime()
        {
            _whiteTimeLabel.Text = GetTimeText(WhiteTimeLeft);
            _blackTimeLabel.Text = GetTimeText(BlackTimeLeft);
        }

        public void ResetTime()
        {
            WhiteTimeLeft = TimeForGame;
            BlackTimeLeft = TimeForGame;
            ShowTime();
        }

        public void ResetTime(int timeForGame)
        {
            if (timeForGame <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            TimeForGame = timeForGame;
            ResetTime();
        }

        public void StartTimer() => _timer.Start();

        public void StopTimer() => _timer.Stop();

        private void Size_Changed(object sender, EventArgs e)
        {
            var labelWidth = _whiteTimeLabel.Width;
            var interval = labelWidth + labelWidth / 2;
            _whiteTimeLabel.Location = new(Width / 2 - interval / 2 - labelWidth, 0);
            _blackTimeLabel.Location = new(Width / 2 + interval / 2, 0);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_form.GamePanel.MovingSideColor == ChessPieceColor.White)
            {
                --WhiteTimeLeft;
                _whiteTimeLabel.Text = GetTimeText(WhiteTimeLeft);

                if (WhiteTimeLeft == 0)
                {
                    _timer.Stop();
                    _form.GamePanel.EndGame(BoardStatus.BlackWin);
                }
            }
            else
            {
                --BlackTimeLeft;
                _blackTimeLabel.Text = GetTimeText(BlackTimeLeft);

                if (BlackTimeLeft == 0)
                {
                    _timer.Stop();
                    _form.GamePanel.EndGame(BoardStatus.WhiteWin);
                }
            }
        }
    }
}
