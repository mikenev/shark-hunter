using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Builds the SeaMap, Dive and SharkFight scenes from code so they can be regenerated
// at any time. Run "Shark Hunter/Generate Placeholder Sprites" first.
public static class SceneBuilder
{
    const string ArtDir = "Assets/Art/Generated/";
    const string SceneDir = "Assets/Scenes/";

    static readonly Color DeepWater = new Color(0.6f, 0.72f, 1f);
    static readonly Color Sky = new Color(0.75f, 0.92f, 1f);

    [MenuItem("Shark Hunter/Build Scenes")]
    public static void BuildAll()
    {
        if (!File.Exists(ArtDir + "boat.png"))
        {
            EditorUtility.DisplayDialog("Shark Hunter",
                "Sprites not found. Run Shark Hunter > Generate Placeholder Sprites first.", "OK");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Directory.CreateDirectory(SceneDir);
        BuildTitle();
        BuildSeaMap();
        BuildDive();
        BuildSharkFight();

        EditorBuildSettings.scenes = new[] { "Title", "SeaMap", "Dive", "SharkFight" }
            .Select(n => new EditorBuildSettingsScene(SceneDir + n + ".unity", true))
            .ToArray();

        PlayerSettings.defaultScreenWidth = 960;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;

        EditorSceneManager.OpenScene(SceneDir + "Title.unity");
        Debug.Log("Built Title, SeaMap, Dive and SharkFight scenes and added them to Build Settings.");
    }

    // ---------------------------------------------------------------- scenes

    static void BuildTitle()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        const float w = 20f, h = 15f;

        // Oversized water layers drift gently so their edges never show on screen.
        var water = Tiled("Water", "tile_water", new Vector2(w / 2, h / 2), new Vector2(w + 4f, h + 4f), 0);
        var drift = water.AddComponent<Patrol>();
        drift.axis = Vector2.right;
        drift.distance = 0.5f;
        drift.speed = 0.3f;

        var swell = Tiled("Swell", "tile_water", new Vector2(w / 2, h / 2 - 3f), new Vector2(w + 4f, h + 4f), 1,
            new Color(1f, 1f, 1f, 0.35f));
        var swellDrift = swell.AddComponent<Patrol>();
        swellDrift.axis = Vector2.right;
        swellDrift.distance = 0.7f;
        swellDrift.speed = 0.5f;

        var fin = Sprite("SharkFin", "fin", new Vector2(14f, 4.5f), 3);
        var finPatrol = fin.AddComponent<Patrol>();
        finPatrol.axis = Vector2.right;
        finPatrol.distance = 3f;
        finPatrol.speed = 0.8f;
        finPatrol.spriteRenderer = fin.GetComponent<SpriteRenderer>();

        new GameObject("TitleScreen").AddComponent<TitleScreen>();

        Camera(null, Vector2.zero, new Vector2(w, h), DeepWater, new Vector2(w / 2, h / 2));
        Save(scene, "Title");
    }

    static void BuildSeaMap()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        const float w = 40f, h = 30f;

        Tiled("Water", "tile_water", new Vector2(w / 2, h / 2), new Vector2(w, h), 0);

        Island("HomeIsland", new Vector2(5.5f, 26f), new Vector2(10f, 8f));
        Island("CenterIsland", new Vector2(20f, 14f), new Vector2(10f, 6f));
        Island("SmallIsland", new Vector2(12f, 6f), new Vector2(6f, 4f));
        Island("EastIsland", new Vector2(36f, 15f), new Vector2(6f, 8f));

        Wall("WallLeft", new Vector2(-0.5f, h / 2), new Vector2(1f, h));
        Wall("WallRight", new Vector2(w + 0.5f, h / 2), new Vector2(1f, h));
        Wall("WallBottom", new Vector2(w / 2, -0.5f), new Vector2(w, 1f));
        Wall("WallTop", new Vector2(w / 2, h + 0.5f), new Vector2(w, 1f));

        var reef = Sprite("Reef", "reef", new Vector2(28f, 8f), 3);
        var reefCol = reef.AddComponent<CircleCollider2D>();
        reefCol.isTrigger = true;
        reefCol.radius = 1.2f;
        var reefTrigger = reef.AddComponent<SceneTrigger>();
        reefTrigger.targetScene = SceneLoader.Dive;

        var fin = Sprite("SharkFin", "fin", new Vector2(31f, 25f), 3);
        var finCol = fin.AddComponent<CircleCollider2D>();
        finCol.isTrigger = true;
        finCol.radius = 1.2f;
        var finTrigger = fin.AddComponent<SceneTrigger>();
        finTrigger.targetScene = SceneLoader.SharkFight;
        finTrigger.requiredConches = GameState.ConchesNeeded;
        finTrigger.blockedMessage = "THE SHARK IS NEAR... FIND {n} MORE CONCH TO LURE IT";
        var finPatrol = fin.AddComponent<Patrol>();
        finPatrol.axis = Vector2.right;
        finPatrol.distance = 2f;
        finPatrol.speed = 1f;

        var boat = new GameObject("Boat");
        boat.transform.position = new Vector2(12f, 24f);
        var boatRb = boat.AddComponent<Rigidbody2D>();
        boatRb.gravityScale = 0f;
        boatRb.freezeRotation = true;
        boatRb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        boatRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        boat.AddComponent<CircleCollider2D>().radius = 0.45f;
        var visual = Sprite("Visual", "boat", boat.transform.position, 5);
        visual.transform.SetParent(boat.transform, true);
        var boatCtrl = boat.AddComponent<BoatController>();
        boatCtrl.visual = visual.transform;

        Camera(boat.transform, Vector2.zero, new Vector2(w, h), DeepWater, boat.transform.position);
        Save(scene, "SeaMap");
    }

    static void BuildDive()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        const float w = 48f, h = 16f, surface = 14f;

        Tiled("Water", "tile_water", new Vector2(w / 2, surface / 2), new Vector2(w, surface), 0, DeepWater);
        Tiled("Sky", "tile_water", new Vector2(w / 2, surface + (h - surface) / 2), new Vector2(w, h - surface), 0, Sky);

        var seabed = Tiled("Seabed", "tile_seabed", new Vector2(w / 2, 1f), new Vector2(w, 2f), 1);
        seabed.AddComponent<BoxCollider2D>().size = new Vector2(w, 2f);

        Wall("WallLeft", new Vector2(-0.5f, h / 2), new Vector2(1f, h));
        Wall("WallRight", new Vector2(w + 0.5f, h / 2), new Vector2(1f, h));
        Wall("Ceiling", new Vector2(w / 2, h + 0.5f), new Vector2(w, 1f));

        // Boat at the surface: touch it to sail back to the sea map.
        var boat = Sprite("BoatSurface", "boat", new Vector2(3f, surface + 0.6f), 4);
        boat.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
        var boatCol = boat.AddComponent<CircleCollider2D>();
        boatCol.isTrigger = true;
        boatCol.radius = 1.5f;
        boat.AddComponent<SceneTrigger>().targetScene = SceneLoader.SeaMap;

        int conchIndex = 1;
        foreach (var x in new[] { 14f, 30f, 44f })
        {
            var conch = Sprite("Conch" + conchIndex, "conch", new Vector2(x, 2.5f), 3);
            var col = conch.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;
            conch.AddComponent<Conch>().id = "Conch" + conchIndex;
            conchIndex++;
        }

        Enemy("Jellyfish1", "jellyfish", new Vector2(10f, 8f), Vector2.up, 3f, 1.2f);
        Enemy("Jellyfish2", "jellyfish", new Vector2(22f, 7f), Vector2.up, 3f, 1.4f);
        Enemy("Jellyfish3", "jellyfish", new Vector2(36f, 9f), Vector2.up, 3f, 1.2f);
        Enemy("Ray1", "ray", new Vector2(24f, 4.5f), Vector2.right, 4f, 2f);
        Enemy("Ray2", "ray", new Vector2(40f, 6f), Vector2.right, 3f, 2f);

        var spawn = new Vector2(6f, 13.5f);
        var diver = Diver(spawn, drainOxygen: true, canFire: false, surfaceY: surface);

        Camera(diver.transform, Vector2.zero, new Vector2(w, h), DeepWater, spawn);
        Save(scene, "Dive");
    }

    static void BuildSharkFight()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        const float w = 20f, h = 15f;

        Tiled("Water", "tile_water", new Vector2(w / 2, h / 2), new Vector2(w, h), 0, DeepWater);
        var seabed = Tiled("Seabed", "tile_seabed", new Vector2(w / 2, 1f), new Vector2(w, 2f), 1);
        seabed.AddComponent<BoxCollider2D>().size = new Vector2(w, 2f);

        Wall("WallLeft", new Vector2(-0.5f, h / 2), new Vector2(1f, h));
        Wall("WallRight", new Vector2(w + 0.5f, h / 2), new Vector2(1f, h));
        Wall("Ceiling", new Vector2(w / 2, h + 0.5f), new Vector2(w, 1f));

        var spawn = new Vector2(4f, 8f);
        var diver = Diver(spawn, drainOxygen: false, canFire: true, surfaceY: h + 10f);

        var shark = Sprite("Shark", "shark", new Vector2(16f, 8f), 4);
        var sharkCol = shark.AddComponent<BoxCollider2D>();
        sharkCol.isTrigger = true;
        sharkCol.size = new Vector2(1.8f, 0.7f);
        shark.AddComponent<Hazard>();
        var boss = shark.AddComponent<SharkBoss>();
        boss.target = diver.transform;
        boss.arenaMin = new Vector2(0f, 2f);
        boss.arenaMax = new Vector2(w, h);
        boss.spriteRenderer = shark.GetComponent<SpriteRenderer>();
        boss.bodyCollider = sharkCol;

        Camera(diver.transform, Vector2.zero, new Vector2(w, h), DeepWater, spawn);
        Save(scene, "SharkFight");
    }

    // --------------------------------------------------------------- helpers

    static GameObject Diver(Vector2 spawn, bool drainOxygen, bool canFire, float surfaceY)
    {
        var diver = Sprite("Diver", "diver", spawn, 5);
        var rb = diver.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        diver.AddComponent<BoxCollider2D>().size = new Vector2(0.8f, 0.7f);

        var ctrl = diver.AddComponent<DiverController>();
        ctrl.drainOxygen = drainOxygen;
        ctrl.canFire = canFire;
        ctrl.surfaceY = surfaceY;
        ctrl.spawnPoint = spawn;
        ctrl.spriteRenderer = diver.GetComponent<SpriteRenderer>();
        ctrl.harpoonSprite = LoadSprite("harpoon");
        return diver;
    }

    static void Enemy(string name, string sprite, Vector2 pos, Vector2 axis, float distance, float speed)
    {
        var go = Sprite(name, sprite, pos, 4);
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;
        go.AddComponent<Hazard>();
        var patrol = go.AddComponent<Patrol>();
        patrol.axis = axis;
        patrol.distance = distance;
        patrol.speed = speed;
        patrol.spriteRenderer = go.GetComponent<SpriteRenderer>();
    }

    // Sand beach with a grassy interior; the beach rect is the solid collider.
    static void Island(string name, Vector2 center, Vector2 size)
    {
        var beach = Tiled(name, "tile_sand", center, size, 1);
        beach.AddComponent<BoxCollider2D>().size = size;
        Tiled(name + "Land", "tile_land", center, size - new Vector2(2f, 2f), 2);
    }

    static void Camera(Transform target, Vector2 min, Vector2 max, Color background, Vector2 startPos)
    {
        var go = new GameObject("Main Camera") { tag = "MainCamera" };
        go.transform.position = new Vector3(startPos.x, startPos.y, -10f);

        var cam = go.AddComponent<UnityEngine.Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = background;
        go.AddComponent<AudioListener>();

        var follow = go.AddComponent<CameraFollow>();
        follow.target = target;
        follow.boundsMin = min;
        follow.boundsMax = max;

        AddPixelPerfect(go);
    }

    // Looked up by name so this keeps compiling whichever assembly the URP version puts it in.
    static void AddPixelPerfect(GameObject cameraObject)
    {
        var type = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
            .FirstOrDefault(t => t.Name == "PixelPerfectCamera" && typeof(Component).IsAssignableFrom(t));
        if (type == null)
        {
            Debug.LogWarning("PixelPerfectCamera component not found; add it to the camera manually (16 PPU, 320x240).");
            return;
        }

        var component = cameraObject.AddComponent(type);
        SetProperty(component, "assetsPPU", 16);
        SetProperty(component, "refResolutionX", 320);
        SetProperty(component, "refResolutionY", 240);
    }

    static void SetProperty(object target, string name, object value)
    {
        var prop = target.GetType().GetProperty(name);
        if (prop == null) { Debug.LogWarning($"PixelPerfectCamera has no property '{name}'"); return; }
        prop.SetValue(target, value);
    }

    static UnityEngine.Sprite LoadSprite(string name)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(ArtDir + name + ".png");
        if (sprite == null) throw new Exception($"Missing sprite '{name}'. Run Shark Hunter > Generate Placeholder Sprites.");
        return sprite;
    }

    static GameObject Sprite(string name, string sprite, Vector2 pos, int order)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(sprite);
        sr.sortingOrder = order;
        return go;
    }

    static GameObject Tiled(string name, string sprite, Vector2 center, Vector2 size, int order, Color? tint = null)
    {
        var go = Sprite(name, sprite, center, order);
        var sr = go.GetComponent<SpriteRenderer>();
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = size;
        if (tint.HasValue) sr.color = tint.Value;
        return go;
    }

    static void Wall(string name, Vector2 center, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.position = center;
        go.AddComponent<BoxCollider2D>().size = size;
    }

    static void Save(UnityEngine.SceneManagement.Scene scene, string name)
    {
        EditorSceneManager.SaveScene(scene, SceneDir + name + ".unity");
    }
}
