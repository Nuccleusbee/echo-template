# Echo Template

A starter mod menu for **Gorilla Tag** (Steam / PC). Drop in a `.cs` file, register one button, build, ship. This is the bones you can fork to make your own menu — no proprietary anti-cheat patches, no obfuscated code, no licensing strings attached beyond keeping the copyright notice.

**Built on:** BepInEx 5 + HarmonyX. **Targets:** GTag flat-PC and VR.

---

## What you get

| File | What it is |
|---|---|
| `Source/Initialization/BepInExInitializer.cs` | Plugin entry point — Harmony patches + game cache |
| `Source/GUI/Main.cs` | Frame-driver — pulses every enabled mod, manages VR menu spawn / drop |
| `Source/GUI/PcMenuManager.cs` | The PC overlay UI (header, mod rows, side rails, footer) |
| `Source/Menu/ButtonHandler.cs` | The button data model + click dispatcher |
| `Source/Menu/Category.cs` | Page enum (Home, Movement, Visual, etc.) |
| `Source/Menu/ModButtons.cs` | The button registry — **add your mods here** |
| `Source/Menu/Optimizations.cs` | Refresh helpers (rebuilds the visible UI after a state change) |
| `Source/Utilities/Variables.cs` | Shared state (theme colors, current page, FPS counter, etc.) |
| `Source/Utilities/NotificationLib.cs` | TMP-based on-screen notification queue |
| `Source/Mods/ExampleHello.cs` | Example: one-press button that pops a notification |
| `Source/Mods/ExampleSpinHead.cs` | Example: continuous toggle that spins your head |
| `Source/Mods/ExampleJump.cs` | Example: one-press button that bumps you up |
| `EchoTemplate.csproj` | Build config — references your local GTag DLLs in `libs/` |
| `LICENSE.md` | Permissive license — use it however, just don't strip the copyright |

**`libs/`** is empty — you copy the GTag reference DLLs there yourself (one-time setup, see [Build](#build)). They're not redistributable.

---

## Quick start

1. **Clone or download** this folder.
2. **Copy GTag DLLs into `libs/`** — see [Build](#build) for the list.
3. **`dotnet build -c Release`** from the folder root.
4. **Drop `bin/Release/EchoTemplate.dll`** into `<GTag install>/BepInEx/plugins/`.
5. **Launch GTag** in flat-PC mode. Press **F1** to toggle the menu.

That's the whole loop. Edit code → rebuild → copy DLL → restart game.

---

## How to use the menu in-game

### Flat-PC mode

- Menu auto-appears when you launch in flat mode. **F1** hides / shows it.
- **Drag the title bar** to move it around the screen.
- **Scroll wheel on the title bar** resizes the panel.
- **Click any row** to toggle a mod.
- **‹ / ›** side rails paginate within a category (8 mods per page).
- **⌂ HOME** at the bottom-right returns to the category list.
- Some mods come with sliders (`Change X Speed` etc.) — **scroll wheel on the slider row** adjusts the value on PC.

### VR mode

- **Hold the secondary face button** (B on right Quest controller, X on left) to summon the menu next to that hand.
- Point the **opposite hand** at a button to click it.
- **Side bars** scroll category pages (`<` previous, `>` next).
- **Home** button at the bottom returns to the category list.

---

## Adding a mod

A mod is a static class with two methods. Drop it in `Source/Mods/` and register one button line in `ModButtons.cs`. See **[ADDING-A-MOD.md](ADDING-A-MOD.md)** for the full walkthrough.

Quick version:

```csharp
// Source/Mods/MyCoolMod.cs
using UnityEngine;
using EchoTemplate.Utilities;

namespace EchoTemplate.Mods;

internal static class MyCoolMod
{
    public static void Tick()
    {
        // Runs every frame while the toggle is on.
        VRRig.LocalRig.head.trackingRotationOffset.y += 5f;
    }

    public static void Disable()
    {
        // Runs once when the toggle goes off — clean up here.
        VRRig.LocalRig.head.trackingRotationOffset = Vector3.zero;
    }
}
```

Then in `ModButtons.cs`, find the `Category.Fun` section and add:

```csharp
new ButtonHandler.Button("My Cool Mod", Category.Fun, true, false,
    delegate { MyCoolMod.Tick(); },
    delegate { MyCoolMod.Disable(); }),
```

Build, drop the DLL, restart game. Your mod shows up under the Fun category.

---

## Architecture in 30 seconds

1. `BepInExInitializer.Awake()` runs once on plugin load. It calls `new Harmony(...).PatchAll()` which attaches a Prefix to `GTPlayer.LateUpdate`.
2. The Prefix calls `Main.Prefix()` every frame.
3. `Main.Prefix()` does three things:
   - **Pulse**: iterate every `ModButton` and call `onEnable` if its `Enabled` flag is true. This is how continuous mods stay active.
   - **VR menu**: detect the menu-summon button, spawn / position / drop the floating world-space menu.
   - **PC menu**: tick `PcMenuManager.Tick()` which manages the screen-space overlay.
4. Click flow: the in-game button (VR cube collider OR PC UI button) calls `ButtonHandler.Toggle(button)` which flips `Enabled` and re-renders.

That's the whole engine. ~600 lines of C# total in the rendering / dispatch path.

---

## Build

You need the .NET SDK 8 (or newer) and a working GTag install for the reference DLLs.

```bash
# 1. Drop the GTag reference DLLs into libs/
#    (one-time — these come from <GTag>/Gorilla Tag_Data/Managed/)
cp "<GTag>/Gorilla Tag_Data/Managed/Assembly-CSharp.dll"   libs/
cp "<GTag>/Gorilla Tag_Data/Managed/UnityEngine.CoreModule.dll"  libs/
cp "<GTag>/Gorilla Tag_Data/Managed/UnityEngine.UI.dll"    libs/
cp "<GTag>/Gorilla Tag_Data/Managed/UnityEngine.InputSystem.dll" libs/
cp "<GTag>/Gorilla Tag_Data/Managed/Photon3Unity3D.dll"   libs/
cp "<GTag>/Gorilla Tag_Data/Managed/PhotonUnityNetworking.dll"   libs/
# (the .csproj has the full required list; copy any others that fail to resolve)

# 2. Also need the BepInEx + HarmonyX DLLs
cp "<GTag>/BepInEx/core/BepInEx.dll" libs/
cp "<GTag>/BepInEx/core/0Harmony.dll" libs/

# 3. Build
dotnet build -c Release

# 4. Deploy
cp bin/Release/EchoTemplate.dll "<GTag>/BepInEx/plugins/EchoTemplate.dll"
```

If GTag updates and your menu stops working, replace the DLLs in `libs/` with the new ones from the updated install and rebuild.

---

## Licensing

Permissive. Use it, modify it, sell it, fork it, ship it closed-source. The only ask is keep the copyright in `LICENSE.md` if you redistribute the source. See `LICENSE.md` for the exact wording.

---

## Sharing your fork

When you make your own menu off this template, you can publish it however you want. If you want to keep the "made on Echo Template" credit visible somewhere (about page, README, in-game splash), that's appreciated but not required.

If you want to package YOUR menu as a starter for others to use, copy the structure of this folder, replace the example mods with your own, swap the LICENSE if you want, and ship it. That's the whole point — the bones are reusable.
