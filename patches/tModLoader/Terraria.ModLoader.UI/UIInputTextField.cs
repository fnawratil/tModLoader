using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameInput;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIInputTextField : UIElement
	{
		public delegate void EventHandler(object sender, EventArgs e);

		private readonly string _hintText;
		private string _currentString = "";
		private int _textBlinkerCount;

		public UIInputTextField(string hintText) {
			_hintText = hintText;
		}

		public string Text {
			get => _currentString;
			set {
				if (_currentString == value) return;
				_currentString = value;
				OnTextChange?.Invoke(this, EventArgs.Empty);
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			PlayerInput.WritingText = true;
			Main.instance.HandleIME();
			string newString = Main.GetInputText(_currentString);
			if (newString != _currentString) {
				_currentString = newString;
				OnTextChange?.Invoke(this, EventArgs.Empty);
			}

			string displayString = _currentString;
			if (++_textBlinkerCount / 20 % 2 == 0)
				displayString += "|";

			CalculatedStyle space = GetDimensions();
			if (_currentString.Length == 0) {
				Utils.DrawBorderString(spriteBatch, _hintText, new Vector2(space.X, space.Y), Color.Gray,
									   1f);
			}
			else {
				Utils.DrawBorderString(spriteBatch, displayString, new Vector2(space.X, space.Y), Color.White,
									   1f);
			}
		}

		public event EventHandler OnTextChange;
	}
}
