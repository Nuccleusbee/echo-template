using UnityEngine;
using UnityEngine.InputSystem;
using EchoTemplate.Menu;
using EchoTemplate.Utilities;

namespace EchoTemplate.GUI;

/// Bare-bones IMGUI menu so the template runs out of the box. This is a
/// PLACEHOLDER — replace it with your own UI implementation. F1 toggles
/// visibility. Click a category on the left, click a mod to toggle.
public class PlaceholderUI : MonoBehaviour
{
	public static bool Visible = true;

	private Vector2 _scroll;
	private Category _page = Category.Home;
	private bool _firstFrame = true;

	private void Update()
	{
		var kb = Keyboard.current;
		if (kb != null && kb.f1Key.wasPressedThisFrame) Visible = !Visible;
	}

	private void OnGUI()
	{
		if (!Visible) return;

		if (_firstFrame)
		{
			_firstFrame = false;
			foreach (var b in ModButtons.buttons)
			{
				if (b.Page != Category.Home) { _page = b.Page; break; }
			}
		}

		const int W = 360, H = 480, Pad = 8;
		var rect = new Rect(20, 20, W, H);
		UnityEngine.GUI.Box(rect, "Echo Template  —  [F1] toggle  —  replace this UI");

		GUILayout.BeginArea(new Rect(rect.x + Pad, rect.y + 24, W - Pad * 2, H - 32));

		GUILayout.BeginHorizontal();
		foreach (var cat in CategoriesWithButtons())
		{
			var prev = UnityEngine.GUI.color;
			if (cat == _page) UnityEngine.GUI.color = Color.cyan;
			if (GUILayout.Button(cat.ToString(), GUILayout.MinWidth(60)))
			{
				_page = cat;
				ButtonHandler.ChangePage(cat);
			}
			UnityEngine.GUI.color = prev;
		}
		GUILayout.EndHorizontal();

		GUILayout.Space(6);

		_scroll = GUILayout.BeginScrollView(_scroll);
		foreach (var b in ButtonHandler.GetButtonInfoByPage(_page))
		{
			string raw = b.buttonText ?? "";
			int nl = raw.IndexOf('\n');
			string title = nl >= 0 ? raw.Substring(0, nl) : raw;
			string sub   = nl >= 0 ? raw.Substring(nl + 1) : "";

			var prev = UnityEngine.GUI.color;
			if (b.Enabled) UnityEngine.GUI.color = Color.green;
			if (GUILayout.Button($"{(b.Enabled ? "[on]  " : "[ ]   ")}{title}\n  {sub}", GUILayout.Height(36)))
			{
				ButtonHandler.Toggle(b);
			}
			UnityEngine.GUI.color = prev;
		}
		GUILayout.EndScrollView();

		GUILayout.EndArea();
	}

	private static System.Collections.Generic.List<Category> CategoriesWithButtons()
	{
		var seen = new System.Collections.Generic.HashSet<Category>();
		foreach (var b in ModButtons.buttons)
			if (b.Page != Category.Home) seen.Add(b.Page);
		var list = new System.Collections.Generic.List<Category>(seen);
		list.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), System.StringComparison.Ordinal));
		return list;
	}
}
