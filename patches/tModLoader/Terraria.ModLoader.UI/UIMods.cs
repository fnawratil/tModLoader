using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Gamepad;
using Terraria.ModLoader.Config;

namespace Terraria.ModLoader.UI
{
	// TODO jof: refactor later
	internal class UIMods : UIState
	{
		public bool loading;
		public UICycleImage SearchFilterToggle;
		public ModsMenuSortMode sortMode = ModsMenuSortMode.RecentlyUpdated;
		public EnabledFilter enabledFilterMode = EnabledFilter.All;
		public ModSideFilter modSideFilterMode = ModSideFilter.All;
		public SearchFilter searchFilterMode = SearchFilter.Name;

		internal readonly List<UICycleImage> categoryButtons = new List<UICycleImage>();
		internal string filter;

		private UIElement _uIElement;
		private UIPanel _uIPanel;
		private UILoaderAnimatedImage _uiLoader;
		private bool _needToRemoveLoading;
		private UIList _modList;
		private readonly List<UIModItem> _items = new List<UIModItem>();
		private bool _updateNeeded;
		private UIInputTextField _filterTextBox;
		private UIScalingTextPanel<string> _buttonEa;
		private UIScalingTextPanel<string> _buttonDa;
		private UIScalingTextPanel<string> _buttonRm;
		private UIScalingTextPanel<string> _buttonB;
		private UIScalingTextPanel<string> _buttonOmf;
		private UIScalingTextPanel<string> _buttonMp;

		public override void OnInitialize() {
			_uIElement = new UIElement {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MAX_PANEL_WIDTH,
				Top = { Pixels = 220 },
				Height = { Pixels = -220, Percent = 1f },
				HAlign = 0.5f
			};

			_uIPanel = new UIPanel {
				Width = { Percent = 1f },
				Height = { Pixels = -110, Percent = 1f },
				BackgroundColor = UICommon.MAIN_PANEL_BG_COLOR,
				PaddingTop = 0f
			};
			_uIElement.Append(_uIPanel);

			_uiLoader = new UILoaderAnimatedImage(0.5f, 0.5f, 1f);

			_modList = new UIList {
				Width = { Pixels = -25, Percent = 1f },
				Height = { Pixels = ModLoader.showMemoryEstimates ? -72 : -50, Percent = 1f },
				Top = { Pixels = ModLoader.showMemoryEstimates ? 72 : 50 },
				ListPadding = 5f
			};
			_uIPanel.Append(_modList);

			if (ModLoader.showMemoryEstimates) {
				var ramUsage = new UIMemoryBar() {
					Top = { Pixels = 45 },
				};
				ramUsage.Width.Pixels = -25;
				_uIPanel.Append(ramUsage);
			}

			var uIScrollbar = new UIScrollbar {
				Height = { Pixels = -50, Percent = 1f },
				Top = { Pixels = 50 },
				HAlign = 1f
			}.WithView(100f, 1000f);
			_uIPanel.Append(uIScrollbar);

			_modList.SetScrollbar(uIScrollbar);

			var uIHeaderTexTPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.ModsModsList"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.UI_BLUE_COLOR
			}.WithPadding(15f);
			_uIElement.Append(uIHeaderTexTPanel);

			_buttonEa = new UIScalingTextPanel<string>(Language.GetTextValue("tModLoader.ModsEnableAll")) {
				TextColor = Color.Green,
				Width = new StyleDimension(-10f, 1f / 3f),
				Height = { Pixels = 40 },
				VAlign = 1f,
				Top = { Pixels = -65 }
			}.WithFadedMouseOver();
			_buttonEa.OnClick += EnableAll;
			_uIElement.Append(_buttonEa);

			// TODO CopyStyle doesn't capture all the duplication here, consider an inner method
			_buttonDa = new UIScalingTextPanel<string>(Language.GetTextValue("tModLoader.ModsDisableAll"));
			_buttonDa.CopyStyle(_buttonEa);
			_buttonDa.TextColor = Color.Red;
			_buttonDa.HAlign = 0.5f;
			_buttonDa.WithFadedMouseOver();
			_buttonDa.OnClick += DisableAll;
			_uIElement.Append(_buttonDa);

			_buttonRm = new UIScalingTextPanel<string>(Language.GetTextValue("tModLoader.ModsReloadMods"));
			_buttonRm.CopyStyle(_buttonEa);
			_buttonRm.HAlign = 1f;
			_buttonDa.WithFadedMouseOver();
			_buttonRm.OnClick += ReloadMods;
			_uIElement.Append(_buttonRm);

			_buttonB = new UIScalingTextPanel<string>(Language.GetTextValue("UI.Back"));
			_buttonB.CopyStyle(_buttonEa);
			_buttonB.Top.Pixels = -20;
			_buttonB.WithFadedMouseOver();
			_buttonB.OnClick += BackClick;

			_uIElement.Append(_buttonB);
			_buttonOmf = new UIScalingTextPanel<string>(Language.GetTextValue("tModLoader.ModsOpenModsFolder"));
			_buttonOmf.CopyStyle(_buttonB);
			_buttonOmf.HAlign = 0.5f;
			_buttonOmf.WithFadedMouseOver();
			_buttonOmf.OnClick += OpenModsFolder;
			_uIElement.Append(_buttonOmf);

			var texture = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.UI.UIModBrowserIcons.png"));
			var upperMenuContainer = new UIElement {
				Width = { Percent = 1f },
				Height = { Pixels = 32 },
				Top = { Pixels = 10 }
			};

			for (int j = 0; j < 3; j++) {
				UICycleImage toggleImage;
				if (j == 0) { //TODO: ouch, at least there's a loop but these click events look quite similar
					toggleImage = new UICycleImage(texture, 3, 32, 32,
												   34 * 3, 0);
					toggleImage.SetCurrentState((int)sortMode);
					toggleImage.OnClick += (a, b) => {
						sortMode = sortMode.NextEnum();
						_updateNeeded = true;
					};
					toggleImage.OnRightClick += (a, b) => {
						sortMode = sortMode.PreviousEnum();
						_updateNeeded = true;
					};
				}
				else if (j == 1) {
					toggleImage = new UICycleImage(texture, 3, 32, 32,
												   34 * 4, 0);
					toggleImage.SetCurrentState((int)enabledFilterMode);
					toggleImage.OnClick += (a, b) => {
						enabledFilterMode = enabledFilterMode.NextEnum();
						_updateNeeded = true;
					};
					toggleImage.OnRightClick += (a, b) => {
						enabledFilterMode = enabledFilterMode.PreviousEnum();
						_updateNeeded = true;
					};
				}
				else {
					toggleImage = new UICycleImage(texture, 5, 32, 32,
												   34 * 5, 0);
					toggleImage.SetCurrentState((int)modSideFilterMode);
					toggleImage.OnClick += (a, b) => {
						modSideFilterMode = modSideFilterMode.NextEnum();
						_updateNeeded = true;
					};
					toggleImage.OnRightClick += (a, b) => {
						modSideFilterMode = modSideFilterMode.PreviousEnum();
						_updateNeeded = true;
					};
				}

				toggleImage.Left.Pixels = j * 36 + 8;
				categoryButtons.Add(toggleImage);
				upperMenuContainer.Append(toggleImage);
			}

			var filterTextBoxBackground = new UIPanel {
				Top = { Percent = 0f },
				Left = { Pixels = -170, Percent = 1f },
				Width = { Pixels = 135 },
				Height = { Pixels = 40 }
			};
			filterTextBoxBackground.OnRightClick += (a, b) => _filterTextBox.Text = "";
			upperMenuContainer.Append(filterTextBoxBackground);

			_filterTextBox = new UIInputTextField(Language.GetTextValue("tModLoader.ModsTypeToSearch")) {
				Top = { Pixels = 5 },
				Left = { Pixels = -160, Percent = 1f },
				Width = { Pixels = 120 },
				Height = { Pixels = 20 }
			};
			_filterTextBox.OnTextChange += (a, b) => _updateNeeded = true;
			upperMenuContainer.Append(_filterTextBox);

			SearchFilterToggle = new UICycleImage(texture, 2, 32, 32,
												  34 * 2, 0) {
				Left = { Pixels = 545 }
			};
			SearchFilterToggle.SetCurrentState((int)searchFilterMode);
			SearchFilterToggle.OnClick += (a, b) => {
				searchFilterMode = searchFilterMode.NextEnum();
				_updateNeeded = true;
			};
			SearchFilterToggle.OnRightClick += (a, b) => {
				searchFilterMode = searchFilterMode.PreviousEnum();
				_updateNeeded = true;
			};
			categoryButtons.Add(SearchFilterToggle);
			upperMenuContainer.Append(SearchFilterToggle);

			_buttonMp = new UIScalingTextPanel<string>(Language.GetTextValue("tModLoader.ModsModPacks"));
			_buttonMp.CopyStyle(_buttonOmf);
			_buttonMp.HAlign = 1f;
			_buttonMp.WithFadedMouseOver();
			_buttonMp.OnClick += GotoModPacksMenu;
			_uIElement.Append(_buttonMp);

			_uIPanel.Append(upperMenuContainer);
			Append(_uIElement);
		}

		private static void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11, -1, -1, 1);
			// To prevent entering the game with Configs that violate ReloadRequired
			if (ConfigManager.AnyModNeedsReload()) {
				Main.menuMode = Interface.reloadModsID;
				return;
			}

			foreach (var activeConfigs in ConfigManager.Configs) {
				foreach (var activeConfig in activeConfigs.Value) {
					activeConfig.OnChanged();
				}
			}

			Main.menuMode = 0;
		}

		private void ReloadMods(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			if (_items.Count > 0)
				ModLoader.Reload();
		}

		private static void OpenModsFolder(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			Directory.CreateDirectory(ModLoader.ModPath);
			Process.Start(ModLoader.ModPath);
		}

		private static void GotoModPacksMenu(UIMouseEvent evt, UIElement listeningElement) {
			if (!Interface.modsMenu.loading) {
				Main.PlaySound(12, -1, -1, 1);
				Main.menuMode = Interface.modPacksMenuID;
			}
		}

		private void EnableAll(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(12, -1, -1, 1);
			foreach (UIModItem modItem in _items) {
				modItem.Enable();
			}
		}

		private void DisableAll(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(12, -1, -1, 1);
			foreach (UIModItem modItem in _items) {
				modItem.Disable();
			}
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			if (_needToRemoveLoading) {
				_needToRemoveLoading = false;
				_uIPanel.RemoveChild(_uiLoader);
			}

			if (!_updateNeeded) return;
			_updateNeeded = false;
			filter = _filterTextBox.Text;
			_modList.Clear();
			_modList.AddRange(_items.Where(item => item.PassFilters()));
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			for (int i = 0; i < categoryButtons.Count; i++) {
				if (categoryButtons[i].IsMouseHovering) {
					string text;
					switch (i) {
						case 0:
							text = sortMode.ToFriendlyString();
							break;
						case 1:
							text = enabledFilterMode.ToFriendlyString();
							break;
						case 2:
							text = modSideFilterMode.ToFriendlyString();
							break;
						case 3:
							text = searchFilterMode.ToFriendlyString();
							break;
						default:
							text = "None";
							break;
					}

					UICommon.DrawHoverStringInBounds(spriteBatch, text);
					return;
				}
			}

			UILinkPointNavigator.Shortcuts.BackButtonCommand = 1;
		}

		public override void OnActivate() {
			Main.clrInput();
			_modList.Clear();
			_items.Clear();
			loading = true;
			_uIPanel.Append(_uiLoader);
			ConfigManager.LoadAll(); // Makes sure MP configs are cleared.
			Populate();
		}

		internal void Populate() {
			Task.Factory
				.StartNew(ModOrganizer.FindMods)
				.ContinueWith(task => {
					var mods = task.Result;
					foreach (var mod in mods) {
						UIModItem modItem = new UIModItem(mod);
						_items.Add(modItem);
					}

					_needToRemoveLoading = true;
					_updateNeeded = true;
					loading = false;
				}, TaskScheduler.FromCurrentSynchronizationContext());
		}
	}

	public static class ModsMenuSortModesExtensions
	{
		public static string ToFriendlyString(this ModsMenuSortMode sortmode) {
			switch (sortmode) {
				case ModsMenuSortMode.RecentlyUpdated:
					return Language.GetTextValue("tModLoader.ModsSortRecently");
				case ModsMenuSortMode.DisplayNameAtoZ:
					return Language.GetTextValue("tModLoader.ModsSortNamesAlph");
				case ModsMenuSortMode.DisplayNameZtoA:
					return Language.GetTextValue("tModLoader.ModsSortNamesReverseAlph");
			}

			return "Unknown Sort";
		}
	}

	public static class EnabledFilterModesExtensions
	{
		public static string ToFriendlyString(this EnabledFilter updateFilterMode) {
			switch (updateFilterMode) {
				case EnabledFilter.All:
					return Language.GetTextValue("tModLoader.ModsShowAllMods");
				case EnabledFilter.EnabledOnly:
					return Language.GetTextValue("tModLoader.ModsShowEnabledMods");
				case EnabledFilter.DisabledOnly:
					return Language.GetTextValue("tModLoader.ModsShowDisabledMods");
			}

			return "Unknown Sort";
		}
	}

	public enum ModsMenuSortMode
	{
		RecentlyUpdated,
		DisplayNameAtoZ,
		DisplayNameZtoA,
	}

	public enum EnabledFilter
	{
		All,
		EnabledOnly,
		DisabledOnly,
	}
}
