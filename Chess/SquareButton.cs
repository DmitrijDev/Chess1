using Chess.LogicPart;

namespace Chess
{
    public class SquareButton : Button
    {
        private readonly GameForm _form;
        private readonly int _x;
        private readonly int _y;

        public int DisplayedPieceIndex { get; set; } // 0 - пустое поле, 1-6 - белые фигуры, 7-12 -черные.

        public static Bitmap[] Images { get; set; } 
        // 1-6, 13-18, 25-30 - белые фигуры, 7-12, 19-24,31-36 - черные. 1-12 - фигуры на белом поле, 13-24 - на черном, > 24 - на подсвеченном голубым поле.

        public SquareButton(GameForm form, int x, int y)
        {
            _form = form;
            _x = x;
            _y = y;
            Height = _form.ButtonSize;
            Width = _form.ButtonSize;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.BorderColor = Color.Green;
            BackgroundImageLayout = ImageLayout.Zoom;
            Click += new EventHandler(HandleClick);
        }

        private void HandleClick(object sender, EventArgs e)
        {
            if (_form.ProgramPlaysFor(MovingSideColor) || _form.GameIsOver)
            {
                return;
            }

            if (_form.ClickedButtons.Count == 0) // Т.е. выбор фигуры для хода.
            {
                if (DisplayedPieceIndex == 0)
                {
                    return;
                }

                if ((MovingSideColor == PieceColor.White && DisplayedPieceIndex > 6) || (MovingSideColor == PieceColor.Black && DisplayedPieceIndex <= 6))
                {
                    _form.ShowMessage("Это не ваша фигура.");
                    return;
                }

                _form.ClickedButtons.Add(_x); // Запомнили координаты выбранной фигуры, ждем щелчка по полю на которое нужно сходить.
                _form.ClickedButtons.Add(_y);
                Highlight();
                return;
            }

            if (_x == _form.ClickedButtons[0] && _y == _form.ClickedButtons[1]) // Отмена выбора.
            {
                _form.ClickedButtons.Clear();
                RemoveHighlight();
                return;
            }

            if ((MovingSideColor == PieceColor.White && DisplayedPieceIndex > 0 && DisplayedPieceIndex <= 6) ||
                (MovingSideColor == PieceColor.Black && DisplayedPieceIndex > 6)) //Замена выбранной фигуры на другую.
            {
                _form.RemoveHighlight(_form.ClickedButtons[0], _form.ClickedButtons[1]);
                _form.ClickedButtons.Clear();
                _form.ClickedButtons.Add(_x);
                _form.ClickedButtons.Add(_y);
                Highlight();
                return;
            }

            _form.RemoveHighlight(_form.ClickedButtons[0], _form.ClickedButtons[1]);
            _form.ClickedButtons.Add(_x);
            _form.ClickedButtons.Add(_y);
            var move = _form.ClickedButtons.ToArray();
            _form.ClickedButtons.Clear();
            _form.MakeMove(move);
        }

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

        public PieceColor MovingSideColor => _form.MovingSideColor;
    }
}
