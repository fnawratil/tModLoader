using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.RecipeExamples
{
	public static class RecipeGroups
	{
		// ==========================================
		// ======== BASIC RECIPE GROUP BELOW ========
		// ==========================================
		// This section shows how to make a custom recipe group, using RecipeGroup
		// A group is what allows multiple items to be used as an ingredient in place of one another
		// So if a group consists of a, b and c, a player only needs one of a, b or c to craft the item

		public static void AddRecipeGroups()
		{
			// Make it easier to access our mod in static context
			Mod mod = ExampleMod.instance;

			// Creates a new recipe group
			RecipeGroup group = new RecipeGroup(() => 

			// Language.GetTextValue will get a text value from the language resource files
			// This makes it compatible with language selection in the main menu .
			// To find what is LegacyMisc.37, open the content json file suffixed 'Legacy' in your language (e.g. en-US), and search for 'LegacyMisc'
			Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(mod.ItemType("ExampleItem")), new[]
			{
				mod.ItemType("ExampleItem"),
				mod.ItemType("EquipMaterial"),
				mod.ItemType("BossItem")
			});

			// Note: it is usually cleaner to use string interpolation or string.Format if you intend using multiple sources for the string:
			// $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(mod.ItemType("ExampleItem"))}" // string interpolation (C#6)
			// string.Format("{0} {1}", Language.GetTextValue("LegacyMisc.37"), Lang.GetItemNameValue(mod.ItemType("ExampleItem"))) // string format

			// Registers the new recipe group with the specified name
			RecipeGroup.RegisterGroup("ExampleMod:ExampleItem", group);
			// You or other modders can add this group to their recipe by this name:
			//recipe.AddRecipeGroup("ExampleMod:ExampleItem");
			// It is recommended to prefix the name with your internal mod name followed by :
		}
	}
}
