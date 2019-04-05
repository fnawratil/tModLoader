using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIErrorMessage : UIState
	{
		private UIElement _area;
		private UITextPanel<string> _continueButton; // label changes to retry/exit
		private UITextPanel<string> _exitAndDisableAllButton;
		private int _gotoMenu;
		private string _message;
		private UIMessageBox _messageBox;
		private bool _showRetry;
		private bool _showSkip;
		private UITextPanel<string> _skipLoadButton;
		private UITextPanel<string> _webHelpButton;
		private string _webHelpUrl;

		public override void OnActivate() {
			Netplay.disconnect = true;

			_messageBox.SetText(_message);

			var continueKey = _gotoMenu < 0 ? "Exit" : _showRetry ? "Retry" : "Continue";
			_continueButton.SetText(Language.GetTextValue("tModLoader." + continueKey));
			_continueButton.TextColor = _gotoMenu >= 0 ? Color.White : Color.Red;

			_area.AddOrRemoveChild(_webHelpButton, string.IsNullOrEmpty(_webHelpUrl));
			_area.AddOrRemoveChild(_skipLoadButton, _showSkip);
			_area.AddOrRemoveChild(_exitAndDisableAllButton, _gotoMenu < 0);
		}

		public override void OnInitialize() {
			_area = new UIElement {
				Width = { Percent = 0.8f },
				Top = { Pixels = 200 },
				Height = { Pixels = -210, Percent = 1f },
				HAlign = 0.5f
			};

			_messageBox = new UIMessageBox("") {
				Width = { Percent = 1f },
				Height = { Pixels = -110, Percent = 1f },
				HAlign = 0.5f
			};
			_area.Append(_messageBox);

			_continueButton = new UITextPanel<string>("", 0.7f, true) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 50 },
				Top = { Pixels = -108, Percent = 1f }
			};
			_continueButton.WithFadedMouseOver();
			_continueButton.OnClick += ContinueClick;
			_area.Append(_continueButton);

			var openLogsButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.OpenLogs"), 0.7f, true);
			openLogsButton.CopyStyle(_continueButton);
			openLogsButton.HAlign = 1f;
			openLogsButton.WithFadedMouseOver();
			openLogsButton.OnClick += OpenFile;
			_area.Append(openLogsButton);

			_webHelpButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.OpenWebHelp"), 0.7f, true);
			_webHelpButton.CopyStyle(openLogsButton);
			_webHelpButton.Top.Set(-55f, 1f);
			_webHelpButton.WithFadedMouseOver();
			_webHelpButton.OnClick += VisitRegisterWebpage;
			_area.Append(_webHelpButton);

			_skipLoadButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.SkipToMainMenu"), 0.7f, true);
			_skipLoadButton.CopyStyle(_continueButton);
			_skipLoadButton.Top.Set(-55f, 1f);
			_skipLoadButton.WithFadedMouseOver();
			_skipLoadButton.OnClick += SkipLoad;
			_area.Append(_skipLoadButton);

			_exitAndDisableAllButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.ExitAndDisableAll"), 0.7f, true);
			_exitAndDisableAllButton.CopyStyle(_skipLoadButton);
			_exitAndDisableAllButton.TextColor = Color.Red;
			_exitAndDisableAllButton.WithFadedMouseOver();
			_exitAndDisableAllButton.OnClick += ExitAndDisableAll;

			Append(_area);
		}

		internal void Show(string message, int gotoMenu, string webHelpUrl = "", bool showRetry = false, bool showSkip = false) {
			_message = message;
			_gotoMenu = gotoMenu;
			_webHelpUrl = webHelpUrl;
			_showRetry = showRetry;
			_showSkip = showSkip;
			Main.gameMenu = true;
			Main.menuMode = Interface.errorMessageID;
		}

		private void ContinueClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10);
			if (_gotoMenu < 0)
				Environment.Exit(0);

			Main.menuMode = _gotoMenu;
		}

		private void ExitAndDisableAll(UIMouseEvent evt, UIElement listeningElement) {
			foreach (var mod in ModLoader.EnabledMods)
				ModLoader.DisableMod(mod);

			Environment.Exit(0);
		}

		private void OpenFile(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			Process.Start(Logging.LogPath);
		}

		private void SkipLoad(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			ModLoader.skipLoad = true;
			Main.menuMode = _gotoMenu;
		}

		private void VisitRegisterWebpage(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			Process.Start(_webHelpUrl);
		}
	}
}
