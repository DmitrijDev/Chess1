namespace Chess
{
    public class SwitchMenu : ToolStripMenuItem
    {
        private readonly List<ToolStripMenuItem> _switchItems = new();

        public SwitchMenu(string menuText, int selectedItemIndex, params string[] itemTexts) : base(menuText)
        {
            if (menuText == null || itemTexts == null)
            {
                throw new ArgumentNullException();
            }

            if (menuText.Length == 0)
            {
                throw new ArgumentException("Не указано имя меню.");
            }

            if (itemTexts.Length < 2)
            {
                throw new ArgumentException("В меню должно быть не меньше двух элементов.");
            }

            if (selectedItemIndex < 0 || selectedItemIndex >= itemTexts.Length)
            {
                var message = string.Format("В меню нет элемента с порядковым номером {0}.", selectedItemIndex);
                throw new ArgumentOutOfRangeException(nameof(selectedItemIndex), message);
            }

            foreach (var text in itemTexts)
            {
                if (string.IsNullOrEmpty(text))
                {
                    throw new ArgumentException("Не указано имя элемента.");
                }

                var item = new ToolStripMenuItem(text) { CheckOnClick = true };
                DropDownItems.Add(item);
                _switchItems.Add(item);
                item.Click += Item_Click;
            }

            _switchItems[selectedItemIndex].Checked = true;
        }

        public int GetSelectedSwitchItemIndex()
        {
            var switchItemsCount = 0;
            var result = -1;

            foreach (var obj in DropDownItems)
            {
                if (!_switchItems.Contains(obj))
                {
                    continue;
                }

                var switchItem = (ToolStripMenuItem)obj;
                ++switchItemsCount;

                if (switchItem.Checked)
                {
                    if (result != -1)
                    {
                        throw new InvalidOperationException("Лишнее выделение команды.");
                    }

                    result = switchItemsCount - 1;
                }
            }

            if (result == -1)
            {
                throw new InvalidOperationException("Не выбрана ни одна команда.");
            }

            return result;
        }

        private void Item_Click(object sender, EventArgs e)
        {
            var clickedItem = (ToolStripMenuItem)sender;

            if (!DropDownItems.Contains(clickedItem) || !_switchItems.Contains(clickedItem))
            {
                return;
            }

            if (!clickedItem.Checked)
            {
                clickedItem.Checked = true;
                return;
            }

            var switchItemsCount = 0;
            var clickedItemIndex = -1;

            foreach (var item in DropDownItems)
            {
                if (!_switchItems.Contains(item))
                {
                    continue;
                }

                ++switchItemsCount;

                if (item == sender)
                {
                    clickedItemIndex = switchItemsCount - 1;
                    continue;
                }

                ((ToolStripMenuItem)item).Checked = false;
            }

            Switch?.Invoke(clickedItemIndex);
        }

        public event Action<int> Switch;
    }
}
