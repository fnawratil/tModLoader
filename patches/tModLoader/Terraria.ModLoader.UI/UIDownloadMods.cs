using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	//TODO: downloads and web exceptions need logging
	//TODO: merge all progress/download UIs
	internal class UIDownloadMods : UIState
	{
		private readonly List<string> _missingMods = new List<string>();
		private readonly Queue<UIModDownloadItem> _modsToDownload = new Queue<UIModDownloadItem>();
		private Action _cancelAction;
		private WebClient _client;
		private UIModDownloadItem _currentDownload;
		private UILoadProgress _loadProgress;
		private string _name;

		public override void OnActivate() {
			_loadProgress.SetText(Language.GetTextValue("tModLoader.MBDownloadingMod", _name + ": ???"));
			_loadProgress.SetProgress(0f);
			if (UIModBrowser.PlatformSupportsTls12) // Needed for downloads from Github
			{
				ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; // SecurityProtocolType.Tls12
			}

			if (_modsToDownload != null && _modsToDownload.Count > 0) {
				_client = new WebClient();
				ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, policyErrors) => true;
				SetCancel(_client.CancelAsync);
				_client.DownloadProgressChanged += Client_DownloadProgressChanged;
				_client.DownloadFileCompleted += Client_DownloadFileCompleted;
				_currentDownload = _modsToDownload.Dequeue();
				_loadProgress.SetText(Language.GetTextValue("tModLoader.MBDownloadingMod", $"{_name}: {_currentDownload.displayName}"));
				_client.DownloadFileAsync(new Uri(_currentDownload.download), ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod");
			}
			else {
				Interface.modBrowser.ClearItems();
				Main.menuMode = Interface.modBrowserID;
				if (_missingMods.Count > 0) {
					Interface.infoMessage.Show(Language.GetTextValue("tModLoader.MBModsNotFoundOnline", string.Join(",", _missingMods)), Interface.modBrowserID);
				}
			}
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

			var cancel = new UITextPanel<string>(Language.GetTextValue("UI.Cancel"), 0.75f, true) {
				VAlign = 0.5f,
				HAlign = 0.5f,
				Top = { Pixels = 170 }
			}.WithFadedMouseOver();
			cancel.OnClick += CancelClick;
			Append(cancel);
		}

		public void SetCancel(Action cancelAction) {
			_cancelAction = cancelAction;
		}

		internal void SetDownloading(string name) {
			_name = name;
		}

		internal void SetModsToDownload(List<string> specialModPackFilter, List<UIModDownloadItem> items) {
			_modsToDownload.Clear();
			_missingMods.Clear();
			foreach (var desiredMod in specialModPackFilter) {
				var mod = items.FirstOrDefault(x => x.mod == desiredMod);
				if (mod == null) {
					_missingMods.Add(desiredMod);
				}
				else {
					if (mod.installed != null && !mod.update) {
						// skip mods that are already installed and don't have an update
					}
					else {
						_modsToDownload.Enqueue(mod);
					}
				}
			}
		}

		internal void SetProgress(DownloadProgressChangedEventArgs e) => SetProgress(e.BytesReceived, e.TotalBytesToReceive);

		internal void SetProgress(long count, long len) {
			//loadProgress?.SetText("Downloading: " + name + " -- " + count+"/" + len);
			_loadProgress?.SetProgress((float)count / len);
		}

		private void CancelClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			_cancelAction?.Invoke();
		}

		//public override void Update(GameTime gameTime)
		//{
		//	if (modsToDownload == null || modsToDownload.Count == 0)
		//		Main.menuMode = Interface.modBrowserID;
		//}

		private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
			//Main.menuMode = Interface.modBrowserID;
			if (e.Error != null) {
				if (e.Cancelled) {
					Interface.modBrowser.ClearItems();
					Main.menuMode = Interface.modBrowserID;
				}
				else {
					var errorKey = GetHttpStatusCode(e.Error) == HttpStatusCode.ServiceUnavailable ? "MBExceededBandwidth" : "MBUnknownMBError";
					Interface.errorMessage.Show(Language.GetTextValue("tModLoader." + errorKey), 0);
				}

				File.Delete(ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod");
			}
			else if (!e.Cancelled) {
				// Downloaded OK
				var mod = ModLoader.GetMod(_currentDownload.mod);
				if (mod != null) {
					mod.File?.Close(); // if the mod is currently loaded, the file-handle needs to be released
					Interface.modBrowser.anEnabledModDownloaded = true;
				}

				//string destinationFileName = ModLoader.GetMod(currentDownload.mod) == null ? currentDownload.mod + ".tmod" : currentDownload.mod + ".tmod.update"; // if above fix has issues we can use this.
				File.Copy(ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod", ModLoader.ModPath + Path.DirectorySeparatorChar + _currentDownload.mod + ".tmod", true);
				File.Delete(ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod");
				if (!_currentDownload.update) {
					Interface.modBrowser.aNewModDownloaded = true;
				}
				else {
					Interface.modBrowser.aModUpdated = true;
				}

				if (ModLoader.autoReloadAndEnableModsLeavingModBrowser) {
					ModLoader.EnableMod(_currentDownload.mod);
				}

				// Start next download
				if (_modsToDownload.Count != 0) {
					_currentDownload = _modsToDownload.Dequeue();
					_loadProgress.SetText(Language.GetTextValue("tModLoader.MBDownloadingMod", $"{_name}: {_currentDownload.displayName}"));
					_loadProgress.SetProgress(0f);
					_client.DownloadFileAsync(new Uri(_currentDownload.download), ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod");
				}
				else {
					_client.Dispose();
					_client = null;
					Interface.modBrowser.ClearItems();
					Main.menuMode = Interface.modBrowserID;
					if (_missingMods.Count > 0) {
						Interface.infoMessage.Show(Language.GetTextValue("tModLoader.MBModsNotFoundOnline", string.Join(",", _missingMods)), Interface.modsMenuID);
					}
				}
			}
			else {
				File.Delete(ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod");
			}
		}

		private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
			SetProgress(e);
		}

		private HttpStatusCode GetHttpStatusCode(Exception err) {
			if (err is WebException we && we.Response is HttpWebResponse response) {
				return response.StatusCode;
			}

			return 0;
		}
	}
}
