using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Pinwheel.Memo
{
    public static class GUIUtils
    {
        public static void DrawOutline(Rect r, Color c)
        {
            Vector3[] points = new Vector3[]
            {
                new Vector3(r.min.x, r.min.y, 0),
                new Vector3(r.min.x, r.max.y, 0),
                new Vector3(r.max.x, r.max.y, 0),
                new Vector3(r.max.x, r.min.y, 0),
                new Vector3(r.min.x, r.min.y, 0)
            };
            Handles.BeginGUI();
            Handles.color = c;
            Handles.DrawPolyLine(points);
            Handles.EndGUI();
        }

        public static void DrawBottomLineForRect(Rect r, Color c)
        {
            Vector2 start = new Vector2(r.min.x, r.max.y);
            Vector2 end = new Vector2(r.max.x, r.max.y);
            Handles.color = c;
            Handles.BeginGUI();
            Handles.DrawLine(start, end);
            Handles.EndGUI();
            Handles.color = Color.white;
        }

        public static void Separator()
        {
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(GUILayout.Height(1)), new Color32(0, 0, 0, 40));
        }

        public static void DrawSquaredIcon(Texture2D icon, string tooltip = null)
        {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(EditorGUIUtility.singleLineHeight));
            GUI.DrawTexture(rect, icon);
            if (!string.IsNullOrEmpty(tooltip))
            {
                GUI.Box(rect, EditorGUIUtility.TrTextContent(string.Empty, tooltip), GUIStyle.none);
            }
        }

        public static class DefaultStyles
        {
            private static GUIStyle m_buttonIcon;
            public static GUIStyle buttonIcon
            {
                get
                {
                    if (m_buttonIcon == null)
                    {
                        m_buttonIcon = new GUIStyle(EditorStyles.iconButton);
                        m_buttonIcon.alignment = TextAnchor.MiddleCenter;
                        m_buttonIcon.fixedHeight = 0;
                        m_buttonIcon.fixedWidth = 0;
                    }
                    return m_buttonIcon;
                }
            }


            private static GUIStyle m_button3;
            public static GUIStyle button3
            {
                get
                {
                    if (m_button3 == null)
                    {
                        m_button3 = new GUIStyle(Styles.p1);
                        m_button3.alignment = TextAnchor.MiddleLeft;
                        m_button3.wordWrap = false;
                        m_button3.padding = new RectOffset(1, 1, 0, 0);
                    }
                    return m_button3;
                }
            }

            public static Color32 colorButtonPressed = new Color32(0, 0, 0, 50);
            public static Color32 colorButtonHover = new Color32(255, 255, 255, 20);
        }

        public static bool ButtonIcon(Rect rect, Texture icon, string tooltip = null, GUIStyle style = null)
        {
            //Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, NoteStyles.buttonIcon, GUILayout.Width(EditorGUIUtility.singleLineHeight));
            if (style == null)
            {
                style = DefaultStyles.buttonIcon;
            }

            GUIContent guiContent = EditorGUIUtility.TrIconContent(icon, tooltip);
            Vector2 oldIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(rect.size);
            bool clicked = GUI.Button(rect, guiContent, style);
            EditorGUIUtility.SetIconSize(oldIconSize);
            return clicked;
        }


        public static bool Button3(string text, bool fitText = true, GUIStyle style = null)
        {
            if (style == null)
                style = DefaultStyles.button3;

            Rect rect;
            if (fitText)
            {
                Vector2 controlSize = style.CalcSize(new GUIContent(text));
                rect = EditorGUILayout.GetControlRect(GUILayout.Width(controlSize.x), GUILayout.Height(controlSize.y));
            }
            else
            {
                rect = EditorGUILayout.GetControlRect();
            }

            const string PREF_BUTTON_ACTIVE_RECT = "button-active-rect";
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                SessionState.SetString(PREF_BUTTON_ACTIVE_RECT, rect.ToString());
            }
            if (Event.current.type == EventType.MouseUp)
            {
                SessionState.SetString(PREF_BUTTON_ACTIVE_RECT, null);
            }
            if (rect.Contains(Event.current.mousePosition))
            {
                string activeRect = SessionState.GetString(PREF_BUTTON_ACTIVE_RECT, null);
                if (string.Equals(rect.ToString(), activeRect))
                {
                    EditorGUI.DrawRect(rect, DefaultStyles.colorButtonPressed);
                }
                else
                {
                    EditorGUI.DrawRect(rect, DefaultStyles.colorButtonHover);
                }
            }

            bool clicked = GUI.Button(rect, text, style);

            return clicked;
        }

        public static bool Button3(Rect rect, string text, GUIStyle style = null)
        {
            if (style == null)
                style = DefaultStyles.button3;

            const string PREF_BUTTON_ACTIVE_RECT = "button-active-rect";
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                SessionState.SetString(PREF_BUTTON_ACTIVE_RECT, rect.ToString());
            }
            if (Event.current.type == EventType.MouseUp)
            {
                SessionState.SetString(PREF_BUTTON_ACTIVE_RECT, null);
            }
            if (rect.Contains(Event.current.mousePosition))
            {
                string activeRect = SessionState.GetString(PREF_BUTTON_ACTIVE_RECT, null);
                if (string.Equals(rect.ToString(), activeRect))
                {
                    EditorGUI.DrawRect(rect, DefaultStyles.colorButtonPressed);
                }
                else
                {
                    EditorGUI.DrawRect(rect, DefaultStyles.colorButtonHover);
                }
            }

            bool clicked = GUI.Button(rect, text, style);

            return clicked;
        }

        public static void DrawLinkStrip(string utmCampaign = "", string utmSource = "", string utmMedium = "")
        {
            LinkStripDrawer.Draw(utmCampaign, utmSource, utmMedium);
        }
    }

    public static class LinkStripDrawer
    {
        private static List<string> s_linkLabels = new List<string>();
        private static Dictionary<string, string> s_linkContents = new Dictionary<string, string>()
        {
            {"Documentation",  "https://docs.pinwheelstud.io/memo"},
            {"Support",  "https://discord.gg/btX4pdmZdV"},
            {"|", "" },
            {"Polaris", "https://assetstore.unity.com/packages/tools/terrain/polaris-3-low-poly-terrain-tool-286886" },
            {"Vista", "https://assetstore.unity.com/packages/tools/terrain/procedural-terrain-generator-vista-pro-264414" },
            {"Poseidon", "https://assetstore.unity.com/packages/vfx/shaders/low-poly-water-poseidon-153826" },
            {"Jupiter" ,"https://assetstore.unity.com/packages/2d/textures-materials/sky/procedural-sky-shader-day-night-cycle-jupiter-159992" },
            {"TextureGraph", "https://assetstore.unity.com/packages/tools/visual-scripting/procedural-texture-generator-texture-graph-185542" },
            {"Contour", "https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/contour-edge-detection-outline-post-effect-urp-render-graph-302915" },
            {"Beam","https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/beam-froxel-based-volumetric-lighting-fog-urp-render-graph-317850" }
        };

        private static GUIStyle s_linkStyle;
        private static GUIStyle linkStyle
        {
            get
            {
                if (s_linkStyle == null)
                {
                    s_linkStyle = new GUIStyle(EditorStyles.miniLabel);
                }
                s_linkStyle.normal.textColor = new Color32(125, 170, 240, 255);
                s_linkStyle.alignment = TextAnchor.MiddleLeft;
                return s_linkStyle;
            }
        }

        public static void Draw(string utmCampaign = "", string utmSource = "", string utmMedium = "")
        {
            s_linkLabels.Clear();
            s_linkLabels.AddRange(s_linkContents.Keys);
            Rect r = EditorGUILayout.GetControlRect(false, 12);
            var rects = EditorGUIUtility.GetFlowLayoutedRects(r, linkStyle, 4, 0, s_linkLabels);

            for (int i = 0; i < rects.Count; ++i)
            {
                Rect rect = rects[i];
                string label = s_linkLabels[i];
                string url = s_linkContents[label];

                if (!string.IsNullOrEmpty(url))
                {
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    if (GUI.Button(rect, label, linkStyle))
                    {
                        url = NetUtils.ModURL(url, utmCampaign, utmSource, $"{utmMedium}-{label.Replace(" ", "")}");
                        Application.OpenURL(url);
                    }
                }
                else
                {
                    GUI.Label(rect, label, linkStyle);
                }
            }
        }
    }
}