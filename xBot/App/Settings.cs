﻿using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows.Forms;
using xBot.Game;

namespace xBot
{
	public static class Settings{
		/// <summary>
		/// Avoid loading overload
		/// </summary>
		private static bool LoadingBotSettings = true;
		private static readonly object BotSettingsLock = new object();
		/// <summary>
		/// Avoid saving settings while is loading
		/// </summary>
		private static bool LoadingCharacterSettings;
		private static readonly object CharacterSettingsLock = new object();
		public static void SaveBotSettings()
		{
			if (!LoadingBotSettings)
			{
				lock (BotSettingsLock)
				{
					JObject root = new JObject();
					// Generating/editing nodes manually
					Window w = Window.Get;

					#region (Silkroad Tab)
					JObject silkroadList = new JObject();
					root["Silkroad"] = silkroadList;

					foreach (TreeNode node in w.Settings_lstrSilkroads.Nodes)
					{
						JObject server = new JObject();
						silkroadList[node.Name] = server;

						JArray hosts = new JArray();
						foreach (TreeNode host in node.Nodes["Hosts"].Nodes)
							hosts.Add(host.Text);
						server["Hosts"] = hosts;
						server["RandomHost"] = (bool)node.Nodes["RandomHost"].Tag;
						server["Port"] = (ushort)node.Nodes["Port"].Tag;
						if (node.Nodes.ContainsKey("LauncherPath"))
							server["LauncherPath"] = (string)node.Nodes["LauncherPath"].Tag;
						if (node.Nodes.ContainsKey("ClientPath"))
							server["ClientPath"] = (string)node.Nodes["ClientPath"].Tag;
						server["Version"] = (uint)node.Nodes["Version"].Tag;
						server["Locale"] = (byte)node.Nodes["Locale"].Tag;

						JObject hwid = new JObject();
						server["HWID"] = hwid;

						JObject hwidclient = new JObject();
						hwid["Client"] = hwidclient;
						hwidclient["Opcode"] = ((ushort)node.Nodes["HWID"].Nodes["Client"].Nodes["Opcode"].Tag).ToString("X4");
						hwidclient["SaveFrom"] = (string)node.Nodes["HWID"].Nodes["Client"].Nodes["SaveFrom"].Tag;

						JObject hwidserver = new JObject();
						hwid["Server"] = hwidserver;
						hwidserver["Opcode"] = ((ushort)node.Nodes["HWID"].Nodes["Server"].Nodes["Opcode"].Tag).ToString("X4");
						hwidserver["SendTo"] = (string)node.Nodes["HWID"].Nodes["Server"].Nodes["SendTo"].Tag;

						hwid["SendOnlyOnce"] = (bool)node.Nodes["HWID"].Nodes["SendOnlyOnce"].Tag;
					}
					#endregion

					#region (AutoMagically Tab)
					JObject autoMagic = new JObject();
					root["AutoMagically"] = autoMagic;

					autoMagic["SelectFirstCharacter"] = w.Settings_cbxSelectFirstChar.Checked;
					autoMagic["CreateCharacter"] = w.Settings_cbxCreateChar.Checked;
					autoMagic["CreateCharacterBelow40"] = w.Settings_cbxCreateCharBelow40.Checked;
					autoMagic["CreateCustomName"] = w.Settings_tbxCustomName.Text;
					autoMagic["CreateCustomSequence"] = w.Settings_tbxCustomSequence.Text;
					autoMagic["CreateCustomRace"] = w.Settings_cmbxCreateCharRace.Text;
					autoMagic["CreateCustomGenre"] = w.Settings_cmbxCreateCharGenre.Text;
					autoMagic["DeleteCharacterFrom40To50"] = w.Settings_cbxDeleteChar40to50.Checked;
					autoMagic["LoadDefaultConfigs"] = w.Settings_cbxLoadDefaultConfigs.Checked;
					#endregion

					#region (Packet Analyzer Tab)
					JObject packetAnalyzer = new JObject();
					root["PacketAnalyzer"] = packetAnalyzer;

					JArray opcodes = new JArray();
					WinAPI.InvokeIfRequired(w.Settings_lstvOpcodes, () => {
						foreach (ListViewItem opcode in w.Settings_lstvOpcodes.Items)
							opcodes.Add(opcode.Text);
					});

					packetAnalyzer["Filter"] = opcodes;
					packetAnalyzer["FilterOnlyShow"] = w.Settings_rbnPacketOnlyShow.Checked;
					#endregion

					// File info
					root["_xBot"] = "ProjexNET | Easy & Flexible. Design perfection!";
					root["_Version"] = w.ProductVersion;
					root["_ContactMe"] = "Engels [JellyBitz] Quintero | Discord: JellyBitz#7643";
					// Saving
					File.WriteAllText("Settings.json", root.ToString());
				}
			}
		}
		/// <summary>
		/// Load bot settings or creates a new one.
		/// </summary>
		/// <returns></returns>
		public static void LoadBotSettings()
		{
			string path = "Settings.json";
			if (File.Exists(path))
			{
				try
				{
					LoadingBotSettings = true;
					LoadBotSettings(path);
					LoadingBotSettings = false;
				}
				catch
				{
					LoadingBotSettings = false;
					Window.Get.Log("Error loading bot settings.. using the settings partially loaded");
					File.Move(path, path+".bkp");
					SaveBotSettings();
				}
			}
			else
			{
				SaveBotSettings();
			}
		}
		/// <summary>
		/// Load bot settings if exists.
		/// </summary>
		private static void LoadBotSettings(string path)
		{
			lock (BotSettingsLock)
			{
				Window w = Window.Get;
				JObject root = JObject.Parse(File.ReadAllText(path));

				#region (Silkroad Tab)
				JObject silkroadList = (JObject)root["Silkroad"];
				foreach (JProperty key in silkroadList.Properties())
				{
					JObject silkroad = (JObject)silkroadList[key.Name];

					TreeNode s = new TreeNode(key.Name);
					s.Name = s.Text;

					TreeNode node = new TreeNode("Hosts");
					node.Name = "Hosts";
					foreach (JToken host in (JArray)silkroad["Hosts"])
					{
						node.Nodes.Add((string)host);
					}
					s.Nodes.Add(node);
					node = new TreeNode("Use random host : " + ((bool)silkroad["Port"] ? "Yes" : "No"));
					node.Name = "RandomHost";
					node.Tag = (bool)silkroad["RandomHost"];
					s.Nodes.Add(node);

					node = new TreeNode("Port : " + silkroad["Port"]);
					node.Name = "Port";
					node.Tag = (ushort)silkroad["Port"];
					s.Nodes.Add(node);

					if (silkroad.ContainsKey("LauncherPath"))
					{
						node = new TreeNode("Launcher Path : " + silkroad["LauncherPath"]);
						node.Name = "LauncherPath";
						node.Tag = (string)silkroad["LauncherPath"];
						s.Nodes.Add(node);
					}
					if (silkroad.ContainsKey("ClientPath"))
					{
						node = new TreeNode("Client Path : " + silkroad["ClientPath"]);
						node.Name = "ClientPath";
						node.Tag = (string)silkroad["ClientPath"];
						s.Nodes.Add(node);
					}

					node = new TreeNode("Version : " + silkroad["Version"]);
					node.Name = "Version";
					node.Tag = (uint)silkroad["Version"];
					s.Nodes.Add(node);

					node = new TreeNode("Locale : " + silkroad["Locale"]);
					node.Name = "Locale";
					node.Tag = (byte)silkroad["Locale"];
					s.Nodes.Add(node);

					ushort hwidClientOp = ushort.Parse((string)silkroad["HWID"]["Client"]["Opcode"], System.Globalization.NumberStyles.HexNumber);
					ushort hwidServerOp = ushort.Parse((string)silkroad["HWID"]["Server"]["Opcode"], System.Globalization.NumberStyles.HexNumber);
					TreeNode hwid = new TreeNode("HWID Setup (" + (hwidClientOp == 0 && hwidServerOp == 0 ? "Off" : "On") + ")");
					hwid.Name = "HWID";

					TreeNode hwidclient = new TreeNode("Client");
					hwidclient.Name = "Client";
					hwid.Nodes.Add(hwidclient);
					node = new TreeNode("Opcode : " + (hwidClientOp == 0 ? "None" : "0x" + hwidClientOp.ToString("X4")));
					node.Name = "Opcode";
					node.Tag = hwidClientOp;
					hwidclient.Nodes.Add(node);
					node = new TreeNode("Save from : " + silkroad["HWID"]["Client"]["SaveFrom"].ToString());
					node.Name = "SaveFrom";
					node.Tag = (string)silkroad["HWID"]["Client"]["SaveFrom"];
					hwidclient.Nodes.Add(node);

					TreeNode hwidserver = new TreeNode("Server");
					hwidserver.Name = "Server";
					hwid.Nodes.Add(hwidserver);
					node = new TreeNode("Opcode : " + (hwidServerOp == 0 ? "None" : "0x" + hwidServerOp.ToString("X4")));
					node.Name = "Opcode";
					node.Tag = hwidServerOp;
					hwidserver.Nodes.Add(node);
					node = new TreeNode("Send to : " + silkroad["HWID"]["Server"]["SendTo"].ToString());
					node.Name = "SendTo";
					node.Tag = (string)silkroad["HWID"]["Server"]["SendTo"];
					hwidserver.Nodes.Add(node);

					string hwidData = "";
					if (File.Exists("Data\\" + key.Name + ".hwid"))
						hwidData = WinAPI.ToHexString(File.ReadAllBytes("Data\\" + key.Name + ".hwid"));
					node = new TreeNode("Data : " + (hwidData == "" ? "None" : hwidData));
					node.Name = "Data";
					node.Tag = hwidData;
					hwid.Nodes.Add(node);
					node = new TreeNode("Send Data only once : " + (bool.Parse(silkroad["HWID"]["SendOnlyOnce"].ToString()) ? "Yes" : "No"));
					node.Name = "SendOnlyOnce";
					node.Tag = (bool)silkroad["HWID"]["SendOnlyOnce"];
					hwid.Nodes.Add(node);

					s.Nodes.Add(hwid);

					Window.Get.Settings_lstrSilkroads.Nodes.Add(s);
					Window.Get.Login_cmbxSilkroad.Items.Add(s.Name);
				}
				#endregion

				#region (Auto Magically Tab)
				JObject autoMagic = (JObject)root["AutoMagically"];
				
				w.Settings_cbxSelectFirstChar.Checked = (bool)autoMagic["SelectFirstCharacter"];
				w.Settings_cbxCreateChar.Checked = (bool)autoMagic["CreateCharacter"];
				w.Settings_cbxCreateCharBelow40.Checked = (bool)autoMagic["CreateCharacterBelow40"];
				w.Settings_tbxCustomName.Text = (string)autoMagic["CreateCustomName"];
				w.Settings_tbxCustomSequence.Text = (string)autoMagic["CreateCustomSequence"];
				w.Settings_cmbxCreateCharRace.Text = (string)autoMagic["CreateCustomRace"];
				w.Settings_cmbxCreateCharGenre.Text = (string)autoMagic["CreateCustomGenre"];
				w.Settings_cbxDeleteChar40to50.Checked = (bool)autoMagic["DeleteCharacterFrom40To50"];
				w.Settings_cbxLoadDefaultConfigs.Checked = (bool)autoMagic["LoadDefaultConfigs"];
				#endregion

				#region (Packet Analyzer Tab)
				JObject packetAnalyzer = (JObject)root["PacketAnalyzer"];

				foreach (JToken opcode in (JArray)packetAnalyzer["Filter"])
				{
					ListViewItem item = new ListViewItem((string)opcode);
					item.Name = item.Text;
					w.Settings_lstvOpcodes.Items.Add(item);
				}
				
				w.Settings_rbnPacketOnlyShow.Checked = (bool)packetAnalyzer["FilterOnlyShow"];
				#endregion
			}
		}
		/// <summary>
		/// Save the character settings if it's in game.
		/// </summary>
		public static void SaveCharacterSettings()
		{
			if (!LoadingCharacterSettings && Bot.Get.inGame)
			{
				lock (CharacterSettingsLock)
				{
					Window w = Window.Get;

					// Generating nodes manually
					JObject root = new JObject();

					#region (Character Tab)
					JObject Character = new JObject();
					root["Character"] = Character;

					JObject Inf = new JObject();
					Character["Info"] = Inf;
					Inf["ShowExp"] = w.Character_cbxMessageExp.Checked;
					Inf["ShowUniques"] = w.Character_cbxMessageUniques.Checked;
					Inf["ShowEvents"] = w.Character_cbxMessageEvents.Checked;
					Inf["ShowPicks"] = w.Character_cbxMessagePicks.Checked;

					JObject Potions = new JObject();
					Character["Potions"] = Potions;
					Potions["UseHP"] = w.Character_cbxUseHP.Checked;
					Potions["UseHPPercent"] = w.Character_tbxUseHP.Text;
					Potions["UseHPGrain"] = w.Character_cbxUseHPGrain.Checked;
					Potions["UseHPVigor"] = w.Character_cbxUseHPVigor.Checked;
					Potions["UseMP"] = w.Character_cbxUseMP.Checked;
					Potions["UseMPPercent"] = w.Character_tbxUseMP.Text;
					Potions["UseMPGrain"] = w.Character_cbxUseMPGrain.Checked;
					Potions["UseMPVigor"] = w.Character_cbxUseMPVigor.Checked;
					Potions["UseUniversalPills"] = w.Character_cbxUsePillUniversal.Checked;
					Potions["UsePurificationPills"] = w.Character_cbxUsePillPurification.Checked;
					Potions["UsePetHP"] = w.Character_cbxUsePetHP.Checked;
					Potions["UsePetHPPercent"] = w.Character_tbxUsePetHP.Text;
					Potions["UseTransportHP"] = w.Character_cbxUseTransportHP.Checked;
					Potions["UseTransportHPPercent"] = w.Character_tbxUseTransportHP.Text;

					JObject Misc = new JObject();
					Character["Misc"] = Misc;
					Misc["AcceptRess"] = w.Character_cbxAcceptRess.Checked;
					Misc["AcceptRessPartyOnly"] = w.Character_cbxAcceptRessPartyOnly.Checked;
					#endregion

					#region (Party Tab)
					JObject Party = new JObject();
					root["Party"] = Party;

					JObject Options = new JObject();
					Party["Options"] = Options;

					Options["ExpFree"] = w.Party_rbnSetupExpFree.Checked;
					Options["ItemFree"] = w.Party_rbnSetupItemFree.Checked;
					Options["OnlyMasterInvite"] = w.Party_cbxSetupMasterInvite.Checked;
					Options["AcceptOnlyPartySetup"] = w.Party_cbxAcceptOnlyPartySetup.Checked;
					Options["AcceptAll"] = w.Party_cbxAcceptAll.Checked;
					Options["AcceptPartyList"] = w.Party_cbxAcceptPartyList.Checked;
					Options["AcceptLeaderList"] = w.Party_cbxAcceptLeaderList.Checked;
					Options["LeavePartyLeaderNotFound"] = w.Party_cbxLeavePartyNoneLeader.Checked;
					Options["RefusePartys"] = w.Party_cbxRefusePartys.Checked;
					Options["InviteOnlyPartySetup"] = w.Party_cbxInviteOnlyPartySetup.Checked;
					Options["InviteAll"] = w.Party_cbxInviteAll.Checked;
					Options["InvitePartyList"] = w.Party_cbxInvitePartyList.Checked;

					JArray players = new JArray();
					foreach (ListViewItem item in w.Party_lstvPartyList.Items)
						players.Add(item.Text);
					Options["PartyList"] = players;
					JArray leaders = new JArray();
					foreach (ListViewItem item in w.Party_lstvLeaderList.Items)
						leaders.Add(item.Text);
					Options["LeaderList"] = leaders;
					#endregion

					// Saving
					Info i = Info.Get;
					File.WriteAllText("Config\\" + i.Silkroad + "_" + i.Server + "_" + i.Charname + ".json", root.ToString());
				}
			}
		}
		/// <summary>
		/// Load character settings if exists or load default if it's required.
		/// </summary>
		public static void LoadCharacterSettings()
		{
			// Check folder
			if (!Directory.Exists("Config"))
				Directory.CreateDirectory("Config");

			Window w = Window.Get;
			Info i = Info.Get;
			// Check config path
			string cfgPath = "Config\\"+i.Silkroad + "_" + i.Server + "_" + i.Charname + ".json";
			WinAPI.InvokeIfRequired(w,()=> {
				try {
					LoadingCharacterSettings = true;
          if (File.Exists(cfgPath))
					{
						LoadCharacterSettings(cfgPath);
					}
					else if (w.Settings_cbxLoadDefaultConfigs.Checked && File.Exists("Config\\Default.json"))
					{
						LoadCharacterSettings("Config\\Default.json");
					}
					LoadingCharacterSettings = false;
				}
				catch
				{
					LoadingCharacterSettings = false;
					w.Log("Error loading bot settings.. using the settings partially loaded");
					File.Move(cfgPath, cfgPath + ".bkp");
					SaveCharacterSettings();
				}
			});
		}
		private static void LoadCharacterSettings(string path)
		{
			lock (CharacterSettingsLock)
			{
				LoadingCharacterSettings = true;
				JObject root = JObject.Parse(File.ReadAllText(path));

				Window w = Window.Get;
				#region (Character Tab)
				JObject Character = (JObject)root["Character"];

				JObject Inf = (JObject)Character["Info"];
				if (Inf.ContainsKey("ShowExp"))
					w.Character_cbxMessageExp.Checked = (bool)Inf["ShowExp"];
				else
					w.Character_cbxMessageExp.Checked = false;
				if (Inf.ContainsKey("ShowUniques"))
					w.Character_cbxMessageUniques.Checked = (bool)Inf["ShowUniques"];
				else
					w.Character_cbxMessageUniques.Checked = false;
				if (Inf.ContainsKey("ShowEvents"))
					w.Character_cbxMessageEvents.Checked = (bool)Inf["ShowEvents"];
				else
					w.Character_cbxMessageEvents.Checked = false;
				if (Inf.ContainsKey("ShowPicks"))
					w.Character_cbxMessagePicks.Checked = (bool)Inf["ShowPicks"];
				else
					w.Character_cbxMessagePicks.Checked = false;

				JObject Potions = (JObject)Character["Potions"];
				if (Potions.ContainsKey("UseHP"))
					w.Character_cbxUseHP.Checked = (bool)Potions["UseHP"];
				else
					w.Character_cbxUseHP.Checked = false;
				if (Potions.ContainsKey("UseHPPercent"))
					w.Character_tbxUseHP.Text = (string)Potions["UseHPPercent"];
				else
					w.Character_tbxUseHP.Text = "50";
				if (Potions.ContainsKey("UseHPGrain"))
					w.Character_cbxUseHPGrain.Checked = (bool)Potions["UseHPGrain"];
				else
					w.Character_cbxUseHPGrain.Checked = false;
				if (Potions.ContainsKey("UseHPVigor"))
					w.Character_cbxUseHPVigor.Checked = (bool)Potions["UseHPVigor"];
				else
					w.Character_cbxUseHPVigor.Checked = false;
				if (Potions.ContainsKey("UseMP"))
					w.Character_cbxUseMP.Checked = (bool)Potions["UseMP"];
				else
					w.Character_cbxUseHPVigor.Checked = false;
				if (Potions.ContainsKey("UseMPPercent"))
					w.Character_tbxUseMP.Text = (string)Potions["UseMPPercent"];
				else
					w.Character_tbxUseMP.Text = "50";
				if (Potions.ContainsKey("UseMPGrain"))
					w.Character_cbxUseMPGrain.Checked = (bool)Potions["UseMPGrain"];
				else
					w.Character_cbxUseMPGrain.Checked = false;
				if (Potions.ContainsKey("UseMPVigor"))
					w.Character_cbxUseMPVigor.Checked = (bool)Potions["UseMPVigor"];
				else
					w.Character_cbxUseMPVigor.Checked = false;
				if (Potions.ContainsKey("UseUniversalPills"))
					w.Character_cbxUsePillUniversal.Checked = (bool)Potions["UseUniversalPills"];
				else
					w.Character_cbxUsePillUniversal.Checked = false;
				if (Potions.ContainsKey("UsePurificationPills"))
					w.Character_cbxUsePillPurification.Checked = (bool)Potions["UsePurificationPills"];
				else
					w.Character_cbxUsePillPurification.Checked = false;
				if (Potions.ContainsKey("UsePetHP"))
					w.Character_cbxUsePetHP.Checked = (bool)Potions["UsePetHP"];
				else
					w.Character_cbxUsePetHP.Checked = false;
				if (Potions.ContainsKey("UsePetHPPercent"))
					w.Character_tbxUsePetHP.Text = (string)Potions["UsePetHPPercent"];
				else
					w.Character_tbxUsePetHP.Text = "50";
				if (Potions.ContainsKey("UseTransportHP"))
					w.Character_cbxUseTransportHP.Checked = (bool)Potions["UseTransportHP"];
				else
					w.Character_cbxUseTransportHP.Checked = false;
				if (Potions.ContainsKey("UseTransportHPPercent"))
					w.Character_tbxUseTransportHP.Text = (string)Potions["UseTransportHPPercent"];
				else
					w.Character_tbxUseTransportHP.Text = "50";

				JObject Misc = (JObject)Character["Misc"];
				if (Misc.ContainsKey("AcceptRess"))
					w.Character_cbxAcceptRess.Checked = (bool)Misc["AcceptRess"];
				else
					w.Character_cbxAcceptRess.Checked = false;
				if (Misc.ContainsKey("AcceptRessPartyOnly"))
					w.Character_cbxAcceptRessPartyOnly.Checked = (bool)Misc["AcceptRessPartyOnly"];
				else
					w.Character_cbxAcceptRessPartyOnly.Checked = false;
				#endregion

				#region (Party Tab)
				JObject Party = (JObject)root["Party"];

				JObject Options = (JObject)Party["Options"];
				if (Options.ContainsKey("ExpFree"))
					w.Party_rbnSetupExpFree.Checked = (bool)Options["ExpFree"];
				else
					w.Party_rbnSetupExpFree.Checked = true;
				w.Party_rbnSetupExpShared.Checked = !w.Party_rbnSetupExpFree.Checked;

				if (Options.ContainsKey("ItemFree"))
					w.Party_rbnSetupItemFree.Checked = (bool)Options["ItemFree"];
				else
					w.Party_rbnSetupItemFree.Checked = true;
				w.Party_rbnSetupItemShared.Checked = !w.Party_rbnSetupItemFree.Checked;

				if (Options.ContainsKey("OnlyMasterInvite"))
					w.Party_cbxSetupMasterInvite.Checked = (bool)Options["OnlyMasterInvite"];
				else
					w.Party_cbxSetupMasterInvite.Checked = false;
				if (Options.ContainsKey("AcceptOnlyPartySetup"))
					w.Party_cbxAcceptOnlyPartySetup.Checked = (bool)Options["AcceptOnlyPartySetup"];
				else
					w.Party_cbxAcceptOnlyPartySetup.Checked = false;
				if (Options.ContainsKey("AcceptAll"))
					w.Party_cbxAcceptAll.Checked = (bool)Options["AcceptAll"];
				else
					w.Party_cbxAcceptAll.Checked = false;
				if (Options.ContainsKey("AcceptPartyList"))
					w.Party_cbxAcceptPartyList.Checked = (bool)Options["AcceptPartyList"];
				else
					w.Party_cbxAcceptPartyList.Checked = false;
				if (Options.ContainsKey("AcceptLeaderList"))
					w.Party_cbxAcceptLeaderList.Checked = (bool)Options["AcceptLeaderList"];
				else
					w.Party_cbxAcceptLeaderList.Checked = false;
				if (Options.ContainsKey("LeavePartyLeaderNotFound"))
					w.Party_cbxLeavePartyNoneLeader.Checked = (bool)Options["LeavePartyLeaderNotFound"];
				else
					w.Party_cbxLeavePartyNoneLeader.Checked = false;
				if (Options.ContainsKey("RefusePartys"))
					w.Party_cbxRefusePartys.Checked = (bool)Options["RefusePartys"];
				else
					w.Party_cbxRefusePartys.Checked = false;
				if (Options.ContainsKey("InviteOnlyPartySetup"))
					w.Party_cbxInviteOnlyPartySetup.Checked = (bool)Options["InviteOnlyPartySetup"];
				else
					w.Party_cbxInviteOnlyPartySetup.Checked = false;
				if (Options.ContainsKey("InviteAll"))
					w.Party_cbxInviteAll.Checked = (bool)Options["InviteAll"];
				else
					w.Party_cbxInviteAll.Checked = false;
				if (Options.ContainsKey("InvitePartyList"))
					w.Party_cbxInvitePartyList.Checked = (bool)Options["InvitePartyList"];
				else
					w.Party_cbxInvitePartyList.Checked = false;

				w.Party_lstvPartyList.Items.Clear();
				if (Options.ContainsKey("PartyList"))
				{
					foreach (JToken player in (JArray)Options["PartyList"])
					{
						ListViewItem item = new ListViewItem((string)player);
						item.Name = item.Text.ToLower();
						w.Party_lstvPartyList.Items.Add(item);
					}
				}

				w.Party_lstvLeaderList.Items.Clear();
        if (Options.ContainsKey("LeaderList"))
				{
					foreach (JToken leader in (JArray)Options["LeaderList"])
					{
						ListViewItem item = new ListViewItem((string)leader);
						item.Name = item.Text.ToLower();
						w.Party_lstvLeaderList.Items.Add(item);
					}
				}
				#endregion

				LoadingCharacterSettings = false;
			}
		}
	}
}