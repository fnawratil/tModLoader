using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal sealed class UILoaderAnimatedImage : UIElement
	{
		public const int MAX_DELAY = 5;
		public const int MAX_FRAMES = 16;
		public int frame;
		public int frameTick;

		public bool withBackground = false;
		private readonly Texture2D _backgroundTexture;
		private readonly Texture2D _loaderTexture;

		private readonly float _scale;

		public UILoaderAnimatedImage(float left, float top, float scale = 1f) {
			_backgroundTexture = Texture2D.FromStream(Main.instance.GraphicsDevice,
													  Assembly.GetExecutingAssembly()
															  .GetManifestResourceStream("Terraria.ModLoader.UI.LoaderBG.png"));
			_loaderTexture = Texture2D.FromStream(Main.instance.GraphicsDevice,
												  Assembly.GetExecutingAssembly()
														  .GetManifestResourceStream("Terraria.ModLoader.UI.Loader.png"));
			_scale = scale;
			Width.Pixels = 200f * scale;
			Height.Pixels = 200f * scale;
			HAlign = left;
			VAlign = top;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			if (++frameTick >= MAX_DELAY) {
				frameTick = 0;
				if (++frame >= MAX_FRAMES)
					frame = 0;
			}

			CalculatedStyle dimensions = GetDimensions();
			// Draw BG
			if (withBackground) {
				spriteBatch.Draw(_backgroundTexture,
								 new Vector2((int)dimensions.X, (int)dimensions.Y),
								 new Rectangle(0, 0, 200, 200),
								 Color.White,
								 0f,
								 new Vector2(0, 0),
								 _scale,
								 SpriteEffects.None,
								 0.0f);
			}

			// Draw loader animation
			spriteBatch.Draw(_loaderTexture,
							 new Vector2((int)dimensions.X, (int)dimensions.Y),
							 new Rectangle(200 * (frame / 8), 200 * (frame % 8), 200, 200),
							 Color.White,
							 0f,
							 new Vector2(0, 0),
							 _scale,
							 SpriteEffects.None,
							 0.0f);
		}
	}
}
