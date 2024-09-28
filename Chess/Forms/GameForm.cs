using Chess.LogicPart;
using System.Runtime.Serialization.Formatters.Binary;

namespace Chess
{
    internal partial class GameForm : Form
    {
        private readonly string _settingsFileName = "UserSettings.bin";

        private int _fromDragCursorToGamePanelLeft;
        private int _fromDragCursorToGamePanelTop;

        public MenuStrip MenuStrip { get; } = new();

        public SwitchMenu WhitePlayerMenu { get; private set; }

        public SwitchMenu BlackPlayerMenu { get; private set; }

        public SwitchMenu TimeMenu { get; private set; }

        public SwitchMenu ColorsMenu { get; private set; }

        public TimePanel TimePanel { get; private set; }

        public GamePanel GamePanel { get; private set; }

        public GameForm(bool loadsSettingsInfo)
        {
            InitializeComponent();
            FormSettingsInfo settingsInfo = null;

            if (loadsSettingsInfo)
            {
                settingsInfo = LoadSettingsInfo();
            }

            if (settingsInfo == null)
            {
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                WindowState = settingsInfo.WindowState;
                Location = new(settingsInfo.FormX, settingsInfo.FormY);
                Width = settingsInfo.FormWidth;
                Height = settingsInfo.FormHeight;
            }

            SetControls(settingsInfo);
            var colorsSetIndex = settingsInfo != null ? settingsInfo.ColorsSetIndex : 0;
            SetColors(colorsSetIndex);
            SetEventsHandlers();
            GamePanel.StartNewGame();
        }

        private FormSettingsInfo LoadSettingsInfo()
        {
            using (var stream = new FileStream(_settingsFileName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                return (FormSettingsInfo)new BinaryFormatter().Deserialize(stream);
            }
        }

        private void SetControls(FormSettingsInfo settingsInfo)
        {
            CreateGameMenu(settingsInfo);
            CreateViewMenu(settingsInfo);

            var timeForGame = TimePanel.GetTimeForGameValues()[settingsInfo != null ? settingsInfo.TimeMenuSelectedItemIndex : 0];
            TimePanel = new(ClientRectangle.Width, MenuStrip.Height, timeForGame);
            GamePanel = new(this);

            TimePanel.Location = new(0, MenuStrip.Height);

            if (settingsInfo == null)
            {
                PutGamePanelToCenter();
            }
            else
            {
                GamePanel.Location = new(settingsInfo.BoardX, settingsInfo.BoardY);
                GamePanel.SetSquareSize(settingsInfo.BoardSquareSize);

                if (settingsInfo.BoardIsReversed)
                {
                    GamePanel.Rotate();
                }

                GamePanelSquare.SetBorderSize(settingsInfo.SquareBorderSize);
            }

            var minWidth = Math.Max(GamePanel.Width, TimePanel.MinimumSize.Width) + (Width - ClientRectangle.Width);
            var minHeight = MenuStrip.Height + TimePanel.Height + GamePanel.Height + (Height - ClientRectangle.Height);
            MinimumSize = new(minWidth, minHeight);

            Controls.Add(MenuStrip);
            Controls.Add(TimePanel);
            Controls.Add(GamePanel);
        }

        private void CreateGameMenu(FormSettingsInfo settingsInfo)
        {
            var gameMenu = new ToolStripMenuItem("Игра");

            // Начало новой партии.
            var gameMenuItem = new ToolStripMenuItem("Новая партия");
            gameMenu.DropDownItems.Add(gameMenuItem);
            gameMenuItem.Click += (sender, e) => GamePanel.StartNewGame();

            // Смена игроков.
            var programPlaysForWhite = settingsInfo != null && settingsInfo.ProgramPlaysForWhite;
            WhitePlayerMenu = new("Белые", programPlaysForWhite ? 1 : 0, "Вы", "Соперник");
            gameMenu.DropDownItems.Add(WhitePlayerMenu);
            WhitePlayerMenu.Switch += (itemIndex) => GamePanel.SwitchPlayer(PieceColor.White);

            var programPlaysForBlack = settingsInfo == null || settingsInfo.ProgramPlaysForBlack;
            BlackPlayerMenu = new("Черные", programPlaysForBlack ? 1 : 0, "Вы", "Соперник");
            gameMenu.DropDownItems.Add(BlackPlayerMenu);
            BlackPlayerMenu.Switch += (itemIndex) => GamePanel.SwitchPlayer(PieceColor.Black);

            // Смена контроля времени.          
            TimeMenu = new("Время на партию", settingsInfo == null ? 0 : settingsInfo.TimeMenuSelectedItemIndex,
                "5 минут", "15 минут", "30 минут", "1 час", "1,5 часа", "2 часа", "3 часа");

            gameMenu.DropDownItems.Add(TimeMenu);
            TimeMenu.Switch += (itemIndex) => TimePanel.ResetTime(TimePanel.GetTimeForGameValues()[itemIndex]);

            // Сохранение игры.
            gameMenuItem = new("Сохранить игру");
            gameMenu.DropDownItems.Add(gameMenuItem);
            gameMenuItem.Click += (sender, e) => GamePanel.SaveGame();

            // Выход.
            gameMenuItem = new("Выход");
            gameMenu.DropDownItems.Add(gameMenuItem);
            gameMenuItem.Click += (sender, e) => Close();

            MenuStrip.Items.Add(gameMenu);
        }

        private void CreateViewMenu(FormSettingsInfo settingsInfo)
        {
            var viewMenu = new ToolStripMenuItem("Вид");

            // Разворот доски.
            var viewMenuItem = new ToolStripMenuItem("Развернуть доску");
            viewMenu.DropDownItems.Add(viewMenuItem);
            viewMenuItem.Click += (sender, e) => GamePanel.Rotate();

            // Выбор цветов.
            var colorsSetsNames = ColorsSet.GetStandartSets().Select(set => set.Name).ToArray();
            ColorsMenu = new("Выбор цветов", settingsInfo == null ? 0 : settingsInfo.ColorsSetIndex, colorsSetsNames);
            viewMenu.DropDownItems.Add(ColorsMenu);
            ColorsMenu.Switch += (itemIndex) => SetColors(itemIndex);

            // Изменение размера поля.
            viewMenuItem = new ToolStripMenuItem("Изменить размер доски");
            viewMenu.DropDownItems.Add(viewMenuItem);
            viewMenuItem.Click += (sender, e) => GamePanel.ShowChangeSizeForm();

            // Изменение размера рамки.
            var index = settingsInfo == null ? 0 : settingsInfo.SquareBorderSize == 1 ? 0 : settingsInfo.SquareBorderSize == 2 ? 1 : 2;
            viewMenuItem = new SwitchMenu("Рамка", index, "Обычная", "Жирная", "Нет");
            viewMenu.DropDownItems.Add(viewMenuItem);
            (viewMenuItem as SwitchMenu).Switch += (itemIndex) => GamePanelSquare.SetBorderSize(itemIndex == 0 ? 1 : itemIndex == 1 ? 2 : 0);

            MenuStrip.Items.Add(viewMenu);
        }

        private void PutGamePanelToCenter()
        {
            var gamePanelX = (ClientRectangle.Width - GamePanel.Width) / 2;

            var gamePanelY = MenuStrip.Height + TimePanel.Height +
            (ClientRectangle.Height - MenuStrip.Height - TimePanel.Height - GamePanel.Height) / 2;

            GamePanel.Location = new(gamePanelX, gamePanelY);
        }

        private void SetColors(int colorsSetIndex)
        {
            var colorsSet = ColorsSet.GetStandartSets()[colorsSetIndex];
            SetColors(colorsSet);
        }

        private void SetColors(ColorsSet colorsSet)
        {
            BackColor = colorsSet.FormBackColor;
            GamePanel.SetColors(colorsSet.BoardColor, colorsSet.LightSquaresColor, colorsSet.DarkSquaresColor, colorsSet.WhitePiecesColor,
            colorsSet.BlackPiecesColor, colorsSet.HighlightColor, colorsSet.OutlineColor);
        }

        private void SetEventsHandlers()
        {
            MouseClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    GamePanel.CancelUserMoveParams();
                }
            };

            SizeChanged += Size_Changed;
            GamePanel.SizeChanged += GamePanel_SizeChanged;
            GamePanel.MouseDown += GamePanel_MouseDown;
            QueryContinueDrag += Query_Continue_Drag;
            MenuStrip.MouseClick += (sender, e) => OnMouseClick(e);
            TimePanel.MouseClick += (sender, e) => OnMouseClick(e);
            FormClosing += (sender, e) => SaveSettingsInfo();

            GamePanel.PositionChanged += () => TimePanel.MovingSideColor = GamePanel.MovingSideColor;
            GamePanel.GameStartPositionSet += TimePanel.ResetTime;
            GamePanel.StartedExpectingMove += TimePanel.StartTimer;
            GamePanel.StoppedExpectingMove += TimePanel.StopTimer;

            TimePanel.TimeElapsed += (losingSideColor) =>
            {
                var gameResult = losingSideColor == PieceColor.White ? BoardStatus.BlackWon : BoardStatus.WhiteWon;
                var message = losingSideColor == PieceColor.White ? "Время истекло. Победа черных." : "Время истекло. Победа белых.";
                GamePanel.BreakGame(gameResult, message);
            };
        }

        private void SaveSettingsInfo()
        {
            try
            {
                if (File.Exists(_settingsFileName))
                {
                    File.SetAttributes(_settingsFileName, FileAttributes.Normal);
                }

                using (var stream = new FileStream(_settingsFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    new BinaryFormatter().Serialize(stream, new FormSettingsInfo(this));
                    File.SetAttributes(_settingsFileName, FileAttributes.Hidden | FileAttributes.ReadOnly);
                }
            }

            catch
            {
                try
                {
                    File.SetAttributes(_settingsFileName, FileAttributes.Hidden | FileAttributes.ReadOnly);
                }

                catch { }
            }
        }

        public int GetCaptionHeight()
        {
            var clientRectangle = RectangleToScreen(ClientRectangle);
            return clientRectangle.Top - Top;
        }

        private void Size_Changed(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                return;
            }

            TimePanel.Width = ClientRectangle.Width;

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

        private void Query_Continue_Drag(object sender, EventArgs e)
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

        private void GameForm_Load(object sender, EventArgs e)
        { }
    }
}
