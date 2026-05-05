using UnityEngine;

namespace EchoTemplate.Mods;

/// Continuous toggle. Spins your networked rig's head while toggled on.
/// Anti-ban posture: client-sided (only writes your OWN rig's tracking offset).
internal static class ExampleSpinHead
{
	public static float SpeedDegPerSec = 360f;

	public static void Tick()
	{
		var rig = VRRig.LocalRig;
		if (rig == null) return;
		var off = rig.head.trackingRotationOffset;
		off.y += SpeedDegPerSec * Time.deltaTime;
		rig.head.trackingRotationOffset = off;
	}

	public static void Disable()
	{
		var rig = VRRig.LocalRig;
		if (rig != null) rig.head.trackingRotationOffset = Vector3.zero;
	}
}
