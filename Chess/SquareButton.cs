using Chess.LogicPart;
using Board = Chess.GameBoard;
using System.Threading;

namespace Chess
{
    public class SquareButton : Button
    {
        private readonly GameForm _form;
        private readonly int _x;
        private readonly int _y;
        private static List<int> _clickedButtons = new();
        private static string[] _piecesNames = new string[13] { "", "Кр", "Ф", "Л", "К", "С", "П", "кр", "ф", "л", "к", "с", "п" };
        // 1-6 - белые фигуры, 7-12 - черные (маленькими буквами).        

        public int ContainedPieceIndex { get; set; } // 0 - пустое поле, 1-6 - белые фигуры, 7-12 -черные.

        //public static SquareButton[,] FormButtons { get; private set; } = new SquareButton[8, 8];

        public SquareButton(GameForm form, int x, int y)
        {
            _form = form;
            _x = x;
            _y = y;
            Height = _form.ButtonSize;
            Width = _form.ButtonSize;
            FlatStyle = FlatStyle.Flat;
            Click += new EventHandler(ClickSquare);
        }

        private void ClickSquare(object sender, EventArgs e)
        {
            if ((MovingSideColor == PieceColor.White && Program.WhiteVirtualPlayer != null) || (MovingSideColor == PieceColor.Black && Program.BlackVirtualPlayer != null))
            {
                return;
            }

            if (_clickedButtons.Count == 0) // Т.е. выбор фигуры для хода.
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

                _clickedButtons.Add(_x); // Запомнили координаты выбранной фигуры, ждем щелчка по полю на которое нужно сходить.
                _clickedButtons.Add(_y);
                return;
            }

            if (_x == _clickedButtons[0] && _y == _clickedButtons[1]) // Отмена выбора.
            {
                _clickedButtons.Clear();
                return;
            }

            if ((MovingSideColor == PieceColor.White && ContainedPieceIndex <= 6 && ContainedPieceIndex > 0) ||
                (MovingSideColor == PieceColor.Black && ContainedPieceIndex > 6)) //Замена выбранной фигуры на другую.
            {
                _clickedButtons.Clear();
                _clickedButtons.Add(_x);
                _clickedButtons.Add(_y);
                return;
            }

            _clickedButtons.Add(_x);
            _clickedButtons.Add(_y);

            //Теперь делаем ход.

            var move = _clickedButtons.ToArray();
            _clickedButtons.Clear();

            try
            {
                Board.Board.MakeMove(move);
            }

            catch (Exception exception) // На случай, если ход не по правилам.
            {
                _form.ShowMessage(exception.Message);
                return;
            }

            _form.RenewPosition();

            if (Board.Board.Status == GameStatus.WhiteWin)
            {
                _form.ShowMessage("Мат черным.");
            }

            if (Board.Board.Status == GameStatus.BlackWin)
            {
                _form.ShowMessage("Мат белым.");
            }

            if (Board.Board.Status == GameStatus.Draw)
            {
                _form.ShowMessage("Ничья.");
            }            
        }

        // Пока временно вместо изображения фигуры просто пишем ее сокращенное название.
        public void RenewText() => Text = _piecesNames[ContainedPieceIndex];

        public static PieceColor MovingSideColor => Board.Board.MovingSideColor;
    }
}
