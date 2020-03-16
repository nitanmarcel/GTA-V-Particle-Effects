using NativeUI;

namespace PTFX
{
    class ModMenu
    {
        public MenuPool menuPool;
        public UIMenu mainMenu;


        public ModMenu(string _name, string _description)
        {
            menuPool = new MenuPool();

            mainMenu = new UIMenu(_name, _description);

            menuPool.Add(mainMenu);
            menuPool.RefreshIndex();
        }

        public UIMenuItem AddMenuItem(string _name, string _description, UIMenu _menu = null)
        {
            var menu = _menu ?? mainMenu;
            var newitem = new UIMenuItem(_name, _description);

            menu.AddItem(newitem);
            menuPool.RefreshIndex();
            return newitem;
        }

        public UIMenuCheckboxItem AddMenuCheckBox(string _name, string _description, bool _defaultValue = false, UIMenu _menu = null)
        {
            var menu = _menu ?? mainMenu;
            var newitem = new UIMenuCheckboxItem(_name, _defaultValue, _description);
            menu.AddItem(newitem);
            menuPool.RefreshIndex();
            return newitem;
        }

        public UIMenu NewMenu(string _name)
        {
            var submenu = menuPool.AddSubMenu(mainMenu, _name);

            menuPool.RefreshIndex();
            return submenu;
        }

        public void SwitchMenu()
        {
            if (menuPool.IsAnyMenuOpen())
            {
                menuPool.CloseAllMenus();
                mainMenu.Visible = false;
            }
            else mainMenu.Visible = !mainMenu.Visible;

        }
    }
}
