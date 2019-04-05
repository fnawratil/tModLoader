using Microsoft.Xna.Framework;
using System;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UILoadMods : UIState
	{
		public int modCount;

		private string _stageText;
		private UILoadProgress _loadProgress;
		private UIText _subProgress;

		public override void OnInitialize() {
			_loadProgress = new UILoadProgress {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MAX_PANEL_WIDTH,
				Height = { Pixels = 150 },
				HAlign = 0.5f,
				VAlign = 0.5f,
				Top = { Pixels = 10 }
			};
			Append(_loadProgress);

			_subProgress = new UIText("", 0.5f, true) {
				Top = { Pixels = 65 },
				HAlign = 0.5f,
				VAlign = 0.5f
			};
			Append(_subProgress);
		}

		public override void OnActivate() {
			ModLoader.BeginLoad();
			GLCallLocker.ActionsAreSpeedrun = true;
		}

		public override void OnDeactivate() {
			GLCallLocker.ActionsAreSpeedrun = false;
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			GLCallLocker.SpeedrunActions();
		}

		public string SubProgressText {
			set => _subProgress?.SetText(value);
		}

		public void SetLoadStage(string stageText, int modCount = -1) {
			_stageText = stageText;
			this.modCount = modCount;
			if (modCount < 0)
				SetProgressText(Language.GetTextValue(stageText));

			_loadProgress?.SetProgress(0);
			SubProgressText = "";
		}

		private void SetProgressText(string text) {
			Logging.tML.Info(text);
			if (Main.dedServ)
				Console.WriteLine(text);
			else
				_loadProgress.SetText(text);
		}

		public void SetCurrentMod(int i, string mod) {
			SetProgressText(Language.GetTextValue(_stageText, mod));
			_loadProgress?.SetProgress(i / (float)modCount);
		}
	}
}
