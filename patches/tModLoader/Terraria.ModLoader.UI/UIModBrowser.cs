using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI
{
	// TODO jof: refactor later
	internal class UIModBrowser : UIState
	{
		public bool aModUpdated;
		public bool anEnabledModDownloaded; // disregard user preference for mods that are enabled
		public bool aNewModDownloaded;
		public bool loading;
		public UIList modList;
		public ModSideFilter modSideFilterMode = ModSideFilter.All;
		public UICycleImage ModSideFilterToggle;
		public SearchFilter searchFilterMode = SearchFilter.Name;
		public UICycleImage SearchFilterToggle;
		public UIModDownloadItem selectedItem;
		public ModBrowserSortMode sortMode = ModBrowserSortMode.RecentlyUpdated;
		public UICycleImage SortModeFilterToggle;
		public UITextPanel<string> uIHeaderTextPanel;
		public UIText uINoModsFoundText;
		public UpdateFilter updateFilterMode = UpdateFilter.Available;
		public UICycleImage UpdateFilterToggle;

		internal readonly List<UICycleImage> categoryButtons = new List<UICycleImage>();
		internal string filter;
		internal UIInputTextField filterTextBox;
		internal bool updateNeeded;

		private readonly List<UIModDownloadItem> _items = new List<UIModDownloadItem>();
		private UITextPanel<string> _clearButton;
		private UITextPanel<string> _downloadAllButton;
		private UITextPanel<string> _reloadButton;
		private List<string> _specialModPackFilter;
		private string _specialModPackFilterTitle;
		private UIElement _uIElement;
		private UILoaderAnimatedImage _uILoader;
		private UIPanel _uIPanel;
		private UITextPanel<string> _updateAllButton;
		private bool _updateAvailable;
		private string _updateText;
		private string _updateUrl;

		internal static bool PlatformSupportsTls12 {
			get {
				foreach (SecurityProtocolType protocol in Enum.GetValues(typeof(SecurityProtocolType))) {
					if (protocol.GetHashCode() == 3072) {
						return true;
					}
				}

				return false;
			}
		}

		public List<string> SpecialModPackFilter {
			get => _specialModPackFilter;
			set {
				if (_specialModPackFilter != null && value == null) {
					_uIPanel.BackgroundColor = UICommon.MAIN_PANEL_BG_COLOR;
					_uIElement.RemoveChild(_clearButton);
					_uIElement.RemoveChild(_downloadAllButton);
				}
				else if (_specialModPackFilter == null && value != null) {
					_uIPanel.BackgroundColor = Color.Purple * 0.7f;
					_uIElement.Append(_clearButton);
					_uIElement.Append(_downloadAllButton);
				}

				_specialModPackFilter = value;
			}
		}

		internal string SpecialModPackFilterTitle {
			get => _specialModPackFilterTitle;
			set {
				_clearButton.SetText(Language.GetTextValue("tModLoader.MBClearSpecialFilter", value));
				_specialModPackFilterTitle = value;
			}
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
				BackgroundColor = UICommon.MAIN_PANEL_BG_COLOR,
				PaddingTop = 0f
			};
			_uIElement.Append(_uIPanel);

			_uILoader = new UILoaderAnimatedImage(0.5f, 0.5f, 1f);

			modList = new UIList {
				Width = { Pixels = -25, Percent = 1f },
				Height = { Pixels = -50, Percent = 1f },
				Top = { Pixels = 50 },
				ListPadding = 5f
			};
			_uIPanel.Append(modList);

			var uIScrollbar = new UIScrollbar {
				Height = { Pixels = -50, Percent = 1f },
				Top = { Pixels = 50 },
				HAlign = 1f
			}.WithView(100f, 1000f);
			_uIPanel.Append(uIScrollbar);

			uINoModsFoundText = new UIText(Language.GetTextValue("tModLoader.MBNoModsFound")) {
				HAlign = 0.5f
			}.WithPadding(15f);

			modList.SetScrollbar(uIScrollbar);
			uIHeaderTextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.MenuModBrowser"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.UI_BLUE_COLOR
			}.WithPadding(15f);
			_uIElement.Append(uIHeaderTextPanel);

			_reloadButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBGettingData")) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 25 },
				VAlign = 1f,
				Top = { Pixels = -65 }
			}.WithFadedMouseOver();
			_reloadButton.OnClick += ReloadList;
			_uIElement.Append(_reloadButton);

			var backButton = new UITextPanel<string>(Language.GetTextValue("UI.Back")) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 25 },
				VAlign = 1f,
				Top = { Pixels = -20 }
			}.WithFadedMouseOver();
			backButton.OnClick += BackClick;
			_uIElement.Append(backButton);

			_clearButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBClearSpecialFilter", "??")) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 25 },
				HAlign = 1f,
				VAlign = 1f,
				Top = { Pixels = -65 },
				BackgroundColor = Color.Purple * 0.7f
			}.WithFadedMouseOver(Color.Purple, Color.Purple * 0.7f);
			_clearButton.OnClick += (s, e) => {
				// TODO: isn't `Interface.modBrowser` redundant
				Interface.modBrowser.SpecialModPackFilter = null;
				Interface.modBrowser.SpecialModPackFilterTitle = null;
				Interface.modBrowser.updateNeeded = true;
				Main.PlaySound(SoundID.MenuTick);
			};

			_downloadAllButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBDownloadAll")) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 25 },
				HAlign = 1f,
				VAlign = 1f,
				Top = { Pixels = -20 },
				BackgroundColor = Color.Azure * 0.7f
			}.WithFadedMouseOver(Color.Azure, Color.Azure * 0.7f);
			_downloadAllButton.OnClick += (s, e) => DownloadMods(SpecialModPackFilter, SpecialModPackFilterTitle);

			_updateAllButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBUpdateAll")) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 25 },
				HAlign = 1f,
				VAlign = 1f,
				Top = { Pixels = -20 },
				BackgroundColor = Color.Orange * 0.7f
			}.WithFadedMouseOver(Color.Orange, Color.Orange * 0.7f);
			_updateAllButton.OnClick += (s, e) => {
				//TODO: move all click events to separate methods, behavior buried in layout is hard to find
				if (!loading) {
					var updatableMods = _items.Where(x => x.update && !x.updateIsDowngrade).Select(x => x.mod).ToList();
					DownloadMods(updatableMods, Language.GetTextValue("tModLoader.MBUpdateAll"));
				}
			};

			Append(_uIElement);

			var upperMenuContainer = new UIElement {
				Width = { Percent = 1f },
				Height = { Pixels = 32 },
				Top = { Pixels = 10 }
			};
			var texture = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.UI.UIModBrowserIcons.png"));

			//TODO: lots of duplication in these buttons
			SortModeFilterToggle = new UICycleImage(texture, 6, 32, 32,
													0, 0);
			SortModeFilterToggle.SetCurrentState((int)sortMode);
			SortModeFilterToggle.OnClick += (a, b) => {
				sortMode = sortMode.NextEnum();
				updateNeeded = true;
			};
			SortModeFilterToggle.OnRightClick += (a, b) => {
				sortMode = sortMode.PreviousEnum();
				updateNeeded = true;
			};
			SortModeFilterToggle.Left.Pixels = 0 * 36 + 8;
			categoryButtons.Add(SortModeFilterToggle);
			upperMenuContainer.Append(SortModeFilterToggle);

			UpdateFilterToggle = new UICycleImage(texture, 3, 32, 32,
												  34, 0);
			UpdateFilterToggle.SetCurrentState((int)updateFilterMode);
			UpdateFilterToggle.OnClick += (a, b) => {
				updateFilterMode = updateFilterMode.NextEnum();
				updateNeeded = true;
			};
			UpdateFilterToggle.OnRightClick += (a, b) => {
				updateFilterMode = updateFilterMode.PreviousEnum();
				updateNeeded = true;
			};
			UpdateFilterToggle.Left.Pixels = 1 * 36 + 8;
			categoryButtons.Add(UpdateFilterToggle);
			upperMenuContainer.Append(UpdateFilterToggle);

			ModSideFilterToggle = new UICycleImage(texture, 5, 32, 32,
												   34 * 5, 0);
			ModSideFilterToggle.SetCurrentState((int)modSideFilterMode);
			ModSideFilterToggle.OnClick += (a, b) => {
				modSideFilterMode = modSideFilterMode.NextEnum();
				updateNeeded = true;
			};
			ModSideFilterToggle.OnRightClick += (a, b) => {
				modSideFilterMode = modSideFilterMode.PreviousEnum();
				updateNeeded = true;
			};
			ModSideFilterToggle.Left.Pixels = 2 * 36 + 8;
			categoryButtons.Add(ModSideFilterToggle);
			upperMenuContainer.Append(ModSideFilterToggle);

			var filterTextBoxBackground = new UIPanel {
				Top = { Percent = 0f },
				Left = { Pixels = -170, Percent = 1f },
				Width = { Pixels = 135 },
				Height = { Pixels = 40 }
			};
			filterTextBoxBackground.OnRightClick += (a, b) => filterTextBox.Text = "";
			upperMenuContainer.Append(filterTextBoxBackground);

			filterTextBox = new UIInputTextField(Language.GetTextValue("tModLoader.ModsTypeToSearch")) {
				Top = { Pixels = 5 },
				Left = { Pixels = -160, Percent = 1f },
				Width = { Pixels = 100 },
				Height = { Pixels = 10 }
			};
			filterTextBox.OnTextChange += (sender, e) => updateNeeded = true;
			upperMenuContainer.Append(filterTextBox);

			SearchFilterToggle = new UICycleImage(texture, 2, 32, 32,
												  34 * 2, 0);
			SearchFilterToggle.SetCurrentState((int)searchFilterMode);
			SearchFilterToggle.OnClick += (a, b) => {
				searchFilterMode = searchFilterMode.NextEnum();
				updateNeeded = true;
			};
			SearchFilterToggle.OnRightClick += (a, b) => {
				searchFilterMode = searchFilterMode.PreviousEnum();
				updateNeeded = true;
			};
			SearchFilterToggle.Left.Pixels = 545f;
			categoryButtons.Add(SearchFilterToggle);
			upperMenuContainer.Append(SearchFilterToggle);
			_uIPanel.Append(upperMenuContainer);
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
							text = updateFilterMode.ToFriendlyString();
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

			if (_updateAvailable) {
				_updateAvailable = false;
				Interface.updateMessage.SetMessage(_updateText);
				Interface.updateMessage.SetGotoMenu(Interface.modBrowserID);
				Interface.updateMessage.SetUrl(_updateUrl);
				Main.menuMode = Interface.updateMessageID;
			}

			UILinkPointNavigator.Shortcuts.BackButtonCommand = 101;
		}

		public static void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuClose);
			Main.menuMode = 0;
			if ((Interface.modBrowser.aModUpdated || Interface.modBrowser.aNewModDownloaded) && ModLoader.autoReloadAndEnableModsLeavingModBrowser || Interface.modBrowser.anEnabledModDownloaded) {
				Main.menuMode = Interface.reloadModsID;
			}
			//TODO why do I have to read this, the only difference between these is Enable vs Reload
			else if (Interface.modBrowser.aModUpdated && !ModLoader.dontRemindModBrowserUpdateReload) {
				Interface.infoMessage.Show(Language.GetTextValue("tModLoader.ReloadModsReminder"),
										   0, null, Language.GetTextValue("tModLoader.DontShowAgain"),
										   () => {
											   ModLoader.dontRemindModBrowserUpdateReload = true;
											   Main.SaveSettings();
										   });
			}
			else if (Interface.modBrowser.aNewModDownloaded && !ModLoader.dontRemindModBrowserDownloadEnable) {
				Interface.infoMessage.Show(Language.GetTextValue("tModLoader.EnableModsReminder"),
										   0, null, Language.GetTextValue("tModLoader.DontShowAgain"),
										   () => {
											   ModLoader.dontRemindModBrowserDownloadEnable = true;
											   Main.SaveSettings();
										   });
			}

			Interface.modBrowser.aModUpdated = false;
			Interface.modBrowser.aNewModDownloaded = false;
			Interface.modBrowser.anEnabledModDownloaded = false;
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			if (!updateNeeded) return;
			updateNeeded = false;
			if (!loading) _uIPanel.RemoveChild(_uILoader);
			filter = filterTextBox.Text;
			modList.Clear();
			modList.AddRange(_items.Where(item => item.PassFilters()));
			bool hasNoModsFoundNotif = modList.HasChild(uINoModsFoundText);
			if (modList.Count <= 0 && !hasNoModsFoundNotif)
				modList.Add(uINoModsFoundText);
			else if (hasNoModsFoundNotif)
				modList.RemoveChild(uINoModsFoundText);
			_uIElement.RemoveChild(_updateAllButton);
			if (SpecialModPackFilter == null && _items.Count(x => x.update && !x.updateIsDowngrade) > 0) {
				_uIElement.Append(_updateAllButton);
			}
		}

		public override void OnActivate() {
			Main.clrInput();
			if (!loading && _items.Count <= 0)
				PopulateModBrowser();
		}

		public void UploadComplete(object sender, UploadValuesCompletedEventArgs e) {
			if (e.Error != null) {
				ShowOfflineTroubleshootingMessage();
				if (e.Cancelled) { }
				else {
					HttpStatusCode httpStatusCode = GetHttpStatusCode(e.Error);
					if (httpStatusCode == HttpStatusCode.ServiceUnavailable) {
						SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser") + " " + Language.GetTextValue("tModLoader.MBOfflineWithReason", Language.GetTextValue("tModLoader.MBBusy")));
					}
					else {
						SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser") + " " + Language.GetTextValue("tModLoader.MBOfflineWithReason", Language.GetTextValue("tModLoader.MBUnknown")));
					}
				}

				loading = false;
				_reloadButton.SetText(Language.GetTextValue("tModLoader.MBReloadBrowser"));
			}
			else if (!e.Cancelled) {
				_reloadButton.SetText(Language.GetTextValue("tModLoader.MBPopulatingBrowser"));
				byte[] result = e.Result;
				string response = Encoding.UTF8.GetString(result);
				Task.Factory
					.StartNew(ModOrganizer.FindMods)
					.ContinueWith(task => {
						PopulateFromJson(task.Result, response);
						loading = false;
						_reloadButton.SetText(Language.GetTextValue("tModLoader.MBReloadBrowser"));
					}, TaskScheduler.FromCurrentSynchronizationContext());
			}
		}

		internal void ClearItems() {
			_items.Clear();
		}

		internal static void LogModBrowserException(Exception e) {
			string errorMessage = Language.GetTextValue("tModLoader.MBBrowserError") + "\n\n" + e.Message + "\n" + e.StackTrace;
			Logging.tML.Error(errorMessage);
			Interface.errorMessage.Show(errorMessage, 0);
		}

		internal static void LogModPublishInfo(string message) {
			Logging.tML.Info(message);
			Interface.errorMessage.Show(Language.GetTextValue("tModLoader.MBServerResponse", message), Interface.modSourcesID);
		}

		internal static void LogModUnpublishInfo(string message) {
			Logging.tML.Info(message);
			Interface.errorMessage.Show(Language.GetTextValue("tModLoader.MBServerResponse", message), Interface.managePublishedID);
		}

		private void ReloadList(UIMouseEvent evt, UIElement listeningElement) {
			if (!loading) {
				Main.PlaySound(SoundID.MenuOpen);
				PopulateModBrowser();
			}
		}

		private void PopulateModBrowser() {
			loading = true;
			SpecialModPackFilter = null;
			SpecialModPackFilterTitle = null;
			_reloadButton.SetText(Language.GetTextValue("tModLoader.MBGettingData"));
			SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser"));
			_uIPanel.Append(_uILoader);
			modList.Clear();
			_items.Clear();
			modList.Deactivate();
			try {
				ServicePointManager.Expect100Continue = false;
				string url = "http://javid.ddns.net/tModLoader/listmods.php";
				var values = new NameValueCollection {
					{ "modloaderversion", ModLoader.versionedName },
					{ "platform", ModLoader.compressedPlatformRepresentation }
				};
				using (WebClient client = new WebClient()) {
					ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, policyErrors) => { return true; };
					client.UploadValuesCompleted += UploadComplete;
					client.UploadValuesAsync(new Uri(url), "POST", values);
				}
			}
			catch (WebException e) {
				ShowOfflineTroubleshootingMessage();
				if (e.Status == WebExceptionStatus.Timeout) {
					SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser") + " " + Language.GetTextValue("tModLoader.MBOfflineWithReason", Language.GetTextValue("tModLoader.MBBusy")));
					return;
				}

				if (e.Status == WebExceptionStatus.ProtocolError) {
					var resp = (HttpWebResponse)e.Response;
					if (resp.StatusCode == HttpStatusCode.NotFound) {
						SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser") + " " + Language.GetTextValue("tModLoader.MBOfflineWithReason", resp.StatusCode));
						return;
					}

					SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser") + " " + Language.GetTextValue("tModLoader.MBOfflineWithReason", resp.StatusCode));
				}
			}
			catch (Exception e) {
				LogModBrowserException(e);
			}
		}

		private void PopulateFromJson(LocalMod[] installedMods, string json) {
			string tls = PlatformSupportsTls12 ? "&tls12=y" : "";
			try {
				JObject jsonObject;
				try {
					jsonObject = JObject.Parse(json);
				}
				catch (Exception e) {
					throw new Exception("Bad JSON: " + json, e);
				}

				JObject updateObject = (JObject)jsonObject["update"];
				if (updateObject != null) {
					_updateAvailable = true;
					_updateText = (string)updateObject["message"];
					_updateUrl = (string)updateObject["url"];
				}

				JArray modlist = (JArray)jsonObject["modlist"];
				foreach (JObject mod in modlist.Children<JObject>()) {
					string displayname = (string)mod["displayname"];
					//reloadButton.SetText("Adding " + displayname + "...");
					string name = (string)mod["name"];
					string version = (string)mod["version"];
					string author = (string)mod["author"];
					string download = ((string)mod["download"] ?? $"http://javid.ddns.net/tModLoader/download.php?Down=mods/{name}.tmod") + tls;
					int downloads = (int)mod["downloads"];
					int hot = (int)mod["hot"]; // for now, hotness is just downloadsYesterday
					string timeStamp = (string)mod["updateTimeStamp"];
					//string[] modreferences = ((string)mod["modreferences"]).Split(',');
					string modreferences = (string)mod["modreferences"] ?? "";
					ModSide modside = ModSide.Both; // TODO: add filter option for modside.
					string modIconUrl = (string)mod["iconurl"];
					string modsideString = (string)mod["modside"];
					if (modsideString == "Client") modside = ModSide.Client;
					if (modsideString == "Server") modside = ModSide.Server;
					if (modsideString == "NoSync") modside = ModSide.NoSync;
					//bool exists = false; // unused?
					bool update = false;
					bool updateIsDowngrade = false;
					var installed = installedMods.FirstOrDefault(m => m.Name == name);
					if (installed != null) {
						//exists = true;
						var cVersion = new Version(version.Substring(1));
						if (cVersion > installed.modFile.version)
							update = true;
						else if (cVersion < installed.modFile.version)
							update = updateIsDowngrade = true;
					}

					UIModDownloadItem modItem = new UIModDownloadItem(displayname, name, version, author,
																	  modreferences, modside, modIconUrl, download,
																	  downloads, hot, timeStamp, update,
																	  updateIsDowngrade, installed);
					_items.Add(modItem);
				}

				updateNeeded = true;
			}
			catch (Exception e) {
				LogModBrowserException(e);
			}
		}

		private void DownloadMods(List<string> specialModPackFilter, string specialModPackFilterTitle) {
			Main.PlaySound(SoundID.MenuTick);
			Interface.downloadMods.SetDownloading(specialModPackFilterTitle);
			Interface.downloadMods.SetModsToDownload(specialModPackFilter, _items);
			Interface.modBrowser.updateNeeded = true;
			Main.menuMode = Interface.downloadModsID;
		}

		private void SetHeading(string heading) {
			uIHeaderTextPanel.SetText(heading, 0.8f, true);
			uIHeaderTextPanel.Recalculate();
		}

		private void ShowOfflineTroubleshootingMessage() {
			var message = new UIMessageBox(Language.GetTextValue("tModLoader.MBOfflineTroubleshooting")) {
				Width = { Percent = 1 },
				Height = { Pixels = 400, Percent = 0 }
			};
			message.OnDoubleClick += (a, b) => { Process.Start("http://javid.ddns.net/tModLoader/DirectModDownloadListing.php"); };
			modList.Add(message);
			message.SetScrollbar(new UIScrollbar());
			_uIPanel.RemoveChild(_uILoader);
		}

		//unused
		//public XmlDocument GetDataFromUrl(string url)
		//{
		//	XmlDocument urlData = new XmlDocument();
		//	HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(url);
		//	rq.Timeout = 5000;
		//	HttpWebResponse response = rq.GetResponse() as HttpWebResponse;
		//	using (Stream responseStream = response.GetResponseStream())
		//	{
		//		XmlTextReader reader = new XmlTextReader(responseStream);
		//		urlData.Load(reader);
		//	}
		//	return urlData;
		//}

		private HttpStatusCode GetHttpStatusCode(Exception err) {
			if (err is WebException) {
				WebException we = (WebException)err;
				if (we.Response is HttpWebResponse) {
					HttpWebResponse response = (HttpWebResponse)we.Response;
					return response.StatusCode;
				}
			}

			return 0;
		}
	}

	public static class ModBrowserSortModesExtensions
	{
		public static string ToFriendlyString(this ModBrowserSortMode sortmode) {
			switch (sortmode) {
				case ModBrowserSortMode.DisplayNameAtoZ:
					return Language.GetTextValue("tModLoader.ModsSortNamesAlph");
				case ModBrowserSortMode.DisplayNameZtoA:
					return Language.GetTextValue("tModLoader.ModsSortNamesReverseAlph");
				case ModBrowserSortMode.DownloadsDescending:
					return Language.GetTextValue("tModLoader.MBSortDownloadDesc");
				case ModBrowserSortMode.DownloadsAscending:
					return Language.GetTextValue("tModLoader.MBSortDownloadAsc");
				case ModBrowserSortMode.RecentlyUpdated:
					return Language.GetTextValue("tModLoader.MBSortByRecentlyUpdated");
				case ModBrowserSortMode.Hot:
					return Language.GetTextValue("tModLoader.MBSortByPopularity");
			}

			return "Unknown Sort";
		}
	}

	public static class UpdateFilterModesExtensions
	{
		public static string ToFriendlyString(this UpdateFilter updateFilterMode) {
			switch (updateFilterMode) {
				case UpdateFilter.All:
					return Language.GetTextValue("tModLoader.MBShowAllMods");
				case UpdateFilter.Available:
					return Language.GetTextValue("tModLoader.MBShowNotInstalledUpdates");
				case UpdateFilter.UpdateOnly:
					return Language.GetTextValue("tModLoader.MBShowUpdates");
			}

			return "Unknown Sort";
		}
	}

	public static class ModSideFilterModesExtensions
	{
		public static string ToFriendlyString(this ModSideFilter modSideFilterMode) {
			switch (modSideFilterMode) {
				case ModSideFilter.All:
					return Language.GetTextValue("tModLoader.MBShowMSAll");
				case ModSideFilter.Both:
					return Language.GetTextValue("tModLoader.MBShowMSBoth");
				case ModSideFilter.Client:
					return Language.GetTextValue("tModLoader.MBShowMSClient");
				case ModSideFilter.Server:
					return Language.GetTextValue("tModLoader.MBShowMSServer");
				case ModSideFilter.NoSync:
					return Language.GetTextValue("tModLoader.MBShowMSNoSync");
			}

			return "Unknown Sort";
		}
	}

	public static class SearchFilterModesExtensions
	{
		public static string ToFriendlyString(this SearchFilter searchFilterMode) {
			switch (searchFilterMode) {
				case SearchFilter.Name:
					return Language.GetTextValue("tModLoader.ModsSearchByModName");
				case SearchFilter.Author:
					return Language.GetTextValue("tModLoader.ModsSearchByAuthor");
			}

			return "Unknown Sort";
		}
	}

	public enum ModBrowserSortMode
	{
		DisplayNameAtoZ,
		DisplayNameZtoA,
		DownloadsDescending,
		DownloadsAscending,
		RecentlyUpdated,
		Hot
	}

	public enum UpdateFilter
	{
		All,
		Available,
		UpdateOnly
	}

	public enum SearchFilter
	{
		Name,
		Author
	}

	public enum ModSideFilter
	{
		All,
		Both,
		Client,
		Server,
		NoSync
	}
}
