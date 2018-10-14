using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameInput;
using Terraria.UI;

namespace ExampleMod.UI_Examples.CustomItemSlot
{
	//@todo showcase fully custom ItemSlot class

	// This class wraps the vanilla ItemSlot class into a UIElement.
	// The ItemSlot class was made before the UI system was made, so it can't be used normally with UIState. 
	// By wrapping the vanilla ItemSlot class, we can easily use ItemSlot in our own UI
	// ItemSlot isn't very modder friendly and operates based on a "Context" number that dictates how the slot behaves when left, right, or shift clicked and the background used when drawn. 
	// If you want more control, it is better to write your own UIElement.
	// There's basic validation in place in the slot via the validItem Func. 
	// See ExamplePersonUI for usage and use the Awesomify chat option of Example Person to see in action.
	public class VanillaItemSlotWrapper : UIElement
	{
		internal Item Item;
		readonly int _context;
		readonly float _scale;
		public Func<Item, bool> IsItemValid { get; set; }

		public VanillaItemSlotWrapper(int context = Terraria.UI.ItemSlot.Context.BankItem, float scale = 1f)
		{
			_context = context;
			_scale = scale;

			Item = new Item();
			Item.SetDefaults();

			Width.Set(Main.inventoryBack9Texture.Width * scale, 0f);
			Height.Set(Main.inventoryBack9Texture.Height * scale, 0f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			float oldScale = Main.inventoryScale;
			Main.inventoryScale = _scale;
			Rectangle rectangle = GetDimensions().ToRectangle();

			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface)
			{
				Main.LocalPlayer.mouseInterface = true;
				if (IsItemValid == null || IsItemValid(Main.mouseItem))
				{
					// Handle handles all the click and hover actions based on the context.
					Terraria.UI.ItemSlot.Handle(ref Item, _context);
				}
			}
			// Draw draws the slot itself and Item. Depending on context, the color will change, as will drawing other things like stack counts.
			Terraria.UI.ItemSlot.Draw(spriteBatch, ref Item, _context, rectangle.TopLeft());
			Main.inventoryScale = oldScale;
		}
	}
}