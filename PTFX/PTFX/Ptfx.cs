namespace PTFX
{
    using GTA;
    using GTA.Math;
    using GTA.Native;
    using NativeUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;

    /// <summary>
    /// Defines the <see cref="Ptfx" />
    /// </summary>
    public class Ptfx : Script
    {
        /// <summary>
        /// Defines the modmenu
        /// </summary>
        public ModMenu modmenu;

        /// <summary>
        /// Defines the updateItem
        /// </summary>
        public UIMenuItem updateItem;

        /// <summary>
        /// Defines the file
        /// </summary>
        public string file;

        /// <summary>
        /// Defines the fileName
        /// </summary>
        public string fileName;

        /// <summary>
        /// Defines the webFile
        /// </summary>
        public string webFile;

        /// <summary>
        /// Defines the uri
        /// </summary>
        public string uri;

        /// <summary>
        /// Defines the updateAvailable
        /// </summary>
        public bool updateAvailable;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ptfx"/> class.
        /// </summary>
        public Ptfx()
        {

            this.Tick += OnTick;
            this.KeyDown += OnKeyDown;

            uri = "https://raw.githubusercontent.com/nitanmarcel/GTA-V-Particle-Effects/master/PTFX/PTFX/particles.ini";
            fileName = @"scripts/particles.ini";

            updateAvailable = false;

            file = File.ReadAllText(fileName);

            try
            {

                System.Net.WebClient wc = new System.Net.WebClient();
                webFile = wc.DownloadString(uri);
                if (file.CompareTo(webFile) > 0)
                {
                    updateAvailable = true;
                }

            }
            catch
            {
                updateAvailable = false;
            }

            var particlesDB = file.Split(Environment.NewLine.ToCharArray());


            modmenu = new ModMenu("Particle Effects", "Particle effects Test");
            if (updateAvailable)
            {
                var updateMenu = modmenu.NewMenu("Update Available!");

                updateMenu.OnItemSelect += UpdateItemSelectHandler;

                updateItem = modmenu.AddMenuItem("Update Databalse", "Updates the ptfx database from github.", updateMenu);
            }

            var result = IniToDictionary(particlesDB);
            foreach (var entry in result)
            {
                var menu = modmenu.NewMenu(entry.Key);
                entry.Value.Sort();
                foreach (var v in entry.Value)
                {

                    var item = modmenu.AddMenuItem(v, entry.Key, menu);
                }
                menu.OnItemSelect += ItemSelectHandler;
            }
        }

        /// <summary>
        /// The OnTick
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="EventArgs"/></param>
        public void OnTick(object sender, EventArgs e)
        {
            modmenu.menuPool.ProcessMenus();
            if (updateAvailable)
            {
                UI.Notify("New database update available", true);
                updateAvailable = false;
            }
        }

        /// <summary>
        /// The OnKeyDown
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="System.Windows.Forms.KeyEventArgs"/></param>
        public void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F9)
            {
                modmenu.SwitchMenu();
            }
        }

        /// <summary>
        /// The UpdateItemSelectHandler
        /// </summary>
        /// <param name="sender">The sender<see cref="UIMenu"/></param>
        /// <param name="selectedItem">The selectedItem<see cref="UIMenuItem"/></param>
        /// <param name="index">The index<see cref="int"/></param>
        public void UpdateItemSelectHandler(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            File.WriteAllText(fileName, webFile);
            modmenu.mainMenu.RemoveItemAt(0);
            modmenu.menuPool.CloseAllMenus();
            modmenu.mainMenu.Visible = false;

            UI.Notify("Database updated, restart the game or open the console (F4) and write Reload() then press enter");
        }

        /// <summary>
        /// The ItemSelectHandler
        /// </summary>
        /// <param name="sender">The sender<see cref="UIMenu"/></param>
        /// <param name="selectedItem">The selectedItem<see cref="UIMenuItem"/></param>
        /// <param name="index">The index<see cref="int"/></param>
        public void ItemSelectHandler(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, selectedItem.Description);
            Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, selectedItem.Description);
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, selectedItem.Description);

            Vector3 pos = Game.Player.Character.GetOffsetInWorldCoords(new Vector3(0, 2, 0));
            Vector3 rot = default;

            Function.Call(Hash._START_PARTICLE_FX_AT_COORD, selectedItem.Text, pos.X, pos.Y, pos.Z, rot.X, rot.Y, rot.Z, 1.0f, false, false, false);
        }

        /// <summary>
        /// The IniToDictionary
        /// </summary>
        /// <param name="lines">The lines<see cref="IEnumerable{string}"/></param>
        /// <returns>The <see cref="Dictionary{string, List{string}}"/></returns>
        private static Dictionary<string, List<string>> IniToDictionary(IEnumerable<string> lines)
        {
            Dictionary<string, List<string>> result =
              new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            string category = "";

            foreach (string line in lines)
            {
                string record = line.Trim();

                if (string.IsNullOrEmpty(record) || record.StartsWith("#"))
                    continue;
                else if (record.StartsWith("[") && record.EndsWith("]"))
                    category = record.Substring(1, record.Length - 2);
                else
                {
                    int index = record.IndexOf('=');

                    string name = index > 0 ? record.Substring(0, index) : record;

                    if (result.TryGetValue(category, out List<string> list))
                        list.Add(name);
                    else
                        result.Add(category, new List<string>() { name });
                }
            }

            return result;
        }
    }
}
