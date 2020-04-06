using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using GTA;
using GTA.UI;
using PTFX.Menu;
using Screen = GTA.UI.Screen;

namespace PTFX
{
    internal class Ptfx : Script
    {
        public string dataDownloaded;
        public string dataPath = @"scripts/particleEffectsCompact.json";

        public string dataSource =
            "https://raw.githubusercontent.com/DurtyFree/gta-v-data-dumps/master/particleEffectsCompact.json";

        public UIMenu MainMenu;
        public MenuPool MenuPool;

        public float particlesSize = 1.0f;

        public bool updateAvailable;


        public Ptfx()
        {
            MenuPool = new MenuPool();
            updateAvailable = CheckForUpdate();
            UpdateMenu();

            Tick += (sender, e) =>
            {
                if (updateAvailable)
                {
                    Notification.Show("New PTFX Database update available", true);
                    updateAvailable = false;
                }

                if (MenuPool != null)
                {
                    MenuPool.ProcessMenus();
                    if (!MenuPool.IsAnyMenuOpen() && !Game.Player.Character.IsVisible)
                        Game.Player.Character.IsVisible = true;
                }
            };

            KeyDown += (sender, e) =>
            {
                if (MenuPool != null && e.KeyCode == Keys.F9) MenuPool.OpenCloseLastMenu();
            };
        }

        public bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (client.OpenRead("http://google.com/generate_204"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public bool CheckForUpdate()
        {
            if (CheckForInternetConnection())
            {
                var exists = File.Exists(dataPath);

                using (var client = new WebClient())
                {
                    dataDownloaded = client.DownloadString(dataSource);
                }

                if (!exists) WriteData();

                return string.Compare(File.ReadAllText(dataPath), dataDownloaded, StringComparison.Ordinal) > 0;
            }

            return false;
        }

        public void WriteData()
        {
            File.WriteAllText(dataPath, dataDownloaded);
        }

        public void UpdateMenu()
        {
            if (MenuPool != null) MenuPool.RemoveAllMenus();

            MenuPool = new MenuPool();
            MainMenu = new UIMenu("Ptfx Menu");

            if (updateAvailable)
            {
                var item = new UIMenuItem("Update PTFX Database", "Database Update Available");
                MainMenu.AddMenuItem(item);
                MainMenu.OnItemSelect += (sender, selectedItem, index) =>
                {
                    if (selectedItem.Text == "Update PTFX Database")
                    {
                        WriteData();
                        UpdateMenu();
                        Notification.Show("PTFX Database Updated");
                    }
                };
            }

            MainMenu.OnMenuOpen += sender => { Game.Player.Character.IsVisible = false; };

            MenuPool.AddMenu(MainMenu);

            MainMenu.TitleColor = Color.FromArgb(255, 255, 255, 255);
            MainMenu.TitleBackgroundColor = Color.FromArgb(240, 0, 0, 0);
            MainMenu.TitleUnderlineColor = Color.FromArgb(255, 255, 90, 90);
            MainMenu.DefaultBoxColor = Color.FromArgb(160, 0, 0, 0);
            MainMenu.DefaultTextColor = Color.FromArgb(230, 255, 255, 255);
            MainMenu.HighlightedBoxColor = Color.FromArgb(130, 237, 90, 90);
            MainMenu.HighlightedItemTextColor = Color.FromArgb(255, 255, 255, 255);
            MainMenu.DescriptionBoxColor = Color.FromArgb(255, 0, 0, 0);
            MainMenu.DescriptionTextColor = Color.FromArgb(255, 255, 255, 255);
            MainMenu.SubsectionDefaultBoxColor = Color.FromArgb(160, 0, 0, 0);
            MainMenu.SubsectionDefaultTextColor = Color.FromArgb(180, 255, 255, 255);

            MenuPool.SubmenuItemIndication = "  ~r~>";

            var serializer = new JavaScriptSerializer();
            var json = File.ReadAllText(dataPath);
            dynamic res = serializer.DeserializeObject(json);

            foreach (var i in res)
            {
                var subMenu = new UIMenu(i["DictionaryName"]);
                var effectNames = ((IEnumerable) i["EffectNames"]).Cast<string>().ToList();
                foreach (var e in effectNames)
                {
                    var item = new UIMenuItem(e, i["DictionaryName"]);
                    subMenu.AddMenuItem(item);

                    subMenu.OnItemSelect += (sender, selectedItem, index) =>
                    {
                        World.RemoveAllParticleEffectsInRange(Game.Player.Character.Position, 10);
                        Game.Player.Character.RemoveParticleEffects();

                        var asset = new ParticleEffectAsset(selectedItem.Description);
                        asset.Request();

                        while (!asset.IsLoaded) Wait(0);

                        var particle = World.CreateParticleEffectNonLooped(asset, selectedItem.Text,
                            Game.Player.Character.Position, default, particlesSize);

                        if (!particle)
                        {
                            var particle2 = World.CreateParticleEffect(asset, selectedItem.Text,
                                Game.Player.Character.Position, default, particlesSize);
                        }

                        asset.MarkAsNoLongerNeeded();
                        Screen.ShowSubtitle(selectedItem.Description + "@" + selectedItem.Text + particlesSize);
                    };

                    subMenu.OnItemLeftRight += (sender, selectedItem, index, direction) =>
                    {
                        if (direction == UIMenu.Direction.Right) particlesSize += 0.5f;
                        else if (particlesSize != 1.0f) particlesSize -= 0.5f;

                        Screen.ShowSubtitle("Particle size = " + particlesSize);
                    };
                }

                MenuPool.AddSubMenu(subMenu, MainMenu, i["DictionaryName"]);
            }
        }
    }
}