using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI
{
	internal class UIModInfo : UIState
	{
		public UIMessageBox modInfo;
		public UITextPanel<string> uITextPanel;

		internal UIElement uIElement;
		internal UIScalingTextPanel<string> modHomepageButton;
		internal UIScalingTextPanel<string> extractButton;
		internal UIScalingTextPanel<string> deleteButton;

		private int _gotoMenu = 0;
		private LocalMod _localMod;
		private string _url = "";
		private string _info = "";
		private string _modDisplayName = "";

		public override void OnInitialize() {
			uIElement = new UIElement {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MAX_PANEL_WIDTH,
				Top = { Pixels = 220 },
				Height = { Pixels = -220, Percent = 1f },
				HAlign = 0.5f
			};

			var uIPanel = new UIPanel {
				Width = { Percent = 1f },
				Height = { Pixels = -110, Percent = 1f },
				BackgroundColor = UICommon.MAIN_PANEL_BG_COLOR
			};
			uIElement.Append(uIPanel);

			modInfo = new UIMessageBox("This is a test of mod info here.") {
				Width = { Pixels = -25, Percent = 1f },
				Height = { Percent = 1f }
			};
			uIPanel.Append(modInfo);

			var uIScrollbar = new UIScrollbar {
				Height = { Pixels = -20, Percent = 1f },
				VAlign = 0.5f,
				HAlign = 1f
			}.WithView(100f, 1000f);
			uIPanel.Append(uIScrollbar);

			modInfo.SetScrollbar(uIScrollbar);
			uITextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.ModInfoHeader"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.UI_BLUE_COLOR
			}.WithPadding(15f);
			uIElement.Append(uITextPanel);

			modHomepageButton = new UIScalingTextPanel<string>(Language.GetTextValue("tModLoader.ModInfoVisitHomepage")) {
				Width = { Percent = 1f },
				Height = { Pixels = 40 },
				VAlign = 1f,
				Top = { Pixels = -65 }
			}.WithFadedMouseOver();
			modHomepageButton.OnClick += VisitModHomePage;
			uIElement.Append(modHomepageButton);

			var backButton = new UIScalingTextPanel<string>(Language.GetTextValue("UI.Back")) {
				Width = { Pixels = -10, Percent = 0.333f },
				Height = { Pixels = 40 },
				VAlign = 1f,
				Top = { Pixels = -20 }
			}.WithFadedMouseOver();
			backButton.OnClick += BackClick;
			uIElement.Append(backButton);

			extractButton = new UIScalingTextPanel<string>(Language.GetTextValue("tModLoader.ModInfoExtract")) {
				Width = { Pixels = -10, Percent = 0.333f },
				Height = { Pixels = 40 },
				VAlign = 1f,
				HAlign = 0.5f,
				Top = { Pixels = -20 }
			}.WithFadedMouseOver();
			extractButton.OnClick += ExtractClick;
			uIElement.Append(extractButton);

			deleteButton = new UIScalingTextPanel<string>(Language.GetTextValue("UI.Delete")) {
				Width = { Pixels = -10, Percent = 0.333f },
				Height = { Pixels = 40 },
				VAlign = 1f,
				HAlign = 1f,
				Top = { Pixels = -20 }
			}.WithFadedMouseOver();
			deleteButton.OnClick += DeleteClick;
			uIElement.Append(deleteButton);

			Append(uIElement);
		}

		// TODO use Show pattern
		internal void SetModInfo(string text) {
			_info = text;
			if (_info.Equals("")) {
				_info = Language.GetTextValue("tModLoader.ModInfoNoDescriptionAvailable");
			}
		}

		internal void SetModName(string text) {
			_modDisplayName = text;
		}

		internal void SetGotoMenu(int gotoMenu) {
			_gotoMenu = gotoMenu;
		}

		internal void SetUrl(string url) {
			_url = url;
		}

		internal void SetMod(LocalMod mod) {
			_localMod = mod;
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11);
			Main.menuMode = _gotoMenu;
		}

		private void ExtractClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(ID.SoundID.MenuOpen);
			Interface.extractMod.Show(_localMod, _gotoMenu);
		}

		private void DeleteClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(ID.SoundID.MenuClose);
			File.Delete(_localMod.modFile.path);
			Main.menuMode = _gotoMenu;
		}

		private void VisitModHomePage(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10);
			Process.Start(_url);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			UILinkPointNavigator.Shortcuts.BackButtonCommand = 100;
			UILinkPointNavigator.Shortcuts.BackButtonGoto = _gotoMenu;
			if (modHomepageButton.IsMouseHovering) {
				UICommon.DrawHoverStringInBounds(spriteBatch, _url);
			}
		}

		public override void OnActivate() {
			uITextPanel.SetText(Language.GetTextValue("tModLoader.ModInfoHeader") + _modDisplayName, 0.8f, true);
			modInfo.SetText(_info);
			if (_url.Equals("")) {
				modHomepageButton.Remove();
			}
			else {
				uIElement.Append(modHomepageButton);
			}

			if (_localMod != null) {
				uIElement.AddOrRemoveChild(deleteButton, ModLoader.Mods.All(x => x.Name != _localMod.Name));
				uIElement.Append(extractButton);
			}
			else {
				deleteButton.Remove();
				extractButton.Remove();
			}
		}
	}
}
