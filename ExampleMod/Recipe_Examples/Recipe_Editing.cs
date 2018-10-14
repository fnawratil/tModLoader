using ExampleMod.Items.Placeable;
using ExampleMod.Recipe_Examples.Inheritance;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Recipe_Examples
{
	// Showcase RecipeFinder and RecipeEditor
	// With these classes, you can find and edit recipes
	public class Recipe_Editing
	{
		// Declare a finder we can reuse. Uses our own class.
		// See the PolymorphicExample() function for more in depth use
		// (see Inheritance/ChainFinder.cs and Inheritance/AdditionalIngredientFinder.cs)
		private AdditionalIngredientFinder _finder = new ChainFinder();

		public void ExampleRecipeEditing(Mod mod)
		{
			void EditChainRecipes()
			{
				// In the following example, we find recipes that uses a chain as ingredient and then we remove that ingredient from the recipe.
				// We use our own, custom finder that already looks for chain (see Inheritance/ChainFinder.cs)

				// loop every recipe found by the finder
				foreach (Recipe recipe in _finder.SearchRecipes()) 
				{
					RecipeEditor editor = new RecipeEditor(recipe);
					editor.DeleteIngredient(ItemID.Chain);
				}
			}
			EditChainRecipes();

			void PolymorphismExample()
			{
				// For this example, we'll add a useless recipe that we can safely delete (it has no use)
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddRecipeGroup("IronBar");
				recipe.AddIngredient(mod.ItemType<ExampleBlock>());
				recipe.AddTile(TileID.Anvils);
				recipe.SetResult(ItemID.Chain, 10);
				recipe.AddRecipe();

				// The following is a more precise example, finding an exact recipe and deleting it if possible.

				// You cannot reset a finder, so the best way to reset is by making a new one
				// This time however, we don't want to find by a chain, but a different item
				// Because we declared _finder as an AdditionFinder, we can input anything we want in a new instance
				// this is the power of Polymorphism
				_finder = new AdditionalIngredientFinder(mod.ItemType<ExampleBlock>());
				// This time, we look for an ExampleBlock ingredient (not a Chain this time!)

				_finder.AddRecipeGroup("IronBar");
				_finder.AddTile(TileID.Anvils);
				_finder.SetResult(ItemID.Chain, 10);

				// try to find the exact recipe matching our criteria
				Recipe exactRecipe = _finder.FindExactRecipe();

				// if our recipe is not null, it means we found the exact recipe
				if (exactRecipe != null)
				{
					RecipeEditor editor = new RecipeEditor(exactRecipe); // for our recipe, make a new RecipeEditor
					editor.DeleteRecipe(); // delete the recipe
				}
			}
			PolymorphismExample();
		}
	}
}
