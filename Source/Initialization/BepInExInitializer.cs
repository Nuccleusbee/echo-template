using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace EchoTemplate.Initialization;

[BepInPlugin(GUID, NAME, VERSION)]
public class BepInExInitializer : BaseUnityPlugin
{
	public const string GUID    = "com.echo.template";
	public const string NAME    = "Echo Template";
	public const string VERSION = "1.0.0";

	private void Awake()
	{
		Application.runInBackground = true;
		new Harmony(GUID).PatchAll();
		Debug.Log($"[{NAME}] loaded {VERSION}");
	}
}
