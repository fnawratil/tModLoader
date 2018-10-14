using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

namespace ExampleMod.UI_Examples
{
	// This class aims to provide helpful annotations for methods available in UIState
	// The UIState inherits UIElement
	// UIState really functions as a 'container' around a UIElement, and signifies the state a UI is in
	[Obsolete("Do not use this class", error: true)] // This attribute is here to make sure this class isn't used as it exists purely to document the class
	internal class UIState_Explained : UIState
	{
		private UIElement _exampleElement;

		// OnInitialize is called when this state is initialized
		// You'll usually want to create new, child elements here and append them to the UIState
		// UIState classes have width and height equal to the full screen, because of this, usually we first define a UIElement that will act as the container for our UI.
		// We then place various other UIElement onto that container UIElement positioned relative to the container UIElement.
		public override void OnInitialize()
		{
			// It's usually a good idea to call the base class, in this case UIState
			// UIState actually doesn't initialize anything, but if in your case you inherit
			// your own class or someone else's, it likely does.
			base.OnInitialize();

			// Here we initialize some element, and append it to our state.
			// This will actually make it activate, and draw as a child element of this state
			_exampleElement = new UIElement();
			Append(_exampleElement);
		}

		// OnDeactivate is called when the UserInterface switches to a different state. 
		// Using OnDeactivate is useful for clearing static variables, or for example giving back an item slotted in the UI
		public override void OnDeactivate()
		{
			base.OnDeactivate();

			void RecursiveParentDeregister(UIElement el, MouseEvent evt)
			{
				if (el.Parent != null)
				{
					RecursiveParentDeregister(el, evt);
				}
				else
				{
					el.OnClick -= evt;
				}
			}

			// See OnActivate why this is here
			RecursiveParentDeregister(this, RegisterExampleClick);
		}

		// OnActivate is called when the UserInterface swithces to this state
		// Useful for initializing something you need when activated, or trigger an event of sorts
		public override void OnActivate()
		{
			base.OnActivate();

			//@todo move to actual example for recursion
			// This is an example of a recursive method
			// (it calls itself recursively)
			// This way, the traverse parents of parents
			// and register our OnClick functionality to the highest parent
			// Note that, for a UIState this is likely useless, but can be useful for actual nested elements
			void RecursiveParentRegister(UIElement el, MouseEvent evt)
			{
				if (el.Parent != null)
				{
					RecursiveParentRegister(el.Parent, evt);
				}
				else
				{
					el.OnClick += evt;
				}
			}

			// See below for more info. 
			RecursiveParentRegister(this, RegisterExampleClick);
		}

		// OnDeactive and OnActivate could be used to attach to and detach from EventHandlers.

		// Update is called on a UIState while it is the active state of the UserInterface.
		public override void Update(GameTime gameTime)
		{
			// Don't delete this or the UIElements attached to this UIState will cease to function.
			// This is because this method will run Update(gameTime) on all children of this state
			base.Update(gameTime);
		}

		// Draws this State (using DrawSelf) and afterwards also all appended children (using DrawChildren)
		// Recommended is, to only use this function if you have something custom that needs to draw
		// (e.g. your own children list or element before/after self and children)
		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
		}

		// If you want to alter how 'this' draws, you should use this
		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			// DrawSelf actually has NO base implementation. 
			// You could use DrawSelf to draw a background for example.
		}

		// The base call of DrawChildren actually iterates this.Elements and calls .Draw on each of them
		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			base.DrawChildren(spriteBatch);
		}

		//@todo: do not put this as part of UIElement if only used by tML itself
		public override bool PassFilters()
		{
			return base.PassFilters();
		}

		// Many of these methods (such as Click, MouseOver, ScrollWheel etc.) invoke all handlers
		// registered on their respective Events
		// Recommend is to NEVER override these methods, but instead register your own handler in OnActivate()
		// (see below for example)
		public override void Click(UIMouseEvent evt)
		{
			base.Click(evt);
		}

		// This method is registered in OnActivate()
		private void RegisterExampleClick(UIMouseEvent evt, UIElement el)
		{
			// You can access static variables here!
			ExampleMod.instance.Logger.Info("I clicked something!");
		}

		// This method returns if the given Vector is within the dimensions of this element
		// This is used in GetElementAt(point), which returns the UIElement (if any) at a given vector within a state
		// Useful if you somehow want this dimension to be larger/smaller or have any custom behavior for this
		public override bool ContainsPoint(Vector2 point)
		{
			return base.ContainsPoint(point);
		}

		// Used to compare this element to another, used when ordering elements
		public override int CompareTo(object obj)
		{
			return base.CompareTo(obj);
		}
	}
}
