
namespace Chess
{
    public class SwitchingMenu : ToolStripMenuItem
    {
        private Action<int> _switchTo = (itemIndex) => { };

        public SwitchingMenu(string name, int selectedItemDefaultIndex, params string[] itemTexts) : base(name)
        {
            if (itemTexts.Length < 2)
            {
                throw new ArgumentException("В меню должно быть не меньше двух элементов.");
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

            var selectedItem = (ToolStripMenuItem)DropDownItems[selectedItemDefaultIndex];
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
