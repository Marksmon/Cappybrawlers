#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PlaceholderSpriteGenerator
{
    // ── Directories ───────────────────────────────────────────────────────────────
    private const string CreaturesDir = "Assets/_Game/Art/Sprites/Creatures";
    private const string CardsDir     = "Assets/_Game/Art/Sprites/Cards";
    private const string UIDir        = "Assets/_Game/Art/Sprites/UI";
    private const string EquipDir     = "Assets/_Game/Art/Sprites/UI/Equipment";
    private const string RanksDir     = "Assets/_Game/Art/Sprites/UI/Ranks";
    private const string BgDir        = "Assets/_Game/Art/Sprites/Backgrounds";

    // ── Element palette (matches user-specified: Storm=yellow, Rock=gray) ─────────
    private static readonly (string id, Color32 primary, Color32 accent)[] Elements =
    {
        ("flame", C(232,  74,  26), C(255, 170,  68)),
        ("storm", C(245, 197,  24), C(255, 241, 118)),
        ("plant", C( 56, 168,  50), C(170, 255, 204)),
        ("rock",  C(112, 112, 112), C(176, 176, 176)),
        ("water", C( 26, 111, 232), C( 85, 204, 255)),
        ("moon",  C(155,  63, 200), C(221, 170, 255)),
    };

    private static readonly (string id, Color32 col)[] Ranks =
    {
        ("pup",       C(158, 158, 158)),
        ("sprout",    C( 56, 168,  50)),
        ("brawler",   C( 41, 121, 255)),
        ("stonefang", C(139, 105,  20)),
        ("tempest",   C( 74, 144, 226)),
        ("grandcapy", C(255, 214,   0)),
    };

    private static readonly string[] Slots = { "helm", "armor", "weapon" };
    private static readonly string[] Tiers = { "a", "b", "c" };

    [MenuItem("Capybrawlers/Generate Placeholder Sprites")]
    public static void Generate()
    {
        EnsureDirs();
        GenerateCreatureSprites();
        GenerateElementIcons();
        GenerateEquipmentIcons();
        GenerateCardArtworks();
        GenerateUISprites();
        GenerateRankBadges();
        GenerateBattleBackground();
        AssetDatabase.Refresh();
        Debug.Log("[Capybrawlers] Placeholder sprites generated in Assets/_Game/Art/Sprites/");
    }

    // ── Creature sprites (512×512) ────────────────────────────────────────────────
    static void GenerateCreatureSprites()
    {
        foreach (var (id, primary, accent) in Elements)
        {
            const int w = 512, h = 512;
            var px = new Color32[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(0, 0, 0, 0);

            var shadow = Darken(primary, 0.45f);
            var light  = Lighten(primary, 1.18f);

            // Body (outline then fill)
            FillEllipse(px, w, h, 256, 185, 153, 133, shadow);
            FillEllipse(px, w, h, 256, 185, 146, 126, primary);

            // Head (outline then fill)
            FillCircle(px, w, h, 256, 350, 105, shadow);
            FillCircle(px, w, h, 256, 350,  98, light);

            // Ears
            FillCircle(px, w, h, 196, 436, 31, shadow);
            FillCircle(px, w, h, 196, 436, 24, primary);
            FillCircle(px, w, h, 318, 436, 31, shadow);
            FillCircle(px, w, h, 318, 436, 24, primary);

            // Eyes
            FillCircle(px, w, h, 228, 360, 15, C(20, 10, 10));
            FillCircle(px, w, h, 286, 360, 15, C(20, 10, 10));
            FillCircle(px, w, h, 222, 366,  5, C(255, 255, 255, 200));
            FillCircle(px, w, h, 280, 366,  5, C(255, 255, 255, 200));

            // Nose
            FillEllipse(px, w, h, 256, 328, 15,  9, C(30, 14,  8));

            // Legs (4 stubby paws at bottom)
            FillRect(px, w, h,  88, 20, 134, 112, shadow);
            FillRect(px, w, h, 144, 20, 190, 112, shadow);
            FillRect(px, w, h, 320, 20, 366, 112, shadow);
            FillRect(px, w, h, 376, 20, 422, 112, shadow);

            // Element accent badge on chest
            FillCircle(px, w, h, 256, 175, 32, Darken(accent, 0.75f));
            FillCircle(px, w, h, 256, 175, 24, accent);

            var tex = MakeTex(w, h, px);
            Save(tex, $"{CreaturesDir}/capy_{id}.png");
            Object.DestroyImmediate(tex);
        }
    }

    // ── Element icons (80×80) ─────────────────────────────────────────────────────
    static void GenerateElementIcons()
    {
        foreach (var (id, primary, accent) in Elements)
        {
            const int w = 80, h = 80;
            var px = Blank(w, h);

            FillCircle(px, w, h, 40, 40, 39, Darken(primary, 0.5f));
            FillCircle(px, w, h, 40, 40, 35, primary);
            FillCircle(px, w, h, 40, 40, 21, accent);

            var tex = MakeTex(w, h, px);
            Save(tex, $"{UIDir}/icon_{id}.png");
            Object.DestroyImmediate(tex);
        }
    }

    // ── Equipment icons (80×80) ──────────────────────────────────────────────────
    static void GenerateEquipmentIcons()
    {
        foreach (var (id, primary, accent) in Elements)
        {
            foreach (var slot in Slots)
            {
                foreach (var tier in Tiers)
                {
                    const int w = 80, h = 80;
                    var px = Blank(w, h);
                    var wh = C(255, 255, 255, 210);

                    FillCircle(px, w, h, 40, 40, 38, Darken(primary, 0.55f));
                    FillCircle(px, w, h, 40, 40, 33, primary);

                    if (slot == "helm")
                    {
                        // Crown: base bar + three prongs
                        FillRect(px, w, h, 18, 22, 62, 36, wh);
                        FillRect(px, w, h, 18, 36, 32, 56, wh);
                        FillRect(px, w, h, 37, 36, 43, 62, wh);
                        FillRect(px, w, h, 48, 36, 62, 56, wh);
                    }
                    else if (slot == "armor")
                    {
                        // Shield silhouette
                        FillRect(px, w, h, 24, 26, 56, 52, wh);
                        FillEllipse(px, w, h, 40, 44, 16, 14, wh);
                    }
                    else
                    {
                        // Sword: blade, crossguard, grip
                        FillRect(px, w, h, 37, 18, 43, 56, wh);
                        FillRect(px, w, h, 22, 40, 58, 46, wh);
                        FillRect(px, w, h, 38,  8, 42, 20, wh);
                    }

                    // Tier dots at bottom (a=1, b=2, c=3)
                    int dots = tier == "a" ? 1 : tier == "b" ? 2 : 3;
                    int dotStartX = 40 - (dots - 1) * 8;
                    for (int d = 0; d < dots; d++)
                        FillCircle(px, w, h, dotStartX + d * 16, 10, 4, accent);

                    var tex = MakeTex(w, h, px);
                    Save(tex, $"{EquipDir}/equip_{id}_{slot}_{tier}.png");
                    Object.DestroyImmediate(tex);
                }
            }
        }
    }

    // ── Card artworks (380×280) ──────────────────────────────────────────────────
    static void GenerateCardArtworks()
    {
        foreach (var (id, primary, accent) in Elements)
        {
            foreach (var slot in Slots)
            {
                foreach (var tier in Tiers)
                {
                    const int w = 380, h = 280;
                    var px = new Color32[w * h];

                    // Top-to-bottom gradient: accent → primary
                    for (int y = 0; y < h; y++)
                    {
                        var col = Lerp32(primary, accent, y / (float)(h - 1));
                        for (int x = 0; x < w; x++)
                            px[y * w + x] = col;
                    }

                    // Central element medallion
                    FillCircle(px, w, h, 190, 140, 72, Darken(primary, 0.5f));
                    FillCircle(px, w, h, 190, 140, 64, Lighten(primary, 1.22f));
                    FillCircle(px, w, h, 190, 140, 40, accent);

                    // Border
                    var border = Darken(primary, 0.45f);
                    for (int y = 0; y < h; y++)
                        for (int x = 0; x < w; x++)
                            if (x < 5 || x >= w - 5 || y < 5 || y >= h - 5)
                                px[y * w + x] = border;

                    var tex = MakeTex(w, h, px);
                    Save(tex, $"{CardsDir}/card_{id}_{slot}_{tier}.png");
                    Object.DestroyImmediate(tex);
                }
            }
        }
    }

    // ── UI sprites ────────────────────────────────────────────────────────────────
    static void GenerateUISprites()
    {
        // Wood button (420×120)
        {
            const int w = 420, h = 120;
            var px = new Color32[w * h];
            var woodLight = C(123, 74, 32);
            var woodDark  = C( 61, 35, 16);
            var gold      = C(200, 144, 42);
            for (int y = 0; y < h; y++)
            {
                var col = Lerp32(woodDark, woodLight, y / (float)(h - 1));
                for (int x = 0; x < w; x++) px[y * w + x] = col;
            }
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    if (x < 6 || x >= w - 6 || y < 6 || y >= h - 6)
                        px[y * w + x] = gold;
            var tex = MakeTex(w, h, px);
            Save(tex, $"{UIDir}/btn_wood_normal.png");
            Object.DestroyImmediate(tex);
        }

        // Wood panel 9-slice (128×128)
        {
            const int w = 128, h = 128;
            var px = Fill(w, h, C(59, 35, 16));
            var gold = C(200, 144, 42);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    if (x < 8 || x >= w - 8 || y < 8 || y >= h - 8)
                        px[y * w + x] = gold;
            var tex = MakeTex(w, h, px);
            Save(tex, $"{UIDir}/panel_wood.png");
            Object.DestroyImmediate(tex);
        }

        // Parchment panel 9-slice (128×128)
        {
            const int w = 128, h = 128;
            var px = Fill(w, h, C(245, 232, 200));
            var edge = C(160, 110, 50);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    if (x < 8 || x >= w - 8 || y < 8 || y >= h - 8)
                        px[y * w + x] = edge;
            var tex = MakeTex(w, h, px);
            Save(tex, $"{UIDir}/panel_parchment.png");
            Object.DestroyImmediate(tex);
        }

        // Target ring (128×128, transparent center)
        {
            const int w = 128, h = 128;
            var px = Blank(w, h);
            FillCircle(px, w, h, 64, 64, 60, C(255, 220, 0, 180));
            FillCircle(px, w, h, 64, 64, 49, C(0,   0,   0,   0));
            var tex = MakeTex(w, h, px);
            Save(tex, $"{UIDir}/target_ring.png");
            Object.DestroyImmediate(tex);
        }

        // HP bar sprites (4×20)
        var fillTex = MakeTex(4, 20, Fill(4, 20, C(50, 220, 60)));
        Save(fillTex, $"{UIDir}/hp_bar_fill.png");
        Object.DestroyImmediate(fillTex);

        var bgTex = MakeTex(4, 20, Fill(4, 20, C(30, 30, 30)));
        Save(bgTex, $"{UIDir}/hp_bar_bg.png");
        Object.DestroyImmediate(bgTex);

        // Stamina pips (32×32)
        {
            const int w = 32, h = 32;
            var ap = Blank(w, h);
            FillCircle(ap, w, h, 16, 16, 14, C(255, 224, 102));
            var at = MakeTex(w, h, ap); Save(at, $"{UIDir}/stamina_pip_active.png"); Object.DestroyImmediate(at);

            var ep = Blank(w, h);
            FillCircle(ep, w, h, 16, 16, 14, C(68, 68, 68));
            var et = MakeTex(w, h, ep); Save(et, $"{UIDir}/stamina_pip_empty.png"); Object.DestroyImmediate(et);
        }
    }

    // ── Rank badges (120×120) ─────────────────────────────────────────────────────
    static void GenerateRankBadges()
    {
        foreach (var (id, col) in Ranks)
        {
            const int w = 120, h = 120;
            var px = Blank(w, h);
            FillCircle(px, w, h, 60, 60, 58, Darken(col, 0.45f));
            FillCircle(px, w, h, 60, 60, 50, col);
            FillCircle(px, w, h, 60, 60, 34, Lighten(col, 1.35f));
            var tex = MakeTex(w, h, px);
            Save(tex, $"{RanksDir}/rank_{id}.png");
            Object.DestroyImmediate(tex);
        }
    }

    // ── Battle background (1080×1920) ─────────────────────────────────────────────
    static void GenerateBattleBackground()
    {
        const int w = 1080, h = 1920;
        var px = new Color32[w * h];

        var skyTop = C(  8,  18,  8);
        var skyMid = C( 22,  52, 22);
        var ground = C( 12,  30, 12);

        for (int y = 0; y < h; y++)
        {
            float t = y / (float)(h - 1);
            var col = t > 0.5f
                ? Lerp32(skyMid, skyTop, (t - 0.5f) * 2f)
                : Lerp32(ground, skyMid, t * 2f);
            for (int x = 0; x < w; x++) px[y * w + x] = col;
        }

        // Tree trunks + canopy
        var trunk  = C(20, 14,  6);
        var canopy = C(15, 45, 15);
        int[] treeXs = { 60, 200, 380, 560, 700, 860, 960, 130, 450, 790 };
        foreach (int tx in treeXs)
        {
            int tw = 38 + (tx % 18);
            FillRect(px, w, h, tx - tw / 2, 0, tx + tw / 2, h / 2, trunk);
            FillCircle(px, w, h, tx, h / 2 + 80, 115 + (tx % 38), canopy);
        }

        var tex = MakeTex(w, h, px);
        Save(tex, $"{BgDir}/bg_battle_forest.png");
        Object.DestroyImmediate(tex);
    }

    // ── Pixel drawing primitives ──────────────────────────────────────────────────

    static void FillCircle(Color32[] px, int w, int h, int cx, int cy, int r, Color32 col)
    {
        int r2 = r * r;
        for (int y = Mathf.Max(0, cy - r); y <= Mathf.Min(h - 1, cy + r); y++)
            for (int x = Mathf.Max(0, cx - r); x <= Mathf.Min(w - 1, cx + r); x++)
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r2)
                    px[y * w + x] = col;
    }

    static void FillEllipse(Color32[] px, int w, int h, int cx, int cy, int rx, int ry, Color32 col)
    {
        for (int y = Mathf.Max(0, cy - ry); y <= Mathf.Min(h - 1, cy + ry); y++)
        {
            float fy = (y - cy) / (float)ry;
            for (int x = Mathf.Max(0, cx - rx); x <= Mathf.Min(w - 1, cx + rx); x++)
            {
                float fx = (x - cx) / (float)rx;
                if (fx * fx + fy * fy <= 1f) px[y * w + x] = col;
            }
        }
    }

    static void FillRect(Color32[] px, int w, int h, int x1, int y1, int x2, int y2, Color32 col)
    {
        for (int y = Mathf.Max(0, y1); y < Mathf.Min(h, y2); y++)
            for (int x = Mathf.Max(0, x1); x < Mathf.Min(w, x2); x++)
                px[y * w + x] = col;
    }

    // ── Color utilities ───────────────────────────────────────────────────────────

    static Color32 Darken(Color32 c, float f) =>
        new Color32((byte)(c.r * f), (byte)(c.g * f), (byte)(c.b * f), c.a);

    static Color32 Lighten(Color32 c, float f) =>
        new Color32(
            (byte)Mathf.Min(255, c.r * f),
            (byte)Mathf.Min(255, c.g * f),
            (byte)Mathf.Min(255, c.b * f),
            c.a);

    static Color32 Lerp32(Color32 a, Color32 b, float t) =>
        new Color32(
            (byte)(a.r + (b.r - a.r) * t),
            (byte)(a.g + (b.g - a.g) * t),
            (byte)(a.b + (b.b - a.b) * t),
            (byte)(a.a + (b.a - a.a) * t));

    static Color32 C(int r, int g, int b, int a = 255) =>
        new Color32((byte)r, (byte)g, (byte)b, (byte)a);

    // ── Texture & file helpers ────────────────────────────────────────────────────

    static Color32[] Blank(int w, int h)
    {
        var px = new Color32[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = new Color32(0, 0, 0, 0);
        return px;
    }

    static Color32[] Fill(int w, int h, Color32 col)
    {
        var px = new Color32[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = col;
        return px;
    }

    static Texture2D MakeTex(int w, int h, Color32[] px)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }

    static void Save(Texture2D tex, string assetPath)
    {
        var fullPath = Path.GetFullPath(assetPath);
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
    }

    static void EnsureDirs()
    {
        foreach (var dir in new[] { CreaturesDir, CardsDir, UIDir, EquipDir, RanksDir, BgDir })
        {
            var full = Path.GetFullPath(dir);
            if (!Directory.Exists(full)) Directory.CreateDirectory(full);
        }
    }
}
#endif
