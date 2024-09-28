using Chess.LogicPart;
using System.Text;
using Timer = System.Windows.Forms.Timer;

namespace Chess
{
    internal class TimePanel : Panel
    {
        private readonly Timer _timer = new() { Interval = 1000 };

        private readonly Label _whiteTimeLabel;
        private readonly Label _blackTimeLabel;
        private readonly int _interval;

        private int _timeForGame;
        private int _whiteTimeLeft;
        private int _blackTimeLeft;

        public PieceColor MovingSideColor { get; set; }

        public TimePanel(int width, int height, int timeForGame)
        {
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = DefaultBackColor;
            Width = width;
            Height = height;
            SetFont();

            _whiteTimeLabel = GetLabel();
            _blackTimeLabel = GetLabel();
            Controls.Add(_whiteTimeLabel);
            Controls.Add(_blackTimeLabel);

            var labelWidth = _whiteTimeLabel.Width;
            _interval = labelWidth + labelWidth / 2;
            _interval += _interval % 2;
            MinimumSize = new(labelWidth * 2 + _interval + (Width - ClientRectangle.Width), Height);
            LocateLabels();

            ResetTime(timeForGame);

            SizeChanged += (sender, e) => LocateLabels();
            _timer.Tick += Timer_Tick;
        }

        private void SetFont()
        {
            var font = new Font("TimesNewRoman", 1, FontStyle.Bold, GraphicsUnit.Pixel);

            for (; ; )
            {
                var newFont = new Font("TimesNewRoman", font.Size + 1, FontStyle.Bold, GraphicsUnit.Pixel);

                if (newFont.Height <= ClientRectangle.Height)
                {
                    font = newFont;
                }
                else
                {
                    break;
                }
            }

            Font = font;
        }

        private Label GetLabel()
        {
            var label = new Label()
            {
                BackColor = Color.LightSlateGray,
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = ClientRectangle.Height,
                Width = Font.Height * 4
            };

            label.MouseClick += (sender, e) => OnMouseClick(e);
            return label;
        }

        private void LocateLabels()
        {
            var labelWidth = _whiteTimeLabel.Width;
            _whiteTimeLabel.Location = new(ClientRectangle.Width / 2 - _interval / 2 - labelWidth, 0);
            _blackTimeLabel.Location = new(ClientRectangle.Width / 2 + _interval / 2, 0);
        }

        public void ResetTime(int timeForGame)
        {
            if (timeForGame <= 0 || timeForGame >= 36000)
            {
                throw new ArgumentOutOfRangeException();
            }

            var timeText = GetTimeText(timeForGame);
            _whiteTimeLabel.Text = timeText;
            _blackTimeLabel.Text = timeText;

            _timeForGame = timeForGame;
            _whiteTimeLeft = _timeForGame;
            _blackTimeLeft = _timeForGame;
        }

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

        public void ResetTime() => ResetTime(_timeForGame);

        public static int[] GetTimeForGameValues() =>
        new int[] { 300, 900, 1800, 3600, 5400, 7200, 10800 };

        public void StartTimer()
        {
            if (_whiteTimeLeft > 0 && _blackTimeLeft > 0)
            {
                _timer.Start();
            }
            else
            {
                throw new InvalidOperationException("Время истекло.");
            }
        }

        public void StopTimer() => _timer.Stop();

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (MovingSideColor == PieceColor.White)
            {
                --_whiteTimeLeft;
                _whiteTimeLabel.Text = GetTimeText(_whiteTimeLeft);

                if (_whiteTimeLeft == 0)
                {
                    _timer.Stop();
                    TimeElapsed?.Invoke(PieceColor.White);
                }
            }
            else
            {
                --_blackTimeLeft;
                _blackTimeLabel.Text = GetTimeText(_blackTimeLeft);

                if (_blackTimeLeft == 0)
                {
                    _timer.Stop();
                    TimeElapsed?.Invoke(PieceColor.Black);
                }
            }
        }

        public event Action<PieceColor> TimeElapsed;
    }
}
