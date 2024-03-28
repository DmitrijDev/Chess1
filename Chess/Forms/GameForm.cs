using Chess.LogicPart;

namespace Chess
{
    internal partial class GameForm : Form
    {
        private int _fromDragCursorToGamePanelLeft;
        private int _fromDragCursorToGamePanelTop;

        public MenuStrip MenuStrip { get; } = new();

        public TimePanel TimePanel { get; private set; }

        public GamePanel GamePanel { get; private set; }

        public ColorSet ColorSet { get; private set; }

        public GameForm()
        {
            InitializeComponent();
            Text = "Шахматы";
            Icon = new("Images/Icon-2.png");
            WindowState = FormWindowState.Maximized;
            SetControls();
            SetColors(0);

            SizeChanged += Size_Changed;
            GamePanel.SizeChanged += GamePanel_SizeChanged;
            GamePanel.MouseDown += GamePanel_MouseDown;
            QueryContinueDrag += GamePanel_Drag;
            MouseClick += CancelMoveChoice;
            MenuStrip.MouseClick += CancelMoveChoice;

            GamePanel.StartNewGame();
        }

        private void SetControls()
        {
            MenuStrip.Items.Add(CreateGameMenu());
            MenuStrip.Items.Add(CreateViewMenu());

            TimePanel = new(this);
            GamePanel = new(this);

            TimePanel.Location = new(0, MenuStrip.Height);
            PutGamePanelToCenter();

            Controls.Add(MenuStrip);
            Controls.Add(TimePanel);
            Controls.Add(GamePanel);

            var minWidth = Math.Max(GamePanel.Width, TimePanel.MinimumSize.Width) + (Width - ClientRectangle.Width);
            var minHeight = MenuStrip.Height + TimePanel.Height + GamePanel.Height + (Height - ClientRectangle.Height);
            MinimumSize = new(minWidth, minHeight);
        }

        private ToolStripMenuItem CreateGameMenu()
        {
            var gameMenu = new ToolStripMenuItem("Игра");

            // Начало новой партии.
            var gameMenuItem = new ToolStripMenuItem("Новая партия");
            gameMenu.DropDownItems.Add(gameMenuItem);
            gameMenuItem.Click += (sender, e) => GamePanel.StartNewGame();

            // Смена игроков.
            gameMenuItem = new SwitchingMenu("Белые", 0, "Вы", "Соперник");
            gameMenu.DropDownItems.Add(gameMenuItem);
            (gameMenuItem as SwitchingMenu).SwitchTo = (itemIndex) => GamePanel.ChangePlayer(ChessPieceColor.White);

            gameMenuItem = new SwitchingMenu("Черные", 1, "Вы", "Соперник");
            gameMenu.DropDownItems.Add(gameMenuItem);
            (gameMenuItem as SwitchingMenu).SwitchTo = (itemIndex) => GamePanel.ChangePlayer(ChessPieceColor.Black);

            // Смена контроля времени.
            var timeForGameValues = new int[] { 300, 900, 1800, 3600, 5400, 7200, 10800 };
            gameMenuItem = new SwitchingMenu("Время на партию", 0, "5 минут", "15 минут", "30 минут", "1 час", "1,5 часа", "2 часа", "3 часа");
            gameMenu.DropDownItems.Add(gameMenuItem);
            (gameMenuItem as SwitchingMenu).SwitchTo = (itemIndex) => TimePanel.ResetTime(timeForGameValues[itemIndex]);

            // Сохранение игры.
            gameMenuItem = new ToolStripMenuItem("Сохранить игру");
            gameMenu.DropDownItems.Add(gameMenuItem);
            gameMenuItem.Click += (sender, e) => GamePanel.SaveGame();

            // Выход.
            gameMenuItem = new("Выход");
            gameMenu.DropDownItems.Add(gameMenuItem);
            gameMenuItem.Click += (sender, e) => Close();

            return gameMenu;
        }

        private ToolStripMenuItem CreateViewMenu()
        {
            var viewMenu = new ToolStripMenuItem("Вид");

            // Разворот доски.
            var viewMenuItem = new ToolStripMenuItem("Развернуть доску");
            viewMenu.DropDownItems.Add(viewMenuItem);
            viewMenuItem.Click += (sender, e) => GamePanel.Rotate();

            // Выбор цветов.
            var colorSetsNames = ColorSet.GetStandartSets().Select(theme => theme.Name).ToArray();
            viewMenuItem = new SwitchingMenu("Выбор цветов", 0, colorSetsNames);
            viewMenu.DropDownItems.Add(viewMenuItem);
            (viewMenuItem as SwitchingMenu).SwitchTo = (itemIndex) => SetColors(itemIndex);

            // Изменение размера.
            viewMenuItem = new ToolStripMenuItem("Изменить размер доски");
            viewMenu.DropDownItems.Add(viewMenuItem);
            viewMenuItem.Click += (sender, e) => new GamePanelSizeForm(GamePanel).ShowDialog();

            return viewMenu;
        }

        private void SetColors(int colorsThemeIndex)
        {
            ColorSet = ColorSet.GetStandartSets()[colorsThemeIndex];
            BackColor = ColorSet.FormBackColor;
            GamePanel.SetColors();
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
