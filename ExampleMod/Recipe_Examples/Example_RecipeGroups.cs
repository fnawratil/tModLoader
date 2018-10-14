using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Recipe_Examples
{
	public class Example_RecipeGroups
	{
		public void AddExampleRecipeGroup(Mod mod)
		{
			// Creates a new recipe group
			RecipeGroup group = new RecipeGroup(() => 
			Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(mod.ItemType("ExampleItem")), new[]
			{
				mod.ItemType("ExampleItem"),
				mod.ItemType("EquipMaterial"),
				mod.ItemType("BossItem")
			});

			// Registers the new recipe group with the specified name
			RecipeGroup.RegisterGroup("ExampleMod:ExampleItem", group);
		}
	}
}
