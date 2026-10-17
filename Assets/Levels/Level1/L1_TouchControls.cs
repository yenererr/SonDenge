using UnityEngine;
using UnityEngine.InputSystem;

namespace Jam.Levels.Level1
{
    /// <summary>Shared screen regions for the mobile direction pad and action button.</summary>
    public static class L1_TouchControls
    {
        private static GUIStyle paperButton;
        private static Rect Left => new Rect(20f, Screen.height - 95f, 60f, 60f);
        private static Rect Right => new Rect(160f, Screen.height - 95f, 60f, 60f);
        private static Rect Up => new Rect(90f, Screen.height - 165f, 60f, 60f);
        private static Rect Down => new Rect(90f, Screen.height - 95f, 60f, 60f);
        private static Rect Action => new Rect(Screen.width - 92f, Screen.height - 95f, 70f, 70f);

        public static bool Available => Touchscreen.current != null;
        public static float Horizontal => (Held(Right) ? 1f : 0f) - (Held(Left) ? 1f : 0f);
        public static float Vertical => (Held(Up) ? 1f : 0f) - (Held(Down) ? 1f : 0f);
        public static bool ActionHeld => Held(Action);

        public static bool IsControlPosition(Vector2 bottomLeftPosition)
        {
            var point = new Vector2(bottomLeftPosition.x, Screen.height - bottomLeftPosition.y);
            return Left.Contains(point) || Right.Contains(point) || Up.Contains(point) ||
                Down.Contains(point) || Action.Contains(point);
        }

        private static bool Held(Rect region)
        {
            var screen = Touchscreen.current;
            if (screen == null) return false;
            for (var i = 0; i < screen.touches.Count; i++)
            {
                var touch = screen.touches[i];
                if (!touch.press.isPressed) continue;
                var position = touch.position.ReadValue();
                if (region.Contains(new Vector2(position.x, Screen.height - position.y))) return true;
            }
            return false;
        }

        public static void Draw()
        {
            if (!Available) return;
            DrawButton(Left, "◀", Held(Left));
            DrawButton(Right, "▶", Held(Right));
            DrawButton(Up, "▲", Held(Up));
            DrawButton(Down, "▼", Held(Down));
            DrawButton(Action, "E", ActionHeld);
        }

        private static void DrawButton(Rect region, string caption, bool held)
        {
            if (paperButton == null)
            {
                paperButton = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 23, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter
                };
                paperButton.normal.textColor = L1_PaperGui.Ink;
            }
            L1_PaperGui.Panel(region, held ? L1_PaperGui.Slip : L1_PaperGui.Parchment,
                held ? new Color32(156, 88, 33, 255) : L1_PaperGui.Edge);
            GUI.Label(region, caption, paperButton);
        }
    }
}
