using UnityEngine;
using GorillaLocomotion;

namespace EchoTemplate.Mods;

/// Action mod (one-press). Adds an upward velocity bump to the player.
/// Anti-ban posture: client-sided (only touches your local rigidbody).
internal static class ExampleJump
{
	public static float JumpVelocity = 8f;

	public static void Run()
	{
		var rb = GorillaTagger.Instance?.rigidbody;
		if (rb == null) return;
		var v = rb.velocity;
		v.y = JumpVelocity;
		rb.velocity = v;
	}
}
