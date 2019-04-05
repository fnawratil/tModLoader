using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent.UI.Elements;

namespace Terraria.ModLoader.UI
{
	/// <summary>
	/// Functions like UITextPanel except we scale and manipulate Text to preserve original dimensions.
	/// </summary>
	public class UIScalingTextPanel<T> : UIPanel
	{
		private Vector2[] _drawOffsets;
		private Rectangle _oldInnerDimensions;

		private T _text;
		private string[] _textStrings;

		public UIScalingTextPanel(T text, float textScaleMax = 1f, bool large = false) {
			SetText(text, textScaleMax, large);
		}

		public bool IsLarge { get; private set; }
		public bool DrawPanel { get; set; } = true;
		public float TextScaleMax { get; set; } = 1f;
		public float TextScale { get; set; } = 1f;
		public Vector2 TextSize { get; private set; } = Vector2.Zero;
		public Color TextColor { get; set; } = Color.White;
		public string Text => _text?.ToString() ?? "";

		public override void Recalculate() {
			SetText(_text, TextScaleMax, IsLarge);
			base.Recalculate();
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			if (DrawPanel) {
				base.DrawSelf(spriteBatch);
			}

			var innerDimensions = GetDimensions().ToRectangle();
			innerDimensions.Inflate(-4, 0);
			innerDimensions.Y += 8;
			innerDimensions.Height -= 8;
			for (int i = 0; i < _textStrings.Length; i++) {
				//Vector2 pos = innerDimensions.Center.ToVector2() + drawOffsets[i];
				Vector2 pos = innerDimensions.TopLeft() + _drawOffsets[i];
				Utils.DrawBorderString(spriteBatch, _textStrings[i], pos, TextColor,
									   TextScale, 0f, 0f, -1);
			}

			//foreach (var singleLine in textStrings)
			//{
			//	Vector2 pos = innerDimensions.Position();
			//	if (this.IsLarge)
			//	{
			//		pos.Y -= 10f * this.TextScale * this.TextScale;
			//	}
			//	else
			//	{
			//		pos.Y -= 2f * this.TextScale;
			//	}
			//	pos.X += (innerDimensions.Width - this.TextSize.X) * 0.5f;
			//	if (this.IsLarge)
			//	{
			//		Utils.DrawBorderStringBig(spriteBatch, this.Text, pos, this.TextColor, this.TextScale, 0f, 0f, -1);
			//		return;
			//	}
			//	Utils.DrawBorderString(spriteBatch, this.Text, pos, this.TextColor, this.TextScale, 0f, 0f, -1);
			//}
		}

		public virtual void SetText(T text, float textScaleMax, bool large) {
			var innerDimensionsRectangle = GetDimensions().ToRectangle();
			if (text.ToString() == _text?.ToString() && _oldInnerDimensions == innerDimensionsRectangle)
				return;

			_oldInnerDimensions = innerDimensionsRectangle;

			TextScaleMax = textScaleMax;
			DynamicSpriteFont dynamicSpriteFont = large ? Main.fontDeathText : Main.fontMouseText;
			//Vector2 textSize = new Vector2(dynamicSpriteFont.MeasureString(text.ToString()).X, large ? 32f : 16f) * TextScaleMax;
			Vector2 textSize = dynamicSpriteFont.MeasureString(text.ToString()) * TextScaleMax;

			innerDimensionsRectangle.Inflate(-4, 0);

			var availableSpace = new Vector2(innerDimensionsRectangle.Width, innerDimensionsRectangle.Height);

			if (textSize.X > availableSpace.X || textSize.Y > availableSpace.Y) {
				float scale = textSize.X / availableSpace.X > textSize.Y / availableSpace.Y ? availableSpace.X / textSize.X : availableSpace.Y / textSize.Y;
				TextScale = scale;
				textSize = dynamicSpriteFont.MeasureString(text.ToString()) * TextScaleMax;
			}
			else {
				TextScale = TextScaleMax;
			}

			innerDimensionsRectangle.Y += 8;
			innerDimensionsRectangle.Height -= 8;
			_text = text;
			//this.TextScale = textScaleMax;
			TextSize = textSize;
			IsLarge = large;
			_textStrings = text.ToString().Split('\n');
			// offset off left corner for centering
			_drawOffsets = new Vector2[_textStrings.Length];
			for (int i = 0; i < _textStrings.Length; i++) {
				Vector2 size = dynamicSpriteFont.MeasureString(_textStrings[i]) * TextScale;
				//size.Y = size.Y * 0.9f;
				float x = (innerDimensionsRectangle.Width - size.X) * 0.5f;
				float y = -_textStrings.Length * size.Y * 0.5f + i * size.Y + innerDimensionsRectangle.Height * 0.5f;
				_drawOffsets[i] = new Vector2(x, y);
			}

			//this.MinWidth.Set(textSize.X + this.PaddingLeft + this.PaddingRight, 0f);
			//this.MinHeight.Set(textSize.Y + this.PaddingTop + this.PaddingBottom, 0f);
		}

		public void SetText(T text) {
			SetText(text, TextScaleMax, IsLarge);
		}
	}
}
