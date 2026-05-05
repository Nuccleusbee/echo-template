# Adding a mod

Every mod is a static class with two methods. The menu's frame loop calls `Tick()` every frame the toggle is on, and `Disable()` once when the toggle goes off. That's it. No MonoBehaviour, no Awake, no Update.

---

## 1. Pick a classification

| Type | Label suffix | Lifecycle | Examples |
|---|---|---|---|
| **Continuous toggle** | `"Name"` | `Tick()` runs every frame while toggled on | Spin Head, Invisible, Fly |
| **Trigger-gated** | `"Name T"` | `Tick()` runs every frame; you read trigger and gate the effect inside | Grab Rig T, Spaz Rig T |
| **Grip-gated** | `"Name G"` | Same but gates on grip | Wall Walk |
| **Action (one-press)** | `"Name"` | `onClick` fires once per click — no toggle state | Disconnect, Untag All |

**Decision tree:**
- Effect finishes by itself after one press → **action** (`isToggle: false`, no `Disable`)
- Effect runs the whole time the toggle is on → **continuous** (`isToggle: true`)
- Effect only while you hold trigger / grip → **gated** (label suffix `T`/`G`, gate inside `Tick`)

---

## 2. Write the file

Drop a new file into `Source/Mods/`. The shape:

```csharp
using UnityEngine;
using EchoTemplate.Utilities;

namespace EchoTemplate.Mods;

/// One paragraph: what this mod does and how to use it.
/// Anti-ban posture: client-sided / read-only / RPC-emitting / unknown.
internal static class YourMod
{
    private static bool _wasPressed;       // example state field

    public static void Tick()
    {
        // Effect goes here.
    }

    public static void Disable()
    {
        // Reset every state field you touched in Tick.
        _wasPressed = false;
    }
}
```

---

## 3. Register the button

Open `Source/Menu/ModButtons.cs`. Find the `Category.X` section that fits and paste ONE of these forms:

```csharp
// Continuous toggle
new ButtonHandler.Button("Your Mod Name", Category.Fun, true, false,
    delegate { YourMod.Tick(); },
    delegate { YourMod.Disable(); }),

// Trigger-gated
new ButtonHandler.Button("Your Mod Name T", Category.Movement, true, false,
    delegate { YourMod.Tick(); },
    delegate { YourMod.Disable(); }),

// Action (one-press)
new ButtonHandler.Button("Your Mod Name", Category.Room, false, false,
    delegate { YourMod.Run(); }),

// With a description subtitle
new ButtonHandler.Button("Your Mod Name\nWhat it does in 4-6 words", Category.Visual, true, false,
    delegate { YourMod.Tick(); },
    delegate { YourMod.Disable(); }),
```

Constructor signature:

```csharp
ButtonHandler.Button(
    string   label,           // shown on the button (use \n to add a subtitle)
    Category page,            // which menu page it lives on
    bool     isToggle,        // true = stays toggled, false = one-press action
    bool     isActive,        // initial state (almost always false)
    Action   onClick,         // called when toggled on (or each press for action)
    Action   onDisable = null,// called when toggled off (omit for action mods)
    bool     doesNeedMaster = false)  // gates on PhotonNetwork.IsMasterClient
```

---

## 4. Reading inputs (controllers / mouse / keyboard)

```csharp
using UnityEngine.InputSystem;

// Controllers (VR + the WalkSim virtual mouse mappings on PC)
var poller = ControllerInputPoller.instance;
if (poller == null) return;

bool rTrigger  = poller.rightControllerIndexFloat > 0.5f;
bool rGrip     = poller.rightControllerGripFloat  > 0.5f;
bool rPrimary  = poller.rightControllerPrimaryButton;
bool rSecondary= poller.rightControllerSecondaryButton;
bool lTrigger  = poller.leftControllerIndexFloat  > 0.5f;
bool lGrip     = poller.leftControllerGripFloat   > 0.5f;

// Keyboard / mouse (PC)
var kb    = Keyboard.current;
var mouse = Mouse.current;
if (kb != null && kb.spaceKey.isPressed) { /* ... */ }
if (mouse != null && mouse.rightButton.isPressed) { /* ... */ }
```

**Edge-trigger** (one fire per click, ignore held repeats):

```csharp
private static bool _wasPressed;

bool pressed = poller.rightControllerIndexFloat > 0.5f;
bool risingEdge = pressed && !_wasPressed;
_wasPressed = pressed;
if (!risingEdge) return;
// fire once
```

---

## 5. Notifications

```csharp
NotificationLib.SendNotification("Hello world");

// With color (Unity rich text):
NotificationLib.SendNotification("<color=red>WARN</color> something happened");

// Custom display duration in milliseconds (default 8000):
NotificationLib.SendNotification("Quick", 2000);
```

Don't post every frame — wrap them in an event (button press, state change). Spamming the queue makes the HUD unreadable.

---

## 6. Useful APIs

**Your own rig (your local view of yourself):**
```csharp
GorillaTagger.Instance.offlineVRRig             // self-view rig
GorillaTagger.Instance.rightHandTransform       // your right hand
GorillaTagger.Instance.leftHandTransform
GTPlayer.Instance                               // your physics / locomotion
```

**Your networked rig (what others see):**
```csharp
VRRig.LocalRig                                  // your remote rig
VRRig.LocalRig.head.trackingRotationOffset      // Vector3 — drives Spin Head
VRRig.LocalRig.headMesh
VRRig.LocalRig.leftHandTransform
VRRig.LocalRig.rightHandTransform
```

**All other rigs in the room:**
```csharp
foreach (var rig in VRRigCache.Instance.allRigs)
{
    if (rig == VRRig.LocalRig) continue;
    // ...
}
```

**Photon networking state:**
```csharp
Photon.Pun.PhotonNetwork.InRoom
Photon.Pun.PhotonNetwork.CurrentRoom
Photon.Pun.PhotonNetwork.IsMasterClient
NetworkSystem.Instance.RoomName
NetworkSystem.Instance.ReturnToSinglePlayer()  // proper way to leave
```

**Cleaning up:** anything you spawn (GameObject, line renderer, etc.) needs to be destroyed in `Disable()`.

---

## 7. Don't forget

- **Cleanup in `Disable()`** — every state field, GameObject, line renderer, rigidbody flag you touched in `Tick()` must be reset / destroyed in `Disable()`. The user can toggle on / off arbitrarily; if you leave state behind, the next on-cycle starts dirty.
- **Don't write `Physics.gravity`** or `Time.timeScale` directly — use rigidbody flags or local timers instead. Global writes break other mods and the game itself.
- **Don't send unsolicited Photon events** that target other players to hurt their gameplay (kicks, crashes, lag attacks). They get instant-detected by every vanilla MonkeAgent in the room and the user gets banned. The template doesn't ship anything that does this and shouldn't.
- **URP shader is mandatory** for visible primitives in GTag — the default `Standard` shader fails silently. Use `Shader.Find("Universal Render Pipeline/Unlit")` with `GorillaTag/UberShader` as a fallback.

---

## 8. Worked example

Open `Source/Mods/ExampleSpinHead.cs` for a complete continuous-toggle mod, or `Source/Mods/ExampleHello.cs` for an action mod. They're a couple of lines each — copy one as your starting file, rename, fill in the body.
