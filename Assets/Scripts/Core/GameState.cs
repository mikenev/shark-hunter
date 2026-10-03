using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Persistent run state (health, score, conches). Created automatically before the first
// scene loads, so any scene can be played on its own.
public class GameState : MonoBehaviour
{
    public const int MaxHealth = 5;
    public const float MaxOxygen = 25f;
    public const int ConchesNeeded = 3;
    const float InvulnerableSeconds = 1.2f;

    public static GameState Instance { get; private set; }

    public int Health { get; private set; }
    public int Score { get; private set; }
    public int Conches => collected.Count;
    public bool Ending { get; private set; }
    public bool HudVisible { get; private set; } = true;

    // Set by scene objects, read by the HUD.
    public float Oxygen;
    public bool OxygenActive;
    public float BossHealth01 = -1f;

    public string MessageText { get; private set; } = "";
    public float MessageUntil { get; private set; }

    public bool HasSavedBoatPos { get; private set; }
    public Vector2 SavedBoatPos { get; private set; }

    readonly HashSet<string> collected = new HashSet<string>();
    float invulnerableUntil;

    public bool Invulnerable => Time.time < invulnerableUntil;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("GameState");
        go.AddComponent<GameState>();
        go.AddComponent<GameHud>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ResetGame();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void ResetGame()
    {
        Health = MaxHealth;
        Score = 0;
        Oxygen = MaxOxygen;
        Ending = false;
        HasSavedBoatPos = false;
        invulnerableUntil = 0f;
        collected.Clear();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Oxygen = MaxOxygen;
        OxygenActive = false;
        BossHealth01 = -1f;
        invulnerableUntil = 0f;
        HudVisible = scene.name != SceneLoader.Title;

        switch (scene.name)
        {
            case SceneLoader.SeaMap:
                ShowMessage(Conches < ConchesNeeded
                    ? "SAIL TO THE REEF AND DIVE FOR CONCHES"
                    : "THE SHARK IS WAITING. FIND ITS FIN!", 4f);
                break;
            case SceneLoader.Dive:
                ShowMessage("COLLECT THE CONCHES. WATCH YOUR AIR!", 4f);
                break;
            case SceneLoader.SharkFight:
                ShowMessage("HARPOON THE SHARK!  SPACE / Z TO FIRE", 4f);
                break;
        }
    }

    public void ShowMessage(string text, float seconds)
    {
        MessageText = text;
        MessageUntil = Time.time + seconds;
    }

    public void AddScore(int amount) => Score += amount;

    public bool HasCollected(string id) => collected.Contains(id);

    public void Collect(string id)
    {
        if (!collected.Add(id)) return;
        AudioManager.Play(Sfx.Collect);
        AddScore(100);
        ShowMessage(Conches >= ConchesNeeded
            ? "ALL CONCHES! RETURN TO THE BOAT"
            : $"CONCH {Conches}/{ConchesNeeded}", 2.5f);
    }

    public void SaveBoatPos(Vector2 pos)
    {
        HasSavedBoatPos = true;
        SavedBoatPos = pos;
    }

    // Returns true if the hit landed (false while invulnerable or already ending).
    public bool Damage(int amount)
    {
        if (Ending || Invulnerable) return false;

        Health = Mathf.Max(0, Health - amount);
        invulnerableUntil = Time.time + InvulnerableSeconds;

        if (Health <= 0) StartCoroutine(EndRoutine("GAME OVER", 2.5f, Sfx.GameOver));
        else AudioManager.Play(Sfx.PlayerHurt);
        return true;
    }

    public void Win()
    {
        if (Ending) return;
        AddScore(1000);
        StartCoroutine(EndRoutine($"THE SHARK IS DEAD!  SCORE {Score}", 4f, Sfx.Win));
    }

    IEnumerator EndRoutine(string text, float seconds, Sfx stinger)
    {
        Ending = true;
        ShowMessage(text, seconds + 1f);
        AudioManager.StopMusic();
        AudioManager.Play(stinger);
        yield return new WaitForSeconds(seconds);
        ResetGame();
        SceneLoader.Load(SceneLoader.Title);
    }
}
