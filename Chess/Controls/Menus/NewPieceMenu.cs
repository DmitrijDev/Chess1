using Chess.LogicPart;

namespace Chess
{
    internal class NewPieceMenu : ContextMenuStrip
    {
        private readonly GamePanel _gamePanel;

        public NewPieceMenu(GamePanel gamePanel)
        {
            _gamePanel = gamePanel;
            var items = new ToolStripMenuItem[4] { new ToolStripMenuItem("Ферзь"), new ToolStripMenuItem("Ладья"), new ToolStripMenuItem("Конь"), new ToolStripMenuItem("Слон") };
            Items.AddRange(items);
            Array.ForEach(items, item => item.Click += new EventHandler(PromotePawn));
        }

        private void PromotePawn(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            var texts = new string[6] { null, null, "Ферзь", "Ладья", "Конь", "Слон" };
            int newPieceIndex;

            for (var i = 2; ; ++i)
            {
                if (texts[i] == menuItem.Text)
                {
                    newPieceIndex = i;
                    break;
                }
            }

            if (_gamePanel.MovingSideColor == PieceColor.Black)
            {
                newPieceIndex += 6;
            }

            _gamePanel.PromotePawn(newPieceIndex);
        }
    }
}
