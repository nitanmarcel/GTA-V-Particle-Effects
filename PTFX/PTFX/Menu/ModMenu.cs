// Menu Source code: https://github.com/LfxB/SimpleUI


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using Control = GTA.Control;
using Font = GTA.UI.Font;
using Screen = GTA.UI.Screen;

namespace PTFX.Menu
{
    public class MenuPool
    {
        /// <summary>
        ///     Disable this before editing the menu so that the pool will stop iterating over the menus and not crash.
        /// </summary>
        private static readonly bool AllowMenuDraw = true;

        /// <summary>
        ///     Additional text displayed on the right side of a Submenu's parent item.
        ///     <para>Default is "  ~r~>" (with two spaces in front)</para>
        ///     <para>Colour codes: https://pastebin.com/nqNYWMSB </para>
        /// </summary>
        public string SubmenuItemIndication = "  ~r~>";

        public UIMenu LastUsedMenu { get; set; }

        public List<UIMenu> UIMenuList { get; set; } = new List<UIMenu>();

        public void AddMenu(UIMenu menu)
        {
            UIMenuList.Add(menu);
            if (UIMenuList.Count == 1) LastUsedMenu = menu;
        }

        /// <summary>
        ///     Adds a submenu to a parent menu and to the MenuPool. Returns UIMenuItem that links the parent menu to the submenu.
        ///     <para><see cref="SubmenuItemIndication" /> will be added to the end of the item text.</para>
        /// </summary>
        /// <param name="SubMenu">The submenu</param>
        /// <param name="ParentMenu">The parent menu.</param>
        /// <param name="text">The text of the menu item in the parent menu that leads to the submenu when entered.</param>
        public void AddSubMenu(UIMenu SubMenu, UIMenu ParentMenu, string text, bool UseSameColorsAsParent = true)
        {
            AddMenu(SubMenu);
            /*SubMenu.ParentMenu = ParentMenu;
            ParentMenu.NextMenu = SubMenu;*/
            var item = new UIMenuItem(text + SubmenuItemIndication); //colour codes: https://pastebin.com/nqNYWMSB
            //ParentMenu.BindingMenuItem = BindingItem;
            ParentMenu.AddMenuItem(item);
            //ParentMenu.BindingMenuItem = item;
            ParentMenu.BindItemToSubmenu(SubMenu, item);

            if (UseSameColorsAsParent) ApplyColorScheme(SubMenu, ParentMenu);
        }

        /// <summary>
        ///     Adds a submenu to a parent menu and to the MenuPool. Returns UIMenuItem that links the parent menu to the submenu.
        ///     <para><see cref="SubmenuItemIndication" /> will be added to the end of the item text.</para>
        /// </summary>
        /// <param name="SubMenu">The submenu</param>
        /// <param name="ParentMenu">The parent menu.</param>
        /// <param name="text">The text of the menu item in the parent menu that leads to the submenu when entered.</param>
        /// <param name="description">The description of the menu item that leads to the submenu when entered.</param>
        public void AddSubMenu(UIMenu SubMenu, UIMenu ParentMenu, string text, string description,
            bool UseSameColorsAsParent = true)
        {
            AddMenu(SubMenu);
            //SubMenu.ParentMenu = ParentMenu;
            //ParentMenu.NextMenu = SubMenu;
            var item = new UIMenuItem(text + SubmenuItemIndication, null, description);
            //ParentMenu.BindingMenuItem = BindingItem;
            ParentMenu.AddMenuItem(item);
            //ParentMenu.BindingMenuItem = item;
            ParentMenu.BindItemToSubmenu(SubMenu, item);

            if (UseSameColorsAsParent) ApplyColorScheme(SubMenu, ParentMenu);
        }

        /// <summary>
        ///     Applies the color scheme of the baseMenu to the targetMenu.
        /// </summary>
        /// <param name="targetMenu">The UIMenu you would like to modify.</param>
        /// <param name="baseMenu">The UIMenu that has the color scheme you would like to copy.</param>
        public void ApplyColorScheme(UIMenu targetMenu, UIMenu baseMenu)
        {
            targetMenu.TitleColor = baseMenu.TitleColor;
            targetMenu.TitleUnderlineColor = baseMenu.TitleUnderlineColor;
            targetMenu.TitleBackgroundColor = baseMenu.TitleBackgroundColor;

            targetMenu.DefaultTextColor = baseMenu.DefaultTextColor;
            targetMenu.DefaultBoxColor = baseMenu.DefaultBoxColor;
            targetMenu.HighlightedItemTextColor = baseMenu.HighlightedItemTextColor;
            targetMenu.HighlightedBoxColor = baseMenu.HighlightedBoxColor;
            targetMenu.SubsectionDefaultTextColor = baseMenu.SubsectionDefaultTextColor;
            targetMenu.SubsectionDefaultBoxColor = baseMenu.SubsectionDefaultBoxColor;

            targetMenu.DescriptionTextColor = baseMenu.DescriptionTextColor;
            targetMenu.DescriptionBoxColor = baseMenu.DescriptionBoxColor;
        }

        /// <summary>
        ///     Draws all visible menus.
        /// </summary>
        public void Draw()
        {
            foreach (var menu in UIMenuList.Where(menu => menu.IsVisible).ToList())
            {
                menu.Draw();
                SetLastUsedMenu(menu);
            }
        }

        /// <summary>
        ///     Set the last used menu.
        /// </summary>
        public void SetLastUsedMenu(UIMenu menu)
        {
            LastUsedMenu = menu;
        }

        /// <summary>
        ///     Process all of your menus' functions. Call this in a tick event.
        /// </summary>
        public void ProcessMenus()
        {
            if (!AllowMenuDraw) return;

            if (LastUsedMenu == null) LastUsedMenu = UIMenuList[0];
            Draw();
        }

        /// <summary>
        ///     Checks if any menu is currently visible.
        /// </summary>
        /// <returns>true if at least one menu is visible, false if not.</returns>
        public bool IsAnyMenuOpen()
        {
            return UIMenuList.Any(menu => menu.IsVisible);
        }

        public bool IsMenuDrawAllowed()
        {
            return AllowMenuDraw;
        }

        /// <summary>
        ///     Closes all of your menus.
        /// </summary>
        public void CloseAllMenus()
        {
            foreach (var menu in UIMenuList.Where(menu => menu.IsVisible).ToList()) menu.IsVisible = false;
        }

        public void RemoveAllMenus()
        {
            UIMenuList.Clear();
        }

        public void OpenCloseLastMenu()
        {
            if (IsAnyMenuOpen())
                CloseAllMenus();
            else
                LastUsedMenu.IsVisible = !LastUsedMenu.IsVisible;
        }
    }

    public delegate void MenuOpenEvent(UIMenu sender);

    public delegate void ItemHighlightEvent(UIMenu sender, UIMenuItem selectedItem, int index);

    public delegate void ItemSelectEvent(UIMenu sender, UIMenuItem selectedItem, int index);

    public delegate void ItemLeftRightEvent(UIMenu sender, UIMenuItem selectedItem, int index,
        UIMenu.Direction direction);

    public class UIMenu
    {
        public enum Direction
        {
            None,
            Left,
            Right
        }

        private readonly string AUDIO_BACK = "BACK";

        private readonly string AUDIO_LIBRARY = "HUD_FRONTEND_DEFAULT_SOUNDSET";
        private readonly string AUDIO_SELECT = "SELECT";

        private readonly string AUDIO_UPDOWN = "NAV_UP_DOWN";

        private readonly List<Control> ControlsToEnable = new List<Control>
        {
            /*Control.FrontendAccept,
            Control.FrontendAxisX,
            Control.FrontendAxisY,
            Control.FrontendDown,
            Control.FrontendUp,
            Control.FrontendLeft,
            Control.FrontendRight,
            Control.FrontendCancel,
            Control.FrontendSelect,
            Control.CharacterWheel,
            Control.CursorScrollDown,
            Control.CursorScrollUp,
            Control.CursorX,
            Control.CursorY,*/
            Control.MoveUpDown,
            Control.MoveLeftRight,
            Control.Sprint,
            Control.Jump,
            Control.Enter,
            Control.VehicleExit,
            Control.VehicleAccelerate,
            Control.VehicleBrake,
            Control.VehicleMoveLeftRight,
            Control.VehicleFlyYawLeft,
            Control.FlyLeftRight,
            Control.FlyUpDown,
            Control.VehicleFlyYawRight,
            Control.VehicleHandbrake,
            /*Control.VehicleRadioWheel,
            Control.VehicleRoof,
            Control.VehicleHeadlight,
            Control.VehicleCinCam,
            Control.Phone,
            Control.MeleeAttack1,
            Control.MeleeAttack2,
            Control.Attack,
            Control.Attack2*/
            Control.LookUpDown,
            Control.LookLeftRight
        };

        /*Scroll or nah?*/
        private readonly bool UseScroll = true;
        protected List<UIMenuItem> _itemList = new List<UIMenuItem>();

        private bool _visible;
        private string AUDIO_LEFTRIGHT = "NAV_LEFT_RIGHT";
        public int boxHeight = 38; //height in pixels
        public int boxScrollWidth = 4; //width in pixels
        public int boxTitleHeight = 76; //height in pixels
        public int boxUnderlineHeight = 1; //height in pixels
        public int boxWidth = 500; //width in pixels
        public Color DefaultBoxColor = Color.FromArgb(144, 0, 0, 0);

        /*UIMenuItem Formatting*/
        public Color DefaultTextColor = Color.FromArgb(255, 255, 255, 255);
        public Color DescriptionBoxColor = Color.FromArgb(150, 0, 255, 255);

        /*Description Formatting*/
        public Color DescriptionTextColor = Color.FromArgb(255, 0, 0, 0);
        internal float heightItemBG;
        public Color HighlightedBoxColor = Color.FromArgb(255, 0, 0, 0);
        public Color HighlightedItemTextColor = Color.FromArgb(255, 0, 255, 255);

        internal float ItemTextFontSize;
        internal Font ItemTextFontType;
        protected int maxItem = 14; //must always be 1 less than MaxItemsOnScreen
        protected int MaxItemsOnScreen = 15;
        internal float MenuBGWidth;

        public int menuXPos = 38; //pixels from the top
        public int menuYPos = 38; //pixels from the left
        protected int minItem;
        internal float posMultiplier;

        protected float ScrollBarWidth;

        public int SelectedIndex;

        public UIMenuItem SelectedItem;
        public Color SubsectionDefaultBoxColor = Color.FromArgb(144, 0, 0, 0);

        public Color SubsectionDefaultTextColor = Color.FromArgb(180, 255, 255, 255);
        public Color TitleBackgroundColor = Color.FromArgb(144, 0, 0, 0);
        internal float TitleBGHeight;

        /*Title Formatting*/
        public Color TitleColor = Color.FromArgb(255, 255, 255, 255);

        /*Title*/
        public float TitleFontSize;
        public Color TitleUnderlineColor = Color.FromArgb(140, 0, 255, 255);
        internal float UnderlineHeight;

        public bool UseEventBasedControls = true;

        /*Rectangle box for UIMenuItem objects*/
        internal float xPosBG;
        internal float xPosItemText;
        internal float xPosItemValue;
        internal float xPosRightEndOfMenu;
        protected float xPosScrollBar;
        internal int YPosBasedOnScroll;
        internal int YPosDescBasedOnScroll;
        internal float yPosItem;
        internal float yPosItemBG;
        private float YPosSmoothScrollBar;
        internal float yPosTitleBG;
        internal float yPosTitleText;
        internal float yPosUnderline;
        internal float yTextOffset;

        //protected event KeyEventHandler KeyUp;
        //bool AcceptPressed;
        //bool CancelPressed;

        public UIMenu(string title)
        {
            Title = title;

            TitleFontSize = 0.9f; //TitleFont = 1.1f; for no-value fit.
            ItemTextFontSize = 0.452f;
            ItemTextFontType = Font.ChaletComprimeCologne;

            CalculateMenuPositioning();

            //KeyUp += UIMenu_KeyUp;
        }

        /// <summary>
        ///     If this UIMenu object is not a submenu, ParentMenu returns null.
        /// </summary>
        public UIMenu ParentMenu { get; set; }

        /// <summary>
        ///     Returns the UIMenuItem object within the ParentMenu that is binded to this menu when selected, assuming this menu
        ///     is a submenu.
        /// </summary>
        public UIMenuItem ParentItem { get; set; }

        public bool IsVisible
        {
            get => _visible;
            set
            {
                if (value && !_visible)
                {
                    SaveIndexPositionFromOutOfBounds();
                    MenuOpen();
                }

                _visible = value;
            }
        }

        public string Title { get; set; }

        public List<UIMenuItem> UIMenuItemList
        {
            get => _itemList;
            set => _itemList = value;
        }

        public List<BindedItem> BindedList { get; set; } = new List<BindedItem>();

        public List<UIMenuItem> DisabledList { get; set; } = new List<UIMenuItem>();

        /// <summary>
        ///     Called when menu is opened.
        /// </summary>
        public event MenuOpenEvent OnMenuOpen;

        /// <summary>
        ///     Called while item is highlighted/hovered over.
        /// </summary>
        public event ItemHighlightEvent WhileItemHighlight;

        /// <summary>
        ///     Called when user selects a simple item.
        /// </summary>
        public event ItemSelectEvent OnItemSelect;

        /// <summary>
        ///     Called when user presses left or right over a simple item.
        /// </summary>
        public event ItemLeftRightEvent OnItemLeftRight;

        /*private void UIMenu_KeyUp(object sender, KeyEventArgs e)
        {
            if (IsVisible)
            {
                if (e.KeyCode == Keys.NumPad5 || e.KeyCode == Keys.Enter)
                {
                    AcceptPressed = true;
                    GTA.UI.Notification.Show("HI");
                }

                if (e.KeyCode == Keys.NumPad0 || e.KeyCode == Keys.Back)
                {
                    CancelPressed = true;
                }
            }
        }*/

        public virtual void CalculateMenuPositioning()
        {
            const float height = 1080f;
            var ratio = (float) Screen.Resolution.Width / Screen.Resolution.Height;
            var width = height * ratio;

            TitleBGHeight = boxTitleHeight / height; //0.046f
            yPosTitleBG = menuYPos / height + TitleBGHeight * 0.5f;
            MenuBGWidth = boxWidth / width; //MenuBGWidth = 0.24f; for no-value fit.
            xPosBG = menuXPos / width + MenuBGWidth * 0.5f; //xPosBG = 0.13f; for no-value fit.
            xPosItemText = (menuXPos + 10) / width;
            heightItemBG = boxHeight / height;
            UnderlineHeight = boxUnderlineHeight / height; //0.002f;
            posMultiplier = boxHeight / height;
            yTextOffset = 0.015f; //offset between text pos and box pos. yPosItemBG - yTextOffset
            ScrollBarWidth = boxScrollWidth / width;

            yPosTitleText = yPosTitleBG - TitleFontSize / 35f;
            yPosUnderline = yPosTitleBG + TitleBGHeight / 2 + UnderlineHeight / 2;
            yPosItemBG = yPosUnderline + UnderlineHeight / 2 + heightItemBG / 2; //0.0655f;
            yPosItem = yPosItemBG - ItemTextFontSize / 30.13f;
            //xPosItemText = xPosBG - (MenuBGWidth / 2) + 0.0055f;
            xPosRightEndOfMenu = xPosBG + MenuBGWidth / 2; //will Right Justify
            xPosScrollBar = xPosRightEndOfMenu - ScrollBarWidth / 2;
            xPosItemValue = xPosScrollBar - ScrollBarWidth / 2;
            YPosSmoothScrollBar =
                yPosItemBG; //sets starting scroll bar Y pos. Will be manipulated for smooth scrolling later.
        }

        public void MaxItemsInMenu(int number)
        {
            MaxItemsOnScreen = number;
            maxItem = number - 1;
        }

        public void ResetIndexPosition()
        {
            SelectedIndex = 0;
            minItem = 0;
            MaxItemsInMenu(MaxItemsOnScreen);
        }

        public void SaveIndexPositionFromOutOfBounds()
        {
            SetIndexPosition(SelectedIndex >= _itemList.Count ? _itemList.Count - 1 : SelectedIndex);
        }

        public void SetIndexPosition(int indexPosition)
        {
            if (indexPosition >= _itemList.Count) return;

            SelectedIndex = indexPosition;

            if (SelectedIndex < MaxItemsOnScreen)
            {
                minItem = 0;
                maxItem = MaxItemsOnScreen - 1;
            }
            else
            {
                maxItem = SelectedIndex;
                minItem = SelectedIndex - (MaxItemsOnScreen - 1);
            }
        }

        public void AddMenuItem(UIMenuItem item)
        {
            _itemList.Add(item);
            item.PersistentIndex = _itemList.IndexOf(item);
        }

        public void BindItemToSubmenu(UIMenu submenu, UIMenuItem itemToBindTo)
        {
            submenu.ParentMenu = this;
            submenu.ParentItem = itemToBindTo;
            itemToBindTo.SubmenuWithin = submenu;
            BindedList.Add(new BindedItem {BindedSubmenu = submenu, BindedItemToSubmenu = itemToBindTo});
        }

        public virtual void Draw()
        {
            if (IsVisible)
            {
                DisplayMenu();
                DisableControls();
                DrawScrollBar();
                ManageCurrentIndex();

                if (BindedList.Count > 0)
                    if (JustPressedAccept() && BindedList.Any(bind => bind.BindedItemToSubmenu == SelectedItem))
                    {
                        IsVisible = false;

                        foreach (var bind in BindedList.Where(bind => bind.BindedItemToSubmenu == SelectedItem))
                            bind.BindedSubmenu.IsVisible = true;

                        if (UseEventBasedControls) ItemSelect(SelectedItem, SelectedIndex);
                        UIInput.InputTimer = DateTime.Now.AddMilliseconds(350);
                    }

                if (JustPressedCancel())
                {
                    GoBack(false);

                    UIInput.InputTimer = DateTime.Now.AddMilliseconds(350);
                }

                if (UseEventBasedControls)
                {
                    if (JustPressedAccept())
                    {
                        ItemSelect(SelectedItem, SelectedIndex);
                        UIInput.InputTimer = DateTime.Now.AddMilliseconds(UIInput.InputWait);
                        //AcceptPressed = false;
                    }

                    if (JustPressedLeft())
                    {
                        ItemLeftRight(SelectedItem, SelectedIndex, Direction.Left);
                        UIInput.InputTimer = DateTime.Now.AddMilliseconds(UIInput.InputWait);
                    }

                    if (JustPressedRight())
                    {
                        ItemLeftRight(SelectedItem, SelectedIndex, Direction.Right);
                        UIInput.InputTimer = DateTime.Now.AddMilliseconds(UIInput.InputWait);
                    }

                    ItemHighlight(SelectedItem, SelectedIndex);
                }
            }
        }


        protected void DisplayMenu()
        {
            DrawCustomText(Title, TitleFontSize, Font.HouseScript, TitleColor.R, TitleColor.G, TitleColor.B,
                TitleColor.A, xPosBG, yPosTitleText, TextJustification.Center); //Draw title text
            DrawRectangle(xPosBG, yPosTitleBG, MenuBGWidth, TitleBGHeight, TitleBackgroundColor.R,
                TitleBackgroundColor.G, TitleBackgroundColor.B, TitleBackgroundColor.A); //Draw main rectangle
            DrawRectangle(xPosBG, yPosUnderline, MenuBGWidth, UnderlineHeight, TitleUnderlineColor.R,
                TitleUnderlineColor.G, TitleUnderlineColor.B,
                TitleUnderlineColor.A); //Draw rectangle as underline of title

            foreach (var item in _itemList)
            {
                var ScrollOrNotDecision =
                    UseScroll && _itemList.IndexOf(item) >= minItem && _itemList.IndexOf(item) <= maxItem || !UseScroll;
                if (ScrollOrNotDecision)
                {
                    YPosBasedOnScroll = UseScroll && _itemList.Count > MaxItemsOnScreen
                        ? CalculatePosition(_itemList.IndexOf(item), minItem, maxItem, 0, MaxItemsOnScreen - 1)
                        : _itemList.IndexOf(item);
                    YPosDescBasedOnScroll = UseScroll && _itemList.Count > MaxItemsOnScreen
                        ? MaxItemsOnScreen
                        : _itemList.Count;

                    item.Draw(this);
                }
            }
        }

        protected void DrawScrollBar()
        {
            if (UseScroll && _itemList.Count > MaxItemsOnScreen)
            {
                YPosSmoothScrollBar = CalculateSmoothPosition(YPosSmoothScrollBar,
                    CalculateScroll(SelectedIndex, 0, _itemList.Count - 1, yPosItemBG,
                        yPosItemBG + (MaxItemsOnScreen - 1) * posMultiplier), 0.0005f, yPosItemBG,
                    yPosItemBG + (MaxItemsOnScreen - 1) * posMultiplier);
                DrawRectangle(xPosScrollBar, YPosSmoothScrollBar, ScrollBarWidth, heightItemBG, TitleUnderlineColor.R,
                    TitleUnderlineColor.G, TitleUnderlineColor.B, TitleUnderlineColor.A);

                //DrawRectangle(xPosScrollBar, CalculateScroll(SelectedIndex, 0, _itemList.Count - 1, yPosItemBG, yPosItemBG + (MaxItemsOnScreen - 1) * posMultiplier), ScrollBarWidth, heightItemBG, TitleUnderlineColor.R, TitleUnderlineColor.G, TitleUnderlineColor.B, TitleUnderlineColor.A);
            }
        }

        private int CalculatePosition(int input, int inputMin, int inputMax, int outputMin, int outputMax)
        {
            //http://stackoverflow.com/questions/22083199/method-for-calculating-a-value-relative-to-min-max-values
            //Making sure bounderies arent broken...
            if (input > inputMax) input = inputMax;
            if (input < inputMin) input = inputMin;
            //Return value in relation to min og max

            var position = (double) (input - inputMin) / (inputMax - inputMin);

            var relativeValue = (int) (position * (outputMax - outputMin)) + outputMin;

            return relativeValue;
        }

        private float CalculateScroll(float input, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            //http://stackoverflow.com/questions/22083199/method-for-calculating-a-value-relative-to-min-max-values
            //Making sure bounderies arent broken...
            if (input > inputMax) input = inputMax;
            if (input < inputMin) input = inputMin;
            //Return value in relation to min og max

            var position = (double) (input - inputMin) / (inputMax - inputMin);

            var relativeValue = (float) (position * (outputMax - outputMin)) + outputMin;

            return relativeValue;
        }

        private float CalculateSmoothPosition(float currentPosition, float desiredPosition, float step, float min,
            float max)
        {
            if (currentPosition == desiredPosition) return currentPosition;

            if (currentPosition < desiredPosition)
            {
                //currentPosition += (desiredPosition - currentPosition) * 0.1f;
                currentPosition += (desiredPosition - currentPosition) * 5f * Game.LastFrameTime;
                if (currentPosition > max) currentPosition = max;
                return currentPosition;
            }

            if (currentPosition > desiredPosition)
            {
                //currentPosition -= (currentPosition - desiredPosition) * 0.1f;
                currentPosition -= (currentPosition - desiredPosition) * 5f * Game.LastFrameTime;
                if (currentPosition < min) currentPosition = min;
                return currentPosition;
            }

            return currentPosition;
        }

        internal void DrawCustomText(string Message, float FontSize, Font FontType, int Red, int Green, int Blue,
            int Alpha, float XPos, float YPos, TextJustification justifyType = TextJustification.Left,
            bool ForceTextWrap = false)
        {
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "jamyfafi"); //Required
            Function.Call(Hash.SET_TEXT_SCALE, 1.0f, FontSize);
            Function.Call(Hash.SET_TEXT_FONT, (int) FontType);
            Function.Call(Hash.SET_TEXT_COLOUR, Red, Green, Blue, Alpha);
            //Function.Call(Hash.SET_TEXT_DROPSHADOW, 0, 0, 0, 0, 0);
            Function.Call(Hash.SET_TEXT_JUSTIFICATION, (int) justifyType);
            if (justifyType == TextJustification.Right || ForceTextWrap)
                Function.Call(Hash.SET_TEXT_WRAP, xPosItemText, xPosItemValue);

            //Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, Message);
            StringHelper.AddLongString(Message);

            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, XPos, YPos);
        }

        internal void DrawRectangle(float BgXpos, float BgYpos, float BgWidth, float BgHeight, int bgR, int bgG,
            int bgB, int bgA)
        {
            Function.Call(Hash.DRAW_RECT, BgXpos, BgYpos, BgWidth, BgHeight, bgR, bgG, bgB, bgA);
        }

        protected virtual void ManageCurrentIndex()
        {
            if (JustPressedUp()) MoveUp();

            if (JustPressedDown()) MoveDown();
        }

        public void MoveUp()
        {
            if (SelectedIndex > 0 && SelectedIndex <= _itemList.Count - 1)
            {
                SelectedIndex--;
                if (SelectedIndex < minItem && minItem > 0)
                {
                    minItem--;
                    maxItem--;
                }
            }
            else if (SelectedIndex == 0)
            {
                SelectedIndex = _itemList.Count - 1;
                minItem = _itemList.Count - MaxItemsOnScreen;
                maxItem = _itemList.Count - 1;
            }
            else
            {
                SelectedIndex = _itemList.Count - 1;
                minItem = _itemList.Count - MaxItemsOnScreen;
                maxItem = _itemList.Count - 1;
            }

            if (IsHoldingSpeedupControl())
                UIInput.InputTimer = DateTime.Now.AddMilliseconds(20);
            else
                UIInput.InputTimer = DateTime.Now.AddMilliseconds(UIInput.InputWait);
        }

        public void MoveDown()
        {
            if (SelectedIndex >= 0 && SelectedIndex < _itemList.Count - 1)
            {
                SelectedIndex++;
                if (SelectedIndex >= maxItem + 1)
                {
                    minItem++;
                    maxItem++;
                }
            }
            else if (SelectedIndex == _itemList.Count - 1)
            {
                SelectedIndex = 0;
                minItem = 0;
                maxItem = MaxItemsOnScreen - 1;
            }
            else
            {
                SelectedIndex = 0;
                minItem = 0;
                maxItem = MaxItemsOnScreen - 1;
            }

            if (IsHoldingSpeedupControl())
                UIInput.InputTimer = DateTime.Now.AddMilliseconds(20);
            else
                UIInput.InputTimer = DateTime.Now.AddMilliseconds(UIInput.InputWait);
        }

        public void DisableItem(UIMenuItem itemToDisable)
        {
            if (_itemList.Contains(itemToDisable))
            {
                DisabledList.Add(itemToDisable);
                _itemList.Remove(itemToDisable);
                SaveIndexPositionFromOutOfBounds();
            }
        }

        public void ReenableItem(UIMenuItem itemToEnable)
        {
            if (DisabledList.Contains(itemToEnable))
            {
                //_itemList.Insert(itemToEnable.PersistentIndex, itemToEnable);
                _itemList.Add(itemToEnable);
                //SortMenuItemsByOriginalOrder();
                DisabledList.Remove(itemToEnable);
                SaveIndexPositionFromOutOfBounds();
            }
        }

        public void ReenableAllItems()
        {
            DisabledList.ForEach(d => _itemList.Add(d));
            //SortMenuItemsByOriginalOrder();
            DisabledList.Clear();
            SaveIndexPositionFromOutOfBounds();
        }

        public void ResetOriginalOrder()
        {
            _itemList.ForEach(i => i.PersistentIndex = _itemList.IndexOf(i));
        }

        public void SortMenuItemsByOriginalOrder()
        {
            _itemList.Sort((x, y) => x.PersistentIndex.CompareTo(y.PersistentIndex));
        }

        protected void DisableControls()
        {
            Game.DisableAllControlsThisFrame();

            foreach (var con in ControlsToEnable) Game.EnableControlThisFrame(con);
        }

        private bool IsGamepad()
        {
            return Game.LastInputMethod == InputMethod.GamePad;
        }

        internal bool IsHoldingUp()
        {
            return IsGamepad() && Game.IsControlPressed(Control.PhoneUp) || Game.IsKeyPressed(Keys.NumPad8) ||
                   Game.IsKeyPressed(Keys.Up);
        }

        public bool JustPressedUp()
        {
            if (IsHoldingUp())
                if (UIInput.InputTimer < DateTime.Now)
                {
                    Audio.ReleaseSound(Audio.PlaySoundFrontend(AUDIO_UPDOWN, AUDIO_LIBRARY));
                    return true;
                }

            return false;
        }

        private bool IsHoldingDown()
        {
            return IsGamepad() && Game.IsControlPressed(Control.PhoneDown) || Game.IsKeyPressed(Keys.NumPad2) ||
                   Game.IsKeyPressed(Keys.Down);
        }

        public bool JustPressedDown()
        {
            if (IsHoldingDown())
                if (UIInput.InputTimer < DateTime.Now)
                {
                    Audio.ReleaseSound(Audio.PlaySoundFrontend(AUDIO_UPDOWN, AUDIO_LIBRARY));
                    return true;
                }

            return false;
        }

        public bool JustPressedLeft()
        {
            if (IsGamepad() && Game.IsControlPressed(Control.PhoneLeft) || Game.IsKeyPressed(Keys.NumPad4) ||
                Game.IsKeyPressed(Keys.Left))
                if (UIInput.InputTimer < DateTime.Now)
                {
                    Audio.ReleaseSound(Audio.PlaySoundFrontend(AUDIO_UPDOWN, AUDIO_LIBRARY));
                    return true;
                }

            return false;
        }

        public bool JustPressedRight()
        {
            if (IsGamepad() && Game.IsControlPressed(Control.PhoneRight) || Game.IsKeyPressed(Keys.NumPad6) ||
                Game.IsKeyPressed(Keys.Right))
                if (UIInput.InputTimer < DateTime.Now)
                {
                    Audio.ReleaseSound(Audio.PlaySoundFrontend(AUDIO_UPDOWN, AUDIO_LIBRARY));
                    return true;
                }

            return false;
        }

        /*public bool JustPressedAccept()
        {
            if ((IsGamepad() && Game.IsControlJustPressed(2, Control.PhoneSelect)) || AcceptPressed)
            {
                Audio.ReleaseSound(Audio.PlaySoundFrontend(AUDIO_SELECT, AUDIO_LIBRARY);
                //AcceptPressed = false;
                return true;
            }
            return false;
        }

        public bool JustPressedCancel()
        {
            if ((IsGamepad() && Game.IsControlJustPressed(2, Control.PhoneCancel)) || CancelPressed)
            {
                Audio.ReleaseSound(Audio.PlaySoundFrontend(AUDIO_BACK, AUDIO_LIBRARY);
                //CancelPressed = false;
                return true;
            }
            return false;
        }*/

        public bool JustPressedAccept()
        {
            if (Game.IsControlPressed(Control.PhoneSelect) || Game.IsKeyPressed(Keys.NumPad5) ||
                Game.IsKeyPressed(Keys.Enter))
                if (UIInput.InputTimer < DateTime.Now)
                {
                    Audio.ReleaseSound(Audio.PlaySoundFrontend(AUDIO_SELECT, AUDIO_LIBRARY));
                    //InputTimer = Game.GameTime + 350;
                    return true;
                }

            return false;
        }

        public bool JustPressedCancel()
        {
            if (Game.IsControlPressed(Control.PhoneCancel) || Game.IsKeyPressed(Keys.NumPad0) ||
                Game.IsKeyPressed(Keys.Back))
                if (UIInput.InputTimer < DateTime.Now)
                {
                    Audio.ReleaseSound(Audio.PlaySoundFrontend(AUDIO_BACK, AUDIO_LIBRARY));
                    //InputTimer = Game.GameTime + InputWait;
                    return true;
                }

            return false;
        }

        public void GoBack(bool withSound)
        {
            IsVisible = false;

            if (ParentMenu != null) ParentMenu.IsVisible = true;

            if (withSound)
                Audio.ReleaseSound(Audio.PlaySoundFrontend(AUDIO_BACK, AUDIO_LIBRARY));
        }

        private bool IsHoldingSpeedupControl()
        {
            if (IsGamepad())
                return Game.IsControlPressed(Control.VehicleHandbrake);
            return Game.IsKeyPressed(Keys.ShiftKey);
        }

        public void SetInputWait(int ms = 350)
        {
            UIInput.InputTimer = DateTime.Now.AddMilliseconds(ms);
        }

        /// <summary>
        ///     Control a bool easily inside a <see cref="OnItemSelect" /> event
        /// </summary>
        /// <param name="boolToControl">The bool to control</param>
        /// <param name="controllingItem">The item that controls this bool</param>
        public void ControlBoolValue(ref bool boolToControl, UIMenuItem controllingItem)
        {
            if (IsVisible && SelectedItem == controllingItem) boolToControl = !boolToControl;
            controllingItem.Value = boolToControl;
        }

        public bool ControlBoolValue(bool boolToControl, UIMenuItem controllingItem)
        {
            ControlBoolValue(ref boolToControl, controllingItem);
            return boolToControl;
        }

        public bool ControlBoolValue_NoEvent(UIMenuItem item, bool boolToControl)
        {
            item.Value = boolToControl;

            if (IsVisible && SelectedItem == item)
                if (JustPressedAccept())
                {
                    boolToControl = !boolToControl;
                    item.Value = boolToControl;
                    UIInput.InputTimer = DateTime.Now.AddMilliseconds(UIInput.InputWait);
                    return boolToControl;
                }

            return boolToControl;
        }

        /// <summary>
        ///     Control a float easily inside a <see cref="OnItemLeftRight" /> event
        /// </summary>
        /// <param name="numberToControl">The float to control</param>
        /// <param name="controllingItem">The item that controls this float</param>
        /// <param name="left">The direction given by the <see cref="OnItemLeftRight" /> event</param>
        /// <param name="incrementValue">How many units you want to add/subtract when left/right is pressed</param>
        /// <param name="incrementValueFast">Same as incrementValue, but when holding SHIFT or the R1/RB gamepad button</param>
        /// <param name="decimals">How many decimals to round to</param>
        /// <param name="limit">Set whether you'd like to limit how high/low this float can go</param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void ControlFloatValue(ref float numberToControl, UIMenuItem controllingItem, Direction direction,
            float incrementValue, float incrementValueFast, int decimals = 2, bool limit = false, float min = 0f,
            float max = 1f)
        {
            if (IsVisible && SelectedItem == controllingItem)
            {
                var temp = numberToControl;
                if (direction == Direction.Left)
                {
                    if (IsHoldingSpeedupControl())
                        temp -= incrementValueFast;
                    else
                        temp -= incrementValue;
                }
                else if (direction == Direction.Right)
                {
                    if (IsHoldingSpeedupControl())
                        temp += incrementValueFast;
                    else
                        temp += incrementValue;
                }

                if (limit)
                {
                    if (temp < min)
                        temp = min;
                    else if (temp > max) temp = max;
                }

                numberToControl = (float) Math.Round(temp, decimals);
            }

            controllingItem.Value = "< " + numberToControl + " >";
        }

        public float ControlFloatValue(float numberToControl, UIMenuItem controllingItem, Direction direction,
            float incrementValue, float incrementValueFast, int decimals = 2, bool limit = false, float min = 0f,
            float max = 1f)
        {
            ControlFloatValue(ref numberToControl, controllingItem, direction, incrementValue, incrementValueFast,
                decimals, limit, min, max);
            return numberToControl;
        }

        public float ControlFloatValue_NoEvent(UIMenuItem item, float numberToControl, float incrementValue,
            float incrementValueFast, int decimals = 2, bool limit = false, float min = 0f, float max = 1f)
        {
            item.Value = "< " + numberToControl + " >";

            if (IsVisible && SelectedItem == item)
            {
                if (JustPressedLeft())
                {
                    if (IsHoldingSpeedupControl())
                        numberToControl -= incrementValueFast;
                    else
                        numberToControl -= incrementValue;

                    if (limit)
                    {
                        if (numberToControl < min) numberToControl = min;
                        if (numberToControl > max) numberToControl = max;
                    }

                    item.Value = "< " + numberToControl + " >";

                    UIInput.InputTimer = DateTime.Now.AddMilliseconds(UIInput.InputWait);

                    return (float) Math.Round(numberToControl, decimals);
                }

                if (JustPressedRight())
                {
                    if (IsHoldingSpeedupControl())
                        numberToControl += incrementValueFast;
                    else
                        numberToControl += incrementValue;

                    if (limit)
                    {
                        if (numberToControl < min) numberToControl = min;
                        if (numberToControl > max) numberToControl = max;
                    }

                    item.Value = "< " + numberToControl + " >";

                    UIInput.InputTimer = DateTime.Now.AddMilliseconds(UIInput.InputWait);

                    return (float) Math.Round(numberToControl, decimals);
                }
            }

            return numberToControl;
        }

        /// <summary>
        ///     Control an integer easily inside a <see cref="OnItemLeftRight" /> event
        /// </summary>
        /// <param name="numberToControl">The int to control</param>
        /// <param name="controllingItem">The item that controls this int</param>
        /// <param name="left">The direction given by the <see cref="OnItemLeftRight" /> event</param>
        /// <param name="incrementValue">How many units you want to add/subtract when left/right is pressed</param>
        /// <param name="incrementValueFast">Same as incrementValue, but when holding SHIFT or the R1/RB gamepad button</param>
        /// <param name="limit">Set whether you'd like to limit how high/low this int can go</param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void ControlIntValue(ref int numberToControl, UIMenuItem controllingItem, Direction direction,
            int incrementValue, int incrementValueFast, bool limit = false, int min = 0, int max = 100)
        {
            if (IsVisible && SelectedItem == controllingItem)
            {
                var temp = numberToControl;
                if (direction == Direction.Left)
                {
                    if (IsHoldingSpeedupControl())
                        temp -= incrementValueFast;
                    else
                        temp -= incrementValue;
                }
                else if (direction == Direction.Right)
                {
                    if (IsHoldingSpeedupControl())
                        temp += incrementValueFast;
                    else
                        temp += incrementValue;
                }

                if (limit)
                {
                    if (temp < min)
                        temp = min;
                    else if (temp > max) temp = max;
                }

                numberToControl = temp;
            }

            controllingItem.Value = "< " + numberToControl + " >";
        }

        public int ControlIntValue(int numberToControl, UIMenuItem controllingItem, Direction direction,
            int incrementValue, int incrementValueFast, bool limit = false, int min = 0, int max = 100)
        {
            ControlIntValue(ref numberToControl, controllingItem, direction, incrementValue, incrementValueFast, limit,
                min, max);
            return numberToControl;
        }

        public int ControlIntValue_NoEvent(UIMenuItem item, int numberToControl, int incrementValue,
            int incrementValueFast, bool limit = false, int min = 0, int max = 100)
        {
            item.Value = "< " + numberToControl + " >";

            if (IsVisible && SelectedItem == item)
            {
                if (JustPressedLeft())
                {
                    if (IsHoldingSpeedupControl())
                        numberToControl -= incrementValueFast;
                    else
                        numberToControl -= incrementValue;

                    if (limit)
                    {
                        if (numberToControl < min) numberToControl = min;
                        if (numberToControl > max) numberToControl = max;
                    }

                    item.Value = "< " + numberToControl + " >";

                    UIInput.InputTimer = DateTime.Now.AddMilliseconds(UIInput.InputWait);

                    return numberToControl;
                }

                if (JustPressedRight())
                {
                    if (IsHoldingSpeedupControl())
                        numberToControl += incrementValueFast;
                    else
                        numberToControl += incrementValue;

                    if (limit)
                    {
                        if (numberToControl < min) numberToControl = min;
                        if (numberToControl > max) numberToControl = max;
                    }

                    item.Value = "< " + numberToControl + " >";

                    UIInput.InputTimer = DateTime.Now.AddMilliseconds(UIInput.InputWait);

                    return numberToControl;
                }
            }

            return numberToControl;
        }

        /// <summary>
        ///     Control an Enum easily inside a <see cref="OnItemLeftRight" /> event
        /// </summary>
        /// <typeparam name="T">Must be an Enum</typeparam>
        /// <param name="enumToControl">The Enum to control</param>
        /// <param name="controllingItem">The item that controls this enum</param>
        /// <param name="left">The direction given by the <see cref="OnItemLeftRight" /> event</param>
        public void ControlEnumValue<T>(ref T enumToControl, UIMenuItem controllingItem, Direction direction)
            where T : struct
        {
            if (IsVisible && SelectedItem == controllingItem)
            {
                if (direction == Direction.Left)
                    enumToControl = enumToControl.Previous();
                else if (direction == Direction.Right) enumToControl = enumToControl.Next();
            }

            controllingItem.Value = "< " + enumToControl + " >";
        }

        protected virtual void MenuOpen()
        {
            OnMenuOpen?.Invoke(this);
        }

        protected virtual void ItemHighlight(UIMenuItem selecteditem, int index)
        {
            WhileItemHighlight?.Invoke(this, selecteditem, index);
        }

        protected virtual void ItemSelect(UIMenuItem selecteditem, int index)
        {
            OnItemSelect?.Invoke(this, selecteditem, index);
        }

        protected virtual void ItemLeftRight(UIMenuItem selecteditem, int index, Direction direction)
        {
            OnItemLeftRight?.Invoke(this, selecteditem, index, direction);
        }

        public void UnsubscribeAll_OnMenuOpen()
        {
            OnMenuOpen = null;
        }

        public void UnsubscribeAll_OnItemSelect()
        {
            OnItemSelect = null;
            //OnItemSelect = delegate { }; // Causes more overhead
        }

        public void UnsubscribeAll_OnItemLeftRight()
        {
            OnItemLeftRight = null;
        }

        public void UnsubscribeAll_WhileItemHighlight()
        {
            WhileItemHighlight = null;
        }

        public void Dispose()
        {
            UnsubscribeAll_OnMenuOpen();
            UnsubscribeAll_OnItemSelect();
            UnsubscribeAll_OnItemLeftRight();
            UnsubscribeAll_WhileItemHighlight();

            ParentMenu = null;
            ParentItem = null;

            SelectedItem = null;

            foreach (var item in _itemList.ToList()) item.Dispose();
            _itemList.Clear();

            foreach (var item in DisabledList.ToList()) item.Dispose();
            DisabledList.Clear();

            BindedList.Clear();
        }

        internal enum TextJustification
        {
            Center = 0,
            Left,
            Right //requires SET_TEXT_WRAP
        }
    }

    public class UIMenuDisplayOnly : UIMenu
    {
        public UIMenuDisplayOnly(string text) : base(text)
        {
            TitleFontSize = 0.5f;
            boxWidth = 400;

            CalculateMenuPositioning();

            MaxItemsInMenu(8);
        }

        public override void Draw()
        {
            if (IsVisible)
            {
                DisplayMenu();
                DisableControls();
                DrawScrollBar();
                //ManageCurrentIndex();
            }
        }

        protected override void ManageCurrentIndex()
        {
            //base.ManageCurrentIndex();
        }

        public void GoToNextItem()
        {
            SelectedIndex++;
            if (SelectedIndex >= maxItem + 1)
            {
                minItem++;
                maxItem++;
            }
        }

        public void GoToFirstItem()
        {
            SelectedIndex = 0;
            minItem = 0;
            maxItem = MaxItemsOnScreen - 1;
        }

        public void GoToPreviousItem()
        {
            SelectedIndex--;
            if (SelectedIndex < minItem && minItem > 0)
            {
                minItem--;
                maxItem--;
            }
        }

        public void GoToLastItem()
        {
            SelectedIndex = _itemList.Count - 1;
            minItem = _itemList.Count - MaxItemsOnScreen;
            maxItem = _itemList.Count - 1;
        }
    }

    public class UIMenuItem
    {
        private string _description;

        public UIMenuItem(string text)
        {
            Text = text;
        }

        public UIMenuItem(string text, dynamic value)
        {
            Text = text;
            Value = value;
        }

        public UIMenuItem(string text, dynamic value, string description)
        {
            Text = text;
            Value = value;
            Description = description;
        }

        public UIMenuItem(string text, string description)
        {
            Text = text;
            Description = description;
        }

        //public List<string> DescriptionTexts;
        public float DescriptionWidth { get; set; }

        public string Text { get; set; }

        public dynamic Value { get; set; }

        public string Description
        {
            get => _description;
            set
            {
                if (value != null)
                    DescriptionWidth = StringHelper.MeasureStringWidth(value, Font.ChaletComprimeCologne, 0.452f);

                _description = value;
            }
        }

        public int PersistentIndex { get; set; }

        /// <summary>
        ///     The Submenu which this UIMenuItem leads to, if any.
        /// </summary>
        public UIMenu SubmenuWithin { get; set; }

        public virtual void Draw(UIMenu sourceMenu)
        {
            if (sourceMenu.UIMenuItemList.IndexOf(this) == sourceMenu.SelectedIndex)
            {
                sourceMenu.DrawCustomText(Text, sourceMenu.ItemTextFontSize, sourceMenu.ItemTextFontType,
                    sourceMenu.HighlightedItemTextColor.R, sourceMenu.HighlightedItemTextColor.G,
                    sourceMenu.HighlightedItemTextColor.B, sourceMenu.HighlightedItemTextColor.A,
                    sourceMenu.xPosItemText,
                    sourceMenu.yPosItem +
                    sourceMenu.YPosBasedOnScroll * sourceMenu.posMultiplier); //Draw highlighted item text

                if (Value != null)
                    sourceMenu.DrawCustomText(Convert.ToString(Value), sourceMenu.ItemTextFontSize,
                        sourceMenu.ItemTextFontType, sourceMenu.HighlightedItemTextColor.R,
                        sourceMenu.HighlightedItemTextColor.G, sourceMenu.HighlightedItemTextColor.B,
                        sourceMenu.HighlightedItemTextColor.A, sourceMenu.xPosItemValue,
                        sourceMenu.yPosItem + sourceMenu.YPosBasedOnScroll * sourceMenu.posMultiplier,
                        UIMenu.TextJustification.Right);

                sourceMenu.DrawRectangle(sourceMenu.xPosBG,
                    sourceMenu.yPosItemBG + sourceMenu.YPosBasedOnScroll * sourceMenu.posMultiplier,
                    sourceMenu.MenuBGWidth, sourceMenu.heightItemBG, sourceMenu.HighlightedBoxColor.R,
                    sourceMenu.HighlightedBoxColor.G, sourceMenu.HighlightedBoxColor.B,
                    sourceMenu.HighlightedBoxColor.A); //Draw rectangle over highlighted text

                if (Description != null)
                {
                    /*foreach (string desc in item.DescriptionTexts)
                    {
                        DrawCustomText(desc, ItemTextFontSize, ItemTextFontType, DescriptionTextColor.R, DescriptionTextColor.G, DescriptionTextColor.B, DescriptionTextColor.A, xPosItemText, yPosItem + (item.DescriptionTexts.IndexOf(desc) + YPosDescBasedOnScroll) * posMultiplier, TextJustification.Left, false); // Draw description text at bottom of menu
                        DrawRectangle(xPosBG, yPosItemBG + (item.DescriptionTexts.IndexOf(desc) + YPosDescBasedOnScroll) * posMultiplier, MenuBGWidth, heightItemBG, DescriptionBoxColor.R, DescriptionBoxColor.G, DescriptionBoxColor.B, DescriptionBoxColor.A); //Draw rectangle over description text at bottom of the list.
                    }*/

                    sourceMenu.DrawCustomText(Description, sourceMenu.ItemTextFontSize, sourceMenu.ItemTextFontType,
                        sourceMenu.DescriptionTextColor.R, sourceMenu.DescriptionTextColor.G,
                        sourceMenu.DescriptionTextColor.B, sourceMenu.DescriptionTextColor.A, sourceMenu.xPosItemText,
                        sourceMenu.yPosItem + sourceMenu.YPosDescBasedOnScroll * sourceMenu.posMultiplier,
                        UIMenu.TextJustification.Left, true); // Draw description text at bottom of menu
                    var numLines = DescriptionWidth / (sourceMenu.boxWidth - 10);
                    for (var l = 0; l < (int) Math.Ceiling(numLines); l++)
                        sourceMenu.DrawRectangle(sourceMenu.xPosBG,
                            sourceMenu.yPosItemBG + (l + sourceMenu.YPosDescBasedOnScroll) * sourceMenu.posMultiplier,
                            sourceMenu.MenuBGWidth, sourceMenu.heightItemBG, sourceMenu.DescriptionBoxColor.R,
                            sourceMenu.DescriptionBoxColor.G, sourceMenu.DescriptionBoxColor.B,
                            sourceMenu.DescriptionBoxColor
                                .A); //Draw rectangle over description text at bottom of the list.
                    //GTA.UI.Notification.Show(numLines.ToString());
                }

                sourceMenu.SelectedItem = this;
            }
            else
            {
                sourceMenu.DrawCustomText(Text, sourceMenu.ItemTextFontSize, sourceMenu.ItemTextFontType,
                    sourceMenu.DefaultTextColor.R, sourceMenu.DefaultTextColor.G, sourceMenu.DefaultTextColor.B,
                    sourceMenu.DefaultTextColor.A,
                    sourceMenu.xPosItemText,
                    sourceMenu.yPosItem + sourceMenu.YPosBasedOnScroll * sourceMenu.posMultiplier); //Draw item text

                if (Value != null)
                    sourceMenu.DrawCustomText(Convert.ToString(Value), sourceMenu.ItemTextFontSize,
                        sourceMenu.ItemTextFontType, sourceMenu.DefaultTextColor.R, sourceMenu.DefaultTextColor.G,
                        sourceMenu.DefaultTextColor.B, sourceMenu.DefaultTextColor.A, sourceMenu.xPosItemValue,
                        sourceMenu.yPosItem + sourceMenu.YPosBasedOnScroll * sourceMenu.posMultiplier,
                        UIMenu.TextJustification.Right);

                sourceMenu.DrawRectangle(sourceMenu.xPosBG,
                    sourceMenu.yPosItemBG + sourceMenu.YPosBasedOnScroll * sourceMenu.posMultiplier,
                    sourceMenu.MenuBGWidth, sourceMenu.heightItemBG, sourceMenu.DefaultBoxColor.R,
                    sourceMenu.DefaultBoxColor.G, sourceMenu.DefaultBoxColor.B,
                    sourceMenu.DefaultBoxColor.A); //Draw background rectangle around item.
            }
        }

        public virtual void Dispose()
        {
            SubmenuWithin = null;
        }
    }

    public class UIMenuNumberValueItem : UIMenuItem
    {
        public UIMenuNumberValueItem(string text, dynamic value) : base(text, (object) value)
        {
            Text = text;
            SetValue(value);
        }

        public UIMenuNumberValueItem(string text, dynamic value, string description) : base(text, (object) value,
            description)
        {
            Text = text;
            SetValue(value);
            Description = description;
        }

        public void SetValue(dynamic value)
        {
            Value = "< " + value + " >";
        }
    }

    public class UIMenuListItem : UIMenuItem
    {
        public int SelectedIndex;

        public UIMenuListItem(string text, string description, List<object> list) : base(text, description)
        {
            Text = text;
            Description = description;
            List = list;
            Value = "< " + List[SelectedIndex] + " >";
        }

        private List<object> List { get; set; }

        public object CurrentListItem => List[SelectedIndex];

        public int IndexFromItem(object item)
        {
            return List.FindIndex(i => ReferenceEquals(i, item));
        }

        public object ItemFromIndex(int index)
        {
            return List[index];
        }

        public override void Draw(UIMenu sourceMenu)
        {
            if (sourceMenu.UIMenuItemList.IndexOf(this) == sourceMenu.SelectedIndex)
            {
                if (sourceMenu.JustPressedLeft())
                    MovePrevious();
                else if (sourceMenu.JustPressedRight()) MoveNext();
            }

            base.Draw(sourceMenu);
        }

        private void MoveNext()
        {
            if (SelectedIndex >= List.Count - 1)
                SelectedIndex = 0;
            else
                SelectedIndex++;
            Value = "< " + List[SelectedIndex] + " >";
        }

        private void MovePrevious()
        {
            if (SelectedIndex <= 0)
                SelectedIndex = List.Count - 1;
            else
                SelectedIndex--;
            Value = "< " + List[SelectedIndex] + " >";
        }

        /// <summary>
        ///     Call this after updating your List<>.
        /// </summary>
        public void SaveListUpdateFromOutOfBounds()
        {
            if (SelectedIndex < 0)
                SelectedIndex = 0;
            else if (SelectedIndex >= List.Count) SelectedIndex = List.Count - 1;
            Value = "< " + List[SelectedIndex] + " >";
        }

        public override void Dispose()
        {
            base.Dispose();
            List = null;
        }
    }

    public class UIMenuSubsectionItem : UIMenuItem
    {
        public UIMenuSubsectionItem(string text) : base(text)
        {
            Text = text;
        }

        public UIMenuSubsectionItem(string text, string description) : base(text, description)
        {
            Text = text;
            Description = description;
        }

        public override void Draw(UIMenu sourceMenu)
        {
            if (sourceMenu.UIMenuItemList.IndexOf(this) == sourceMenu.SelectedIndex)
            {
                if (sourceMenu.IsHoldingUp())
                    sourceMenu.MoveUp();
                else
                    sourceMenu.MoveDown();

                sourceMenu.DrawCustomText(Text, sourceMenu.ItemTextFontSize, sourceMenu.ItemTextFontType,
                    sourceMenu.SubsectionDefaultTextColor.R, sourceMenu.SubsectionDefaultTextColor.G,
                    sourceMenu.SubsectionDefaultTextColor.B, sourceMenu.SubsectionDefaultTextColor.A,
                    sourceMenu.xPosBG, sourceMenu.yPosItem + sourceMenu.YPosBasedOnScroll * sourceMenu.posMultiplier,
                    UIMenu.TextJustification.Center); //Draw item text

                sourceMenu.DrawRectangle(sourceMenu.xPosBG,
                    sourceMenu.yPosItemBG + sourceMenu.YPosBasedOnScroll * sourceMenu.posMultiplier,
                    sourceMenu.MenuBGWidth, sourceMenu.heightItemBG, sourceMenu.SubsectionDefaultBoxColor.R,
                    sourceMenu.SubsectionDefaultBoxColor.G, sourceMenu.SubsectionDefaultBoxColor.B,
                    sourceMenu.SubsectionDefaultBoxColor.A); //Draw background rectangle around item.
            }
            else
            {
                sourceMenu.DrawCustomText(Text, sourceMenu.ItemTextFontSize, sourceMenu.ItemTextFontType,
                    sourceMenu.SubsectionDefaultTextColor.R, sourceMenu.SubsectionDefaultTextColor.G,
                    sourceMenu.SubsectionDefaultTextColor.B, sourceMenu.SubsectionDefaultTextColor.A,
                    sourceMenu.xPosBG, sourceMenu.yPosItem + sourceMenu.YPosBasedOnScroll * sourceMenu.posMultiplier,
                    UIMenu.TextJustification.Center); //Draw item text

                sourceMenu.DrawRectangle(sourceMenu.xPosBG,
                    sourceMenu.yPosItemBG + sourceMenu.YPosBasedOnScroll * sourceMenu.posMultiplier,
                    sourceMenu.MenuBGWidth, sourceMenu.heightItemBG, sourceMenu.SubsectionDefaultBoxColor.R,
                    sourceMenu.SubsectionDefaultBoxColor.G, sourceMenu.SubsectionDefaultBoxColor.B,
                    sourceMenu.SubsectionDefaultBoxColor.A); //Draw background rectangle around item.
            }
        }
    }

    public static class StringHelper
    {
        public static void AddLongString(string str)
        {
            const int strLen = 99;
            for (var i = 0; i < str.Length; i += strLen)
            {
                var substr = str.Substring(i, Math.Min(strLen, str.Length - i));
                Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, substr);
            }
        }

        public static float MeasureStringWidth(string str, Font font, float fontsize)
        {
            const float height = 1080f;
            var ratio = (float) Screen.Resolution.Width / Screen.Resolution.Height;
            var width = height * ratio;
            return MeasureStringWidthNoConvert(str, font, fontsize) * width;
        }

        private static float MeasureStringWidthNoConvert(string str, Font font, float fontsize)
        {
            Function.Call((Hash) 0x54CE8AC98E120CAB, "jamyfafi"); //_BEGIN_TEXT_COMMAND_WIDTH
            AddLongString(str);
            Function.Call(Hash.SET_TEXT_FONT, (int) font);
            Function.Call(Hash.SET_TEXT_SCALE, fontsize, fontsize);
            return Function.Call<float>(Hash.END_TEXT_COMMAND_GET_SCREEN_WIDTH_OF_DISPLAY_TEXT, true);
        }
    }

    public class BindedItem
    {
        public UIMenu BindedSubmenu { get; set; }

        public UIMenuItem BindedItemToSubmenu { get; set; }
    }

    internal static class UIInput
    {
        internal static DateTime InputTimer;
        internal static int InputWait = 80;
    }

    public static class StringExtensions
    {
        /// <summary>
        ///     Use this function like string.Split but instead of a character to split on,
        ///     use a maximum line width size. This is similar to a Word Wrap where no words will be split.
        /// </summary>
        /// Note if the a word is longer than the maxcharactes it will be trimmed from the start.
        /// <param name="initial">The string to parse.</param>
        /// <param name="MaxCharacters">The maximum size.</param>
        /// <remarks>This function will remove some white space at the end of a line, but allow for a blank line.</remarks>
        /// <returns>An array of strings.</returns>
        public static List<string> SplitOn(this string initial, int MaxCharacters)
        {
            var lines = new List<string>();

            if (string.IsNullOrEmpty(initial) == false)
            {
                var targetGroup = "Line";
                var pattern = string.Format(@"(?<{0}>.{{1,{1}}})(?:\W|$)", targetGroup, MaxCharacters);

                lines = Regex.Matches(initial, pattern, RegexOptions.Multiline | RegexOptions.CultureInvariant)
                    .OfType<Match>()
                    .Select(mt => mt.Groups[targetGroup].Value)
                    .ToList();
            }

            return lines;
        }
    }

    public static class EnumExtensions
    {
        public static T Next<T>(this T src) where T : struct
        {
            //if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argumnent {0} is not an Enum", typeof(T).FullName));

            var Arr = (T[]) Enum.GetValues(src.GetType());
            var j = Array.IndexOf(Arr, src) + 1;
            return Arr.Length == j ? Arr[0] : Arr[j];
        }

        public static T Previous<T>(this T src) where T : struct
        {
            //if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argumnent {0} is not an Enum", typeof(T).FullName));

            var Arr = (T[]) Enum.GetValues(src.GetType());
            var j = Array.IndexOf(Arr, src) - 1;
            return j < 0 ? Arr[Arr.Length - 1] : Arr[j];
        }
    }
}