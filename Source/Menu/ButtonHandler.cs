using System;
using System.Collections.Generic;
using EchoTemplate.Utilities;

namespace EchoTemplate.Menu;

public class ButtonHandler
{
	public class Button
	{
		public string   buttonText  { get; set; }
		public bool     isToggle    { get; set; }
		public bool     NeedsMaster { get; set; }
		public bool     Enabled     { get; set; }
		public Action   onEnable    { get; set; }
		public Action   onDisable   { get; set; }
		public Category Page        { get; set; }

		public Button(string label, Category page, bool isToggle, bool isActive,
			Action onClick, Action onDisable = null, bool doesNeedMaster = false)
		{
			buttonText = label;
			this.isToggle = isToggle;
			Enabled = isActive;
			onEnable = onClick;
			Page = page;
			this.onDisable = onDisable;
			NeedsMaster = doesNeedMaster;
		}

		public void SetText(string newText) => buttonText = newText;
	}

	public static void Toggle(Button button)
	{
		switch (button.buttonText)
		{
			case "<":  NavigatePage(false); break;
			case ">":  NavigatePage(true);  break;
			case "ReturnButton":     ReturnHome(); break;
			case "DisconnectButton": Photon.Pun.PhotonNetwork.Disconnect(); break;
			default: ToggleButton(button); break;
		}
	}

	public static void NavigatePage(bool forward)
	{
		int total = GetTotalPages(Variables.currentPage);
		int last  = total - 1;
		Variables.currentCategoryPage += forward ? 1 : -1;
		if (Variables.currentCategoryPage < 0)    Variables.currentCategoryPage = last;
		else if (Variables.currentCategoryPage > last) Variables.currentCategoryPage = 0;
	}

	private static void ReturnHome()
	{
		Variables.currentPage = Category.Home;
		Variables.currentCategoryPage = 0;
	}

	private static int GetTotalPages(Category page)
	{
		int count = GetButtonInfoByPage(page).Count;
		if (count == 0) return 1;
		return (count + Variables.ButtonsPerPage - 1) / Variables.ButtonsPerPage;
	}

	public static void ChangePage(Category page)
	{
		Variables.currentCategoryPage = 0;
		Variables.currentPage = page;
	}

	public static void ToggleButton(Button b)
	{
		if (!b.isToggle)
		{
			b.onEnable?.Invoke();
			return;
		}
		b.Enabled = !b.Enabled;
		if (b.Enabled) b.onEnable?.Invoke();
		else           b.onDisable?.Invoke();
	}

	public static List<Button> GetButtonInfoByPage(Category page)
	{
		var list = new List<Button>();
		foreach (var b in ModButtons.buttons)
			if (b.Page == page) list.Add(b);
		return list;
	}

	public static void ChangeButtonText(string current, string newText)
	{
		foreach (var b in ModButtons.buttons)
			if (b.buttonText.StartsWith(current)) { b.SetText(newText); return; }
	}
}
