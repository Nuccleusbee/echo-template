using System;
using HarmonyLib;
using GorillaLocomotion;
using UnityEngine;
using EchoTemplate.Menu;
using EchoTemplate.Utilities;

namespace EchoTemplate.GUI;

[HarmonyPatch(typeof(GTPlayer), "LateUpdate")]
public class Main
{
	[HarmonyPrefix]
	public static void Prefix()
	{
		try
		{
			NotificationLib.Tick();
			HandleButtonActions();
			PcMenuManager.Tick();
		}
		catch (Exception ex)
		{
			Debug.LogError($"[EchoTemplate] {ex.Message}\n{ex.StackTrace}");
		}
	}

	private static void HandleButtonActions()
	{
		foreach (var b in ModButtons.buttons)
		{
			if (!b.Enabled) continue;
			try { b.onEnable?.Invoke(); }
			catch { }
		}
	}
}
