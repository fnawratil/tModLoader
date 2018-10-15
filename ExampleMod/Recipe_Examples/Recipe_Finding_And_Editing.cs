using System.Collections.Generic;
using ExampleMod.Items.Placeable;
using ExampleMod.Recipe_Examples.Inheritance;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Recipe_Examples
{
	// Showcase RecipeFinder and RecipeEditor
	// With these classes, you can find and edit recipes
	public class Recipe_Finding_And_Editing
	{
		// Define a mod we can use in this class
		private readonly Mod _mod;

		public Recipe_Finding_And_Editing(Mod mod)
		{
			_mod = mod;
		}

		// This is called by our mod class and runs the example methods
		internal void Example_Recipe_Finding_And_Editing()
		{
			ExampleRecipeFinding();
			// If you want to learn more about polymorphism, you can continue with this method
			ExamplePolymorphicEditing();
		}

		// ===============================================
		// ======== BASIC FINDING & EDITING BELOW ========
		// ===============================================

		// Define a finder we can reuse in this class.
		private RecipeFinder _finder = new RecipeFinder();

		public void ExampleRecipeFinding()
		{
			// In this method, we will set the search criteria on our finder
			// so that it will find all recipes using stone block and creates 4 stone walls
			void InitializeFinder()
			{
				// First, set our search criteria
				_finder.AddIngredient(ItemID.StoneBlock);
				_finder.SetResult(ItemID.StoneWall, 4);
				// However, to make sure we only find the vanilla recipe, we also require the tile
				_finder.AddTile(TileID.WorkBenches);
			}
			InitializeFinder();

			// In this method, we will find the recipes using our finder
			// matching our search criteria, and then proceed to edit them.
			void DoEditRecipes()
			{
				List<Recipe> recipes = _finder.SearchRecipes();
				// Continue to this method to see how a recipe can be edited. (see below)
				ExampleRecipeEditing(recipes);
			}
			DoEditRecipes();

			// Now if we want to reuse a finder to find another recipe,
			// we have to initialize it as a new finder:
			_finder = new RecipeFinder();
			// Now we can setup search criteria like before, and search for different recipes
		}

		private void ExampleRecipeEditing(List<Recipe> recipes)
		{
			// In this method, we simply reset a recipe's result to be 1 copper coin.
			// We do this using a RecipeEditor
			// This method is used in the finder examples below
			void EditBasicRecipe(Recipe recipe)
			{
				RecipeEditor editor = new RecipeEditor(recipe);
				editor.SetResult(ItemID.CopperCoin);
				// we don't need to do anything else: the recipe is edited
			}

			// We will iterate over the given recipes,
			// and for each perform an edit (see above)
			foreach (Recipe r in recipes)
			{
				EditBasicRecipe(r);
			}
		}

		// ============================================
		// ======== POLYMORPHISM EXAMPLE BELOW ========
		// ============================================

		// Declare a finder we can reuse.
		// See the PolymorphismExample() function for more in depth use
		// (see Inheritance/ChainFinder.cs and Inheritance/AdditionalIngredientFinder.cs)
		private AdditionalIngredientFinder _polymorphicFinder = new ChainFinder();

		private void ExamplePolymorphicEditing()
		{
			void EditChainRecipes()
			{
				// In the following example, we find recipes that uses a chain as ingredient and then we remove that ingredient from the recipe.
				// We use our own, custom finder that already looks for chain (see Inheritance/ChainFinder.cs)

				// loop every recipe found by the finder
				foreach (Recipe recipe in _polymorphicFinder.SearchRecipes())
				{
					RecipeEditor editor = new RecipeEditor(recipe);
					editor.DeleteIngredient(ItemID.Chain);
				}
			}
			EditChainRecipes();

			void PolymorphismExample()
			{
				// For this example, we'll add a useless recipe that we can safely delete (it has no use)
				ModRecipe recipe = new ModRecipe(_mod);
				recipe.AddRecipeGroup("IronBar");
				recipe.AddIngredient(_mod.ItemType<ExampleBlock>());
				recipe.AddTile(TileID.Anvils);
				recipe.SetResult(ItemID.Chain, 10);
				recipe.AddRecipe();

				// The following is a more precise example, finding an exact recipe and deleting it if possible.

				// You cannot reset a finder, so the best way to reset is by making a new one
				// This time however, we don't want to find by a chain, but a different item
				// Because we declared _finder as an AdditionFinder, we can input anything we want in a new instance
				// this is the power of Polymorphism
				_polymorphicFinder = new AdditionalIngredientFinder(_mod.ItemType<ExampleBlock>());
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
			PolymorphismExample();
		}
	}
}
