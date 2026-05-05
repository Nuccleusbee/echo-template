using EchoTemplate.Mods;

namespace EchoTemplate.Menu;

public class ModButtons
{
	public static ButtonHandler.Button[] buttons = new ButtonHandler.Button[]
	{
		// ──────────────── HOME ────────────────
		new ButtonHandler.Button("Movement", Category.Home, false, false,
			delegate { ButtonHandler.ChangePage(Category.Movement); }),
		new ButtonHandler.Button("Visual",   Category.Home, false, false,
			delegate { ButtonHandler.ChangePage(Category.Visual);   }),
		new ButtonHandler.Button("Fun",      Category.Home, false, false,
			delegate { ButtonHandler.ChangePage(Category.Fun);      }),
		new ButtonHandler.Button("Room",     Category.Home, false, false,
			delegate { ButtonHandler.ChangePage(Category.Room);     }),

		// ──────────────── MOVEMENT ────────────────
		new ButtonHandler.Button("Jump\nOne-press upward bump", Category.Movement, false, false,
			delegate { ExampleJump.Run(); }),

		// ──────────────── FUN ────────────────
		new ButtonHandler.Button("Hello\nPops a notification on click", Category.Fun, false, false,
			delegate { ExampleHello.Run(); }),
		new ButtonHandler.Button("Spin Head\nRotates your rig's head", Category.Fun, true, false,
			delegate { ExampleSpinHead.Tick(); },
			delegate { ExampleSpinHead.Disable(); }),
	};
}
