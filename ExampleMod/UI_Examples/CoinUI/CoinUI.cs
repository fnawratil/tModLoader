using ExampleMod.UI_Examples.Inheritance;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExampleMod.UI_Examples.CoinUI
{
	// ExampleUIs visibility is toggled by typing "/coin" in chat. (See CoinCommand.cs)
	// ExampleUI is a simple UI example showing how to use UIPanel, UIImageButton, and even a custom UIElement.
	public class CoinUI : UIState
	{
		public static bool Visible;

		private DraggableUIPanel _coinCounterPanel;
		private UIMoneyDisplay _moneyDiplay;

		public override void OnInitialize()
		{
			void CreateBasePanel()
			{
				// Here we define our container UIElement. 
				// In DraggableUIPanel.cs, you can see that DraggableUIPanel is a UIPanel with a couple added features.
				_coinCounterPanel = new DraggableUIPanel();
				_coinCounterPanel.SetPadding(0);
				// We need to place this UIElement in relation to its Parent. Later we will be calling `base.Append(coinCounterPanel);`. 
				// This means that this class, ExampleUI, will be our Parent. Since ExampleUI is a UIState, the Left and Top are relative to the top left of the screen.
				_coinCounterPanel.Left.Set(400f, 0f);
				_coinCounterPanel.Top.Set(100f, 0f);
				_coinCounterPanel.Width.Set(170f, 0f);
				_coinCounterPanel.Height.Set(70f, 0f);
				_coinCounterPanel.BackgroundColor = new Color(73, 94, 171);
				Append(_coinCounterPanel);
			}

			void CreatePlayButton()
			{
				// Next, we create another UIElement that we will place. Since we will be calling `coinCounterPanel.Append(playButton);`, Left and Top are relative to the top left of the coinCounterPanel UIElement. 
				// By properly nesting UIElements, we can position things relatively to each other easily.
				Texture2D buttonPlayTexture = ModContent.GetTexture("Terraria/UI/ButtonPlay");
				UIHoverImageButton playButton = new UIHoverImageButton(buttonPlayTexture, "Reset Coins Per Minute Counter");
				playButton.Left.Set(110, 0f);
				playButton.Top.Set(10, 0f);
				playButton.Width.Set(22, 0f);
				playButton.Height.Set(22, 0f);
				// UIHoverImageButton doesn't do anything when Clicked. Here we assign a method that we'd like to be called when the button is clicked.
				playButton.OnClick += PlayButtonClicked;
				_coinCounterPanel.Append(playButton);
			}

			void CreateDeleteButton()
			{
				Texture2D buttonDeleteTexture = ModContent.GetTexture("Terraria/UI/ButtonDelete");
				UIHoverImageButton closeButton = new UIHoverImageButton(buttonDeleteTexture, Language.GetTextValue("LegacyInterface.52")); // Localized text for "Close"
				closeButton.Left.Set(140, 0f);
				closeButton.Top.Set(10, 0f);
				closeButton.Width.Set(22, 0f);
				closeButton.Height.Set(22, 0f);
				closeButton.OnClick += CloseButtonClicked;
				_coinCounterPanel.Append(closeButton);
			}

			void CreateMoneyDisplay()
			{
				// UIMoneyDisplay is a fairly complicated custom UIElement. UIMoneyDisplay handles drawing some text and coin textures.
				// Organization is key to managing UI design. Making a contained UIElement like UIMoneyDisplay will make many things easier.
				_moneyDiplay = new UIMoneyDisplay();
				_moneyDiplay.Left.Set(15, 0f);
				_moneyDiplay.Top.Set(20, 0f);
				_moneyDiplay.Width.Set(100f, 0f);
				_moneyDiplay.Height.Set(0, 1f);
				_coinCounterPanel.Append(_moneyDiplay);
			}

			CreateBasePanel();
			CreatePlayButton();
			CreateDeleteButton();
			CreateMoneyDisplay();

			// As a recap, CoinUI is a UIState, meaning it covers the whole screen. 
			// We attach coinCounterPanel to CoinUI some distance from the top left corner.
			// We then place playButton, closeButton, and moneyDiplay onto coinCounterPanel so we can easily place these UIElements relative to coinCounterPanel.
			// Since coinCounterPanel will move, this proper organization will move playButton, closeButton, and moneyDiplay properly when coinCounterPanel moves.
		}

		private void PlayButtonClicked(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(SoundID.MenuOpen);
			_moneyDiplay.ResetCoins();
		}

		private void CloseButtonClicked(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(SoundID.MenuOpen);
			Visible = false;
		}

		public void UpdateValue(int pickedUp)
		{
			_moneyDiplay.Coins += pickedUp;
			_moneyDiplay.AddCoinsPerMinute(pickedUp);
		}
	}




}
