using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using GTA;
using GTA.Math;
using GTA.Native;
using NativeUI;

namespace PTFX
{
	public class Ptfx : Script
	{
		ModMenu modmenu;
		public Ptfx()
		{
			this.Tick += OnTick;
			this.KeyDown += OnKeyDown;

			string uri = "https://raw.githubusercontent.com/nitanmarcel/GTA-V-Particle-Effects/master/particles.ini";
			IEnumerable<string> file;
			//	System.Ge

			try
			{
				System.Net.WebClient wc = new System.Net.WebClient();
				file = wc.DownloadString(uri).Split(Environment.NewLine.ToCharArray());
			}
			catch
			{
				file = File.ReadLines(@"scripts/particles.ini");
			}

			modmenu = new ModMenu("Particle Effects", "Particle effects Test");
			var result = IniToDictionary(file);
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

		public void OnTick(object sender, EventArgs e)
		{
			modmenu.menuPool.ProcessMenus();
		}

		public void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F5)
			{
				modmenu.SwitchMenu();
			}
		}

		public void ItemSelectHandler(UIMenu sender, UIMenuItem selectedItem, int index)
		{
			Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, selectedItem.Description);
			Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, selectedItem.Description);
			Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, selectedItem.Description);

			Vector3 pos = Game.Player.Character.GetOffsetInWorldCoords(new Vector3(0, 2, 0));
			Vector3 rot = default;

			Function.Call(Hash._START_PARTICLE_FX_AT_COORD, selectedItem.Text, pos.X, pos.Y, pos.Z, rot.X, rot.Y, rot.Z, 1.0f, false, false, false);

		}

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
