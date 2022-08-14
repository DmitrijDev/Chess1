using Chess.LogicPart;

namespace Chess
{
    public class SquareButton : Button
    {
        private readonly GameForm _form;
        private readonly int _x;
        private readonly int _y;
        private static readonly string[] _piecesNames = new string[13] { "", "Кр", "Ф", "Л", "К", "С", "П", "кр", "ф", "л", "к", "с", "п" };
        // 1-6 - белые фигуры, 7-12 - черные (маленькими буквами).        

        public int ContainedPieceIndex { get; set; } // 0 - пустое поле, 1-6 - белые фигуры, 7-12 -черные.

        public SquareButton(GameForm form, int x, int y)
        {
            _form = form;
            _x = x;
            _y = y;
            Height = _form.ButtonSize;
            Width = _form.ButtonSize;
            FlatStyle = FlatStyle.Flat;
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
                if (ContainedPieceIndex == 0)
                {
                    return;
                }

                if ((MovingSideColor == PieceColor.White && ContainedPieceIndex > 6) || (MovingSideColor == PieceColor.Black && ContainedPieceIndex <= 6))
                {
                    _form.ShowMessage("Это не ваша фигура.");
                    return;
                }

                _form.ClickedButtons.Add(_x); // Запомнили координаты выбранной фигуры, ждем щелчка по полю на которое нужно сходить.
                _form.ClickedButtons.Add(_y);
                return;
            }

            if (_x == _form.ClickedButtons[0] && _y == _form.ClickedButtons[1]) // Отмена выбора.
            {
                _form.ClickedButtons.Clear();
                return;
            }

            if ((MovingSideColor == PieceColor.White && ContainedPieceIndex > 0 && ContainedPieceIndex <= 6) ||
                (MovingSideColor == PieceColor.Black && ContainedPieceIndex > 6)) //Замена выбранной фигуры на другую.
            {
                _form.ClickedButtons.Clear();
                _form.ClickedButtons.Add(_x);
                _form.ClickedButtons.Add(_y);
                return;
            }

            _form.ClickedButtons.Add(_x);
            _form.ClickedButtons.Add(_y);
            var move = _form.ClickedButtons.ToArray();
            _form.ClickedButtons.Clear();
            _form.MakeMove(move);
        }

        // Пока временно вместо изображения фигуры просто пишем ее сокращенное название.
        public void RenewText() => Text = _piecesNames[ContainedPieceIndex];

        public PieceColor MovingSideColor => _form.MovingSideColor;
    }
}
