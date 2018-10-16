using Terraria.ID;

namespace ExampleMod.RecipeExamples.Inheritance
{
	// Is an AdditionalIngredientFinder, but the item type is always of type Chain
	// PLEASE NOTE: this is shown to show you the WRONG way of doing this, as ChainFinder provides no
	// extra functionality over AdditionalIngredientFinder, and the word 'ChainFinder'
	// isn't necessarily more descriptive.
	// Instead, you should have a (static) method that generates an AdditionalIngredientFinder instance
	// that took ItemID.Chain as the itemType
	public class ChainFinder : AdditionalIngredientFinder
	{
		public ChainFinder(int itemStack = 1) : base(ItemID.Chain, itemStack)
		{
		}
	}
}
