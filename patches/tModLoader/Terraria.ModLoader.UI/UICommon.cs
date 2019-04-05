using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	/// <summary>
	/// Provides common utilities for UI components for which most act like fluent APIs
	/// </summary>
	public static class UICommon
	{
		public static Color UI_BLUE_COLOR = new Color(73, 94, 171);
		public static Color UI_BLUE_MOUSEOVER_COLOR = new Color(63, 82, 151) * 0.7f;
		public static Color MAIN_PANEL_BG_COLOR = new Color(33, 43, 79) * 0.8f;
		public static StyleDimension MAX_PANEL_WIDTH = new StyleDimension(600, 0);

		public static T WithFadedMouseOver<T>(this T elem, Color overColor = default, Color outColor = default) where T : UIPanel {
			if (overColor == default)
				overColor = UI_BLUE_COLOR;

			if (outColor == default)
				outColor = UI_BLUE_MOUSEOVER_COLOR;

			elem.OnMouseOver += (evt, _) => {
				Main.PlaySound(SoundID.MenuTick);
				elem.BackgroundColor = overColor;
			};
			elem.OnMouseOut += (evt, _) => { elem.BackgroundColor = outColor; };
			return elem;
		}

		public static T WithPadding<T>(this T elem, float pixels) where T : UIElement {
			elem.SetPadding(pixels);
			return elem;
		}

		// TODO unused, needed?
		public static T WithPadding<T>(this T elem, string name, int id, Vector2? anchor = null, Vector2? offset = null) where T : UIElement {
			elem.SetSnapPoint(name, id, anchor, offset);
			return elem;
		}

		public static T WithView<T>(this T elem, float viewSize, float maxViewSize) where T : UIScrollbar {
			elem.SetView(viewSize, maxViewSize);
			return elem;
		}

		public static T AddOrRemoveChild<T>(this T elem, UIElement child, bool add) where T : UIElement {
			if (!add)
				elem.RemoveChild(child);
			else if (!elem.HasChild(child))
				elem.Append(child);
			return elem;
		}

		public static void DrawHoverStringInBounds(SpriteBatch spriteBatch, string text, Rectangle? bounds = null) {
			if (bounds == null)
				bounds = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
			float x = Main.fontMouseText.MeasureString(text).X;
			Vector2 vector = Main.MouseScreen + new Vector2(16f);
			vector.X = Math.Min(vector.X, bounds.Value.Right - x - 16);
			vector.Y = Math.Min(vector.Y, bounds.Value.Bottom - 30);
			Color drawColor = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
			Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, text, vector.X,
										  vector.Y, drawColor, Color.Black, Vector2.Zero,
										  1f);
		}
	}
}
