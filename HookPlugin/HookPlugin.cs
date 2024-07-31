using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using System;
using System.Net.NetworkInformation;
using CounterStrikeSharp.API.Core.Attributes.Registration;

namespace HookPlugin
{
	public class HookPluginConfig : BasePluginConfig
	{
		public string Prefix { get; set; } = "{green}[Hook]";
		public string HookYetkisi { get; set; } = "@css/ban";
		public int HookVarsayilanHiz { get; set; } = 1500;
		public int HookUzunlugu { get; set; } = 2500;
		public bool RoundBasiHooklarAcilsin { get; set; } = true;
		public string YetkinYokText { get; set; } = "Bu komutu kullanmak için yeterli yetkiniz yok!";
	}

	public class HookPlugin : BasePlugin, IPluginConfig<HookPluginConfig>
	{
		public override string ModuleName => "HookPlugin";
		public override string ModuleVersion => "1.0.1";
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
			RegisterEventHandler<EventRoundStart>((@event, _) =>
			{
				try
				{
					if (@event == null)
						return HookResult.Continue;

					// HER YENI ROUND ZATEN BEAMLER SIFIRLAINYOR BU YUZDEN GEREKSIZ
					//PlayersGrapples
					//	.ToList()
					//	.ForEach(x =>
					//	{
					//		x.Value.Remove();
					//	});

					PlayersGrapples?.Clear();

					if (Config.RoundBasiHooklarAcilsin)
					{
						HookEnabledForCt = true;
						HookEnabledForT = true;
					}

					return HookResult.Continue;
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					return HookResult.Continue;
				}
			});

			RegisterListener<Listeners.OnTick>(OnTick);
			base.Load(hotReload);
		}

		public override void Unload(bool hotReload)
		{
			PlayersGrapples
				.ToList()
				.ForEach(x =>
				{
					x.Value.Remove();
				});

			PlayersGrapples.Clear();

			RemoveListener<Listeners.OnTick>(OnTick);
			base.Unload(hotReload);
		}

		[ConsoleCommand("hook1")]
		public void HookOne(CCSPlayerController? player, CommandInfo info)
		{
			if (IsPlayerValid(player) == false)
			{
				return;
			}

			if (!HasHookPlayers.Contains(player!.SteamID) && !AdminManager.PlayerHasPermissions(player, Config.HookYetkisi))
			{
				player!.PrintToChat($"{Config.Prefix} {ChatColors.White}{Config.YetkinYokText}");
				return;
			}

			if (!player!.PawnIsAlive)
			{
				return;
			}

			var team = player.Team;
			if (team == CsTeam.Terrorist)
			{
				if (HookEnabledForT == false)
				{
					player.PrintToChat($"{Config.Prefix}{ChatColors.White} Hook t takımına kapalı.");
					return;
				}
			}
			else if (team == CsTeam.CounterTerrorist)
			{
				if (HookEnabledForCt == false)
				{
					player.PrintToChat($"{Config.Prefix}{ChatColors.White} Hook ct takımına kapalı.");
					return;
				}
			}

			HookStartXX(player);
		}


		[ConsoleCommand("hook0")]
		public void HookZero(CCSPlayerController? player, CommandInfo info)
		{
			if (IsPlayerValid(player) == false)
			{
				return;
			}

			if (!HasHookPlayers.Contains(player!.SteamID) && !AdminManager.PlayerHasPermissions(player, Config.HookYetkisi))
			{
				player!.PrintToChat($"{Config.Prefix} {ChatColors.White}{Config.YetkinYokText}");
				return;
			}

			if (!player!.PawnIsAlive)
			{
				return;
			}

			if (PlayersGrapples.TryGetValue(player.SteamID, out var laser))
			{
				laser.Remove();
				PlayersGrapples.Remove(player.SteamID);
			}
		}


		[ConsoleCommand("hookver")]
		[CommandHelper(1, "<hedef>")]
		public void HookVer(CCSPlayerController? player, CommandInfo info)
		{
			if (IsPlayerValid(player) == false)
			{
				return;
			}

			if (!AdminManager.PlayerHasPermissions(player, Config.HookYetkisi))
			{
				player!.PrintToChat($"{Config.Prefix} {ChatColors.White}{Config.YetkinYokText}");
				return;
			}

			var target = info.GetArgTargetResult(1);

			if (target == null)
			{
				player!.PrintToChat($"{Config.Prefix}{ChatColors.White} Hedef hatalı.");
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
					Server.PrintToChatAll($"{Config.Prefix} {ChatColors.Green}{player!.PlayerName}{ChatColors.White} isimli yetkili, {ChatColors.Yellow}{x.PlayerName}{ChatColors.White} adlı oyuncuya hook verdi.");
				});

		}

		[ConsoleCommand("hookal")]
		[ConsoleCommand("hooksil")]
		[CommandHelper(1, "<hedef>")]
		public void HookSil(CCSPlayerController? player, CommandInfo info)
		{
			if (IsPlayerValid(player) == false)
			{
				return;
			}

			if (!AdminManager.PlayerHasPermissions(player, Config.HookYetkisi))
			{
				player!.PrintToChat($"{Config.Prefix} {ChatColors.White}{Config.YetkinYokText}");
				return;
			}

			var target = info.GetArgTargetResult(1);

			if (target == null)
			{
				player!.PrintToChat($"{Config.Prefix}{ChatColors.White} Hedef hatalı.");
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
					Server.PrintToChatAll($"{Config.Prefix} {ChatColors.Green}{player!.PlayerName}{ChatColors.White} isimli yetkili, {ChatColors.Yellow}{x.PlayerName}{ChatColors.White} adlı oyuncunun hookunu sildi.");
				});

		}

		[ConsoleCommand("hookhiz")]
		[ConsoleCommand("hhiz")]
		[ConsoleCommand("hspeed")]
		public void HookHiz(CCSPlayerController? player, CommandInfo info)
		{
			if (IsPlayerValid(player) == false)
			{
				return;
			}

			if (!AdminManager.PlayerHasPermissions(player, Config.HookYetkisi))
			{
				player!.PrintToChat($"{Config.Prefix} {ChatColors.White}{Config.YetkinYokText}");
				return;
			}

			if (!int.TryParse(info.GetArg(1), out int x))
			{
				player!.PrintToChat($"{Config.Prefix} Hook hız değeri hatalı.");
				return;
			}

			Config.HookVarsayilanHiz = x;

			Server.PrintToChatAll($"{Config.Prefix} {ChatColors.Green}{player!.PlayerName}{ChatColors.White} isimli yetkili, hookun hızını {ChatColors.Gold}{x}{ChatColors.Default} olarak değiştirdi.");
		}

		[ConsoleCommand("ha")]
		[ConsoleCommand("hookac")]
		public void HookAc(CCSPlayerController? player, CommandInfo info)
		{
			var callerName = player == null ? "Konsol" : player.PlayerName;

			if (player != null && !AdminManager.PlayerHasPermissions(player, Config.HookYetkisi))
			{
				player.PrintToChat($"{Config.Prefix} {ChatColors.White}{Config.YetkinYokText}");
				return;
			}

			HookEnabledForT = true;
			HookEnabledForCt = true;

			Server.PrintToChatAll($"{Config.Prefix} {ChatColors.Green}{player!.PlayerName}{ChatColors.White} isimli yetkili, hooku açtı.");
		}

		[ConsoleCommand("hat")]
		[ConsoleCommand("hookact")]
		public void HookAcT(CCSPlayerController? player, CommandInfo info)
		{
			var callerName = player == null ? "Konsol" : player.PlayerName;

			if (player != null && !AdminManager.PlayerHasPermissions(player, Config.HookYetkisi))
			{
				player.PrintToChat($"{Config.Prefix} {ChatColors.White}{Config.YetkinYokText}");
				return;
			}

			HookEnabledForT = true;

			Server.PrintToChatAll($"{Config.Prefix} {ChatColors.Green}{player!.PlayerName}{ChatColors.White} isimli yetkili, hooku t takımına açtı.");
		}

		[ConsoleCommand("hact")]
		[ConsoleCommand("hookacct")]
		public void HookAcCt(CCSPlayerController? player, CommandInfo info)
		{
			var callerName = player == null ? "Konsol" : player.PlayerName;

			if (player != null && !AdminManager.PlayerHasPermissions(player, Config.HookYetkisi))
			{
				player.PrintToChat($"{Config.Prefix} {ChatColors.White}{Config.YetkinYokText}");
				return;
			}

			HookEnabledForCt = true;

			Server.PrintToChatAll($"{Config.Prefix} {ChatColors.Green}{player!.PlayerName}{ChatColors.White} isimli yetkili, hooku ct takımına açtı.");
		}

		[ConsoleCommand("hk")]
		[ConsoleCommand("hookkapat")]
		[ConsoleCommand("hookkapa")]
		public void HookKapa(CCSPlayerController? player, CommandInfo info)
		{
			var callerName = player == null ? "Konsol" : player.PlayerName;

			if (player != null && !AdminManager.PlayerHasPermissions(player, Config.HookYetkisi))
			{
				player.PrintToChat($"{Config.Prefix} {ChatColors.White}{Config.YetkinYokText}");
				return;
			}

			HookEnabledForT = false;
			HookEnabledForCt = false;

			Server.PrintToChatAll($"{Config.Prefix} {ChatColors.Green}{player!.PlayerName}{ChatColors.White} isimli yetkili, hooku kapattı.");
		}

		[ConsoleCommand("hkt")]
		[ConsoleCommand("hookkapatt")]
		public void HookKapaT(CCSPlayerController? player, CommandInfo info)
		{
			var callerName = player == null ? "Konsol" : player.PlayerName;

			if (player != null && !AdminManager.PlayerHasPermissions(player, Config.HookYetkisi))
			{
				player.PrintToChat($"{Config.Prefix} {ChatColors.White}{Config.YetkinYokText}");
				return;
			}

			HookEnabledForT = false;

			Server.PrintToChatAll($"{Config.Prefix} {ChatColors.Green}{player!.PlayerName}{ChatColors.White} isimli yetkili, t takımının hookunu kapattı.");
		}

		[ConsoleCommand("hkct")]
		[ConsoleCommand("hookkapatct")]
		public void HookKapaCT(CCSPlayerController? player, CommandInfo info)
		{
			var callerName = player == null ? "Konsol" : player.PlayerName;

			if (player != null && !AdminManager.PlayerHasPermissions(player, Config.HookYetkisi))
			{
				player.PrintToChat($"{Config.Prefix} {ChatColors.White}{Config.YetkinYokText}");
				return;
			}

			HookEnabledForCt = false;

			Server.PrintToChatAll($"{Config.Prefix} {ChatColors.Green}{player!.PlayerName}{ChatColors.White} isimli yetkili, ct takımının hookunu kapattı.");
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
							laser.Remove();
							PlayersGrapples.Remove(player.SteamID);
							continue;
						}

						if (!HasHookPlayers.Contains(player!.SteamID) && !AdminManager.PlayerHasPermissions(player, Config.HookYetkisi))
						{
							continue;
						}

						var team = player.Team;
						if (team == CsTeam.Terrorist)
						{
							if (HookEnabledForT == false)
							{
								player.PrintToChat($"{Config.Prefix}{ChatColors.White} Hook t takımına kapalı.");
								continue;
							}
						}
						else if (team == CsTeam.CounterTerrorist)
						{
							if (HookEnabledForCt == false)
							{
								player.PrintToChat($"{Config.Prefix}{ChatColors.White} Hook ct takımına kapalı.");
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
							direction.X * Config.HookVarsayilanHiz,
							direction.Y * Config.HookVarsayilanHiz,
							direction.Z * Config.HookVarsayilanHiz
						);

						if (player.PlayerPawn.Value.AbsVelocity != null)
						{
							player.PlayerPawn.Value.AbsVelocity.X = newVelocity.X;
							player.PlayerPawn.Value.AbsVelocity.Y = newVelocity.Y;
							player.PlayerPawn.Value.AbsVelocity.Z = newVelocity.Z;
						}

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
			double distance = Config.HookUzunlugu;
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

			if (player == null || player.PlayerPawn == null || player.PlayerPawn.Value?.CBodyComponent == null || playerPosition == null || !player.IsValid || !player.PawnIsAlive)
			{
				Console.WriteLine("Player is null.");
				return;
			}

			if (player.PlayerPawn.Value.CBodyComponent.SceneNode == null)
			{
				Console.WriteLine("SceneNode is null. Skipping pull.");
				return;
			}

			if (grappleTarget == null)
			{
				Console.WriteLine("Grapple target is null.");
				return;
			}

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


		private bool IsPlayerValid(CCSPlayerController? player)
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
