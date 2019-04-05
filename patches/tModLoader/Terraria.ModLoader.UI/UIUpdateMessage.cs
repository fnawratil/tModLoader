using System.Diagnostics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	//TODO how is this different to UIInfoMessage?
	internal class UIUpdateMessage : UIState
	{
		private readonly UIMessageBox _message = new UIMessageBox("");
		private int _gotoMenu;
		private string _url;

		public override void OnInitialize() {
			var area = new UIElement {
				Width = { Percent = 0.8f },
				Top = { Pixels = 200 },
				Height = { Pixels = -240, Percent = 1f },
				HAlign = 0.5f
			};

			_message.Width.Percent = 1f;
			_message.Height.Percent = 0.8f;
			_message.HAlign = 0.5f;
			area.Append(_message);

			var button = new UITextPanel<string>("Ignore", 0.7f, true) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 50 },
				VAlign = 1f,
				Top = { Pixels = -30 }
			};
			button.WithFadedMouseOver();
			button.OnClick += IgnoreClick;
			area.Append(button);

			var button2 = new UITextPanel<string>("Download", 0.7f, true);
			button2.CopyStyle(button);
			button2.HAlign = 1f;
			button2.WithFadedMouseOver();
			button2.OnClick += OpenUrl;
			area.Append(button2);
			Append(area);
		}

		internal void SetGotoMenu(int gotoMenu) {
			_gotoMenu = gotoMenu;
		}

		internal void SetMessage(string text) {
			_message.SetText(text);
		}

		internal void SetUrl(string url) {
			_url = url;
		}

		private void IgnoreClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			Main.menuMode = _gotoMenu;
		}

		private void OpenUrl(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			Process.Start(_url);
		}
	}
}
