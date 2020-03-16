using NativeUI;

namespace PTFX
{
    /// <summary>
    ///     Defines the <see cref="ModMenu" />
    /// </summary>
    public class ModMenu
    {
        /// <summary>
        ///     Defines the mainMenu
        /// </summary>
        public UIMenu mainMenu;

        /// <summary>
        ///     Defines the menuPool
        /// </summary>
        public MenuPool menuPool;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModMenu" /> class.
        /// </summary>
        /// <param name="_name">The _name<see cref="string" /></param>
        /// <param name="_description">The _description<see cref="string" /></param>
        public ModMenu(string _name, string _description)
        {
            menuPool = new MenuPool();

            mainMenu = new UIMenu(_name, _description);

            menuPool.Add(mainMenu);
            menuPool.RefreshIndex();
        }

        /// <summary>
        ///     The AddMenuItem
        /// </summary>
        /// <param name="_name">The _name<see cref="string" /></param>
        /// <param name="_description">The _description<see cref="string" /></param>
        /// <param name="_menu">The _menu<see cref="UIMenu" /></param>
        /// <returns>The <see cref="UIMenuItem" /></returns>
        public UIMenuItem AddMenuItem(string _name, string _description, UIMenu _menu = null)
        {
            var menu = _menu ?? mainMenu;
            var newitem = new UIMenuItem(_name, _description);

            menu.AddItem(newitem);
            menuPool.RefreshIndex();
            return newitem;
        }

        /// <summary>
        ///     The AddMenuCheckBox
        /// </summary>
        /// <param name="_name">The _name<see cref="string" /></param>
        /// <param name="_description">The _description<see cref="string" /></param>
        /// <param name="_defaultValue">The _defaultValue<see cref="bool" /></param>
        /// <param name="_menu">The _menu<see cref="UIMenu" /></param>
        /// <returns>The <see cref="UIMenuCheckboxItem" /></returns>
        public UIMenuCheckboxItem AddMenuCheckBox(string _name, string _description, bool _defaultValue = false,
            UIMenu _menu = null)
        {
            var menu = _menu ?? mainMenu;
            var newitem = new UIMenuCheckboxItem(_name, _defaultValue, _description);
            menu.AddItem(newitem);
            menuPool.RefreshIndex();
            return newitem;
        }

        /// <summary>
        ///     The NewMenu
        /// </summary>
        /// <param name="_name">The _name<see cref="string" /></param>
        /// <returns>The <see cref="UIMenu" /></returns>
        public UIMenu NewMenu(string _name)
        {
            var submenu = menuPool.AddSubMenu(mainMenu, _name);

            menuPool.RefreshIndex();
            return submenu;
        }

        /// <summary>
        ///     The SwitchMenu
        /// </summary>
        public void SwitchMenu()
        {
            if (menuPool.IsAnyMenuOpen())
            {
                menuPool.CloseAllMenus();
                mainMenu.Visible = false;
            }
            else
            {
                mainMenu.Visible = !mainMenu.Visible;
            }
        }
    }
}