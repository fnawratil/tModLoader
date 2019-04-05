using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Ionic.Zip;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIDeveloperModeHelp : UIState
	{
		private bool _allChecksSatisfied;
		private UIPanel _backPanel;
		private UITextPanel<string> _bottomButton;
		private UIImage _refAssemDirectDlButton;

		public override void OnActivate() {
			_backPanel.RemoveAllChildren();

			int i = 0;

			UIMessageBox AddMessageBox(string text) {
				var msgBox = new UIMessageBox(text) {
					Width = { Percent = 1f },
					Height = { Percent = .2f },
					Top = { Percent = i++ / 4f + 0.05f }
				};
				_backPanel.Append(msgBox);
				return msgBox;
			}

			void AddButton(UIElement elem, string text, Action clickAction) {
				var button = new UITextPanel<string>(text) {
					Top = { Pixels = -2 },
					Left = { Pixels = -2 },
					HAlign = 1,
					VAlign = 1
				}.WithFadedMouseOver();
				button.OnClick += (evt, _) => clickAction();
				elem.Append(button);
			}

			bool dotNetCheck = ModCompile.DotNet46Check(out string dotNetMsg);
			var dotNetMsgBox = AddMessageBox(dotNetMsg);
			if (!dotNetCheck)
				AddButton(dotNetMsgBox, Language.GetTextValue("tModLoader.MBDownload"), DownloadDotNet);

			bool modCompileCheck = ModCompile.ModCompileVersionCheck(out string modCompileMsg);
			var modCompileMsgBox = AddMessageBox(Language.GetTextValue(modCompileMsg));
#if !DEBUG
			if (!modCompileCheck)
				AddButton(modCompileMsgBox, Language.GetTextValue("tModLoader.MBDownload"), DownloadModCompile);
#endif

			bool refAssemCheck = ModCompile.ReferenceAssembliesCheck(out string refAssemMsg);
			var refAssemMsgBox = AddMessageBox(Language.GetTextValue(refAssemMsg));
			if (!refAssemCheck) {
				AddButton(refAssemMsgBox, Language.GetTextValue("tModLoader.DMVisualStudio"), DevelopingWithVisualStudio);

				var icon = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.UI.ButtonExclamation.png"));
				_refAssemDirectDlButton = new UIHoverImage(icon, Language.GetTextValue("tModLoader.DMReferenceAssembliesDownload")) {
					Left = { Pixels = -1 },
					Top = { Pixels = -1 },
					VAlign = 1
				};
				_refAssemDirectDlButton.OnClick += (evt, _) => DirectDownloadRefAssemblies();
				refAssemMsgBox.Append(_refAssemDirectDlButton);
			}

			var tutorialMsgBox = AddMessageBox(Language.GetTextValue("tModLoader.DMTutorialWelcome"));
			AddButton(tutorialMsgBox, Language.GetTextValue("tModLoader.DMTutorial"), OpenTutorial);

			_allChecksSatisfied = dotNetCheck && modCompileCheck && refAssemCheck;
			_bottomButton.SetText(_allChecksSatisfied ? Language.GetTextValue("tModLoader.Continue") : Language.GetTextValue("UI.Back"));
		}

		public override void OnInitialize() {
			var area = new UIElement {
				Width = { Percent = 0.5f },
				Top = { Pixels = 200 },
				Height = { Pixels = -240, Percent = 1f },
				HAlign = 0.5f
			};

			_backPanel = new UIPanel {
				Width = { Percent = 1f },
				Height = { Pixels = -90, Percent = 1f },
				BackgroundColor = UICommon.MAIN_PANEL_BG_COLOR
			};
			area.Append(_backPanel);

			var heading = new UITextPanel<string>(Language.GetTextValue("tModLoader.MenuEnableDeveloperMode"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -45 },
				BackgroundColor = UICommon.UI_BLUE_COLOR
			}.WithPadding(15);
			area.Append(heading);

			_bottomButton = new UITextPanel<string>(Language.GetTextValue("UI.Back"), 0.7f, true) {
				Width = { Percent = 0.5f },
				Height = { Pixels = 50 },
				HAlign = 0.5f,
				VAlign = 1f,
				Top = { Pixels = -30 }
			}.WithFadedMouseOver();
			_bottomButton.OnClick += BackClick;
			area.Append(_bottomButton);

			Append(area);
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			if (_allChecksSatisfied) {
				Main.PlaySound(SoundID.MenuOpen);
				Main.menuMode = Interface.modSourcesID;
			}
			else {
				Main.PlaySound(SoundID.MenuClose);
				Main.menuMode = 0;
			}
		}

		private void DevelopingWithVisualStudio() {
			Process.Start("https://github.com/blushiemagic/tModLoader/wiki/Developing-with-Visual-Studio");
		}

		private void DirectDownloadRefAssemblies() {
			Main.PlaySound(SoundID.MenuOpen);
			const string url = "https://tmodloader.net/dl/ext/v45ReferenceAssemblies.zip"; // This never changes, maybe put it on 0.11 release only and leave it out of other release uploads.
			string folder = Path.Combine(ModCompile.modCompileDir, "v4.5 Reference Assemblies");
			string file = Path.Combine(folder, "v4.5 Reference Assemblies.zip");
			Directory.CreateDirectory(folder);
			DownloadFile("v4.5 Reference Assemblies", url, file, () => Extract(file));
		}

		private void DownloadDotNet() {
			Process.Start("https://www.microsoft.com/net/download/thank-you/net472");
		}

		private void DownloadFile(string name, string url, string file, Action downloadModCompileComplete) {
			Interface.downloadFile.SetDownloading(name, url, file, downloadModCompileComplete);
			Main.menuMode = Interface.downloadFileID;
		}

		private void DownloadModCompile() {
			Main.PlaySound(SoundID.MenuOpen);
			string url = $"https://github.com/blushiemagic/tModLoader/releases/download/{ModLoader.versionTag}/ModCompile_{(ModLoader.windows ? "Windows" : "Mono")}.zip";
			string file = Path.Combine(ModCompile.modCompileDir, $"ModCompile_{ModLoader.versionedName}.zip");
			Directory.CreateDirectory(ModCompile.modCompileDir);
			DownloadFile("ModCompile", url, file, () => {
				Extract(file);
				var currentExeFilename = Process.GetCurrentProcess().ProcessName;
				string originalXmlFile = Path.Combine(ModCompile.modCompileDir, "Terraria.xml");
				// TODO can throw exception, not caught
				string correctXmlFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"{currentExeFilename}.xml");
				File.Copy(originalXmlFile, correctXmlFile, true);
				File.Delete(originalXmlFile);
			});
		}

		private void Extract(string zipFile, bool deleteFiles = false) {
			string folder = Path.GetDirectoryName(zipFile);
			Directory.CreateDirectory(folder);
			if (deleteFiles) {
				foreach (FileInfo file in new DirectoryInfo(folder).EnumerateFiles()) {
					if (file.Name != Path.GetFileName(zipFile))
						file.Delete();
				}
			}

			using (var zip = ZipFile.Read(zipFile))
				zip.ExtractAll(folder, ExtractExistingFileAction.OverwriteSilently);

			File.Delete(zipFile);
			Main.menuMode = Interface.developerModeHelpID;
		}

		private void OpenTutorial() {
			Process.Start("https://github.com/blushiemagic/tModLoader/wiki/Basic-tModLoader-Modding-Guide");
		}
	}
}
