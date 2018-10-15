using ExampleMod.Items;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Recipe_Examples
{
	// In this class we separate recipe related code from our main class
	public class Recipes_Explained
	{
		// Define a mod we can use in this class
		private readonly Mod _mod;

		public Recipes_Explained(Mod mod)
		{
			_mod = mod;
		}

		// This is called by our mod class and runs the example methods
		internal void Example_Recipes()
		{
			// Learn about basic recipes
			MakeBasicRecipes();

			// Get some advice on how to structure your big recipe class
			MakeLotsOfRecipes();

			// To learn about polymorphism in recipes, please see Inheritance/ExampleAdvancedRecipe.cs
			// and Inheritance/RequiresNpcRecipe.cs
		}

		// =====================================
		// ======== BASIC RECIPES BELOW ========
		// =====================================

		private void MakeBasicRecipes()
		{
			// A basic recipe looks like this

			// This creates a new recipe, and specifies our mod as the source
			ModRecipe recipe = new ModRecipe(_mod);

			// This adds ExampleBlock with a stack of 1 as an ingredient
			recipe.AddIngredient(_mod.ItemType("ExampleBlock"));

			// This makes it required to stand near a work bench (any)
			recipe.AddTile(TileID.WorkBenches);

			// This adds our own recipe group as ingredient
			// See Example_RecipeGroups.cs
			recipe.AddRecipeGroup("ExampleMod:ExampleItem");

			// This sets the result of the recipe to be 50 silver coins
			recipe.SetResult(ItemID.SilverCoin, 50);

			// The following booleans have unique uses:
			recipe.anyFragment = true; // allows any pillar fragment (solar, lunar, vortex, stardust)
			recipe.anyIronBar = true; // accepts iron or lead as bar
			recipe.anySand = true; // accepts any sand block (sand, ebonsand, crimsand etc.)
			recipe.anyWood = true; // accepts any wood block (wood, ebonwood, shadewood etc.)
			recipe.anyPressurePlate = true; // accepts any pressure plate (gray, brown, blue etc.)
			// Note: for any of the above booleans to work, you have to actually add one of them as an ingredient

			recipe.needHoney = true; // requires you to stand in or near honey
			recipe.needLava = true; // requires you to stand in or near lava
			recipe.needWater = true; // requires you to stand in or near water
			recipe.needSnowBiome = true; // requires you to be in the snow biome

			recipe.alchemy = true; // requires the alchemy table

			// Finally, this adds the recipe to the game. Always call this last.
			recipe.AddRecipe();

			// If we want to add another recipe, we have to reset the variable
			recipe = new ModRecipe(_mod);
			// Now we can add a new recipe
		}

		// =======================================
		// ======== STRUCTURAL TIPS BELOW ========
		// =======================================

		private void MakeLotsOfRecipes()
		{
			// If you have a big mod with a lot of recipes, it is recommended to split your code
			// into smaller chunks (method separation) so your code is more manageable.
			AddExampleRecipes();
			AddBossRecipes();
			// ...

			// ...
			// Imagine lots of more method calls here.
		}

		// Here we've made a helper method we can use to shorten our code.
		// This is because many of our recipes follow the same terminology: one ingredient, one result, one possible required tile
		// notice the last parameters can be made optional by specifying a default value
		private void MakeSimpleRecipe(string modIngredient, short resultType, int ingredientStack = 1, int resultStack = 1, string reqTile = null) 
		{
			// This initializes a new recipe, and marks it coming from _mod
			ModRecipe recipe = new ModRecipe(_mod);

			// This adds an ingredient to the recipe, coming from mod by item name and a certain stack size
			recipe.AddIngredient(_mod, modIngredient, ingredientStack);

			// If a required tile was passed..
			if (reqTile != null)
			{
				// That tile is then added as a required tile for crafting this recipe
				recipe.AddTile(_mod, reqTile);
			}

			// The result of the recipe is set
			recipe.SetResult(resultType, resultStack);

			// Finally, the recipe is added
			recipe.AddRecipe(); 
		}

		// We can make a method 'overload' this way, changing the first parameter
		private void MakeSimpleRecipe(ModItem modIngredient, short resultType, int ingredientStack = 1, int resultStack = 1, ModTile reqTile = null)
		{
			MakeSimpleRecipe(modIngredient.Name, resultType, ingredientStack, resultStack, reqTile?.Name);
		}

		// Add recipes
		private void AddExampleRecipes()
		{
			// Check the method signature of MakeSimpleRecipes for the arguments, this is a method signature:
			// private void MakeSimpleRecipe(string _modIngredient, short resultType, int ingredientStack = 1, int resultStack = 1, string reqTile = null) 

			// ExampleItem crafts into the following items
			MakeSimpleRecipe("ExampleItem", ItemID.Silk, 999);
			MakeSimpleRecipe("ExampleItem", ItemID.IronOre, 999);
			MakeSimpleRecipe("ExampleItem", ItemID.GravitationPotion, 20);

			// Notice how we can omit the stack now, it has a default value
			MakeSimpleRecipe("ExampleItem", ItemID.GoldChest); 
			MakeSimpleRecipe("ExampleItem", ItemID.MusicBoxDungeon);
		}

		// Add boss related recipes
		private void AddBossRecipes()
		{
			// BossItem crafts into the following items
			// We are using the same helper method here, and we are making use of the reqTile parameter
			MakeSimpleRecipe("BossItem", ItemID.SuspiciousLookingEye, 10, 20, "ExampleWorkbench");
			MakeSimpleRecipe("BossItem", ItemID.BloodySpine, 10, 20, "ExampleWorkbench");
			MakeSimpleRecipe("BossItem", ItemID.Abeemination, 10, 20, "ExampleWorkbench");

			// Notice how we can skip optional parameters by specifying the target parameter with 'reqTile:', this means the resultStack will remain 1
			MakeSimpleRecipe("BossItem", ItemID.GuideVoodooDoll, 10, reqTile: "ExampleWorkbench");
			MakeSimpleRecipe("BossItem", ItemID.MechanicalEye, 10, 20, "ExampleWorkbench");
			MakeSimpleRecipe("BossItem", ItemID.MechanicalWorm, 10, 20, "ExampleWorkbench");
			MakeSimpleRecipe("BossItem", ItemID.MechanicalSkull, 10, 20, "ExampleWorkbench");

			// Here we see another way to retrieve items and tiles from classnames, using generic calls
			// Useful for those who program in an IDE who wish to avoid spelling mistakes.
			// What's also neat is that the references to classes can be automatically included in refactors, string literals cannot. (unless you have ReSharper)
			MakeSimpleRecipe(_mod.GetItem<BossItem>(), ItemID.LihzahrdPowerCell, 10, 20, _mod.GetTile<Tiles.ExampleWorkbench>());
		}
	}
}