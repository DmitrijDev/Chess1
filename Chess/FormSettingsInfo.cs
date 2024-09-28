
namespace Chess
{
    [Serializable]
    internal class FormSettingsInfo
    {
        public FormWindowState WindowState { get; }

        public int FormX { get; }

        public int FormY { get; }

        public int FormWidth { get; }

        public int FormHeight { get; }

        public int BoardX { get; }

        public int BoardY { get; }

        public int BoardSquareSize { get; }

        public bool BoardIsReversed { get; }

        public byte ColorsSetIndex { get; }

        public byte SquareBorderSize { get; }

        public bool ProgramPlaysForWhite { get; }

        public bool ProgramPlaysForBlack { get; }

        public byte TimeMenuSelectedItemIndex { get; }

        public FormSettingsInfo(GameForm gameForm)
        {
            WindowState = gameForm.WindowState;

            FormX = gameForm.Location.X;
            FormY = gameForm.Location.Y;

            FormWidth = gameForm.Width;
            FormHeight = gameForm.Height;

            var gamePanel = gameForm.GamePanel;

            BoardX = gamePanel.Location.X;
            BoardY = gamePanel.Location.Y;

            BoardSquareSize = gamePanel.SquareSize;

            BoardIsReversed = gamePanel.IsReversed;
            ColorsSetIndex = (byte)gameForm.ColorsMenu.GetSelectedSwitchItemIndex();
            SquareBorderSize = (byte)GamePanelSquare.BorderSize;

            ProgramPlaysForWhite = gameForm.WhitePlayerMenu.GetSelectedSwitchItemIndex() == 1;
            ProgramPlaysForBlack = gameForm.BlackPlayerMenu.GetSelectedSwitchItemIndex() == 1;

            TimeMenuSelectedItemIndex = (byte)gameForm.TimeMenu.GetSelectedSwitchItemIndex();
        }
    }
}
