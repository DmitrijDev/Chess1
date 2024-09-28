
namespace Chess
{
    internal class GamePanelSquare : Control
    {
        private readonly Control _innerControl;

        public static int BorderSize { get; private set; } = 1;

        public GamePanelSquare()
        {
            MinimumSize = new(5, 5);

            _innerControl = new()
            {
                Width = Width - BorderSize * 2,
                Height = Height - BorderSize * 2,
                Location = new(BorderSize, BorderSize),
                BackgroundImageLayout = ImageLayout.Zoom
            };

            Controls.Add(_innerControl);

            SizeChanged += (sender, e) =>
            {
                _innerControl.Width = Width - BorderSize * 2;
                _innerControl.Height = Height - BorderSize * 2;
            };

            BorderSizeChanged += () =>
            {
                _innerControl.Width = Width - BorderSize * 2;
                _innerControl.Height = Height - BorderSize * 2;
                _innerControl.Location = new(BorderSize, BorderSize);
            };

            _innerControl.MouseClick += (sender, e) => OnMouseClick(e);
        }

        public void SetImage(Bitmap image) => _innerControl.BackgroundImage = image;

        public void SetColor(Color color)
        {
            BackColor = color;
            _innerControl.BackColor = color;
        }

        public void Outline(Color color) => BackColor = color;

        public void RemoveOutline() => BackColor = _innerControl.BackColor;

        public static void SetBorderSize(int newSize)
        {
            BorderSize = newSize;
            BorderSizeChanged?.Invoke();
        }

        public static event Action BorderSizeChanged;
    }
}
