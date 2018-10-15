using Terraria.ID;

namespace ExampleMod.Recipe_Examples.Inheritance
{
	// Is an AdditionalIngredientFinder, but the item type is always of type Chain
	// PLEASE NOTE: this is actually not recommended at all, as ChainFinder provides no
	// extra functionality over AdditionalIngredientFinder, and the word 'ChainFinder'
	// isn't necessarily more descriptive
	public class ChainFinder : AdditionalIngredientFinder
	{
		public ChainFinder(int itemStack = 1) : base(ItemID.Chain, itemStack)
		{
		}
	}
}
