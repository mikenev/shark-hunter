using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Generates placeholder pixel-art PNGs from ASCII definitions into Assets/Art/Generated.
// Replace any sprite later by dropping real art in with the same filename.
public static class PlaceholderSpriteGenerator
{
    const string OutputDir = "Assets/Art/Generated";
    const int PixelsPerUnit = 16;

    // NES-flavoured palette. '.' is transparent.
    static readonly Dictionary<char, Color32> Palette = new Dictionary<char, Color32>
    {
        { '.', new Color32(0, 0, 0, 0) },
        { 'k', new Color32(16, 16, 16, 255) },     // black
        { 'w', new Color32(248, 248, 248, 255) },  // white
        { 'l', new Color32(188, 188, 188, 255) },  // light gray
        { 'g', new Color32(124, 124, 124, 255) },  // gray
        { 'd', new Color32(0, 0, 188, 255) },      // dark blue
        { 'b', new Color32(0, 88, 248, 255) },     // blue
        { 'B', new Color32(60, 188, 252, 255) },   // light blue
        { 'c', new Color32(0, 232, 216, 255) },    // cyan
        { 'r', new Color32(216, 40, 0, 255) },     // red
        { 'o', new Color32(248, 120, 88, 255) },   // orange
        { 'y', new Color32(248, 184, 0, 255) },    // yellow
        { 'Y', new Color32(252, 224, 168, 255) },  // pale yellow
        { 'n', new Color32(80, 48, 0, 255) },      // brown
        { 't', new Color32(200, 140, 70, 255) },   // tan
        { 'G', new Color32(0, 168, 0, 255) },      // green
        { 'p', new Color32(248, 120, 248, 255) },  // pink
        { 'P', new Color32(252, 200, 252, 255) },  // light pink
    };

    class SpriteDef
    {
        public string Name;
        public int Width, Height;
        public string[] Rows;
    }

    static readonly SpriteDef[] Sprites =
    {
        new SpriteDef { Name = "boat", Width = 16, Height = 16, Rows = new[]
        {
            ".......kk.......",
            "......kwwk......",
            ".....kwwwwk.....",
            ".....kwwwwk.....",
            "....knnnnnnk....",
            "....knwwwwnk....",
            "...knnwwwwnnk...",
            "...knnwwwwnnk...",
            "...knnnnnnnnk...",
            "...knnnrrnnnk...",
            "...knnnrrnnnk...",
            "...knnnnnnnnk...",
            "....knnnnnnk....",
            "....kkkkkkkk....",
        }},
        new SpriteDef { Name = "diver", Width = 16, Height = 16, Rows = new[]
        {
            "................",
            "................",
            ".....kkkk.......",
            "....kBBBBk......",
            "....kBwBBk......",
            "....kBBBBk......",
            "...kkkyykk......",
            "..kkyyyyyykkk...",
            ".kkyyyyyyyykkkk.",
            "..kkyyyyyykk.kk.",
            "...kkyykkkk..kk.",
            "....kk..kk......",
            "....kk..kk......",
            "...kkk..kkk.....",
        }},
        new SpriteDef { Name = "shark", Width = 32, Height = 16, Rows = new[]
        {
            "................................",
            ".............ll.................",
            "............lggl................",
            "...........lgggl................",
            "..........lggggggl..............",
            ".....llllllggggggggllll.........",
            "...llggggggggggggggggggggl......",
            ".lggggggggggggggggggggggggggl...",
            "lggggggggggggggggggggkgggggggl..",
            ".lwwwwwwwwwwwwwwwwwwwwwwwwwwwl..",
            "..llwwwwwwwwwwwwwwwwwwwwwwll....",
            "....llwwwwwwwwwwwwllllll........",
            "......llll.lwwwll...............",
            ".............llll...............",
        }},
        new SpriteDef { Name = "jellyfish", Width = 16, Height = 16, Rows = new[]
        {
            "................",
            ".....pppppp.....",
            "....pppppppp....",
            "...pppwppppPp...",
            "...pppppppppp...",
            "...pppppppppp...",
            "....p.p..p.p....",
            "....p.p..p.p....",
            ".....p.pp.p.....",
            "....p..p..p.....",
        }},
        new SpriteDef { Name = "ray", Width = 16, Height = 16, Rows = new[]
        {
            "................",
            "................",
            ".......ll.......",
            "....llllllll....",
            "..llggggggggll..",
            ".lggggkggkgggl..",
            "lgggggggggggggll",
            ".lggggggggggll.l",
            "..llggggggll...l",
            "....llllll......",
        }},
        new SpriteDef { Name = "conch", Width = 16, Height = 16, Rows = new[]
        {
            "................",
            "................",
            ".....kkkk.......",
            "....kYyyYkk.....",
            "...kYyoyyyYk....",
            "..kYyyoyyyyYk...",
            "..kyoyyyyoyyk...",
            "...kyyyyyyyk....",
            "....kkyyykk.....",
            ".....kkkk.......",
        }},
        new SpriteDef { Name = "harpoon", Width = 16, Height = 16, Rows = new[]
        {
            "................",
            "................",
            "................",
            "................",
            "................",
            "................",
            "...........l....",
            ".nnnnnnnnnnnllw.",
            "...........l....",
        }},
        new SpriteDef { Name = "fin", Width = 16, Height = 16, Rows = new[]
        {
            "................",
            ".......k........",
            "......kgk.......",
            ".....kgggk......",
            "....kgggggk.....",
            "...kgggggggk....",
            "...kgggggggk....",
            "..kgggggggggk...",
            ".kkkkkkkkkkkkkk.",
            "BB..BB..BB..BB..",
        }},
        new SpriteDef { Name = "reef", Width = 16, Height = 16, Rows = new[]
        {
            "................",
            "................",
            "....g.....g.....",
            "...gkg...gkg....",
            "..gggkg.gggkg...",
            ".gggggggggggkg..",
            "gggggggggggggkg.",
            "kggggggggggggkk.",
            ".kkkkkkkkkkkkk..",
        }},
        new SpriteDef { Name = "tile_water", Width = 16, Height = 16, Rows = new[]
        {
            "bbbbbbbbbbbbbbbb",
            "bbbbbbbbbbbbbbbb",
            "bbBBbbbbbbbbbbbb",
            "bbbbbbbbbbBBbbbb",
            "bbbbbbbbbbbbbbbb",
            "bbbbbbbbbbbbbbbb",
            "bbbbbbBBbbbbbbbb",
            "bbbbbbbbbbbbbbbb",
            "bbbbbbbbbbbbbbbb",
            "bbBBbbbbbbbbbbbb",
            "bbbbbbbbbbbbbbbb",
            "bbbbbbbbbbBBbbbb",
            "bbbbbbbbbbbbbbbb",
            "bbbbbbbbbbbbbbbb",
            "bbbbbbBBbbbbbbbb",
            "bbbbbbbbbbbbbbbb",
        }},
        new SpriteDef { Name = "tile_sand", Width = 16, Height = 16, Rows = new[]
        {
            "YYYYYYYYYYYYYYYY",
            "YYYYYYYYYYYYYYYY",
            "YYYtYYYYYYYYYYYY",
            "YYYYYYYYYYYtYYYY",
            "YYYYYYYYYYYYYYYY",
            "YYYYYYYYYYYYYYYY",
            "YYYYYYtYYYYYYYYY",
            "YYYYYYYYYYYYYYYY",
            "YYYYYYYYYYYYYYYY",
            "YYtYYYYYYYYYYYYY",
            "YYYYYYYYYYYYYtYY",
            "YYYYYYYYYYYYYYYY",
            "YYYYYYYYYYYYYYYY",
            "YYYYYYYYtYYYYYYY",
            "YYYYYYYYYYYYYYYY",
            "YYYYYYYYYYYYYYYY",
        }},
        new SpriteDef { Name = "tile_land", Width = 16, Height = 16, Rows = new[]
        {
            "GGGGGGGGGGGGGGGG",
            "GGGGGGGGGGGGGGGG",
            "GGGGGGGnGGGGGGGG",
            "GGGGGGGGGGGGGGGG",
            "GGnGGGGGGGGGGGGG",
            "GGGGGGGGGGGGnGGG",
            "GGGGGGGGGGGGGGGG",
            "GGGGGGGGGGGGGGGG",
            "GGGGGGnGGGGGGGGG",
            "GGGGGGGGGGGGGGGG",
            "GGGGGGGGGGnGGGGG",
            "GGGnGGGGGGGGGGGG",
            "GGGGGGGGGGGGGGGG",
            "GGGGGGGGGGGGGGGG",
            "GGGGGGGGGGGGGnGG",
            "GGGGGGGGGGGGGGGG",
        }},
        new SpriteDef { Name = "tile_seabed", Width = 16, Height = 16, Rows = new[]
        {
            "tttttttttttttttt",
            "tttntttttttttttt",
            "tttttttttttttttt",
            "ttttttttttnttttt",
            "tttttttttttttttt",
            "tnttttttttttttnt",
            "tttttttttttttttt",
            "ttttttnttttttttt",
            "tttttttttttttttt",
            "ttttttttttttnttt",
            "tttttttttttttttt",
            "tttntttttttttttt",
            "tttttttttttttttt",
            "tttttttttttttttt",
            "ttttttttnttttttt",
            "tttttttttttttttt",
        }},
    };

    [MenuItem("Shark Hunter/Generate Placeholder Sprites")]
    public static void Generate()
    {
        Directory.CreateDirectory(OutputDir);

        foreach (var def in Sprites)
        {
            var pixels = new Color32[def.Width * def.Height];
            for (int y = 0; y < def.Height; y++)
            {
                // Row 0 of the ASCII is the top of the image; texture y=0 is the bottom.
                string row = y < def.Rows.Length ? def.Rows[y] : "";
                for (int x = 0; x < def.Width; x++)
                {
                    char c = x < row.Length ? row[x] : '.';
                    if (!Palette.TryGetValue(c, out var color))
                    {
                        Debug.LogWarning($"{def.Name}: unknown palette char '{c}' at ({x},{y})");
                        color = Palette['.'];
                    }
                    pixels[(def.Height - 1 - y) * def.Width + x] = color;
                }
            }

            var tex = new Texture2D(def.Width, def.Height, TextureFormat.RGBA32, false);
            tex.SetPixels32(pixels);
            File.WriteAllBytes(Path.Combine(OutputDir, def.Name + ".png"), tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        AssetDatabase.Refresh();

        foreach (var def in Sprites)
        {
            string path = $"{OutputDir}/{def.Name}.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;

            // Tiled SpriteRenderers (water, sand, ...) need FullRect meshes.
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);

            importer.SaveAndReimport();
        }

        Debug.Log($"Generated {Sprites.Length} placeholder sprites in {OutputDir}");
    }
}
