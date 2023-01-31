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
            Array.ForEach(items, item => item.Click += Item_Click);
        }

        private void Item_Click(object sender, EventArgs e)
        {
            int newPieceIndex;

            for (var i = 2; ; ++i)  // Ферзь - первая фигура в меню - имеет св-во NumeralIndex == 2, если белый.
            {
                if (Items[i - 2] == sender)
                {
                    newPieceIndex = i;
                    break;
                }
            }

            if (_gamePanel.MovingSideColor == PieceColor.Black) // Черная фигура имеет св-во NumeralIndex на 6 больше одноименной белой.
            {
                newPieceIndex += 6;
            }

            _gamePanel.PromotePawn(newPieceIndex);
        }
    }
}
