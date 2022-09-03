using Chess.LogicPart;

namespace Chess
{
    internal class NewPieceMenu : ContextMenuStrip
    {
        private readonly BoardPanel _boardPanel;

        public NewPieceMenu(BoardPanel boardPanel)
        {
            _boardPanel = boardPanel;
            var items = new ToolStripMenuItem[4] { new ToolStripMenuItem("Ферзь"), new ToolStripMenuItem("Ладья"), new ToolStripMenuItem("Конь"), new ToolStripMenuItem("Слон") };
            Items.AddRange(items);
            Array.ForEach(items, item => item.Click += new EventHandler(PromotePawn));
        }

        private void PromotePawn(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem) sender;
            var texts = new string[6] { "", "", "Ферзь", "Ладья", "Конь", "Слон" };
            var newPieceIndex = 0;

            for (var i = 2; i < texts.Length; ++i)
            {
                if (texts[i] == menuItem.Text)
                {
                    newPieceIndex = i;
                    break;
                }
            }

            if (_boardPanel.MovingSideColor == PieceColor.Black)
            {
                newPieceIndex += 6;
            }

            _boardPanel.PromotePawn(newPieceIndex);
        }
    }
}
