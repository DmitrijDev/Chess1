using Chess.LogicPart;
using Chess.Players;
using Board = Chess.GameBoard.GameBoard;
using System.Diagnostics;
using System.Threading;

namespace Chess
{
    internal static class Program
    {
        public static GameForm Form { get; set; }

        public static VirtualPlayer WhiteVirtualPlayer { get; set; } //= new VirtualPlayer(Strategies.ChooseMoveForVirtualFool), или == null, если за эту сторону играет пользователь.

        public static VirtualPlayer BlackVirtualPlayer { get; set; } //= new VirtualPlayer(Strategies.ChooseMoveForVirtualFool); //Аналогично

        public static Thread WhitePlayerThread { get; set; }

        public static Thread BlackPlayerThread { get; set; }

        public static void MakeMoveForWhite()
        {
            Thread.Sleep(5000);

            while (Board.Board.MovingSideColor == PieceColor.Black)
            { }

            if (BlackVirtualPlayer != null)
            {
                BlackPlayerThread = new Thread(MakeMoveForBlack);
                SquareButton.RenewThread = new Thread(SquareButton.ExpectVirtualPlayerMove);
                BlackPlayerThread.Start();
                SquareButton.RenewThread.Start();
            }

            lock (Board.Board)
            {
                var move = WhiteVirtualPlayer.ChooseMove();
                Board.Board.MakeMove(move);
            }
        }

        public static void MakeMoveForBlack()
        {
            Thread.Sleep(5000);

            while (Board.Board.MovingSideColor == PieceColor.White)
            { }

            if (WhiteVirtualPlayer != null)
            {
                WhitePlayerThread = new Thread(MakeMoveForWhite);
                SquareButton.RenewThread = new Thread(SquareButton.ExpectVirtualPlayerMove);
                WhitePlayerThread.Start();
                SquareButton.RenewThread.Start();
            }

            lock (Board.Board)
            {
                var move = BlackVirtualPlayer.ChooseMove();
                Board.Board.MakeMove(move);
            }
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            var whiteMaterial = new string[3] { "King", "ROOK", "rook" };
            var whitePositions = new string[3] { "e1", "a1", "h1" };

            var blackMaterial = new string[3] { "Король ", " ладья ", "Л" };
            var blackPositions = new string[3] { "e8", "a8", "h8" };

            Board.Board.SetPosition(whiteMaterial, whitePositions, blackMaterial, blackPositions, PieceColor.White);
            Form = new GameForm();

            Application.Run(Form);
            //Form.Activate();
            //Form.RenewPosition();
            Process.GetCurrentProcess().Kill(); // Если пользователь закрыл форму.
        }
    }
}