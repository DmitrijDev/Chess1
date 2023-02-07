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
            var piecesNames = new PieceName[] {PieceName.Queen, PieceName.Rook, PieceName.Knight, PieceName.Bishop};

            for (var i = 0; ; ++i) 
            {
                if (Items[i] == sender)
                {
                    _gamePanel.PromotePawn(piecesNames[i]);
                    return;
                }
            }
        }
    }
}
