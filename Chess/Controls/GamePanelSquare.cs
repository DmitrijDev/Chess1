using Chess.LogicPart;

namespace Chess
{
    internal class GamePanelSquare : Control
    {
        private readonly GamePanel _gamePanel;
        private readonly Control _innerControl;
        private static Bitmap[][][] _images;
        /*Три массива, в одном - фигуры на белых полях, в другом - на черных, в третьем - на подсвеченных.
         В каждом из этих массивов еще по два массива: один с белыми фигурами, другой - с черными.*/

        public int Vertical { get; }

        public int Horizontal { get; }

        public bool IsHighlighted { get; private set; }

        public bool IsOutlined { get; private set; }

        public ChessPieceName? DisplayedPieceName { get; private set; }

        public ChessPieceColor? DisplayedPieceColor { get; private set; }

        public static int BorderSize { get; } = 2;

        public GamePanelSquare(GamePanel gamePanel, int x, int y)
        {
            _gamePanel = gamePanel;
            Vertical = x;
            Horizontal = y;

            _innerControl = new()
            {
                BackgroundImageLayout = ImageLayout.Zoom,
                Location = new(BorderSize, BorderSize)
            };

            SizeChanged += (sender, e) =>
            {
                _innerControl.Width = Width - BorderSize * 2;
                _innerControl.Height = _innerControl.Width;
            };

            _innerControl.MouseClick += (sender, e) => OnMouseClick(e);
            Controls.Add(_innerControl);
        }

        public static void SetNewImagesFor(GamePanel gamePanel)
        {
            _images = new Bitmap[3][][] { new Bitmap[2][] { new Bitmap[6] , new Bitmap[6] }, new Bitmap[2][] { new Bitmap[6], new Bitmap[6] },
                new Bitmap[2][] { new Bitmap[6], new Bitmap[6] } };

            var folderPath = "Images/";
            var originalImages = new Bitmap[6] { new Bitmap(folderPath + "Pawn.jpg"), new Bitmap(folderPath + "Knight.jpg"),
            new Bitmap(folderPath + "Bishop.jpg"), new Bitmap(folderPath + "Rook.jpg"), new Bitmap(folderPath + "Queen.jpg"),
            new Bitmap(folderPath + "King.jpg") };

            for (var i = 0; i < 6; ++i)
            {
                _images[0][0][i] = Graphics.GetColoredPicture(originalImages[i], gamePanel.LightSquaresColor, gamePanel.WhitePiecesColor);
                _images[0][1][i] = Graphics.GetColoredPicture(originalImages[i], gamePanel.LightSquaresColor, gamePanel.BlackPiecesColor);

                _images[1][0][i] = Graphics.GetColoredPicture(originalImages[i], gamePanel.DarkSquaresColor, gamePanel.WhitePiecesColor);
                _images[1][1][i] = Graphics.GetColoredPicture(originalImages[i], gamePanel.DarkSquaresColor, gamePanel.BlackPiecesColor);

                _images[2][0][i] = Graphics.GetColoredPicture(originalImages[i], gamePanel.HighlightColor, gamePanel.WhitePiecesColor);
                _images[2][1][i] = Graphics.GetColoredPicture(originalImages[i], gamePanel.HighlightColor, gamePanel.BlackPiecesColor);
            }
        }

        public void SetColors()
        {
            BackColor = IsOutlined || IsHighlighted ? _gamePanel.HighlightColor : MainColor;
            _innerControl.BackColor = IsHighlighted ? _gamePanel.HighlightColor : MainColor;
        }

        public void RenewImage()
        {
            if (IsClear)
            {
                _innerControl.BackgroundImage = null;
                return;
            }

            if (IsHighlighted)
            {
                _innerControl.BackgroundImage = _images[2][DisplayedPieceColor == ChessPieceColor.White ? 0 : 1][(int)DisplayedPieceName];
                return;
            }

            _innerControl.BackgroundImage = _images[Vertical % 2 == Horizontal % 2 ? 1 : 0][DisplayedPieceColor == ChessPieceColor.White ? 0 : 1][(int)DisplayedPieceName];
        }

        public void DisplayPiece(ChessPieceName? pieceName, ChessPieceColor? pieceColor)
        {
            if (pieceName == null ^ pieceColor == null)
            {
                throw new ArgumentException("Некорректные аргументы.");
            }

            if (DisplayedPieceName == pieceName && DisplayedPieceColor == pieceColor)
            {
                return;
            }

            DisplayedPieceName = pieceName;
            DisplayedPieceColor = pieceColor;
            RenewImage();
        }

        public void Highlight()
        {
            _innerControl.BackgroundImage = _images[2][DisplayedPieceColor == ChessPieceColor.White ? 0 : 1][(int)DisplayedPieceName];
            BackColor = _gamePanel.HighlightColor;
            IsHighlighted = true;
        }

        public void RemoveHighlight()
        {
            _innerControl.BackgroundImage = _images[Vertical % 2 == Horizontal % 2 ? 1 : 0][DisplayedPieceColor == ChessPieceColor.White ? 0 : 1][(int)DisplayedPieceName];

            if (!IsOutlined)
            {
                BackColor = MainColor;
            }

            IsHighlighted = false;
        }

        public void Outline()
        {
            BackColor = _gamePanel.HighlightColor;
            IsOutlined = true;
        }

        public void RemoveOutline()
        {
            if (!IsHighlighted)
            {
                BackColor = MainColor;
            }

            IsOutlined = false;
        }

        public bool IsClear => DisplayedPieceName == null;

        public Color MainColor => Vertical % 2 == Horizontal % 2 ? _gamePanel.DarkSquaresColor : _gamePanel.LightSquaresColor;
    }
}
