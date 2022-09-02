
namespace Chess
{
    internal class ViewMenu: ToolStripMenuItem
    {
        private readonly GameForm _form;

        public ViewMenu(GameForm form) : base("Вид")
        {
            _form = form;

            var hideMenusItem = new ToolStripMenuItem("Скрывать меню")
            {
                CheckOnClick = true,
                Checked = _form.HidesMenus
            };

            DropDownItems.Add(hideMenusItem);

            hideMenusItem.Click += new EventHandler(ChangeHideMenusItemState);            
        }

        private void ChangeHideMenusItemState(object sender, EventArgs e) => _form.HidesMenus = !_form.HidesMenus;
    }
}
