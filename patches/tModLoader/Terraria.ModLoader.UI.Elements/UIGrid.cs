using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.ModLoader.UI.Elements
{
	//TODO: wow that's a lot of redundant this.
	public class UIGrid : UIElement
	{
		public delegate bool ElementSearchMethod(UIElement element);

		public List<UIElement> items = new List<UIElement>();
		public float ListPadding = 5f;
		public int Count => items.Count;

		protected UIScrollbar scrollbar;

		internal UIElement innerList = new UIInnerList();

		private float _innerListHeight;

		private class UIInnerList : UIElement
		{
			public override bool ContainsPoint(Vector2 point) {
				return true;
			}

			protected override void DrawChildren(SpriteBatch spriteBatch) {
				Vector2 position = Parent.GetDimensions().Position();
				Vector2 dimensions = new Vector2(Parent.GetDimensions().Width, Parent.GetDimensions().Height);
				foreach (UIElement current in Elements) {
					Vector2 position2 = current.GetDimensions().Position();
					Vector2 dimensions2 = new Vector2(current.GetDimensions().Width, current.GetDimensions().Height);
					if (Collision.CheckAABBvAABBCollision(position, dimensions, position2, dimensions2)) {
						current.Draw(spriteBatch);
					}
				}
			}
		}

		// todo, vertical/horizontal orientation, left to right, etc?
		public UIGrid() {
			innerList.OverflowHidden = false;
			innerList.Width.Set(0f, 1f);
			innerList.Height.Set(0f, 1f);
			OverflowHidden = true;
			Append(innerList);
		}

		public float GetTotalHeight() {
			return _innerListHeight;
		}

		public void Goto(ElementSearchMethod searchMethod, bool center = false) {
			foreach (var item in items.Where(item => searchMethod(item))) {
				scrollbar.ViewPosition = item.Top.Pixels;
				if (center) {
					scrollbar.ViewPosition = item.Top.Pixels - GetInnerDimensions().Height / 2 + item.GetOuterDimensions().Height / 2;
				}

				return;
			}
		}

		public virtual void Add(UIElement item) {
			items.Add(item);
			innerList.Append(item);
			UpdateOrder();
			innerList.Recalculate();
		}

		public virtual void AddRange(IEnumerable<UIElement> items) {
			this.items.AddRange(items);
			foreach (var item in items) {
				innerList.Append(item);
			}

			UpdateOrder();
			innerList.Recalculate();
		}

		public virtual bool Remove(UIElement item) {
			innerList.RemoveChild(item);
			UpdateOrder();
			return items.Remove(item);
		}

		public virtual void Clear() {
			innerList.RemoveAllChildren();
			items.Clear();
		}

		public override void Recalculate() {
			base.Recalculate();
			UpdateScrollbar();
		}

		public override void ScrollWheel(UIScrollWheelEvent evt) {
			base.ScrollWheel(evt);
			if (scrollbar != null) {
				scrollbar.ViewPosition -= (float)evt.ScrollWheelValue;
			}
		}

		public override void RecalculateChildren() {
			float availableWidth = GetInnerDimensions().Width;
			base.RecalculateChildren();
			float top = 0f;
			float left = 0f;
			float maxRowHeight = 0f;
			for (int i = 0; i < items.Count; i++) {
				var item = items[i];
				var outerDimensions = item.GetOuterDimensions();
				if (left + outerDimensions.Width > availableWidth && left > 0) {
					top += maxRowHeight + ListPadding;
					left = 0;
					maxRowHeight = 0;
				}

				maxRowHeight = Math.Max(maxRowHeight, outerDimensions.Height);
				item.Left.Set(left, 0f);
				left += outerDimensions.Width + ListPadding;
				item.Top.Set(top, 0f);
			}

			_innerListHeight = top + maxRowHeight;
		}

		private void UpdateScrollbar() {
			scrollbar?.SetView(GetInnerDimensions().Height, _innerListHeight);
		}

		public void SetScrollbar(UIScrollbar scrollbar) {
			this.scrollbar = scrollbar;
			UpdateScrollbar();
		}

		public void UpdateOrder() {
			items.Sort(SortMethod);
			UpdateScrollbar();
		}

		public int SortMethod(UIElement item1, UIElement item2) {
			return item1.CompareTo(item2);
		}

		public override List<SnapPoint> GetSnapPoints() {
			List<SnapPoint> list = new List<SnapPoint>();
			if (GetSnapPoint(out var item)) {
				list.Add(item);
			}

			foreach (UIElement current in items) {
				list.AddRange(current.GetSnapPoints());
			}

			return list;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			if (scrollbar != null) {
				innerList.Top.Set(-scrollbar.GetValue(), 0f);
			}
		}
	}

	internal class NestedUIGrid : UIGrid
	{
		public NestedUIGrid() { }

		public override void ScrollWheel(UIScrollWheelEvent evt) {
			if (scrollbar != null) {
				float oldpos = scrollbar.ViewPosition;
				scrollbar.ViewPosition -= (float)evt.ScrollWheelValue;
				if (oldpos == scrollbar.ViewPosition) {
					base.ScrollWheel(evt);
				}
			}
			else {
				base.ScrollWheel(evt);
			}
		}
	}
}
