using Chess.LogicPart;
using System.Text;

namespace Chess
{
    internal class GamePanelButton : Button
    {
        private readonly GamePanel _gamePanel;
        private PieceName? _displayedPieceName;
        private PieceColor? _displayedPieceColor;
        private static Bitmap[][][] _images;
        /*Три массива, в одном - фигуры на белых полях, в другом - на черных, в третьем - на подсвеченных.
         В каждом из этих массивов еще по два массива: один с белыми фигурами, другой - с черными.*/

        public int X { get; }

        public int Y { get; }

        public bool IsHighlighted { get; private set; }

        public bool IsOutlined { get; private set; }

        public GamePanelButton(GamePanel gamePanel, int x, int y)
        {
            _gamePanel = gamePanel;
            X = x;
            Y = y;

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 2;
            BackgroundImageLayout = ImageLayout.Zoom;
        }

        internal static void SetNewImagesFor(GamePanel gamePanel)
        {
            _images = new Bitmap[3][][] { new Bitmap[2][] { new Bitmap[6] , new Bitmap[6] }, new Bitmap[2][] { new Bitmap[6], new Bitmap[6] },
                new Bitmap[2][] { new Bitmap[6], new Bitmap[6] } };

            var folderPath = "Images/";
            var originalImages = new Bitmap[6] {new Bitmap(folderPath + "King.jpg"), new Bitmap(folderPath + "Queen.jpg"), new Bitmap(folderPath + "Rook.jpg"),
            new Bitmap(folderPath + "Knight.jpg"), new Bitmap(folderPath + "Bishop.jpg"), new Bitmap(folderPath + "Pawn.jpg") };

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

        public void RenewImage()
        {
            if (IsClear)
            {
                BackgroundImage = null;
                return;
            }

            if (IsHighlighted)
            {
                BackgroundImage = _images[2][_displayedPieceColor == PieceColor.White ? 0 : 1][(int)_displayedPieceName];
                return;
            }

            BackgroundImage = _images[X % 2 == Y % 2 ? 1 : 0][_displayedPieceColor == PieceColor.White ? 0 : 1][(int)_displayedPieceName];
        }

        public void SetDisplayedPiece(PieceName? pieceName, PieceColor? pieceColor)
        {
            if (pieceName == null ^ pieceColor == null)
            {
                throw new ArgumentException();
            }

            if (_displayedPieceName == pieceName && _displayedPieceColor == pieceColor)
            {
                return;
            }

            _displayedPieceName = pieceName;
            _displayedPieceColor = pieceColor;
            RenewImage();
        }

        public void Highlight()
        {
            if (IsClear)
            {
                return;
            }

            BackgroundImage = _images[2][_displayedPieceColor == PieceColor.White ? 0 : 1][(int)_displayedPieceName];
            FlatAppearance.BorderColor = _gamePanel.HighlightColor;
            IsHighlighted = true;
        }

        public void RemoveHighlight()
        {
            if (!IsHighlighted)
            {
                return;
            }

            BackgroundImage = _images[X % 2 == Y % 2 ? 1 : 0][_displayedPieceColor == PieceColor.White ? 0 : 1][(int)_displayedPieceName];

            if (!IsOutlined)
            {
                FlatAppearance.BorderColor = BackColor;
            }

            IsHighlighted = false;
        }

        public void Outline()
        {
            FlatAppearance.BorderColor = _gamePanel.HighlightColor;
            IsOutlined = true;
        }

        public void RemoveOutline()
        {
            if (!IsHighlighted)
            {
                FlatAppearance.BorderColor = BackColor;
            }

            IsOutlined = false;
        }

        public bool IsClear => _displayedPieceName == null;

        public PieceColor? DisplayedPieceColor => _displayedPieceColor;

        public string ChessName
        {
            get
            {
                const string verticalNames = "abcdefgh";
                return new StringBuilder().Append(verticalNames[X]).Append(Y + 1).ToString();
            }
        }
    }
}
