using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UILoadProgress : UIPanel
	{
		private readonly UIText _text;
		private float _progress;

		public UILoadProgress() {
			_text = new UIText("", 0.75f, true) {
				Top = { Pixels = 20 },
				HAlign = 0.5f
			};
			Append(_text);
			_progress = 0f;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			CalculatedStyle space = GetInnerDimensions();
			spriteBatch.Draw(Main.magicPixel, new Rectangle((int)space.X + 10, (int)space.Y + (int)space.Height / 2 + 20, (int)space.Width - 20, 10), new Rectangle(0, 0, 1, 1), new Color(0, 0, 70));
			spriteBatch.Draw(Main.magicPixel, new Rectangle((int)space.X + 10, (int)space.Y + (int)space.Height / 2 + 20, (int)((space.Width - 20) * _progress), 10), new Rectangle(0, 0, 1, 1), new Color(200, 200, 70));
		}

		internal void SetProgress(float progress) {
			_progress = progress;
		}

		internal void SetText(string text) {
			_text.SetText(text, 0.75f, true);
		}
	}
}
