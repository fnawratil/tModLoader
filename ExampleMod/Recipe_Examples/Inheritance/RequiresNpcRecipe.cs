using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Recipe_Examples.Inheritance
{
	// This is the abstraction of the functionality ExampleAdvancedRecipe.cs provides
	// Hopefully with this, you can see what functionality was extracted and why that is useful
	// (hint: you might want to have multiple recipes requiring different Npcs!)
	public class RequiresNpcRecipe : ModRecipe
	{
		// Defines the NpcType required for crafting
		private readonly int _requiredNpcType;

		// The range that will be checked against, in pixels (1 tile = 16 pixels)
		private readonly int _requiredRange;

		public RequiresNpcRecipe(Mod mod, int requiredNpcType, int requiredRange = 480) : base(mod)
		{
			_requiredNpcType = requiredNpcType;
			// note that we provide a default range of 30 tiles (480/16=30)
			_requiredRange = requiredRange;
		}

		// We'll make sure this recipe is only available
		// IF the specified Npc is close.
		public override bool RecipeAvailable()
		{
			// If EoC was defeated we will try find out is there is required npc nearby player
			// Note the use of LINQ and the lambda (where clause)
			// This is considered filtering and useful to only match Npcs by a requirement
			// (this prevents you from iterating Npcs you weren't looking for anyway)
			foreach (NPC npc in Main.npc.Where(n => n.active && n.type == _requiredNpcType))
			{
				// Check if we are close enough
				if (Vector2.Distance(Main.LocalPlayer.Center, npc.Center) <= _requiredRange)
				{
					// We found the required NPC
					return true;
				}
			}

			// If we reach this point, we didn't find the required Npc
			return false;

			// Note: if you use further inheritance, you might want to return the base impl.
			// in our case, ModRecipe by default returns true, so we don't want this.
			// return base.RecipeAvailable();
		}
	}

	// in the ideal scenario (in ExampleAdvancedRecipe.cs), we derive from this class, (ExampleAdvancedRecipe : RequiresNpcRecipe)
	// override RecipeAvailable() and return base.RecipeAvailable() && NPC.downedBoss1
}
