using Chess.LogicPart;
using Chess.Players;
using System.Diagnostics;
using Board = Chess.GameBoard;

namespace Chess
{
    internal static class Program
    {
        public static GameForm Form { get; set; }

        public static VirtualPlayer WhiteVirtualPlayer { get; private set; } //= new VirtualPlayer(Strategies.ChooseMoveForVirtualFool); // или == null, если за эту сторону играет пользователь.

        public static VirtualPlayer BlackVirtualPlayer { get; private set; } = new VirtualPlayer(Strategies.ChooseMoveForVirtualFool); //јналогично

        public static Thread WhitePlayerThread { get; set; }

        public static Thread BlackPlayerThread { get; set; }

        public static Thread GameFormThread { get; private set; }

        private static void PlayForWhite()
        {
            for (; ; )
            {
                if (Board.Board.MovingSideColor == PieceColor.Black)
                {
                    continue;
                }

                lock (Board.Board)
                {
                    var move = WhiteVirtualPlayer.ChooseMove();
                    Board.Board.MakeMove(move);
                }
            }
        }

        private static void PlayForBlack()
        {
            for (; ; )
            {
                if (Board.Board.MovingSideColor == PieceColor.White)
                {
                    continue;
                }

                lock (Board.Board)
                {
                    var move = BlackVirtualPlayer.ChooseMove();
                    Board.Board.MakeMove(move);
                }
            }
        }

        private static void DoGameFormTask()
        {
            Form = new GameForm();
            Form.FormClosed += new FormClosedEventHandler(EndWork);
            Form.ShowDialog();
            var movesCount = Board.Board.MovesCount;

            for (; ; )
            {
                if (Board.Board.MovesCount > movesCount)
                {
                    Form.RenewPosition();
                    movesCount = Board.Board.MovesCount;
                }
            }
        }

        private static void EndWork(object sender, EventArgs e) => Process.GetCurrentProcess().Kill(); // ≈сли пользователь закрыл форму.

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var whiteMaterial = new string[3] { "King", "Rook", "Rook" };
            var whitePositions = new string[3] { "e1", "a1", "h1" };

            var blackMaterial = new string[3] { "King", "Rook", "Rook" };
            var blackPositions = new string[3] { "e8", "a8", "h8" };

            Board.Board.SetPosition(whiteMaterial, whitePositions, blackMaterial, blackPositions, PieceColor.White);

            GameFormThread = new Thread(DoGameFormTask);
            GameFormThread.Start();

            if (WhiteVirtualPlayer != null)
            {
                WhitePlayerThread = new Thread(PlayForWhite);
                WhitePlayerThread.Start();
            }

            if (BlackVirtualPlayer != null)
            {
                BlackPlayerThread = new Thread(PlayForBlack);
                BlackPlayerThread.Start();
            }

        }
    }
}