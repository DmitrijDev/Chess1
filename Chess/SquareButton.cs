using Chess.LogicPart;
using Board = Chess.GameBoard.GameBoard;
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
        private static int _movesCount = -1;

        public int ContainedPieceIndex { get; set; } // 0 - пустое поле, 1-6 - белые фигуры, 7-12 -черные.

        public static SquareButton[,] FormButtons { get; private set; } = new SquareButton[8, 8];

        public static Thread RenewThread { get; set; }

        //public static event Action OnA1Click;

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

        public static void ExpectVirtualPlayerMove()
        {
            while (Program.WhitePlayerThread != null && Program.WhitePlayerThread.ThreadState != ThreadState.Stopped)
            { }

            while (Program.BlackPlayerThread != null && Program.BlackPlayerThread.ThreadState != ThreadState.Stopped)
            { }

            while (_movesCount == Board.Board.MovesCount)
            { }

            GamePosition currentPosition;

            lock (Board.Board)
            {
                currentPosition = Board.Board.CurrentPosition;
            }

            lock (FormButtons)
            {
                for (var i = 0; i < 8; ++i)
                {
                    for (var j = 0; j < 8; ++j)
                    {
                        lock (FormButtons[i, j])
                        {
                            if (FormButtons[i, j].ContainedPieceIndex != currentPosition[i, j])
                            {
                                FormButtons[i, j].ContainedPieceIndex = currentPosition[i, j];
                                var text = _piecesNames[FormButtons[i, j].ContainedPieceIndex];
                                FormButtons[i, j].Text = text;
                            }
                        }
                    }
                }
            }

            _movesCount = Board.Board.MovesCount;
        }

        private void ClickSquare(object sender, EventArgs e)
        {
            lock (FormButtons)

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
                _movesCount = Board.Board.MovesCount;

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

            if (MovingSideColor == PieceColor.White && Program.WhiteVirtualPlayer != null)
            {
                Program.WhitePlayerThread = new Thread(Program.MakeMoveForWhite);
                RenewThread = new Thread(ExpectVirtualPlayerMove);
                Program.WhitePlayerThread.Start();
                RenewThread.Start();
                return;
            }

            if (MovingSideColor == PieceColor.Black && Program.BlackVirtualPlayer != null)
            {
                Program.BlackPlayerThread = new Thread(Program.MakeMoveForBlack);
                RenewThread = new Thread(ExpectVirtualPlayerMove);
                Program.BlackPlayerThread.Start();
                RenewThread.Start();
            }

            /*if (!_form.gameStarted)
            {
                _form.Play();
            }*/
        }

        // Пока временно вместо изображения фигуры просто пишем ее сокращенное название.
        public void RenewText() => Text = _piecesNames[ContainedPieceIndex];

        public static PieceColor MovingSideColor => Board.Board.MovingSideColor;
    }
}
