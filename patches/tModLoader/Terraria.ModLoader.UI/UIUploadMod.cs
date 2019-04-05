using System;
using System.Net;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIUploadMod : UIState
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
			_loadProgress.SetText("Uploading: " + _name);
			_loadProgress.SetProgress(0f);
		}

		internal void SetDownloading(string name) {
			_name = name;
		}

		public void SetCancel(Action cancelAction) {
			_cancelAction = cancelAction;
		}

		internal void SetProgress(UploadProgressChangedEventArgs e) => SetProgress(e.BytesSent, e.TotalBytesToSend);

		internal void SetProgress(long count, long len) {
			_loadProgress?.SetProgress((float)count / len);
		}

		private void CancelClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(ID.SoundID.MenuOpen);
			_cancelAction();
		}
	}
}
