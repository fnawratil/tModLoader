using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace ExampleMod.UI_Examples.CoinUI
{
	public class UIMoneyDisplay : UIElement
	{
		public long Coins;

		public UIMoneyDisplay()
		{
			Width.Set(100, 0f);
			Height.Set(40, 0f);

			for (int i = 0; i < 60; i++)
			{
				_coinBins[i] = -1;
			}
		}

		// Array of ints 60 long.
		// "length" = seconds since reset
		// reset on button or 20 seconds of inactivity?
		// pointer to index so on new you can clear previous
		private readonly int[] _coinBins = new int[60];
		private int _coinBinsIndex;

		public void AddCoinsPerMinute(int coins)
		{
			int second = DateTime.Now.Second;
			if (second != _coinBinsIndex)
			{
				_coinBinsIndex = second;
				_coinBins[_coinBinsIndex] = 0;
			}
			_coinBins[_coinBinsIndex] += coins;
		}

		public int GetCoinsPerMinute()
		{
			int second = DateTime.Now.Second;
			if (second != _coinBinsIndex)
			{
				_coinBinsIndex = second;
				_coinBins[_coinBinsIndex] = 0;
			}

			long sum = _coinBins.Sum(a => a > -1 ? a : 0);
			int count = _coinBins.Count(a => a > -1);
			if (count == 0)
			{
				return 0;
			}
			return (int)((sum * 60f) / count);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle innerDimensions = GetInnerDimensions();
			float shopx = innerDimensions.X;
			float shopy = innerDimensions.Y;
			int[] coinsArray = Utils.CoinsSplit(Coins);

			for (int j = 0; j < 4; j++)
			{
				int num = (j == 0 && coinsArray[3 - j] > 99) ? -6 : 0;
				spriteBatch.Draw(Main.itemTexture[74 - j], new Vector2(shopx + 11f + (float)(24 * j), shopy /*+ 75f*/), null, Color.White, 0f, Main.itemTexture[74 - j].Size() / 2f, 1f, SpriteEffects.None, 0f);
				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontItemStack, coinsArray[3 - j].ToString(), shopx + (float)(24 * j) + (float)num, shopy/* + 75f*/, Color.White, Color.Black, new Vector2(0.3f), 0.75f);
			}

			coinsArray = Utils.CoinsSplit(GetCoinsPerMinute());
			for (int j = 0; j < 4; j++)
			{
				int num = (j == 0 && coinsArray[3 - j] > 99) ? -6 : 0;
				spriteBatch.Draw(Main.itemTexture[74 - j], new Vector2(shopx + 11f + (float)(24 * j), shopy + 25f), null, Color.White, 0f, Main.itemTexture[74 - j].Size() / 2f, 1f, SpriteEffects.None, 0f);
				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontItemStack, coinsArray[3 - j].ToString(), shopx + (float)(24 * j) + (float)num, shopy + 25f, Color.White, Color.Black, new Vector2(0.3f), 0.75f);
			}
			Utils.DrawBorderStringFourWay(spriteBatch, /*ExampleMod.exampleFont*/ Main.fontItemStack, "CPM", shopx + (float)(24 * 4), shopy + 25f, Color.White, Color.Black, new Vector2(0.3f), 0.75f);
		}

		internal void ResetCoins()
		{
			Coins = 0;
			for (int i = 0; i < 60; i++)
			{
				_coinBins[i] = -1;
			}
		}
	}
}
