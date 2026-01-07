using System;
using UnityEngine;

/// <summary>
/// Tiny immediate-mode UI helpers for Unity OnGUI.
/// Intended for debug/dev UI: quick panels, labels, buttons, bars, simple windows.
/// </summary>
public static class UI
{
    // ---------------------------
    // Const
    // ---------------------------
    public const int Gap = 2;

    // ---------------------------
    // Config
    // ---------------------------

    public static class Theme
    {
        public static float LineHeight = 22f;
        public static float Spacing = 6f;
        public static float Padding = 8f;

        public static int FontSize = 13;

        // You can change these at runtime if you want to quickly re-skin debug UI.
        public static Color TextColor = Color.white;
        public static Color SubtleTextColor = new Color(1f, 1f, 1f, 0.75f);
        public static Color PanelColor = new Color(0f, 0f, 0f, 0.55f);
        public static Color PanelHeaderColor = new Color(0f, 0f, 0f, 0.70f);
    }

    // ---------------------------
    // Styles (lazy init)
    // ---------------------------

    private static bool stylesInit;
    private static GUIStyle label;
    private static GUIStyle subtleLabel;
    private static GUIStyle richLabel;
    private static GUIStyle box;
    private static GUIStyle button;
    private static GUIStyle headerLabel;
    private static GUIStyle textField;
    private static GUIStyle toggle;

    private static Texture2D whiteTex;

    private static bool wordWrap = false;
    public static bool WordWrap
    {
        get => wordWrap;
        set
        {
            if (wordWrap == value) 
                return;
            
            wordWrap = value;
            stylesInit = false; // auto-rebuild styles next draw
        }
    }

    private static TextAnchor textAlignment = TextAnchor.UpperLeft;
    public static TextAnchor TextAlignment
    {
        get => textAlignment;
        set
        {
            if (textAlignment == value) return;
            textAlignment = value;
            stylesInit = false;
        }
    }

    private static void EnsureStyles()
    {
        if (stylesInit) 
            return;
        
        stylesInit = true;

        whiteTex = Texture2D.whiteTexture;

        label = new GUIStyle(GUI.skin.label)
        {
            fontSize = Theme.FontSize,
            richText = false,
            wordWrap = WordWrap,
            alignment = TextAlignment
        };
        label.normal.textColor = Theme.TextColor;

        subtleLabel = new GUIStyle(label);
        subtleLabel.normal.textColor = Theme.SubtleTextColor;

        richLabel = new GUIStyle(label) 
        { 
            richText = true,
            wordWrap = WordWrap,
            alignment = TextAlignment
        };

        headerLabel = new GUIStyle(label)
        {
            fontSize = Theme.FontSize + 2,
            fontStyle = FontStyle.Bold
        };

        box = new GUIStyle(GUI.skin.box)
        {
            fontSize = Theme.FontSize,
            alignment = TextAnchor.UpperLeft,
            padding = new RectOffset((int)Theme.Padding, (int)Theme.Padding, (int)Theme.Padding, (int)Theme.Padding)
        };

        button = new GUIStyle(GUI.skin.button)
        {
            fontSize = Theme.FontSize
        };

        textField = new GUIStyle(GUI.skin.textField)
        {
            fontSize = Theme.FontSize
        };

        toggle = new GUIStyle(GUI.skin.toggle)
        {
            fontSize = Theme.FontSize
        };
    }

    public static void InvalidateStyles() => stylesInit = false;

    // ---------------------------
    // Layout helper (simple vertical stack)
    // ---------------------------

    public struct Layout
    {
        public Rect rect;
        public float cursorY;

        public Layout(Rect rect)
        {
            this.rect = rect;
            this.cursorY = rect.yMin;
        }

        public Rect Next(float height)
        {
            var r = new Rect(rect.xMin, cursorY, rect.width, height);
            cursorY += height + Theme.Spacing;
            return r;
        }

        public Rect NextLine(float height = -1f)
        {
            if (height <= 0f) height = Theme.LineHeight;
            return Next(height);
        }

        public Rect NextBox(float height)
        {
            return Next(height);
        }

        public void Space(float pixels) => cursorY += pixels;

        public Rect Remaining()
        {
            return new Rect(rect.xMin, cursorY, rect.width, Mathf.Max(0f, rect.yMax - cursorY));
        }
    }

    // ---------------------------
    // Basic elements
    // ---------------------------

    public static void Label(Rect r, string text)
    {
        EnsureStyles();
        GUI.Label(r, text, label);
    }

    public static void LabelSubtle(Rect r, string text)
    {
        EnsureStyles();
        GUI.Label(r, text, subtleLabel);
    }

    public static void LabelRich(Rect r, string richText)
    {
        EnsureStyles();
        GUI.Label(r, richText, richLabel);
    }

    public static void Header(Rect r, string text)
    {
        EnsureStyles();
        GUI.Label(r, text, headerLabel);
    }

    public static void Box(Rect r, string text = null)
    {
        EnsureStyles();
        GUI.Box(r, text ?? string.Empty, box);
    }

    public static bool Button(Rect r, string text)
    {
        EnsureStyles();
        return GUI.Button(r, text, button);
    }

    public static bool Toggle(Rect r, bool value, string text)
    {
        EnsureStyles();
        return GUI.Toggle(r, value, text, toggle);
    }

    public static string TextField(Rect r, string value)
    {
        EnsureStyles();
        return GUI.TextField(r, value ?? string.Empty, textField);
    }

    public static string TextField(Rect r, string value, int maxLength)
    {
        EnsureStyles();
        return GUI.TextField(r, value ?? string.Empty, maxLength, textField);
    }

    // ---------------------------
    // Colored rect + outline
    // ---------------------------

    public static void Rect(Rect r, Color color)
    {
        EnsureStyles();
        var prev = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(r, whiteTex);
        GUI.color = prev;
    }

    public static void Outline(Rect r, Color color, float thickness = 1f)
    {
        EnsureStyles();
        // Top
        Rect(new Rect(r.xMin, r.yMin, r.width, thickness), color);
        // Bottom
        Rect(new Rect(r.xMin, r.yMax - thickness, r.width, thickness), color);
        // Left
        Rect(new Rect(r.xMin, r.yMin, thickness, r.height), color);
        // Right
        Rect(new Rect(r.xMax - thickness, r.yMin, thickness, r.height), color);
    }

    // ---------------------------
    // Panel / window helpers
    // ---------------------------

    /// <summary>
    /// Draws a simple panel with optional header. Returns a content rect inside the panel padding.
    /// Use returned rect with Layout for easy stacking.
    /// </summary>
    public static Rect Panel(Rect r, string header = null)
    {
        EnsureStyles();

        // Background
        Rect(r, Theme.PanelColor);

        float headerH = 0f;
        if (!string.IsNullOrEmpty(header))
        {
            headerH = Theme.LineHeight + Theme.Padding * 0.5f;
            var hr = new Rect(r.xMin, r.yMin, r.width, headerH);
            Rect(hr, Theme.PanelHeaderColor);
            GUI.Label(new Rect(hr.xMin + Theme.Padding, hr.yMin + 2f, hr.width - Theme.Padding * 2f, hr.height),
                header, headerLabel);
        }

        // Content rect (padding)
        return new Rect(
            r.xMin + Theme.Padding,
            r.yMin + headerH + Theme.Padding,
            r.width - Theme.Padding * 2f,
            r.height - headerH - Theme.Padding * 2f
        );
    }

    /// <summary>
    /// Wrapper around GUI.Window with a simpler callback signature.
    /// </summary>
    public static Rect Window(int id, Rect r, string title, Action<Rect> drawContents, bool draggable = true)
    {
        EnsureStyles();
        return GUI.Window(id, r, (windowId) =>
        {
            var content = new Rect(Theme.Padding, Theme.LineHeight, r.width - Theme.Padding * 2f, r.height - Theme.LineHeight - Theme.Padding);
            drawContents?.Invoke(content);

            if (draggable)
                GUI.DragWindow(new Rect(0, 0, r.width, Theme.LineHeight));
        }, title);
    }

    // ---------------------------
    // Common debug widgets
    // ---------------------------

    public static void KeyValue(Rect r, string key, string value)
    {
        EnsureStyles();
        // key left, value right
        float split = Mathf.Min(220f, r.width * 0.5f);

        var keyRect = new Rect(r.xMin, r.yMin, split, r.height);
        var valRect = new Rect(r.xMin + split, r.yMin, r.width - split, r.height);

        GUI.Label(keyRect, key, subtleLabel);

        var right = new GUIStyle(label) { alignment = TextAnchor.UpperRight };
        GUI.Label(valRect, value, right);
    }

    public static void Separator(Rect r, Color color, float thickness = 1f, float alpha = 0.4f)
    {
        color.a *= alpha;
        Rect(new Rect(r.xMin, r.center.y, r.width, thickness), color);
    }

    /// <summary>
    /// Simple progress bar with a label.
    /// </summary>
    public static void ProgressBar(Rect r, float t01, string labelText = null, 
        Color? backgroundColor = null, 
        Color? fillColor = null,
        Color? outlineColor = null)
    {
        EnsureStyles();
        t01 = Mathf.Clamp01(t01);

        backgroundColor ??= new Color(1f, 1f, 1f, 1f);
        fillColor ??= new Color(0f, 1f, 0f, 1f);
        outlineColor ??= Color.gray;

        // Background
        Rect(r, backgroundColor.Value);

        // Fill (use white texture + GUI.color)
        var fill = new Rect(r.xMin, r.yMin, r.width * t01, r.height);
        Rect(fill, fillColor.Value);

        // Outline
        Outline(r, outlineColor.Value, 1f);

        if (!string.IsNullOrEmpty(labelText))
            GUI.Label(r, labelText, label);
    }

    /// <summary>
    /// Displays a small tooltip near the mouse if hovered is true.
    /// </summary>
    public static void TooltipIf(bool hovered, string text, Vector2 offset = default)
    {
        if (!hovered || string.IsNullOrEmpty(text)) return;
        EnsureStyles();

        if (offset == default) offset = new Vector2(16, 16);

        Vector2 mouse = Event.current.mousePosition;
        var size = richLabel.CalcSize(new GUIContent(text));

        var r = new Rect(mouse.x + offset.x, mouse.y + offset.y, size.x + Theme.Padding * 2f, size.y + Theme.Padding * 2f);
        Rect(r, new Color(0f, 0f, 0f, 0.85f));
        Outline(r, new Color(1f, 1f, 1f, 0.15f), 1f);

        GUI.Label(new Rect(r.xMin + Theme.Padding, r.yMin + Theme.Padding, r.width - Theme.Padding * 2f, r.height - Theme.Padding * 2f), text, richLabel);
    }

    public static void DrawTexture(Rect r, Texture texture, ScaleMode scaleMode = ScaleMode.ScaleToFit, Color? tint = null)
    {
        if (texture == null)
            return;

        EnsureStyles();

        var prev = GUI.color;
        if( tint.HasValue )
            GUI.color = tint.Value;

        GUI.DrawTexture(r, texture, scaleMode);

        GUI.color = prev;
    }

    // ---------------------------
    // Convenience: Begin/End area
    // ---------------------------

    public static void BeginArea(Rect r)
    {
        EnsureStyles();
        GUILayout.BeginArea(r);
    }

    public static void EndArea()
    {
        GUILayout.EndArea();
    }

    // ---------------------------
    // Small helpers
    // ---------------------------

    public static Rect TopLeft(float x, float y, float w, float h) => new Rect(x, y, w, h);

    public static Rect TopRight(float margin, float y, float w, float h)
        => new Rect(Screen.width - margin - w, y, w, h);

    public static Rect BottomLeft(float x, float margin, float w, float h)
        => new Rect(x, Screen.height - margin - h, w, h);

    public static Rect BottomRight(float margin, float w, float h)
        => new Rect(Screen.width - margin - w, Screen.height - margin - h, w, h);

    public static bool MouseOver(Rect r) => r.Contains(Event.current.mousePosition);
}