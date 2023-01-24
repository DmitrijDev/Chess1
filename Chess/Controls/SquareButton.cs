
namespace Chess
{
    internal class SquareButton : Button
    {
        private readonly GamePanel _gamePanel;
        private readonly int _x;
        private readonly int _y;
        private static Bitmap[] _images;
        // 1-6, 13-18, 25-30 - белые фигуры, 7-12, 19-24, 31-36 - черные. 1-12 - фигуры на белом поле, 13-24 - на черном, > 24 - на подсвеченном поле.

        public int DisplayedPieceIndex { get; set; } // 0 - пустое поле, 1-6 - белые фигуры, 7-12 -черные.       

        public SquareButton(GamePanel gamePanel, int x, int y)
        {
            _gamePanel = gamePanel;
            _x = x;
            _y = y;

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.BorderColor = _gamePanel.HighlightColor;
            BackgroundImageLayout = ImageLayout.Zoom;

            if (_images == null)
            {
                CreateImages();
            }

            Click += new EventHandler(HandleClick);
        }

        private void CreateImages()
        {
            _images = new Bitmap[37];

            var originalImages = new Bitmap[7] {null, new Bitmap("Pictures/King.jpg"), new Bitmap("Pictures/Queen.jpg"), new Bitmap("Pictures/Rook.jpg"),
                new Bitmap("Pictures/Knight.jpg"), new Bitmap("Pictures/Bishop.jpg"), new Bitmap("Pictures/Pawn.jpg") };

            for (var i = 1; i < 37; ++i)
            {
                var backColor = i <= 12 ? _gamePanel.LightSquaresColor : i <= 24 ? _gamePanel.DarkSquaresColor : _gamePanel.HighlightColor;
                var imageColor = (i >= 1 && i <= 6) || (i >= 13 && i <= 18) || (i >= 25 && i <= 30) ? _gamePanel.WhitePiecesColor : _gamePanel.BlackPiecesColor;
                _images[i] = Graphics.GetColoredPicture(originalImages[i % 6 > 0 ? i % 6 : 6], backColor, imageColor);
            }
        }

        private void HandleClick(object sender, EventArgs e) => _gamePanel.HandleClickAt(_x, _y);

        public void RenewImage()
        {
            if (DisplayedPieceIndex == 0)
            {
                BackgroundImage = null;
                return;
            }

            for (var i = 25; i < _images.Length; ++i)
            {
                if (_images[i] == BackgroundImage)
                {
                    BackgroundImage = _images[DisplayedPieceIndex + 24];
                    return;
                }
            }

            var newImageIndex = BackColor == _gamePanel.LightSquaresColor ? DisplayedPieceIndex : DisplayedPieceIndex + 12;
            BackgroundImage = _images[newImageIndex];
        }

        public void Highlight()
        {
            if (DisplayedPieceIndex != 0)
            {
                BackgroundImage = _images[DisplayedPieceIndex + 24];
            }
        }

        public void RemoveHighlight() => BackgroundImage = BackColor == _gamePanel.LightSquaresColor ? _images[DisplayedPieceIndex] : _images[DisplayedPieceIndex + 12];
    }
}
