using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace ExampleMod.UIExamples.Inheritance
{
	// This DraggableUIPanel class inherits from UIPanel. 
	// Inheriting is a great tool for UI design. By inheriting, we get the background drawing for free from UIPanel
	// We've added some code to allow the panel to be dragged around. 
	// We've also added some code to ensure that the panel will bounce back into bounds if it is dragged outside or the screen resizes.
	// UIPanel does not prevent the player from using items when the mouse is clicked, so we've added that as well.
	public class DraggableUIPanel : UIPanel
	{
		private Vector2 _offset;
		public bool IsBeingDragged { get; protected set; }

		// In general, it is a good idea to call the base implementation of methods
		// when you use inheritance. This way, the base implementation of something
		// can be called, in this case for MouseDown and MouseUp
		public override void MouseDown(UIMouseEvent evt)
		{
			base.MouseDown(evt);

			_offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
			IsBeingDragged = true;
		}

		public override void MouseUp(UIMouseEvent evt)
		{
			base.MouseUp(evt);

			Vector2 end = evt.MousePosition;
			IsBeingDragged = false;

			Left.Set(end.X - _offset.X, 0f);
			Top.Set(end.Y - _offset.Y, 0f);

			Recalculate();
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			// Checking ContainsPoint and then setting mouseInterface to true is very common.
			// This causes clicks on this UIElement to not cause the player to use current items. 
			if (ContainsPoint(Main.MouseScreen))
			{
				Main.LocalPlayer.mouseInterface = true;
			}

			// The next part handles the dragging movement of the panel
			if (IsBeingDragged)
			{
				Left.Set(Main.mouseX - _offset.X, 0f);
				Top.Set(Main.mouseY - _offset.Y, 0f);
				Recalculate();
			}

			// Here we check if the DraggableUIPanel is outside the Parent UIElement rectangle.
			// By doing this we can make sure this UIPanel stays inside te bounds of its parent.
			Rectangle parentSpace = Parent.GetDimensions().ToRectangle();
			if (!GetDimensions().ToRectangle().Intersects(parentSpace))
			{
				Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
				Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
				// Recalculate forces the UI system to do the positioning math again.
				Recalculate();
			}
		}
	}
}
