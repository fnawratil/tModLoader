using Terraria.ID;

namespace ExampleMod.Recipe_Examples.Inheritance
{
	// Is an AdditionFinder, but the item type is always of type Chain
	public class ChainFinder : AdditionalIngredientFinder
	{
		public ChainFinder(int itemStack = 1) : base(ItemID.Chain, itemStack)
		{
		}
	}
}
