using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using System.Diagnostics.CodeAnalysis;

namespace HookPlugin
{

	public class HookPlugin : BasePlugin, IPluginConfig<HookPluginConfig>
	{
		public override string ModuleName => "HookPlugin";
		public override string ModuleVersion => "1.0.4";
		public override string ModuleAuthor => "Roxy";


		public HookPluginConfig Config { get; set; } = new HookPluginConfig();

		public void OnConfigParsed(HookPluginConfig config)
		{
			config.Prefix = StringExtensions.ReplaceColorTags(config.Prefix);
			Config = config;
		}

		private static Dictionary<ulong, CEnvBeam> PlayersGrapples = new();
		private static List<ulong> HasHookPlayers = new();

		private static bool HookEnabledForCt = true;
		private static bool HookEnabledForT = true;

		public override void Load(bool hotReload)
		{
			AddCommand(Config.Commands.Hook1, "", HookOne);
			AddCommand(Config.Commands.Hook0, "", HookZero);

			foreach (var xC in Config.Commands.OpenHookForAll) AddCommand(xC, "", HookAc);
			foreach (var xC in Config.Commands.OpenHookForT) AddCommand(xC, "", HookAcT);
			foreach (var xC in Config.Commands.OpenHookForCT) AddCommand(xC, "", HookAcCt);
			foreach (var xC in Config.Commands.DisableHookForAll) AddCommand(xC, "", HookKapa);
			foreach (var xC in Config.Commands.DisableHookForT) AddCommand(xC, "", HookKapaT);
			foreach (var xC in Config.Commands.DisableHookForCT) AddCommand(xC, "", HookKapaCT);
			foreach (var xC in Config.Commands.ChangeHookSpeed) AddCommand(xC, "", HookHiz);
			foreach (var xC in Config.Commands.GiveTempHook) AddCommand(xC, "", HookVer);
			foreach (var xC in Config.Commands.RemoveTempHook) AddCommand(xC, "", HookSil);

			RegisterEventHandler<EventRoundStart>((@event, _) =>
			{
				try
				{
					if (@event == null)
						return HookResult.Continue;

					PlayersGrapples?.Clear();

					if (Config.HookSettings.HookActiveResetOnRoundStart)
					{
						HookEnabledForCt = true;
						HookEnabledForT = true;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
				return HookResult.Continue;
			});

			RegisterListener<Listeners.OnTick>(OnTick);
		}

		public override void Unload(bool hotReload)
		{
			PlayersGrapples?.Clear();
			RemoveListener<Listeners.OnTick>(OnTick);
		}

		public void HookOne(CCSPlayerController? player, CommandInfo info)
		{
			if (IsPlayerValid(player) == false)
				return;

			if (!HasHookPlayers.Contains(player!.SteamID) && !AdminManager.PlayerHasPermissions(player, Config.HookSettings.HookPermission))
			{
				player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
				return;
			}

			if (!player.PawnIsAlive)
				return;

			if (PlayersGrapples.TryGetValue(player.SteamID, out var laser))
				return;

			var team = player.Team;
			if (team == CsTeam.Terrorist)
			{
				if (HookEnabledForT == false)
				{
					player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["HookIsDisabledForT"]);
					return;
				}
			}
			else if (team == CsTeam.CounterTerrorist)
			{
				if (HookEnabledForCt == false)
				{
					player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["HookIsDisabledForCT"]);
					return;
				}
			}

			HookStartXX(player);
		}

		public void HookZero(CCSPlayerController? player, CommandInfo info)
		{
			if (IsPlayerValid(player) == false)
				return;

			if (!HasHookPlayers.Contains(player!.SteamID) && !AdminManager.PlayerHasPermissions(player, Config.HookSettings.HookPermission))
			{
				player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
				return;
			}

			if (!player.PawnIsAlive)
				return;

			if (PlayersGrapples.TryGetValue(player.SteamID, out var laser))
			{
				laser.AcceptInput("Kill");
				PlayersGrapples.Remove(player.SteamID);
			}
		}


		[CommandHelper(1, "<target>")]
		public void HookVer(CCSPlayerController? player, CommandInfo info)
		{
			if (IsPlayerValid(player) == false)
				return;

			if (!AdminManager.PlayerHasPermissions(player, Config.HookSettings.HookPermission))
			{
				player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
				return;
			}

			var target = info.GetArgTargetResult(1);

			if (target == null)
			{
				player!.PrintToChat(Config.Prefix + ChatColors.White + Localizer["TargetIsWrong"]);
				return;
			}

			target
				.ToList()
				.ForEach(x =>
				{
					if (!HasHookPlayers.Contains(x.SteamID))
					{
						HasHookPlayers.Add(x.SteamID);
					}
					Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["NamedAdminHookGave", player!.PlayerName, x.PlayerName]);
				});

		}

		[CommandHelper(1, "<target>")]
		public void HookSil(CCSPlayerController? player, CommandInfo info)
		{
			if (IsPlayerValid(player) == false)
				return;

			if (!AdminManager.PlayerHasPermissions(player, Config.HookSettings.HookPermission))
			{
				player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
				return;
			}

			var target = info.GetArgTargetResult(1);

			if (target == null)
			{
				player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["TargetIsWrong"]);
				return;
			}

			target
				.ToList()
				.ForEach(x =>
				{
					if (HasHookPlayers.Contains(x.SteamID))
					{
						HasHookPlayers.RemoveAll(y => y == x.SteamID);
					}
					Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["NamedAdminHookDelete", player.PlayerName, x.PlayerName]);
				});

		}

		[CommandHelper(1, "<speed>")]
		public void HookHiz(CCSPlayerController? player, CommandInfo info)
		{
			if (IsPlayerValid(player) == false)
				return;

			if (!AdminManager.PlayerHasPermissions(player, Config.HookSettings.HookPermission))
			{
				player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
				return;
			}

			if (!int.TryParse(info.GetArg(1), out int x))
			{
				player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["HookSpeedIsWrong"]);
				return;
			}

			Config.HookSettings.HookDefaultSpeed = x;

			Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["NamedAdminChangedHookSpeed", player.PlayerName, x]);
		}

		public void HookAc(CCSPlayerController? player, CommandInfo info)
		{
			var callerName = player == null ? "Console" : player.PlayerName;

			if (player != null && !AdminManager.PlayerHasPermissions(player, Config.HookSettings.HookPermission))
			{
				player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
				return;
			}

			HookEnabledForT = true;
			HookEnabledForCt = true;

			Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["OpenedHookForAll", callerName]);
		}

		public void HookAcT(CCSPlayerController? player, CommandInfo info)
		{
			var callerName = player == null ? "Console" : player.PlayerName;

			if (player != null && !AdminManager.PlayerHasPermissions(player, Config.HookSettings.HookPermission))
			{
				player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
				return;
			}

			HookEnabledForT = true;

			Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["OpenedHookForT", callerName]);
		}

		public void HookAcCt(CCSPlayerController? player, CommandInfo info)
		{
			var callerName = player == null ? "Console" : player.PlayerName;

			if (player != null && !AdminManager.PlayerHasPermissions(player, Config.HookSettings.HookPermission))
			{
				player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
				return;
			}

			HookEnabledForCt = true;

			Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["OpenedHookForCT", callerName]);
		}

		public void HookKapa(CCSPlayerController? player, CommandInfo info)
		{
			var callerName = player == null ? "Console" : player.PlayerName;

			if (player != null && !AdminManager.PlayerHasPermissions(player, Config.HookSettings.HookPermission))
			{
				player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
				return;
			}

			HookEnabledForT = false;
			HookEnabledForCt = false;

			Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["DisabledHookForAll", callerName]);
		}

		public void HookKapaT(CCSPlayerController? player, CommandInfo info)
		{
			var callerName = player == null ? "Console" : player.PlayerName;

			if (player != null && !AdminManager.PlayerHasPermissions(player, Config.HookSettings.HookPermission))
			{
				player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
				return;
			}

			HookEnabledForT = false;

			Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["DisabledHookForT", callerName]);
		}

		public void HookKapaCT(CCSPlayerController? player, CommandInfo info)
		{
			var callerName = player == null ? "Console" : player.PlayerName;

			if (player != null && !AdminManager.PlayerHasPermissions(player, Config.HookSettings.HookPermission))
			{
				player!.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
				return;
			}

			HookEnabledForCt = false;

			Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["DisabledHookForCT", callerName]);
		}


		private void OnTick()
		{
			try
			{
				for (int i = 1; i <= Server.MaxPlayers; i++)
				{
					var ent = NativeAPI.GetEntityFromIndex(i);
					if (ent == 0)
						continue;

					var player = new CCSPlayerController(ent);
					if (player == null || !player.IsValid)
						continue;

					if (PlayersGrapples.TryGetValue(player.SteamID, out var laser))
					{
						if (player.PlayerPawn.Value == null || player.PlayerPawn.Value.AbsOrigin == null || player.PlayerPawn.Value.CBodyComponent?.SceneNode == null)
						{
							continue;
						}

						if (player.PawnIsAlive == false)
						{
							laser.AcceptInput("Kill");
							PlayersGrapples.Remove(player.SteamID);
							continue;
						}

						if (!HasHookPlayers.Contains(player.SteamID) && !AdminManager.PlayerHasPermissions(player, Config.HookSettings.HookPermission))
						{
							continue;
						}

						var team = player.Team;
						if (team == CsTeam.Terrorist)
						{
							if (HookEnabledForT == false)
							{
								player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["HookIsDisabledForT"]);
								continue;
							}
						}
						else if (team == CsTeam.CounterTerrorist)
						{
							if (HookEnabledForCt == false)
							{
								player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["HookIsDisabledForCT"]);
								continue;
							}
						}

						var grappleTarget = laser.EndPos;
						var playerPosition = player.PlayerPawn.Value.AbsOrigin;
						var direction = new Vector(grappleTarget.X - playerPosition.X, grappleTarget.Y - playerPosition.Y, grappleTarget.Z - playerPosition.Z);
						var distanceToTarget = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z);

						if (distanceToTarget < 40f)
						{
							continue;
						}

						direction = new Vector(direction.X / distanceToTarget, direction.Y / distanceToTarget, direction.Z / distanceToTarget);

						var newVelocity = new Vector(
							direction.X * Config.HookSettings.HookDefaultSpeed,
							direction.Y * Config.HookSettings.HookDefaultSpeed,
							direction.Z * Config.HookSettings.HookDefaultSpeed
						);

						if (player.PlayerPawn.Value.AbsVelocity != null)
						{
							player.PlayerPawn.Value.AbsVelocity.X = newVelocity.X;
							player.PlayerPawn.Value.AbsVelocity.Y = newVelocity.Y;
							player.PlayerPawn.Value.AbsVelocity.Z = newVelocity.Z;
						}

						laser.Teleport(playerPosition);
					}


				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"OnTick Error: {e.Message}");
			}
		}


		private void HookStartXX(CCSPlayerController player)
		{
			if (player.PlayerPawn.Value!.AbsOrigin == null || player.PlayerPawn.Value.CBodyComponent?.SceneNode == null)
			{
				return;
			}

			float x, y, z;
			float playerX, playerY, playerZ;
			double angleA, angleB, radianA, radianB;
			double distance = Config.HookSettings.HookLength;
			float targetX, targetY, targetZ;
			float angleDifference;

			x = player.PlayerPawn.Value.AbsOrigin.X;
			y = player.PlayerPawn.Value.AbsOrigin.Y;
			z = player.PlayerPawn.Value.AbsOrigin.Z;

			angleA = -player.PlayerPawn.Value.EyeAngles.X;
			angleB = player.PlayerPawn.Value.EyeAngles.Y;

			radianA = (Math.PI / 180) * angleA;
			radianB = (Math.PI / 180) * angleB;

			targetX = (float)(x + distance * Math.Cos(radianA) * Math.Cos(radianB));
			targetY = (float)(y + distance * Math.Cos(radianA) * Math.Sin(radianB));
			targetZ = (float)(z + distance * Math.Sin(radianA));

			playerX = player.PlayerPawn.Value.CBodyComponent.SceneNode.AbsOrigin.X;
			playerY = player.PlayerPawn.Value.CBodyComponent.SceneNode.AbsOrigin.Y;
			playerZ = player.PlayerPawn.Value.CBodyComponent.SceneNode.AbsOrigin.Z;
			angleA = player.PlayerPawn.Value.EyeAngles.X;
			angleB = player.PlayerPawn.Value.EyeAngles.Y;

			Vector grappleTarget = new Vector(targetX, targetY, targetZ);
			Vector playerPosition = new Vector(playerX, playerY, playerZ);
			float thresholdDistance = 40.0f;

			var direction = new Vector(grappleTarget.X - playerPosition.X, grappleTarget.Y - playerPosition.Y, grappleTarget.Z - playerPosition.Z);
			var distanceToTarget = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z);

			if (distanceToTarget < thresholdDistance)
			{
				return;
			}

			Vector angles1 = new Vector((float)angleA, (float)angleB, 0);
			Vector angles2 = new Vector(targetX - playerX, targetY - playerY, targetZ - playerZ);

			float pitchDiff = Math.Abs(angles1.X - angles2.X);
			float yawDiff = Math.Abs(angles1.Y - angles2.Y);

			pitchDiff = pitchDiff > 180.0f ? 360.0f - pitchDiff : pitchDiff;
			yawDiff = yawDiff > 180.0f ? 360.0f - yawDiff : yawDiff;

			angleDifference = Math.Max(pitchDiff, yawDiff);
			if (angleDifference > 180.0f)
			{
				return;
			}

			var laser = DrawLaser(new Vector(x, y, z), new Vector(targetX, targetY, targetZ), player.SteamID);
		}


		private CEnvBeam? DrawLaser(Vector start, Vector end, ulong steamId)
		{
			CEnvBeam? laser = Utilities.CreateEntityByName<CEnvBeam>("env_beam");

			if (laser == null)
			{
				return null;
			}

			laser.Render = Color.Blue;

			laser.Width = 4;

			laser.Teleport(start, new QAngle(), new Vector());
			laser.EndPos.X = end.X;
			laser.EndPos.Y = end.Y;
			laser.EndPos.Z = end.Z;

			laser.ClipStyle = BeamClipStyle_t.kMODELCLIP;
			laser.TouchType = Touch_t.touch_player_or_npc_or_physicsprop;
			laser.DispatchSpawn();

			if (PlayersGrapples != null)
			{
				if (PlayersGrapples.ContainsKey(steamId) == false)
				{
					PlayersGrapples.Add(steamId, laser);
				}
			}
			return laser;
		}

		private bool IsPlayerValid([NotNullWhen(true)] CCSPlayerController? player)
		{
			if (player == null ||
				!player.IsValid ||
				!player.PlayerPawn.IsValid ||
				player.IsBot ||
				player.IsHLTV ||
				player.Connected != PlayerConnectedState.PlayerConnected)
			{
				return false;
			}
			return true;
		}
	}
}
