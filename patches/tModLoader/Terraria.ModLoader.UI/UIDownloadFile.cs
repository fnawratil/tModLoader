using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	//TODO: downloads and web exceptions need logging
	//TODO: merge all progress/download UIs
	//TODO: of all the download UIs, this one has been refactored the best
	internal class UIDownloadFile : UIState
	{
		private Action _cancelAction;
		private WebClient _client;
		private Action _failure; //TODO unused?
		private string _file;
		private UILoadProgress _loadProgress;
		private string _name;
		private Action _success;
		private string _url;

		public override void OnActivate() {
			_loadProgress.SetText(Language.GetTextValue("tModLoader.MBDownloadingMod", _name));
			_loadProgress.SetProgress(0f);
			if (!UIModBrowser.PlatformSupportsTls12) {
				// Needed for downloads from Github
				Logging.tML.Warn("Detected the client's computer does not support TLS 1.2 which is required for downloading.");
				Interface.errorMessage.Show("TLS 1.2 not supported on this computer.", 0); // github releases
				return;
			}

			ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; // SecurityProtocolType.Tls12

			_client = new WebClient();
			ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, policyErrors) => true;
			SetCancel(_client.CancelAsync);
			_client.DownloadProgressChanged += Client_DownloadProgressChanged;
			_client.DownloadFileCompleted += Client_DownloadFileCompleted;
			_client.DownloadFileAsync(new Uri(_url), _file);
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

		internal void SetDownloading(string name, string url, string file, Action success) {
			_name = name;
			_url = url;
			_file = file;
			_success = success;
		}

		internal void SetProgress(DownloadProgressChangedEventArgs e) => SetProgress(e.BytesReceived, e.TotalBytesToReceive);

		internal void SetProgress(long count, long len) {
			_loadProgress?.SetProgress((float)count / len);
		}

		private void CancelClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			_cancelAction?.Invoke();
		}

		private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
			_client.Dispose();
			_client = null;

			if (e.Error == null && !e.Cancelled) {
				_success();
				return;
			}

			if (e.Cancelled) {
				Main.menuMode = 0;
			}
			else {
				// TODO: Think about what message to put here.
				var errorKey = GetHttpStatusCode(e.Error) == HttpStatusCode.ServiceUnavailable ? "MBExceededBandwidth" : "MBUnknownMBError";
				Interface.errorMessage.Show(Language.GetTextValue("tModLoader." + errorKey), 0);
			}

			if (File.Exists(_file))
				File.Delete(_file);
		}

		private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
			SetProgress(e);
		}

		private HttpStatusCode GetHttpStatusCode(Exception err) => err is WebException exc && exc.Response is HttpWebResponse response ? response.StatusCode : 0;
	}
}
