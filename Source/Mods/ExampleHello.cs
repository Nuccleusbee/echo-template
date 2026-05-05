using EchoTemplate.Utilities;

namespace EchoTemplate.Mods;

/// Action mod (one-press). Pops a notification when clicked.
/// Anti-ban posture: client-sided.
internal static class ExampleHello
{
	public static void Run()
	{
		NotificationLib.SendNotification("<color=cyan>hello world</color> from Echo Template");
	}
}
