using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Wave = ChipSynth.Wave;

public enum Sfx { Fire, SharkHit, PlayerHurt, Collect, Warning, Charge, Win, GameOver, Select }

// Generates all audio procedurally at startup and plays music per scene. Press M to mute.
// Created automatically before the first scene loads.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    AudioSource music;
    AudioSource sfx;
    readonly Dictionary<Sfx, AudioClip> sfxClips = new Dictionary<Sfx, AudioClip>();
    AudioClip seaMusic, diveMusic, fightMusic;
    bool muted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("AudioManager");
        go.AddComponent<AudioManager>();
        DontDestroyOnLoad(go);
    }

    public static void Play(Sfx effect)
    {
        if (Instance != null && Instance.sfxClips.TryGetValue(effect, out var clip))
            Instance.sfx.PlayOneShot(clip);
    }

    public static void StopMusic()
    {
        if (Instance != null) Instance.music.Stop();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        music = gameObject.AddComponent<AudioSource>();
        music.loop = true;
        music.volume = 0.5f;
        sfx = gameObject.AddComponent<AudioSource>();

        BuildSfx();
        BuildMusic();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        var k = Keyboard.current;
        if (k != null && k.mKey.wasPressedThisFrame)
        {
            muted = !muted;
            AudioListener.volume = muted ? 0f : 1f;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioClip clip;
        switch (scene.name)
        {
            case SceneLoader.Dive: clip = diveMusic; break;
            case SceneLoader.SharkFight: clip = fightMusic; break;
            default: clip = seaMusic; break;
        }

        if (music.clip == clip && music.isPlaying) return;
        music.clip = clip;
        music.Play();
    }

    // ------------------------------------------------------------ generation

    void BuildSfx()
    {
        Add(Sfx.Fire, ChipSynth.Tone(900f, 0.12f, Wave.Pulse25, 0.25f, 300f));
        Add(Sfx.SharkHit, Mixed(
            ChipSynth.Tone(220f, 0.18f, Wave.Square, 0.25f, 110f),
            ChipSynth.Tone(300f, 0.1f, Wave.Noise, 0.25f)));
        Add(Sfx.PlayerHurt, Mixed(
            ChipSynth.Tone(440f, 0.3f, Wave.Square, 0.25f, 110f),
            ChipSynth.Tone(200f, 0.15f, Wave.Noise, 0.2f)));
        Add(Sfx.Collect, Arpeggio(Wave.Square, 0.06f, 0.2f, 660f, 880f, 1100f, 1320f));
        Add(Sfx.Warning, ChipSynth.Concat(
            ChipSynth.Tone(330f, 0.1f, Wave.Square, 0.2f, -1f, 0.5f),
            ChipSynth.Silence(0.05f),
            ChipSynth.Tone(330f, 0.1f, Wave.Square, 0.2f, -1f, 0.5f)));
        Add(Sfx.Charge, ChipSynth.Tone(120f, 0.4f, Wave.Noise, 0.25f, -1f, 0.6f));
        Add(Sfx.Win, ChipSynth.Concat(
            Arpeggio(Wave.Pulse25, 0.1f, 0.22f, 523f, 659f, 784f, 1047f),
            ChipSynth.Tone(1047f, 0.5f, Wave.Pulse25, 0.22f, -1f, 0.5f)));
        Add(Sfx.GameOver, ChipSynth.Concat(
            Arpeggio(Wave.Square, 0.18f, 0.2f, 440f, 392f, 349f),
            ChipSynth.Tone(294f, 0.7f, Wave.Triangle, 0.3f, 150f, 0.5f)));
        Add(Sfx.Select, Arpeggio(Wave.Square, 0.05f, 0.2f, 880f, 1320f));
    }

    void BuildMusic()
    {
        // Sea: bouncy A minor.
        seaMusic = MakeClip("SeaTheme", ChipSynth.Track(
            new[] { 69, 0, 72, 0, 76, 0, 72, 69,  67, 0, 71, 0, 74, 0, 71, 67,
                    65, 0, 69, 0, 72, 0, 69, 65,  64, 67, 71, 0, 76, 74, 71, 67 },
            new[] { 45, 0, 45, 0, 45, 0, 45, 0,  43, 0, 43, 0, 43, 0, 43, 0,
                    41, 0, 41, 0, 41, 0, 41, 0,  40, 0, 40, 0, 43, 0, 47, 0 },
            140f));

        // Dive: slow floating D minor arpeggios.
        diveMusic = MakeClip("DiveTheme", ChipSynth.Track(
            new[] { 62, 0, 65, 0, 69, 0, 65, 0,  62, 0, 65, 0, 70, 0, 65, 0,
                    60, 0, 64, 0, 67, 0, 64, 0,  58, 0, 62, 0, 65, 0, 62, 0 },
            new[] { 38, 0, 0, 0, 38, 0, 0, 0,  34, 0, 0, 0, 34, 0, 0, 0,
                    36, 0, 0, 0, 36, 0, 0, 0,  38, 0, 0, 0, 38, 0, 0, 0 },
            90f));

        // Fight: tense semitone pulsing that climbs.
        fightMusic = MakeClip("FightTheme", ChipSynth.Track(
            new[] { 76, 0, 77, 0, 76, 0, 77, 0,  76, 0, 77, 0, 76, 77, 76, 77,
                    79, 0, 80, 0, 79, 0, 80, 0,  79, 80, 79, 80, 79, 80, 79, 80 },
            new[] { 40, 0, 41, 0, 40, 0, 41, 0,  40, 41, 40, 41, 40, 41, 40, 41,
                    43, 0, 44, 0, 43, 0, 44, 0,  43, 44, 43, 44, 43, 44, 43, 44 },
            160f));
    }

    void Add(Sfx effect, float[] samples) => sfxClips[effect] = MakeClip(effect.ToString(), samples);

    static AudioClip MakeClip(string name, float[] samples)
    {
        var clip = AudioClip.Create(name, samples.Length, 1, ChipSynth.SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    static float[] Mixed(float[] a, float[] b)
    {
        var result = new float[Mathf.Max(a.Length, b.Length)];
        ChipSynth.Mix(result, a, 0);
        ChipSynth.Mix(result, b, 0);
        return result;
    }

    static float[] Arpeggio(Wave wave, float noteSeconds, float volume, params float[] freqs)
    {
        var parts = new float[freqs.Length][];
        for (int i = 0; i < freqs.Length; i++)
            parts[i] = ChipSynth.Tone(freqs[i], noteSeconds, wave, volume, -1f, 0.5f);
        return ChipSynth.Concat(parts);
    }
}
