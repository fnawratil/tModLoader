using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.RecipeExamples.Inheritance
{
	// ModRecipe class is useful class that can help us adding custom recipe requirements other than materials
	// In this example my recipe will need specific npc nearby and Eye of Cthulhu defeated
	// For use of this see the class below

	// Note: see RequiresNpcRecipe.cs to learn more about abstraction
	// Using RequiresNpcRecipe here is actually the BETTER approach, to learn
	// we leave adapting this class to do that to you.
	public class ExampleAdvancedRecipe : ModRecipe
	{
		private readonly int _neededNpcType;

		//Range of npc search
		private const int Range = 480; //30 tiles -> 30 * 16

		// In the constructor, we'll add an argument where we will specify the Npc that's needed
		// Mod argument is required here, because ModRecipe itself needs it
		// that's why we have ":base(mod)" here to satisfy constructor of ModRecipe
		public ExampleAdvancedRecipe(Mod mod, int neededNpc) : base(mod)
		{
			_neededNpcType = neededNpc;
		}

		// RecipeAvailable is our goal here, in here we check our custom requirements
		// Also, RecipeAvailable is called on client, so we can use here Main.LocalPlayer without problems
		public override bool RecipeAvailable()
		{
			// by default returns true, so no need to call base implementation here.

			// First we check does EoC was defeated, if not, we will return false, so recipe won't be available
			if (!NPC.downedBoss1) return false;

			// If EoC was defeated we will try find out is there is required npc nearby player
			foreach (NPC npc in Main.npc)
			{
				// If npc isn't active or isn't our needed type, we will skip iteration
				if (!npc.active && npc.type != _neededNpcType) continue;

				// Otherwise we will compare positions
				if (Vector2.Distance(Main.LocalPlayer.Center, npc.Center) <= Range)
				{
					// We found the required NPC
					return true;
				}
			}

			// If we reach this point, we didn't find the required NPC.
			return false;
		}

		// OnCraft is called when we create item
		public override void OnCraft(Item item)
		{
			base.OnCraft(item);

			// And here a little surprise
			Main.LocalPlayer.AddBuff(BuffID.OnFire, 120);
		}
	}

	//Here's the item where we will add our recipe
	public class AdvancedRecipeItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Advanced Recipe Test Item");
			Tooltip.SetDefault("You need help with creating this!");
		}

		public override string Texture => "ExampleMod/Items/ExampleItem";

		public override void SetDefaults()
		{
			item.width = 26;
			item.height = 26;
			item.rare = 1;
		}

		// Using our custom recipe type
		public override void AddRecipes()
		{
			ExampleAdvancedRecipe recipe = new ExampleAdvancedRecipe(mod, NPCID.Guide);
			recipe.AddIngredient(ItemID.DirtBlock, 5);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
