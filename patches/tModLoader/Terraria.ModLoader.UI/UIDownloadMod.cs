using System;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	//TODO: yet another progress UI, this time with cancel button
	internal class UIDownloadMod : UIState
	{
		private UILoadProgress _loadProgress;
		private string _name;
		private Action _cancelAction;

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

		public override void OnActivate() {
			_loadProgress.SetText(Language.GetTextValue("tModLoader.MBDownloadingMod", _name));
			_loadProgress.SetProgress(0f);
		}

		internal void SetDownloading(string name) {
			Logging.tML.InfoFormat("Downloading Mod: {0}", name);
			_name = name;
		}

		public void SetCancel(Action cancelAction) {
			_cancelAction = cancelAction;
		}

		internal void SetProgress(long count, long len) {
			//loadProgress?.SetText("Downloading: " + name + " -- " + count+"/" + len);
			_loadProgress?.SetProgress((float)count / len);
		}

		private void CancelClick(UIMouseEvent evt, UIElement listeningElement) {
			Logging.tML.InfoFormat("Download Cancelled");
			Main.PlaySound(10);
			_cancelAction();
		}
	}
}
