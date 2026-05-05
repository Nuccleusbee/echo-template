using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EchoTemplate.Utilities;

public static class NotificationLib
{
	private class Item { public string Text; public float Until; }

	private static readonly Queue<Item> _queue = new();
	private static GameObject _root;
	private static Text _text;
	private static Item _current;

	public static void SendNotification(string text, int milliseconds = 4000)
	{
		_queue.Enqueue(new Item { Text = text, Until = Time.time + milliseconds / 1000f });
	}

	public static void Tick()
	{
		EnsureBuilt();

		if (_current != null && Time.time >= _current.Until)
		{
			_current = null;
			_text.text = "";
			_root.SetActive(false);
		}

		if (_current == null && _queue.Count > 0)
		{
			_current = _queue.Dequeue();
			_text.text = _current.Text;
			_root.SetActive(true);
		}
	}

	private static void EnsureBuilt()
	{
		if (_root != null) return;
		_root = new GameObject("EchoNotifications");
		Object.DontDestroyOnLoad(_root);

		var canvas = _root.AddComponent<Canvas>();
		canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = 32700;
		_root.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

		var go = new GameObject("Text", typeof(Text));
		go.transform.SetParent(_root.transform, false);
		_text = go.GetComponent<Text>();
		_text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
		          ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
		_text.fontSize = 22;
		_text.fontStyle = FontStyle.Bold;
		_text.alignment = TextAnchor.UpperCenter;
		_text.horizontalOverflow = HorizontalWrapMode.Overflow;
		_text.color = new Color(1f, 1f, 1f, 0.95f);
		_text.supportRichText = true;

		var rt = go.GetComponent<RectTransform>();
		rt.anchorMin = new Vector2(0.5f, 1f);
		rt.anchorMax = new Vector2(0.5f, 1f);
		rt.pivot     = new Vector2(0.5f, 1f);
		rt.sizeDelta = new Vector2(800f, 60f);
		rt.anchoredPosition = new Vector2(0f, -40f);

		_root.SetActive(false);
	}
}
