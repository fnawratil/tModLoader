using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI
{
	internal class UIModSources : UIState
	{
		private readonly List<UIModSourceItem> _items = new List<UIModSourceItem>();
		private UIList _modList;
		private UIElement _uIElement;
		private UILoaderAnimatedImage _uiLoader;
		private UIPanel _uIPanel;
		private bool _updateNeeded;

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			UILinkPointNavigator.Shortcuts.BackButtonCommand = 1;
		}

		public override void OnActivate() {
			ModCompile.UpdateReferencesFolder();
			_uIPanel.Append(_uiLoader);
			_modList.Clear();
			_items.Clear();
			Populate();
		}

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
				BackgroundColor = UICommon.MAIN_PANEL_BG_COLOR
			};
			_uIElement.Append(_uIPanel);

			_uiLoader = new UILoaderAnimatedImage(0.5f, 0.5f, 1f);

			_modList = new UIList {
				Width = { Pixels = -25, Percent = 1f },
				Height = { Percent = 1f },
				ListPadding = 5f
			};
			_uIPanel.Append(_modList);

			var uIScrollbar = new UIScrollbar {
				Height = { Percent = 1f },
				HAlign = 1f
			}.WithView(100f, 1000f);
			_uIPanel.Append(uIScrollbar);
			_modList.SetScrollbar(uIScrollbar);

			var uIHeaderTextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.MenuModSources"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.UI_BLUE_COLOR
			}.WithPadding(15f);
			_uIElement.Append(uIHeaderTextPanel);

			var buttonBa = new UIScalingTextPanel<string>(Language.GetTextValue("tModLoader.MSBuildAll")) {
				Width = { Pixels = -10, Percent = 1f / 3f },
				Height = { Pixels = 40 },
				VAlign = 1f,
				Top = { Pixels = -65 }
			};
			buttonBa.WithFadedMouseOver();
			buttonBa.OnClick += BuildMods;
			_uIElement.Append(buttonBa);

			var buttonBra = new UIScalingTextPanel<string>(Language.GetTextValue("tModLoader.MSBuildReloadAll"));
			buttonBra.CopyStyle(buttonBa);
			buttonBra.HAlign = 0.5f;
			buttonBra.WithFadedMouseOver();
			buttonBra.OnClick += BuildAndReload;
			_uIElement.Append(buttonBra);

			var buttonCreateMod = new UIScalingTextPanel<string>(Language.GetTextValue("tModLoader.MSCreateMod"));
			buttonCreateMod.CopyStyle(buttonBa);
			buttonCreateMod.HAlign = 1f;
			buttonCreateMod.WithFadedMouseOver();
			buttonCreateMod.OnClick += ButtonCreateMod_OnClick;
			_uIElement.Append(buttonCreateMod);

			var buttonB = new UIScalingTextPanel<string>(Language.GetTextValue("UI.Back"));
			buttonB.CopyStyle(buttonBa);
			//buttonB.Width.Set(-10f, 1f / 3f);
			buttonB.Top.Pixels = -20;
			buttonB.WithFadedMouseOver();
			buttonB.OnClick += BackClick;
			_uIElement.Append(buttonB);

			var buttonOs = new UIScalingTextPanel<string>(Language.GetTextValue("tModLoader.MSOpenSources"));
			buttonOs.CopyStyle(buttonB);
			buttonOs.HAlign = .5f;
			buttonOs.WithFadedMouseOver();
			buttonOs.OnClick += OpenSources;
			_uIElement.Append(buttonOs);

			var buttonMp = new UIScalingTextPanel<string>(Language.GetTextValue("tModLoader.MSManagePublished"));
			buttonMp.CopyStyle(buttonB);
			buttonMp.HAlign = 1f;
			buttonMp.WithFadedMouseOver();
			buttonMp.OnClick += ManagePublished;
			_uIElement.Append(buttonMp);
			Append(_uIElement);
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			if (!_updateNeeded) return;
			_updateNeeded = false;
			_uIPanel.RemoveChild(_uiLoader);
			_modList.Clear();
			_modList.AddRange(_items);
		}

		internal void Populate() {
			Task.Factory.StartNew(delegate {
					var modSources = ModCompile.FindModSources();
					var modFiles = ModOrganizer.FindMods();
					return Tuple.Create(modSources, modFiles);
				})
				.ContinueWith(task => {
					var modSources = task.Result.Item1;
					var modFiles = task.Result.Item2;

					foreach (string sourcePath in modSources) {
						var builtMod = modFiles.SingleOrDefault(m => m.Name == Path.GetFileName(sourcePath));
						_items.Add(new UIModSourceItem(sourcePath, builtMod));
					}

					_updateNeeded = true;
				}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11, -1, -1, 1);
			Main.menuMode = 0;
		}

		private void BuildAndReload(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			if (_modList.Count > 0) {
				ModLoader.reloadAfterBuild = true;
				ModLoader.buildAll = true;
				Main.menuMode = Interface.buildAllModsID;
			}
		}

		private void BuildMods(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			if (_modList.Count > 0) {
				ModLoader.reloadAfterBuild = false;
				ModLoader.buildAll = true;
				Main.menuMode = Interface.buildAllModsID;
			}
		}

		private void ButtonCreateMod_OnClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11);
			Main.menuMode = Interface.createModID;
		}

		private void ManagePublished(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11, -1, -1, 1);
			Main.menuMode = Interface.managePublishedID;
		}

		private void OpenSources(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			Directory.CreateDirectory(ModCompile.ModSourcePath);
			Process.Start(ModCompile.ModSourcePath);
		}
	}
}
