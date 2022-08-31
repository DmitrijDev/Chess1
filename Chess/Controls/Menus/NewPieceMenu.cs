using Chess.LogicPart;

namespace Chess
{
    internal class NewPieceMenu : ContextMenuStrip
    {
        private readonly BoardPanel _boardPanel;

        internal NewPieceMenu(BoardPanel boardPanel)
        {
            _boardPanel = boardPanel;
            var items = new ToolStripMenuItem[1] { new ToolStripMenuItem("Ладья") };
            Items.AddRange(items);
            Array.ForEach(items, item => item.Click += new EventHandler(SelectNewPiece));
        }

        public void SelectNewPiece(object sender, EventArgs e)
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

            _boardPanel.SelectNewPiece(newPieceIndex);
        }
    }
}
