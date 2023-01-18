
namespace Chess
{
    internal class ViewMenu : ToolStripMenuItem
    {
        private readonly GameForm _form;

        public ViewMenu(GameForm form) : base("Вид")
        {
            _form = form;
        }
    }
}
