
namespace Chess
{
    internal class SquareButton : Button
    {
        private readonly GamePanel _boardPanel;
        private readonly int _x;
        private readonly int _y;
        private static Bitmap[] _images;
        // 1-6, 13-18, 25-30 - белые фигуры, 7-12, 19-24, 31-36 - черные. 1-12 - фигуры на белом поле, 13-24 - на черном, > 24 - на подсвеченном поле.

        public int DisplayedPieceIndex { get; set; } // 0 - пустое поле, 1-6 - белые фигуры, 7-12 -черные.       

        public SquareButton(GamePanel boardPanel, int x, int y)
        {
            _boardPanel = boardPanel;
            _x = x;
            _y = y;
            Height = _boardPanel.ButtonSize;
            Width = _boardPanel.ButtonSize;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.BorderColor = _boardPanel.HighlightColor;
            BackgroundImageLayout = ImageLayout.Zoom;

            if (_images == null)
            {
                CreateImages(_boardPanel.WhitePiecesColor, _boardPanel.BlackPiecesColor, _boardPanel.LightSquaresColor, _boardPanel.DarkSquaresColor, _boardPanel.HighlightColor);
            }

            Click += new EventHandler(HandleClick);
        }

        public static void CreateImages(Color whitePiecesColor, Color blackPiecesColor, Color lightSquaresColor, Color darkSquaresColor, Color highlightColor)
        {
            _images = new Bitmap[37];
            var initialImages = new Bitmap[7] {null, new Bitmap("Pictures/King.jpg"), new Bitmap("Pictures/Queen.jpg"), new Bitmap("Pictures/Rook.jpg"),
                new Bitmap("Pictures/Knight.jpg"), new Bitmap("Pictures/Bishop.jpg"), new Bitmap("Pictures/Pawn.jpg") };

            for (var i = 1; i < _images.Length; ++i)
            {
                _images[i] = i <= 6 ? new Bitmap(initialImages[i]) : new Bitmap(_images[i - 6]);
            }

            for (var i = 1; i < _images.Length; ++i)
            {
                var backColor = i <= 12 ? lightSquaresColor : i <= 24 ? darkSquaresColor : highlightColor;
                var imageColor = (i >= 1 && i <= 6) || (i >= 13 && i <= 18) || (i >= 25 && i <= 30) ? whitePiecesColor : blackPiecesColor;
                _images[i] = Graphics.GetColoredPicture(_images[i], backColor, imageColor);
            }
        }

        private void HandleClick(object sender, EventArgs e) => _boardPanel.HandleClickAt(_x, _y);

        public void RenewImage()
        {
            if (DisplayedPieceIndex == 0)
            {
                BackgroundImage = null;
                return;
            }

            var newImageIndex = BackColor == _boardPanel.LightSquaresColor ? DisplayedPieceIndex : DisplayedPieceIndex + 12;
            BackgroundImage = _images[newImageIndex];
        }

        public void Highlight()
        {
            if (DisplayedPieceIndex != 0)
            {
                BackgroundImage = _images[DisplayedPieceIndex + 24];
            }
        }

        public void RemoveHighlight() => RenewImage();
    }
}
