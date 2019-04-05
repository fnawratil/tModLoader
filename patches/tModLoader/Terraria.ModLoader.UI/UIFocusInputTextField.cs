using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIFocusInputTextField : UIElement
	{
		internal bool focused;
		internal string currentString = "";

		private readonly string _hintText;
		private int _textBlinkerCount;
		private int _textBlinkerState;

		public delegate void EventHandler(object sender, EventArgs e);

		public event EventHandler OnTextChange;
		public event EventHandler OnUnfocus;

		public UIFocusInputTextField(string hintText) {
			_hintText = hintText;
		}

		public void SetText(string text) {
			if (text == null)
				text = "";
			if (currentString != text) {
				currentString = text;
				OnTextChange?.Invoke(this, new EventArgs());
			}
		}

		public override void Click(UIMouseEvent evt) {
			Main.clrInput();
			focused = true;
		}

		public override void Update(GameTime gameTime) {
			Vector2 mousePosition = new Vector2(Main.mouseX, Main.mouseY);
			if (!ContainsPoint(mousePosition) && Main.mouseLeft) // TODO: && focused maybe?
			{
				focused = false;
				OnUnfocus?.Invoke(this, new EventArgs());
			}

			base.Update(gameTime);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			//Rectangle hitbox = GetInnerDimensions().ToRectangle();
			//Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.Red * 0.6f);

			if (focused) {
				GameInput.PlayerInput.WritingText = true;
				Main.instance.HandleIME();
				string newString = Main.GetInputText(currentString);
				if (!newString.Equals(currentString)) {
					currentString = newString;
					OnTextChange?.Invoke(this, new EventArgs());
				}
				else {
					currentString = newString;
				}

				if (++_textBlinkerCount >= 20) {
					_textBlinkerState = (_textBlinkerState + 1) % 2;
					_textBlinkerCount = 0;
				}
			}

			string displayString = currentString;
			if (_textBlinkerState == 1 && focused) {
				displayString += "|";
			}

			CalculatedStyle space = GetDimensions();
			if (currentString.Length == 0 && !focused) {
				Utils.DrawBorderString(spriteBatch, _hintText, new Vector2(space.X, space.Y), Color.Gray,
									   1f);
			}
			else {
				Utils.DrawBorderString(spriteBatch, displayString, new Vector2(space.X, space.Y), Color.White,
									   1f);
			}
		}
	}
}
