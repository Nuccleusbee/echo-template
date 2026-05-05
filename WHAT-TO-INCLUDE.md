# What to include when distributing

The bones of Echo Template are just a handful of files. When you (or anyone forking this) want to ship a clean starter package, these are the files to copy from the parent EchoMenu source tree:

## Required core files

Copy these into `Source/` matching the existing folder structure:

```
Source/
├── Initialization/
│   └── BepInExInitializer.cs        ← from EchoTemplate.Initialization/
├── GUI/
│   ├── Main.cs                      ← from EchoTemplate.GUI/
│   └── PcMenuManager.cs             ← from EchoTemplate.GUI/
├── Menu/
│   ├── ButtonHandler.cs             ← from EchoTemplate.Menu/
│   ├── Category.cs                  ← from EchoTemplate.Menu/
│   ├── ModButtons.cs                ← from EchoTemplate.Menu/
│   └── Optimizations.cs             ← from EchoTemplate.Menu/
├── Utilities/
│   ├── Variables.cs                 ← from EchoTemplate.Utilities/
│   └── NotificationLib.cs           ← from EchoTemplate.Utilities/
└── Mods/
    ├── ExampleHello.cs              ← already in this template
    ├── ExampleSpinHead.cs           ← already in this template
    └── ExampleJump.cs               ← already in this template
```

Also copy any `IgnoresAccessChecksToAttribute.cs` that the parent uses (it grants internal access to `Assembly-CSharp` so the menu can reach private GTag types). Look in `System.Runtime.CompilerServices/` in the parent.

## What to STRIP if you're forking from the full EchoMenu source

The full menu has more than the template needs. When making a clean fork, **delete** the following before distributing:

- `EchoTemplate.Utilities.Patches/` — the entire anti-ban / report-blocker / honeypot folder. It's specific to the live menu and not part of the rendering pipeline.
- `EchoTemplate.Mods/Admin*.cs` — admin gate + commands + target picker
- `EchoTemplate.Mods/ModderHoneypotMod.cs`, `ModCheckerService.cs`, `ModCheckerMods.cs`, `MenuFingerprintDb.cs`, `RpcObserverPatch.cs`, `DeepCheckMod.cs`, `FpsSpooferMod.cs`, `AntiBanScopeMod.cs`, `AntiClientAttackMod.cs` — anti-modder detection layer
- `EchoTemplate.Mods/MasterProxyMod.cs` — proxy-master command
- `Echo_Menu/Loader.cs`, `Echo_Menu/DisableListService.cs` — the Discord-driven kill-switch loader
- `discord-bot/`, `railway-server/` — backend services
- `BUG-BOUNTY*.md`, `DESIGN.md` — internal docs not for redistribution
- All real mods you don't want to include (gun-style mods, admin OP mods, etc.)

Inside `BepInExInitializer.cs`, also strip the lines that arm the patches at boot:

```csharp
// DELETE these from Awake() in your fork:
PatchHandler.PatchAll();           // arms anti-ban
ModderHoneypotMod.StartHoneypot(); // arms honeypot
DisableListService.StartPolling(); // discord kill-switch
```

Replace them with just the rendering-essential boot:

```csharp
new Harmony("com.yourname.yourmenu").PatchAll();
// cache GTPlayer / GorillaTagger / ControllerInputPoller into Variables
```

`ModButtons.cs` — empty out the entire `buttons` array and replace with only your example/starter mods. The parent file has hundreds of entries, you want maybe 5–10 to demonstrate how it works.

## Final structure for a clean ship

```
YourMenuTemplate/
├── README.md
├── LICENSE.md
├── ADDING-A-MOD.md
├── YourMenu.csproj
├── Source/
│   ├── Initialization/BepInExInitializer.cs
│   ├── GUI/Main.cs
│   ├── GUI/PcMenuManager.cs
│   ├── Menu/ButtonHandler.cs
│   ├── Menu/Category.cs
│   ├── Menu/ModButtons.cs
│   ├── Menu/Optimizations.cs
│   ├── Utilities/Variables.cs
│   ├── Utilities/NotificationLib.cs
│   └── Mods/Example*.cs
├── libs/
│   └── (empty — user fills with their GTag DLLs)
└── System.Runtime.CompilerServices/IgnoresAccessChecksToAttribute.cs
```

That's about 15-20 files. ZIP it, push to GitHub, share the link. Anyone can clone, fill `libs/`, build, and have a working starter menu.
