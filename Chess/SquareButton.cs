
namespace Chess
{
    public class SquareButton : Button
    {
        private readonly GameForm _form;
        private readonly int _x;
        private readonly int _y;

        public int DisplayedPieceIndex { get; set; } // 0 - пустое поле, 1-6 - белые фигуры, 7-12 -черные.

        public static Bitmap[] Images { get; set; }
        // 1-6, 13-18, 25-30 - белые фигуры, 7-12, 19-24, 31-36 - черные. 1-12 - фигуры на белом поле, 13-24 - на черном, > 24 - на подсвеченном поле.

        public SquareButton(GameForm form, int x, int y)
        {
            _form = form;
            _x = x;
            _y = y;
            Height = _form.ButtonSize;
            Width = _form.ButtonSize;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.BorderColor = _form.HighlightColor;
            BackgroundImageLayout = ImageLayout.Zoom;
            Click += new EventHandler(HandleClick);
        }

        private void HandleClick(object sender, EventArgs e) => _form.HandleClickAt(_x, _y);

        public void RenewImage()
        {
            FlatAppearance.BorderSize = 0;

            if (DisplayedPieceIndex == 0)
            {
                BackgroundImage = null;
                return;
            }

            var newImageIndex = BackColor == _form.LightSquaresColor ? DisplayedPieceIndex : DisplayedPieceIndex + 12;
            BackgroundImage = Images[newImageIndex];
        }

        public void Highlight()
        {
            if (DisplayedPieceIndex != 0)
            {
                BackgroundImage = Images[DisplayedPieceIndex + 24];
            }
        }

        public void RemoveHighlight() => RenewImage();
    }
}
