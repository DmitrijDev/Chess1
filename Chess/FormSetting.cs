
namespace Chess
{
    internal class FormSetting
    {
        public FormSetting()
        { }

        public FormWindowState WindowState { get; set; }

        public int FormX { get; set; }

        public int FormY { get; set; }

        public int FormWidth { get; set; }

        public int FormHeight { get; set; }

        public int FormMinWidth { get; set; }

        public int FormMinHeight { get; set; }

        public int BoardX { get; set; }

        public int BoardY { get; set; }

        public int ButtonSize { get; set; }

        public bool BoardIsReversed { get; set; }

        public int ColorSetIndex { get; set; }

        public bool ProgramPlaysForWhite { get; set; }

        public bool ProgramPlaysForBlack { get; set; }

        public int TimeMenuSelectedItemIndex { get; set; }
    }
}
