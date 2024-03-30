using Chess.LogicPart;

namespace Chess
{
    internal partial class GameForm : Form
    {
        private int _fromDragCursorToGamePanelLeft;
        private int _fromDragCursorToGamePanelTop;

        public MenuStrip MenuStrip { get; } = new();

        public SwitchingMenu WhitePlayerMenu { get; private set; }

        public SwitchingMenu BlackPlayerMenu { get; private set; }

        public SwitchingMenu TimeMenu { get; private set; }

        public SwitchingMenu ColorsMenu { get; private set; }

        public TimePanel TimePanel { get; private set; }

        public GamePanel GamePanel { get; private set; }

        public ColorSet ColorSet { get; private set; }

        public static int[] TimeForGameValues { get; } = new int[] { 300, 900, 1800, 3600, 5400, 7200, 10800 };

        public GameForm()
        {
            InitializeComponent();
            Text = "Шахматы";
            Icon = new("Images/Icon-2.png");

            var setting = SettingsSaver.LoadSetting();

            if (setting == null)
            {
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                WindowState = setting.WindowState;
                Location = new(setting.FormX, setting.FormY);
                Width = setting.FormWidth;
                Height = setting.FormHeight;
                MinimumSize = new(setting.FormMinWidth, setting.FormMinHeight);
            }

            SetControls(setting);

            var colorSetIndex = setting != null ? setting.ColorSetIndex : 0;
            SetColors(colorSetIndex);

            SizeChanged += Size_Changed;
            GamePanel.SizeChanged += GamePanel_SizeChanged;
            GamePanel.MouseDown += GamePanel_MouseDown;
            QueryContinueDrag += GamePanel_Drag;
            MouseClick += CancelMoveChoice;
            MenuStrip.MouseClick += CancelMoveChoice;
            FormClosing += (sender, e) => SettingsSaver.SaveSetting(this);

            GamePanel.StartNewGame();
        }

        private void SetControls(FormSetting setting)
        {
            MenuStrip.Items.Add(CreateGameMenu(setting));
            MenuStrip.Items.Add(CreateViewMenu(setting));

            TimePanel = new(this);
            GamePanel = new(this);

            TimePanel.Location = new(0, MenuStrip.Height);

            if (setting == null)
            {
                PutGamePanelToCenter();
            }
            else
            {
                GamePanel.Location = new(setting.BoardX, setting.BoardY);
                GamePanel.SetButtonSize(setting.ButtonSize);

                if (setting.BoardIsReversed)
                {
                    GamePanel.Rotate();
                }
            }

            if (setting == null)
            {
                var minWidth = Math.Max(GamePanel.Width, TimePanel.MinimumSize.Width) + (Width - ClientRectangle.Width);
                var minHeight = MenuStrip.Height + TimePanel.Height + GamePanel.Height + (Height - ClientRectangle.Height);
                MinimumSize = new(minWidth, minHeight);
            }

            Controls.Add(MenuStrip);
            Controls.Add(TimePanel);
            Controls.Add(GamePanel);
        }

        private ToolStripMenuItem CreateGameMenu(FormSetting setting)
        {
            var gameMenu = new ToolStripMenuItem("Игра");

            // Начало новой партии.
            var gameMenuItem = new ToolStripMenuItem("Новая партия");
            gameMenu.DropDownItems.Add(gameMenuItem);
            gameMenuItem.Click += (sender, e) => GamePanel.StartNewGame();

            // Смена игроков.
            var programPlaysForWhite = setting != null && setting.ProgramPlaysForWhite;
            WhitePlayerMenu = new SwitchingMenu("Белые", programPlaysForWhite ? 1 : 0, "Вы", "Соперник");
            gameMenu.DropDownItems.Add(WhitePlayerMenu);
            WhitePlayerMenu.SwitchTo = (itemIndex) => GamePanel.ChangePlayer(ChessPieceColor.White);

            var programPlaysForBlack = setting == null || setting.ProgramPlaysForBlack;
            BlackPlayerMenu = new SwitchingMenu("Черные", programPlaysForBlack ? 1 : 0, "Вы", "Соперник");
            gameMenu.DropDownItems.Add(BlackPlayerMenu);
            BlackPlayerMenu.SwitchTo = (itemIndex) => GamePanel.ChangePlayer(ChessPieceColor.Black);

            // Смена контроля времени.          
            TimeMenu = new SwitchingMenu("Время на партию", setting == null ? 0 : setting.TimeMenuSelectedItemIndex,
                "5 минут", "15 минут", "30 минут", "1 час", "1,5 часа", "2 часа", "3 часа");

            gameMenu.DropDownItems.Add(TimeMenu);
            TimeMenu.SwitchTo = (itemIndex) => TimePanel.ResetTime(TimeForGameValues[itemIndex]);

            // Сохранение игры.
            gameMenuItem = new("Сохранить игру");
            gameMenu.DropDownItems.Add(gameMenuItem);
            gameMenuItem.Click += (sender, e) => GamePanel.SaveGame();

            // Выход.
            gameMenuItem = new("Выход");
            gameMenu.DropDownItems.Add(gameMenuItem);
            gameMenuItem.Click += (sender, e) => Close();

            return gameMenu;
        }

        private ToolStripMenuItem CreateViewMenu(FormSetting setting)
        {
            var viewMenu = new ToolStripMenuItem("Вид");

            // Разворот доски.
            var viewMenuItem = new ToolStripMenuItem("Развернуть доску");
            viewMenu.DropDownItems.Add(viewMenuItem);
            viewMenuItem.Click += (sender, e) => GamePanel.Rotate();

            // Выбор цветов.
            var colorSetsNames = ColorSet.GetStandartSets().Select(set => set.Name).ToArray();
            ColorsMenu = new SwitchingMenu("Выбор цветов", setting == null ? 0 : setting.ColorSetIndex, colorSetsNames);
            viewMenu.DropDownItems.Add(ColorsMenu);
            ColorsMenu.SwitchTo = (itemIndex) => SetColors(itemIndex);

            // Изменение размера.
            viewMenuItem = new ToolStripMenuItem("Изменить размер доски");
            viewMenu.DropDownItems.Add(viewMenuItem);
            viewMenuItem.Click += (sender, e) => new GamePanelSizeForm(GamePanel).ShowDialog();

            return viewMenu;
        }

        private void SetColors(ColorSet colors)
        {
            ColorSet = colors;
            BackColor = colors.FormBackColor;
            GamePanel.SetColors();
        }

        private void SetColors(int colorsSetIndex)
        {
            var colorSet = ColorSet.GetStandartSets()[colorsSetIndex];
            SetColors(colorSet);
        }

        public int GetCaptionHeight()
        {
            var clientRectangle = RectangleToScreen(ClientRectangle);
            return clientRectangle.Top - Top;
        }

        private void PutGamePanelToCenter()
        {
            var gamePanelX = (ClientRectangle.Width - GamePanel.Width) / 2;
            var gamePanelY = MenuStrip.Height + TimePanel.Height + (ClientRectangle.Height - MenuStrip.Height - TimePanel.Height - GamePanel.Height) / 2;
            GamePanel.Location = new(gamePanelX, gamePanelY);
        }

        private void GamePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            var clientRectangle = RectangleToScreen(ClientRectangle);
            _fromDragCursorToGamePanelLeft = Cursor.Position.X - clientRectangle.Left - GamePanel.Location.X;
            _fromDragCursorToGamePanelTop = Cursor.Position.Y - clientRectangle.Top - GamePanel.Location.Y;
            DoDragDrop(GamePanel, DragDropEffects.None);
        }

        private void GamePanel_Drag(object sender, EventArgs e)
        {
            var clientRectangle = RectangleToScreen(ClientRectangle);
            var gamePanelX = Cursor.Position.X - clientRectangle.Left - _fromDragCursorToGamePanelLeft;
            var gamePanelY = Cursor.Position.Y - clientRectangle.Top - _fromDragCursorToGamePanelTop;

            if (gamePanelX < 0)
            {
                gamePanelX = 0;
                _fromDragCursorToGamePanelLeft = Math.Max(Cursor.Position.X - clientRectangle.Left, 0);
            }

            if (gamePanelX > clientRectangle.Width - GamePanel.Width)
            {
                gamePanelX = clientRectangle.Width - GamePanel.Width;
                _fromDragCursorToGamePanelLeft = Math.Min(Cursor.Position.X - clientRectangle.Left - gamePanelX, GamePanel.Width);
            }

            if (gamePanelY < MenuStrip.Height + TimePanel.Height)
            {
                gamePanelY = MenuStrip.Height + TimePanel.Height;
                _fromDragCursorToGamePanelTop = Math.Max(Cursor.Position.Y - clientRectangle.Top - gamePanelY, 0);
            }

            if (gamePanelY > clientRectangle.Height - GamePanel.Height)
            {
                gamePanelY = clientRectangle.Height - GamePanel.Height;
                _fromDragCursorToGamePanelTop = Math.Min(Cursor.Position.Y - clientRectangle.Top - gamePanelY, GamePanel.Height);
            }

            GamePanel.Location = new(gamePanelX, gamePanelY);
        }

        private void Size_Changed(object sender, EventArgs e)
        {
            var gamePanelX = GamePanel.Location.X;
            var gamePanelY = GamePanel.Location.Y;

            if (gamePanelX < 0)
            {
                gamePanelX = 0;
            }

            if (gamePanelX > ClientRectangle.Width - GamePanel.Width)
            {
                gamePanelX = ClientRectangle.Width - GamePanel.Width;
            }

            if (gamePanelY < MenuStrip.Height + TimePanel.Height)
            {
                gamePanelY = MenuStrip.Height + TimePanel.Height;
            }

            if (gamePanelY > ClientRectangle.Height - GamePanel.Height)
            {
                gamePanelY = ClientRectangle.Height - GamePanel.Height;
            }

            GamePanel.Location = new(gamePanelX, gamePanelY);
        }

        private void GamePanel_SizeChanged(object sender, EventArgs e)
        {
            var minWidth = Math.Max(GamePanel.Width, TimePanel.MinimumSize.Width) + (Width - ClientRectangle.Width);
            var minHeight = MenuStrip.Height + TimePanel.Height + GamePanel.Height + (Height - ClientRectangle.Height);
            MinimumSize = new(minWidth, minHeight);

            var gamePanelX = GamePanel.Location.X;
            var gamePanelY = GamePanel.Location.Y;

            if (gamePanelX > ClientRectangle.Width - GamePanel.Width)
            {
                gamePanelX = ClientRectangle.Width - GamePanel.Width;
            }

            if (gamePanelY > ClientRectangle.Height - GamePanel.Height)
            {
                gamePanelY = ClientRectangle.Height - GamePanel.Height;
            }

            GamePanel.Location = new(gamePanelX, gamePanelY);
        }

        public void CancelMoveChoice(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                GamePanel.CancelMoveChoice();
            }
        }

        private void GameForm_Load(object sender, EventArgs e)
        { }
    }
}
