using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.UI_Examples.CoinUI
{
	public class MoneyCounterGlobalItem : GlobalItem
	{
		// We can cast mod to ExampleMod or just utilize ExampleMod.instance.
		// (mod as ExampleMod).exampleUI.updateValue(item.stack);
		// The example of using the instance is that you can make this helper static
		// which means it only lives in one location and not each instanced GlobalItem
		private static CoinUI _coinUI => ExampleMod.instance.CoinUI;

		public override bool OnPickup(Item item, Player player)
		{
			switch (item.type)
			{
				case ItemID.CopperCoin:
					_coinUI?.UpdateValue(item.stack);
					
					break;
				case ItemID.SilverCoin:
					_coinUI?.UpdateValue(item.stack * 100);
					break;
				case ItemID.GoldCoin:
					_coinUI?.UpdateValue(item.stack * 10000);
					break;
				case ItemID.PlatinumCoin:
					_coinUI?.UpdateValue(item.stack * 1000000);
					break;
			}

			return base.OnPickup(item, player);
		}
	}
}
