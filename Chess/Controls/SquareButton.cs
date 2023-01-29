﻿
namespace Chess
{
    internal class SquareButton : Button
    {
        private readonly GamePanel _gamePanel;
        private static Bitmap[] _images;
        // 1-6, 13-18, 25-30 - белые фигуры, 7-12, 19-24, 31-36 - черные. 1-12 - фигуры на белом поле, 13-24 - на черном, > 24 - на подсвеченном поле.

        public int X { get; }

        public int Y { get; }

        public int DisplayedPieceIndex { get; set; } // 0 - пустое поле, 1-6 - белые фигуры, 7-12 -черные.

        public bool IsHighlighted { get; private set; }

        public SquareButton(GamePanel gamePanel, int x, int y)
        {
            _gamePanel = gamePanel;
            X = x;
            Y = y;

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.BorderColor = _gamePanel.HighlightColor;
            BackgroundImageLayout = ImageLayout.Zoom;

            if (_images == null)
            {
                CreateImages(_gamePanel);
            }
        }

        internal static void CreateImages(GamePanel gamePanel)
        {
            _images = new Bitmap[37];

            var originalImages = new Bitmap[7] {null, new Bitmap("Pictures/King.jpg"), new Bitmap("Pictures/Queen.jpg"), new Bitmap("Pictures/Rook.jpg"),
                new Bitmap("Pictures/Knight.jpg"), new Bitmap("Pictures/Bishop.jpg"), new Bitmap("Pictures/Pawn.jpg") };

            for (var i = 1; i < 37; ++i)
            {
                var backColor = i <= 12 ? gamePanel.LightSquaresColor : i <= 24 ? gamePanel.DarkSquaresColor : gamePanel.HighlightColor;
                var imageColor = (i >= 1 && i <= 6) || (i >= 13 && i <= 18) || (i >= 25 && i <= 30) ? gamePanel.WhitePiecesColor : gamePanel.BlackPiecesColor;
                _images[i] = Graphics.GetColoredPicture(originalImages[i % 6 > 0 ? i % 6 : 6], backColor, imageColor);
            }
        }

        public void RenewImage()
        {
            if (DisplayedPieceIndex == 0)
            {
                BackgroundImage = null;
                return;
            }

            if (IsHighlighted)
            {
                BackgroundImage = _images[DisplayedPieceIndex + 24];
                return;
            }

            BackgroundImage = BackColor == _gamePanel.LightSquaresColor ? _images[DisplayedPieceIndex] : _images[DisplayedPieceIndex + 12];
        }

        public void Highlight()
        {
            if (DisplayedPieceIndex != 0)
            {
                BackgroundImage = _images[DisplayedPieceIndex + 24];
            }

            IsHighlighted = true;
        }

        public void RemoveHighlight()
        {
            BackgroundImage = BackColor == _gamePanel.LightSquaresColor ? _images[DisplayedPieceIndex] : _images[DisplayedPieceIndex + 12];
            IsHighlighted = false;
        }

        public void Outline() => FlatAppearance.BorderSize = 2;

        public void RemoveOutline() => FlatAppearance.BorderSize = 0;
    }
}
