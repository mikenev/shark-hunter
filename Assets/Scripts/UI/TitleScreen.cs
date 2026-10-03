using UnityEngine;

// Title screen text and start handling. The animated water and fin are plain scene objects.
public class TitleScreen : MonoBehaviour
{
    static readonly string[] Controls =
    {
        "MOVE: WASD / ARROWS / STICK",
        "FIRE HARPOON: SPACE / Z / A BUTTON",
        "MUTE: M",
    };

    bool starting;
    GUIStyle style;

    void Update()
    {
        if (starting || !GameInput.StartPressed) return;

        starting = true;
        AudioManager.Play(Sfx.Select);
        GameState.Instance.ResetGame();
        SceneLoader.Load(SceneLoader.SeaMap);
    }

    void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
            };
        }

        int unit = Mathf.Max(12, Screen.height / 28);

        style.fontSize = unit * 3;
        Shadowed(new Rect(0, Screen.height * 0.12f, Screen.width, unit * 4f), "SHARK HUNTER",
            new Color(1f, 0.9f, 0.4f), style);

        style.fontSize = unit;
        if ((int)(Time.time * 2f) % 2 == 0)
            Shadowed(new Rect(0, Screen.height * 0.38f, Screen.width, unit * 2f), "PRESS SPACE TO START",
                Color.white, style);

        style.fontSize = Mathf.RoundToInt(unit * 0.8f);
        float y = Screen.height * 0.55f;
        foreach (var line in Controls)
        {
            Shadowed(new Rect(0, y, Screen.width, unit * 1.5f), line, Color.white, style);
            y += unit * 1.5f;
        }
    }

    static void Shadowed(Rect rect, string text, Color color, GUIStyle style)
    {
        var old = GUI.color;
        GUI.color = Color.black;
        GUI.Label(new Rect(rect.x + 3, rect.y + 3, rect.width, rect.height), text, style);
        GUI.color = color;
        GUI.Label(rect, text, style);
        GUI.color = old;
    }
}
