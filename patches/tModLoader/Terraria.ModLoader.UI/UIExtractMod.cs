using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	// TODO: yet another progress bar
	internal class UIExtractMod : UIState
	{
		private static readonly IList<string> CodeExtensions = new List<string>(ModCompile.sourceExtensions) { ".dll", ".pdb" };
		private int _gotoMenu;

		private UILoadProgress _loadProgress;
		private LocalMod _mod;

		public override void OnActivate() {
			Main.menuMode = Interface.extractModID;

			// Expected that this will move out of Activate during progress UI merger
			var task = Task.Factory.StartNew(() => {
				Interface.extractMod._Extract(); // Interface.extractMod is just `this`
			});

			task.ContinueWith(t => { Logging.tML.Error(Language.GetTextValue("tModLoader.ExtractErrorWhileExtractingMod", _mod.Name), t.Exception); },
							  CancellationToken.None,
							  TaskContinuationOptions.OnlyOnFaulted,
							  TaskScheduler.FromCurrentSynchronizationContext());

			task.ContinueWith(t => { Main.menuMode = _gotoMenu; },
							  CancellationToken.None,
							  TaskContinuationOptions.OnlyOnRanToCompletion,
							  TaskScheduler.FromCurrentSynchronizationContext());
		}

		public override void OnInitialize() {
			_loadProgress = new UILoadProgress {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MAX_PANEL_WIDTH,
				Height = { Pixels = 150 },
				HAlign = 0.5f,
				VAlign = 0.5f,
				Top = { Pixels = 10 }
			};
			Append(_loadProgress);
		}

		internal void Show(LocalMod mod, int gotoMenu) {
			_mod = mod;
			_gotoMenu = gotoMenu;
			Main.menuMode = Interface.extractModID;
		}

		private Exception _Extract() {
			StreamWriter log = null;
			IDisposable modHandle = null;
			try {
				var dir = Path.Combine(Main.SavePath, "Mod Reader", _mod.Name);
				if (Directory.Exists(dir))
					Directory.Delete(dir, true);
				Directory.CreateDirectory(dir);

				log = new StreamWriter(Path.Combine(dir, "tModReader.txt")) { AutoFlush = true };

				if (_mod.properties.hideCode)
					log.WriteLine(Language.GetTextValue("tModLoader.ExtractHideCodeMessage"));
				else if (!_mod.properties.includeSource)
					log.WriteLine(Language.GetTextValue("tModLoader.ExtractNoSourceCodeMessage"));
				if (_mod.properties.hideResources)
					log.WriteLine(Language.GetTextValue("tModLoader.ExtractHideResourcesMessage"));

				log.WriteLine(Language.GetTextValue("tModLoader.ExtractFileListing"));

				int i = 0;
				modHandle = _mod.modFile.EnsureOpen();
				foreach (var entry in _mod.modFile) {
					var name = entry.Name;
					ContentConverters.Reverse(ref name, out var converter);

					//this access is not threadsafe, but it should be atomic enough to not cause issues
					_loadProgress.SetText(name);
					_loadProgress.SetProgress(i++ / (float)_mod.modFile.Count);

					if (name == "tModReader.txt")
						continue;

					bool hidden = CodeExtensions.Contains(Path.GetExtension(name))
									  ? _mod.properties.hideCode
									  : _mod.properties.hideResources;

					if (hidden)
						log.Write("[hidden] ");
					log.WriteLine(name);
					if (hidden)
						continue;

					var path = Path.Combine(dir, name);
					Directory.CreateDirectory(Path.GetDirectoryName(path));

					using (var dst = File.OpenWrite(path))
					using (var src = _mod.modFile.GetStream(entry)) {
						if (converter != null)
							converter(src, dst);
						else
							src.CopyTo(dst);
					}
				}
			}
			catch (Exception e) {
				log?.WriteLine(e);
				return e;
			}
			finally {
				log?.Close();
				modHandle?.Dispose();
			}

			return null;
		}
	}
}
