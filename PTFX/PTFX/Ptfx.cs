using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using NativeUI;

namespace PTFX
{
    /// <summary>
    ///     Defines the <see cref="Ptfx" />
    /// </summary>
    public class Ptfx : Script
    {
        /// <summary>
        ///     Defines the file
        /// </summary>
        public string file;

        /// <summary>
        ///     Defines the fileName
        /// </summary>
        public string fileName;

        /// <summary>
        ///     Defines the modmenu
        /// </summary>
        public ModMenu modmenu;

        /// <summary>
        ///     Defines the updateAvailable
        /// </summary>
        public bool updateAvailable;

        /// <summary>
        ///     Defines the updateItem
        /// </summary>
        public UIMenuItem updateItem;

        /// <summary>
        ///     Defines the uri
        /// </summary>
        public string uri;

        /// <summary>
        ///     Defines the webFile
        /// </summary>
        public string webFile;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Ptfx" /> class.
        /// </summary>
        public Ptfx()
        {
            Tick += OnTick;
            KeyDown += OnKeyDown;


            uri = "https://raw.githubusercontent.com/nitanmarcel/GTA-V-Particle-Effects/master/PTFX/PTFX/particles.ini";
            fileName = @"scripts/particles.ini";

            updateAvailable = false;

            file = File.ReadAllText(fileName);

            try
            {
                var wc = new WebClient();
                webFile = wc.DownloadString(uri);
                if (file.CompareTo(webFile) > 0) updateAvailable = true;
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

                updateItem = modmenu.AddMenuItem("Update Database", "Updates the ptfx database from github.",
                    updateMenu);
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
        ///     The OnTick
        /// </summary>
        /// <param name="sender">The sender<see cref="object" /></param>
        /// <param name="e">The e<see cref="EventArgs" /></param>
        public void OnTick(object sender, EventArgs e)
        {
            modmenu.menuPool.ProcessMenus();
            if (updateAvailable)
            {
                GTA.UI.Notification.Show("New database update available", true);
                updateAvailable = false;
            }
        }

        /// <summary>
        ///     The OnKeyDown
        /// </summary>
        /// <param name="sender">The sender<see cref="object" /></param>
        /// <param name="e">The e<see cref="System.Windows.Forms.KeyEventArgs" /></param>
        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F9) modmenu.SwitchMenu();
        }

        /// <summary>
        ///     The UpdateItemSelectHandler
        /// </summary>
        /// <param name="sender">The sender<see cref="UIMenu" /></param>
        /// <param name="selectedItem">The selectedItem<see cref="UIMenuItem" /></param>
        /// <param name="index">The index<see cref="int" /></param>
        public void UpdateItemSelectHandler(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            File.WriteAllText(fileName, webFile);
            modmenu.mainMenu.RemoveItemAt(0);
            modmenu.menuPool.CloseAllMenus();
            modmenu.mainMenu.Visible = false;

            GTA.UI.Notification.Show(
                "Database updated, restart the game or open the console (F4) and write Reload() then press enter");
        }

        /// <summary>
        ///     The ItemSelectHandler
        /// </summary>
        /// <param name="sender">The sender<see cref="UIMenu" /></param>
        /// <param name="selectedItem">The selectedItem<see cref="UIMenuItem" /></param>
        /// <param name="index">The index<see cref="int" /></param>
        public void ItemSelectHandler(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            ParticleEffectAsset asset = new ParticleEffectAsset(selectedItem.Description);
            asset.Request();
            while (!asset.IsLoaded)
            {
                Wait((0));
            }
            Vector3 pos = Game.Player.Character.FrontPosition - new Vector3(2, 0, 0);
            World.CreateParticleEffectNonLooped(asset, selectedItem.Text, pos);
        }

        /// <summary>
        ///     The IniToDictionary
        /// </summary>
        /// <param name="lines">The lines<see cref="IEnumerable{string}" /></param>
        /// <returns>The <see cref="Dictionary{string, List{string}}" /></returns>
        private static Dictionary<string, List<string>> IniToDictionary(IEnumerable<string> lines)
        {
            var result =
                new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            var category = "";

            foreach (var line in lines)
            {
                var record = line.Trim();

                if (string.IsNullOrEmpty(record) || record.StartsWith("#"))
                {
                }
                else if (record.StartsWith("[") && record.EndsWith("]"))
                {
                    category = record.Substring(1, record.Length - 2);
                }
                else
                {
                    var index = record.IndexOf('=');

                    var name = index > 0 ? record.Substring(0, index) : record;

                    if (result.TryGetValue(category, out var list))
                        list.Add(name);
                    else
                        result.Add(category, new List<string> {name});
                }
            }

            return result;
        }
    }
}