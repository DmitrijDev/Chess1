using Chess.LogicPart;

namespace Chess
{
    internal class NewPieceMenu : ContextMenuStrip
    {
        private readonly GamePanel _gamePanel;

        public NewPieceMenu(GamePanel gamePanel)
        {
            _gamePanel = gamePanel;
            var items = new ToolStripMenuItem[] { new ToolStripMenuItem("Ферзь"), new ToolStripMenuItem("Ладья"), new ToolStripMenuItem("Конь"), new ToolStripMenuItem("Слон") };
            Items.AddRange(items);
            Array.ForEach(items, item => item.Click += Item_Click);
        }

        private void Item_Click(object sender, EventArgs e)
        {
            var pieceNames = new ChessPieceName[] {ChessPieceName.Queen, ChessPieceName.Rook, ChessPieceName.Knight, ChessPieceName.Bishop};

            for (var i = 0; ; ++i) 
            {
                if (Items[i] == sender)
                {
                    _gamePanel.PromotePawnTo(pieceNames[i]);
                    return;
                }
            }
        }
    }
}
