using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HookPlugin
{
	public class HookPluginConfig : BasePluginConfig
	{
		public class HookSettingsConfig
		{
			public string HookPermission { get; set; } = "@css/ban";
			public int HookDefaultSpeed { get; set; } = 1500;
			public int HookLength { get; set; } = 2500;
			public bool HookActiveResetOnRoundStart { get; set; } = true;

		}

		//public class CommandsConfig
		//{
		//	public string Hook1 { get; set; } = "hook1";
		//	public string Hook0 { get; set; } = "hook0";
		//	public string[] OpenHookForAll { get; set; } = { "ha", "hookac" };
		//	public string[] OpenHookForT { get; set; } = { "hat", "hookact" };
		//	public string[] OpenHookForCT { get; set; } = { "hact", "hookacct" };
		//	public string[] DisableHookForAll { get; set; } = { "hk", "hookkapa", "hookkapat" };
		//	public string[] DisableHookForT { get; set; } = { "hkt", "hookkapatt" };
		//	public string[] DisableHookForCT { get; set; } = { "hkct", "hookkapatct" };
		//	public string[] ChangeHookSpeed { get; set; } = { "hookhiz", "hhiz", "hspeed" };
		//	public string[] GiveTempHook { get; set; } = { "hookver" };
		//	public string[] RemoveTempHook { get; set; } = { "hookal", "hooksil" };
		//}


		public class CommandsConfig
		{
			public string Hook1 { get; set; } = "hook1";
			public string Hook0 { get; set; } = "hook0";
			public string[] OpenHookForAll { get; set; } = ["enablehook", "hookenable"];
			public string[] OpenHookForT { get; set; } = ["enablehookt", "hookenablet"];
			public string[] OpenHookForCT { get; set; } = ["enablehookct", "hookenablect"];
			public string[] DisableHookForAll { get; set; } = ["disablehook", "hookdisable"];
			public string[] DisableHookForT { get; set; } = ["disablehookt", "hookdisablet"];
			public string[] DisableHookForCT { get; set; } = ["disablehookct", "hookdisablect"];
			public string[] ChangeHookSpeed { get; set; } = ["hookspeed", "hspeed"];
			public string[] GiveTempHook { get; set; } = ["givehook", "hookgive"];
			public string[] RemoveTempHook { get; set; } = ["deletehook", "removehook"];

		}

		public string Prefix { get; set; } = "{green}[Hook]";

		public HookSettingsConfig HookSettings { get; set; } = new HookSettingsConfig();
		public CommandsConfig Commands { get; set; } = new CommandsConfig();
	}
}
