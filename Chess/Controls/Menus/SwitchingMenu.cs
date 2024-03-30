
namespace Chess
{
    internal class SwitchingMenu : ToolStripMenuItem
    {
        private Action<int> _switchTo = (itemIndex) => { };

        public int SelectedItemIndex { get; private set; }

        public SwitchingMenu(string name, int selectedItemDefaultIndex, params string[] itemTexts) : base(name)
        {
            if (name == null || itemTexts == null)
            {
                throw new ArgumentNullException();
            }

            if (name.Length == 0)
            {
                throw new ArgumentException("Не указано имя меню.");
            }

            if (itemTexts.Length < 2)
            {
                throw new ArgumentOutOfRangeException("В меню должно быть не меньше двух элементов.");
            }

            if (selectedItemDefaultIndex < 0 || selectedItemDefaultIndex >= itemTexts.Length)
            {
                var message = string.Format("В меню нет элемента с порядковым номером {0}.", selectedItemDefaultIndex);
                throw new ArgumentOutOfRangeException(nameof(selectedItemDefaultIndex), message);
            }

            foreach (var text in itemTexts)
            {
                var item = new ToolStripMenuItem(text) { CheckOnClick = true };
                DropDownItems.Add(item);
                item.Click += Item_Click;
            }

            SelectedItemIndex = selectedItemDefaultIndex;
            var selectedItem = (ToolStripMenuItem)DropDownItems[SelectedItemIndex];
            selectedItem.Checked = true;
        }

        private void Item_Click(object sender, EventArgs e)
        {
            var clickedItem = (ToolStripMenuItem)sender;

            if (!clickedItem.Checked)
            {
                clickedItem.Checked = true;
                return;
            }

            foreach (var obj in DropDownItems)
            {
                if (obj == sender)
                {
                    continue;
                }

                var menuItem = (ToolStripMenuItem)obj;

                if (menuItem.Checked)
                {
                    menuItem.Checked = false;
                    break;
                }
            }

            for (var i = 0; ; ++i)
            {
                if (DropDownItems[i] == sender)
                {
                    SelectedItemIndex = i;
                    _switchTo(i);
                    return;
                }
            }
        }

        public Action<int> SwitchTo
        {
            set => _switchTo = value ?? throw new InvalidOperationException("Это свойство не может иметь значение == null");
        }
    }
}
