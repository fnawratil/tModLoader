using ExampleMod.Items;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Recipe_Examples
{
	// In this class we separate recipe related code from our main class
	public class Recipes_Explained
	{
		private readonly Mod _mod;

		public Recipes_Explained(Mod mod)
		{
			_mod = mod;
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
		public void AddExampleRecipes()
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

			// Instead of having to call AddBossRecipes from our main file, we can also call it here, as a result the method can remain private
			AddBossRecipes();
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