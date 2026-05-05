using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using EchoTemplate.Menu;
using EchoTemplate.Utilities;

namespace EchoTemplate.GUI;

public static class PcMenuManager
{
	public  static bool          IsVisible { get; private set; } = true;
	private static GameObject    _root;
	private static RectTransform _panel;
	private static GameObject    _header;
	private static Text          _logoText;
	private static Text          _fpsText;
	private static Text          _pageLabel;
	private static GameObject    _disconnect;
	private static GameObject    _home;
	private static Text          _pageIndicator;
	private static GameObject    _modArea;
	private static readonly List<GameObject> _modRows = new();

	private static Category _lastPage         = (Category)(-1);
	private static int      _lastCategoryPage = -1;
	private static long     _lastEnabledHash  = -1;
	private static bool     _lastInRoom;
	private static Vector2  _lastPanelSize;
	internal static void RequestRefresh() => _lastPage = (Category)(-1);

	private static int   _frameCount;
	private static float _fpsTimer;
	private static int   _currentFps;

	private static Font _font;
	private static Font GetFont() => _font ??=
		(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
		 ?? Resources.GetBuiltinResource<Font>("Arial.ttf")
		 ?? Font.CreateDynamicFontFromOSFont("Arial", 14));

	private const float PanelW   = 420f;
	private const float PanelH   = 560f;
	private const float HeaderH  = 64f;
	private const float FooterH  = 44f;
	private const float SidePad  = 20f;
	private const int   PerPage  = 8;

	private static readonly Color C_Bg       = new(0.04f, 0.04f, 0.06f, 0.97f);
	private static readonly Color C_Header   = new(0.08f, 0.08f, 0.10f, 1f);
	private static readonly Color C_Footer   = new(0.07f, 0.07f, 0.10f, 1f);
	private static readonly Color C_Side     = new(0.10f, 0.10f, 0.13f, 1f);
	private static readonly Color C_RowOff   = new(0.10f, 0.10f, 0.13f, 0f);
	private static readonly Color C_RowOn    = new(0.18f, 0.13f, 0.28f, 1f);
	private static readonly Color C_Disc     = new(0.55f, 0.18f, 0.18f, 1f);
	private static readonly Color C_Accent   = new(0.62f, 0.36f, 0.98f, 1f);
	private static readonly Color C_AccentDim= new(0.34f, 0.20f, 0.56f, 1f);
	private static readonly Color C_Text     = new(0.96f, 0.96f, 0.99f, 1f);
	private static readonly Color C_TextDim  = new(0.55f, 0.55f, 0.62f, 1f);
	private static readonly Color C_Edge     = new(0.20f, 0.20f, 0.26f, 1f);

	public static void Tick()
	{
		_frameCount++;
		_fpsTimer += Time.unscaledDeltaTime;
		if (_fpsTimer >= 0.5f)
		{
			_currentFps = Mathf.RoundToInt(_frameCount / _fpsTimer);
			_frameCount = 0;
			_fpsTimer   = 0f;
			if (_fpsText != null) _fpsText.text = $"{_currentFps:000} FPS";
		}

		if (_root == null) Build();
		var kb = Keyboard.current;
		if (kb != null && kb.f1Key.wasPressedThisFrame) Toggle();
		if (IsVisible && StateChanged()) Refresh();
	}

	public static void Toggle()
	{
		if (_root == null) Build();
		IsVisible = !IsVisible;
		_root.SetActive(IsVisible);
	}

	private static bool StateChanged()
	{
		long h = 0;
		foreach (var b in ModButtons.buttons) h = h * 31 + (b.Enabled ? 1 : 0);
		bool inRoom = PhotonNetwork.InRoom;
		Vector2 size = _panel != null ? _panel.sizeDelta : Vector2.zero;
		bool changed = Variables.currentPage != _lastPage
		            || Variables.currentCategoryPage != _lastCategoryPage
		            || h != _lastEnabledHash
		            || inRoom != _lastInRoom
		            || size != _lastPanelSize;
		_lastPage         = Variables.currentPage;
		_lastCategoryPage = Variables.currentCategoryPage;
		_lastEnabledHash  = h;
		_lastInRoom       = inRoom;
		_lastPanelSize    = size;
		return changed;
	}

	private static void Build()
	{
		_root = new GameObject("EchoPcMenu");
		Object.DontDestroyOnLoad(_root);

		var canvas = _root.AddComponent<Canvas>();
		canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = 32760;
		_root.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
		_root.AddComponent<GraphicRaycaster>();

		var panel = NewImage("Panel", _root.transform, C_Bg);
		_panel = panel.rectTransform;
		_panel.anchorMin = _panel.anchorMax = new Vector2(0.5f, 0.5f);
		_panel.pivot     = new Vector2(0.5f, 0.5f);
		_panel.sizeDelta = new Vector2(PanelW, PanelH);
		_panel.anchoredPosition = Vector2.zero;
		AddOutline(panel.gameObject, C_Edge, 1f);

		BuildHeader();
		BuildSideRails();
		BuildFooter();

		_modArea = new GameObject("ModArea", typeof(RectTransform));
		_modArea.transform.SetParent(_panel, false);

		_root.SetActive(IsVisible);
		_lastPage = (Category)(-1);
		Refresh();
	}

	private static void BuildHeader()
	{
		_header = NewImage("Header", _panel, C_Header).gameObject;
		var hrt = _header.GetComponent<RectTransform>();
		hrt.anchorMin = new Vector2(0f, 1f);
		hrt.anchorMax = new Vector2(1f, 1f);
		hrt.pivot     = new Vector2(0.5f, 1f);
		hrt.sizeDelta = new Vector2(0f, HeaderH);
		hrt.anchoredPosition = Vector2.zero;
		_header.AddComponent<PcMenuDragger>().Target = _panel;
		_header.AddComponent<PcMenuResizer>().Target = _panel;

		var stripe = NewImage("Stripe", _header.transform, C_Accent).rectTransform;
		stripe.anchorMin = new Vector2(0f, 0f);
		stripe.anchorMax = new Vector2(0f, 1f);
		stripe.pivot     = new Vector2(0f, 0.5f);
		stripe.sizeDelta = new Vector2(5f, 0f);
		stripe.anchoredPosition = Vector2.zero;

		_logoText = NewText("ECHO", _header.transform, 26, FontStyle.Bold, C_Text, TextAnchor.UpperLeft);
		var ltRt = _logoText.rectTransform;
		ltRt.anchorMin = new Vector2(0f, 0.5f);
		ltRt.anchorMax = new Vector2(1f, 1f);
		ltRt.offsetMin = new Vector2(18f, 0f);
		ltRt.offsetMax = new Vector2(-12f, -4f);

		_pageLabel = NewText("home", _header.transform, 11, FontStyle.Normal, C_AccentDim, TextAnchor.LowerLeft);
		var plRt = _pageLabel.rectTransform;
		plRt.anchorMin = new Vector2(0f, 0f);
		plRt.anchorMax = new Vector2(0.7f, 0.5f);
		plRt.offsetMin = new Vector2(18f, 4f);
		plRt.offsetMax = new Vector2(-4f, 0f);

		_fpsText = NewText("000 FPS", _header.transform, 11, FontStyle.Bold, C_Accent, TextAnchor.LowerRight);
		var fpRt = _fpsText.rectTransform;
		fpRt.anchorMin = new Vector2(0.5f, 0f);
		fpRt.anchorMax = new Vector2(1f, 0.5f);
		fpRt.offsetMin = new Vector2(0f, 4f);
		fpRt.offsetMax = new Vector2(-12f, 0f);
	}

	private static void BuildSideRails()
	{
		var leftRail = NewImage("LeftRail", _panel, C_Side).gameObject;
		var lrt = leftRail.GetComponent<RectTransform>();
		lrt.anchorMin = new Vector2(0f, 0f);
		lrt.anchorMax = new Vector2(0f, 1f);
		lrt.pivot     = new Vector2(0f, 0.5f);
		lrt.sizeDelta = new Vector2(SidePad, -(HeaderH + FooterH));
		lrt.anchoredPosition = new Vector2(0f, -(HeaderH - FooterH) * 0.5f);

		var leftBtn = leftRail.AddComponent<Button>();
		leftBtn.transition = Selectable.Transition.None;
		leftBtn.onClick.AddListener(() => ClickStub("<"));
		var leftGlyph = NewText("‹", leftRail.transform, 28, FontStyle.Bold, C_Accent, TextAnchor.MiddleCenter);
		FillStretch(leftGlyph.rectTransform);

		var rightRail = NewImage("RightRail", _panel, C_Side).gameObject;
		var rrt = rightRail.GetComponent<RectTransform>();
		rrt.anchorMin = new Vector2(1f, 0f);
		rrt.anchorMax = new Vector2(1f, 1f);
		rrt.pivot     = new Vector2(1f, 0.5f);
		rrt.sizeDelta = new Vector2(SidePad, -(HeaderH + FooterH));
		rrt.anchoredPosition = new Vector2(0f, -(HeaderH - FooterH) * 0.5f);

		var rightBtn = rightRail.AddComponent<Button>();
		rightBtn.transition = Selectable.Transition.None;
		rightBtn.onClick.AddListener(() => ClickStub(">"));
		var rightGlyph = NewText("›", rightRail.transform, 28, FontStyle.Bold, C_Accent, TextAnchor.MiddleCenter);
		FillStretch(rightGlyph.rectTransform);
	}

	private static void BuildFooter()
	{
		var footer = NewImage("Footer", _panel, C_Footer).gameObject;
		var frt = footer.GetComponent<RectTransform>();
		frt.anchorMin = new Vector2(0f, 0f);
		frt.anchorMax = new Vector2(1f, 0f);
		frt.pivot     = new Vector2(0.5f, 0f);
		frt.sizeDelta = new Vector2(0f, FooterH);
		frt.anchoredPosition = Vector2.zero;

		_disconnect = MakePill("DC", C_Disc, C_Text, () => ClickStub("DisconnectButton"));
		_disconnect.transform.SetParent(footer.transform, false);
		var dcRt = _disconnect.GetComponent<RectTransform>();
		dcRt.anchorMin = dcRt.anchorMax = new Vector2(0f, 0.5f);
		dcRt.pivot     = new Vector2(0f, 0.5f);
		dcRt.sizeDelta = new Vector2(60f, 26f);
		dcRt.anchoredPosition = new Vector2(28f, 0f);

		_home = MakePill("⌂  HOME", C_AccentDim, C_Text, () => ClickStub("ReturnButton"));
		_home.transform.SetParent(footer.transform, false);
		var hmRt = _home.GetComponent<RectTransform>();
		hmRt.anchorMin = hmRt.anchorMax = new Vector2(1f, 0.5f);
		hmRt.pivot     = new Vector2(1f, 0.5f);
		hmRt.sizeDelta = new Vector2(110f, 30f);
		hmRt.anchoredPosition = new Vector2(-28f, 0f);

		_pageIndicator = NewText("1 / 1", footer.transform, 12, FontStyle.Bold, C_TextDim, TextAnchor.MiddleCenter);
		var piRt = _pageIndicator.rectTransform;
		piRt.anchorMin = new Vector2(0f, 0f);
		piRt.anchorMax = new Vector2(1f, 1f);
		piRt.offsetMin = new Vector2(110f, 0f);
		piRt.offsetMax = new Vector2(-160f, 0f);
	}

	private static void Refresh()
	{
		bool inRoom = PhotonNetwork.InRoom;
		if (_disconnect != null) _disconnect.SetActive(inRoom);

		string cat = Variables.currentPage == Category.Home ? "home" : Variables.currentPage.ToString().ToLower();
		var all = ButtonHandler.GetButtonInfoByPage(Variables.currentPage);
		int totalPages = (all.Count + PerPage - 1) / PerPage;
		if (totalPages < 1) totalPages = 1;
		int curPage = Variables.currentCategoryPage + 1;

		_pageLabel.text = $"// {cat}";
		_pageIndicator.text = $"{curPage:00} / {totalPages:00}";

		var mr = _modArea.GetComponent<RectTransform>();
		mr.anchorMin = new Vector2(0f, 0f);
		mr.anchorMax = new Vector2(1f, 1f);
		mr.pivot     = new Vector2(0.5f, 0.5f);
		mr.offsetMin = new Vector2(SidePad + 6f, FooterH + 4f);
		mr.offsetMax = new Vector2(-(SidePad + 6f), -(HeaderH + 4f));

		foreach (var go in _modRows) Object.Destroy(go);
		_modRows.Clear();

		int skip  = Variables.currentCategoryPage * PerPage;
		int taken = 0;
		var page = new List<ButtonHandler.Button>();
		foreach (var b in all)
		{
			if (skip > 0) { skip--; continue; }
			if (taken >= PerPage) break;
			page.Add(b);
			taken++;
		}

		float listH = _modArea.GetComponent<RectTransform>().rect.height;
		if (listH <= 0f) listH = 380f;
		float rowGap = 4f;
		float rowH   = (listH - (PerPage - 1) * rowGap) / PerPage;

		for (int i = 0; i < page.Count; i++)
		{
			var b = page[i];
			string raw = b.buttonText;
			int nl = raw.IndexOf('\n');
			string title    = nl >= 0 ? raw.Substring(0, nl) : raw;
			string subtitle = nl >= 0 ? raw.Substring(nl + 1) : null;

			var local = b;
			var row = MakeModRow(title, subtitle, b.Enabled, () => ButtonHandler.Toggle(local));
			row.transform.SetParent(_modArea.transform, false);
			var rt = row.GetComponent<RectTransform>();
			rt.anchorMin = new Vector2(0f, 1f);
			rt.anchorMax = new Vector2(1f, 1f);
			rt.pivot     = new Vector2(0.5f, 1f);
			rt.sizeDelta = new Vector2(0f, rowH);
			rt.anchoredPosition = new Vector2(0f, -i * (rowH + rowGap));
			_modRows.Add(row);
		}
	}

	private static GameObject MakeModRow(string title, string subtitle, bool on, System.Action onClick)
	{
		var go = new GameObject("Row", typeof(Image), typeof(Button));
		go.GetComponent<Image>().color = on ? C_RowOn : C_RowOff;

		if (on)
		{
			var bar = NewImage("Bar", go.transform, C_Accent).rectTransform;
			bar.anchorMin = new Vector2(0f, 0f);
			bar.anchorMax = new Vector2(0f, 1f);
			bar.pivot     = new Vector2(0f, 0.5f);
			bar.sizeDelta = new Vector2(3f, 0f);
			bar.anchoredPosition = Vector2.zero;
		}

		bool hasSub = !string.IsNullOrEmpty(subtitle);
		Color titleCol = on ? C_Text : C_Text;

		var t = NewText(title, go.transform, 14, FontStyle.Bold, titleCol,
			hasSub ? TextAnchor.LowerLeft : TextAnchor.MiddleLeft);
		var trt = t.rectTransform;
		trt.anchorMin = new Vector2(0f, hasSub ? 0.45f : 0f);
		trt.anchorMax = new Vector2(1f, 1f);
		trt.offsetMin = new Vector2(14f, 0f);
		trt.offsetMax = new Vector2(-44f, 0f);

		if (hasSub)
		{
			var s = NewText(subtitle, go.transform, 10, FontStyle.Normal, C_TextDim, TextAnchor.UpperLeft);
			var srt = s.rectTransform;
			srt.anchorMin = new Vector2(0f, 0f);
			srt.anchorMax = new Vector2(1f, 0.45f);
			srt.offsetMin = new Vector2(14f, 0f);
			srt.offsetMax = new Vector2(-44f, 0f);
		}

		var ind = NewImage("Ind", go.transform, on ? C_Accent : C_Edge).gameObject;
		var indRt = ind.GetComponent<RectTransform>();
		indRt.anchorMin = indRt.anchorMax = new Vector2(1f, 0.5f);
		indRt.pivot     = new Vector2(1f, 0.5f);
		indRt.sizeDelta = new Vector2(10f, 10f);
		indRt.anchoredPosition = new Vector2(-16f, 0f);
		ind.GetComponent<Image>().sprite = MakeCircleSprite();

		var btn = go.GetComponent<Button>();
		btn.transition = Selectable.Transition.None;
		btn.onClick.AddListener(() => onClick?.Invoke());
		return go;
	}

	private static GameObject MakePill(string label, Color bg, Color fg, System.Action onClick)
	{
		var go = new GameObject("Pill", typeof(Image), typeof(Button));
		go.GetComponent<Image>().color = bg;
		AddOutline(go, C_Edge, 1f);
		var t = NewText(label, go.transform, 12, FontStyle.Bold, fg, TextAnchor.MiddleCenter);
		FillStretch(t.rectTransform);
		var btn = go.GetComponent<Button>();
		btn.transition = Selectable.Transition.None;
		btn.onClick.AddListener(() => onClick?.Invoke());
		return go;
	}

	private static Sprite _circle;
	private static Sprite MakeCircleSprite()
	{
		if (_circle != null) return _circle;
		const int N = 32;
		var tex = new Texture2D(N, N, TextureFormat.ARGB32, false);
		float r = N / 2f;
		for (int y = 0; y < N; y++)
		for (int x = 0; x < N; x++)
		{
			float dx = x + 0.5f - r, dy = y + 0.5f - r;
			float d = Mathf.Sqrt(dx*dx + dy*dy);
			float a = Mathf.Clamp01(r - d);
			tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
		}
		tex.Apply();
		tex.filterMode = FilterMode.Bilinear;
		_circle = Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f));
		return _circle;
	}

	private static void ClickStub(string label)
	{
		var stub = new ButtonHandler.Button(label, Category.Home, false, false, null);
		ButtonHandler.Toggle(stub);
	}

	private static Image NewImage(string name, Transform parent, Color color)
	{
		var go = new GameObject(name, typeof(Image));
		go.transform.SetParent(parent, false);
		var img = go.GetComponent<Image>();
		img.color = color;
		return img;
	}

	private static Text NewText(string s, Transform parent, int size, FontStyle style, Color color, TextAnchor align)
	{
		var go = new GameObject("Text", typeof(Text));
		go.transform.SetParent(parent, false);
		var t = go.GetComponent<Text>();
		t.text       = s;
		t.font       = GetFont();
		t.fontSize   = size;
		t.fontStyle  = style;
		t.color      = color;
		t.alignment  = align;
		t.horizontalOverflow = HorizontalWrapMode.Overflow;
		t.verticalOverflow   = VerticalWrapMode.Overflow;
		t.raycastTarget = false;
		return t;
	}

	private static void FillStretch(RectTransform rt)
	{
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		rt.offsetMin = Vector2.zero;
		rt.offsetMax = Vector2.zero;
	}

	private static void AddOutline(GameObject go, Color color, float distance)
	{
		var ol = go.AddComponent<Outline>();
		ol.effectColor    = color;
		ol.effectDistance = new Vector2(distance, distance);
	}
}

internal class PcMenuDragger : MonoBehaviour, IDragHandler, IBeginDragHandler
{
	public RectTransform Target;
	private Vector2 _grab;
	public void OnBeginDrag(PointerEventData ev) => _grab = (Vector2)Target.position - ev.position;
	public void OnDrag(PointerEventData ev)
	{
		if (Target == null) return;
		Target.position = ev.position + _grab;
		PcMenuManager.RequestRefresh();
	}
}

internal class PcMenuResizer : MonoBehaviour, IScrollHandler
{
	public RectTransform Target;
	private const float Step = 1.07f;
	private const float MinW = 320f, MaxW = 1200f, MinH = 380f, MaxH = 1400f;

	public void OnScroll(PointerEventData ev)
	{
		if (Target == null) return;
		float f = ev.scrollDelta.y > 0f ? Step : 1f / Step;
		var s = Target.sizeDelta;
		float nx = Mathf.Clamp(s.x * f, MinW, MaxW);
		float ny = Mathf.Clamp(s.y * f, MinH, MaxH);
		if (!Mathf.Approximately(nx / s.x, f)) ny = s.y * (nx / s.x);
		else if (!Mathf.Approximately(ny / s.y, f)) nx = s.x * (ny / s.y);
		Target.sizeDelta = new Vector2(nx, ny);
	}
}

