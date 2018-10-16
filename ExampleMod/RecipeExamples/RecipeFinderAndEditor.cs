using System.Collections.Generic;
using ExampleMod.Items.Placeable;
using ExampleMod.RecipeExamples.Inheritance;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.RecipeExamples
{
	// Showcase RecipeFinder and RecipeEditor
	// With these classes, you can find and edit recipes
	public static class RecipeFinderAndEditor
	{
		// This is called by our mod and runs the example methods
		internal static void EditRecipes()
		{
			ExampleRecipeFinding();
			// If you want to learn more about polymorphism, you can learn from this method
			ExamplePolymorphicEditing();
		}

		// ===============================================
		// ======== BASIC FINDING & EDITING BELOW ========
		// ===============================================
		// Below, we will showcase how to find recipes, and edit them
		// using RecipeFinder and RecipeEditor

		// Define a finder we can reuse in this class.
		private static RecipeFinder _finder = new RecipeFinder();

		private static void ExampleRecipeFinding()
		{
			// In this method, we will set the search criteria on our finder
			// so that it will find all recipes using stone block and creates 4 stone walls

			// First, set our search criteria
			_finder.AddIngredient(ItemID.StoneBlock);
			_finder.SetResult(ItemID.StoneWall, 4);
			// However, to make sure we only find the vanilla recipe, we also require the tile
			_finder.AddTile(TileID.WorkBenches);

			// Now, we will find the recipes using our finder
			// matching our search criteria, and then proceed to edit them.

			List<Recipe> recipes = _finder.SearchRecipes();
			// Continue to this method to see how a recipe can be edited. (see below)
			ExampleRecipeEditing(recipes);

			// Now if we want to reuse a finder to find another recipe,
			// we have to initialize it as a new finder:
			_finder = new RecipeFinder();
			// Now we can setup search criteria like before, and search for different recipes
		}

		private static void ExampleRecipeEditing(List<Recipe> recipes)
		{
			// We will iterate over the given recipes,
			// and for each perform an edit (see above)
			foreach (Recipe r in recipes)
			{
				// Here, we simply reset a recipe's result to be 1 copper coin.
				// We do this using a RecipeEditor
				// This method is used in the finder examples below
				RecipeEditor editor = new RecipeEditor(r);
				editor.SetResult(ItemID.CopperCoin);
				// we don't need to do anything else: the recipe is edited
			}
		}

		// ============================================
		// ======== POLYMORPHISM EXAMPLE BELOW ========
		// ============================================
		// In this example we will showcase the use of polymorphism
		// in the context of finding and editing recipes.

		// Declare a finder we can reuse in this class
		// (see Inheritance/ChainFinder.cs and Inheritance/AdditionalIngredientFinder.cs)
		private static AdditionalIngredientFinder _polymorphicFinder = new ChainFinder();

		// Because ChainFinder is not recommended (see Inheritance/ChainFinder.cs to read why),
		// this is the proposed (better) implementation
		private static AdditionalIngredientFinder GetChainFinder(int stack = 1)
			=> new AdditionalIngredientFinder(ItemID.Chain, stack);
		// The result of this method is EXACTLY like ChainFinder,
		// because ChainFinder provides no additional functionality

		private static void ExamplePolymorphicEditing()
		{
			// In the following example, we find recipes that uses a chain as ingredient and then we remove that ingredient from the recipe.
			// We use our own, custom finder that already looks for chain (see Inheritance/ChainFinder.cs)

			// loop every recipe found by the finder
			foreach (Recipe r in _polymorphicFinder.SearchRecipes())
			{
				RecipeEditor editor = new RecipeEditor(r);
				editor.DeleteIngredient(ItemID.Chain);
			}

			// For the next example, we'll add a useless recipe that we can safely delete
			ModRecipe recipe = new ModRecipe(ExampleMod.instance);
			recipe.AddRecipeGroup("IronBar");
			recipe.AddIngredient(ExampleMod.instance.ItemType<ExampleBlock>());
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(ItemID.Chain, 10);
			recipe.AddRecipe();

			// The following is a more precise example, finding an exact recipe and deleting it if possible.

			// You cannot reset a finder, so the best way to reset is by making a new one
			// This time however, we don't want to find by a chain, but a different item
			// Because we declared _finder as an AdditionalIngredientFinder, we can input anything we want in a new instance
			// This is the power of polymorphism!
			_polymorphicFinder = new AdditionalIngredientFinder(ExampleMod.instance.ItemType<ExampleBlock>());
			// This time, we look for an ExampleBlock ingredient (not a Chain this time!)

			_finder.AddRecipeGroup("IronBar");
			_finder.AddTile(TileID.Anvils);
			_finder.SetResult(ItemID.Chain, 10);

			// try to find the exact recipe matching our criteria
			// PLEASE check FindExactRecipe() on AdditionalIngredientFinder.cs
			Recipe exactRecipe = _finder.FindExactRecipe();

			// if our recipe is not null, it means we found the exact recipe
			if (exactRecipe != null)
			{
				RecipeEditor editor = new RecipeEditor(exactRecipe);
				editor.DeleteRecipe();
			}
		}
	}
}
