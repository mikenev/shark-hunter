using UnityEngine;

// IMGUI HUD so no Canvas/font setup is needed. Lives on the persistent GameState object.
public class GameHud : MonoBehaviour
{
    GUIStyle label;
    GUIStyle center;

    void OnGUI()
    {
        var gs = GameState.Instance;
        if (gs == null) return;

        int fontSize = Mathf.Max(12, Screen.height / 28);
        if (label == null)
        {
            label = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            center = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter };
        }
        label.fontSize = fontSize;
        center.fontSize = fontSize;

        float line = fontSize * 1.6f;

        if (Time.time < gs.MessageUntil)
        {
            Shadowed(new Rect(0, Screen.height * 0.3f, Screen.width, line * 1.5f), gs.MessageText, center);
        }

        if (!gs.HudVisible) return;

        string stats = $"HP {gs.Health}/{GameState.MaxHealth}   SCORE {gs.Score}   CONCH {gs.Conches}/{GameState.ConchesNeeded}";
        Shadowed(new Rect(10, 6, Screen.width, line), stats, label);

        if (gs.OxygenActive)
        {
            Shadowed(new Rect(10, 6 + line, fontSize * 3f, line), "AIR", label);
            Bar(new Rect(10 + fontSize * 3f, 6 + line + fontSize * 0.4f, Screen.width * 0.25f, fontSize * 0.7f),
                gs.Oxygen / GameState.MaxOxygen, new Color(0f, 0.9f, 0.85f));
        }

        if (gs.BossHealth01 >= 0f)
        {
            float w = Screen.width * 0.5f;
            var r = new Rect((Screen.width - w) / 2f, Screen.height - fontSize * 2.5f, w, fontSize * 0.9f);
            Shadowed(new Rect(r.x, r.y - line, w, line), "SHARK", center);
            Bar(r, gs.BossHealth01, new Color(0.85f, 0.15f, 0f));
        }
    }

    static void Shadowed(Rect rect, string text, GUIStyle style)
    {
        var old = GUI.color;
        GUI.color = Color.black;
        GUI.Label(new Rect(rect.x + 2, rect.y + 2, rect.width, rect.height), text, style);
        GUI.color = Color.white;
        GUI.Label(rect, text, style);
        GUI.color = old;
    }

    static void Bar(Rect rect, float fill01, Color color)
    {
        var old = GUI.color;
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4), Texture2D.whiteTexture);
        GUI.color = color;
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(fill01), rect.height), Texture2D.whiteTexture);
        GUI.color = old;
    }
}
