using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public const string SeaMap = "SeaMap";
    public const string Dive = "Dive";
    public const string SharkFight = "SharkFight";

    public static void Load(string sceneName) => SceneManager.LoadScene(sceneName);
}
