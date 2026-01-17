// Copyright (c) 2025 PinePie. All rights reserved.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PinePie.PieTabs
{
    public static partial class Navigator
    {
        private static MethodInfo showFolderContents;
        private static MethodInfo EndRenamingMI;

        private static Type winType;
        private static Type entityIdType;
        private static EditorWindow window;

        public static void ForceEndRenaming()
        {
            if (winType == null) winType = Type.GetType("UnityEditor.ProjectBrowser, UnityEditor");

            if (EndRenamingMI == null) EndRenamingMI = winType?.GetMethod("EndRenaming", BindingFlags.Instance | BindingFlags.Public);

            if (window == null) window = GetWin();

            EndRenamingMI.Invoke(window, null);
        }

        // internal values fetcher
        private static float GetSideRectWidth(EditorWindow win)
        {
            if (win.GetType().ToString() == "UnityEditor.ProjectBrowser")
            {
                var type = win.GetType();
                FieldInfo Field = type.GetField("m_DirectoriesAreaWidth", BindingFlags.NonPublic | BindingFlags.Instance);

                if (Field != null)
                {
                    var rect = Field.GetValue(win);
                    return (float)rect;
                }
                else
                    return 0;
            }
            else
                return 0;
        }

        public static bool IsTwoColumnMode()
        {
            var projectBrowsers = Resources.FindObjectsOfTypeAll(typeof(EditorWindow));
            foreach (EditorWindow window in projectBrowsers.Cast<EditorWindow>())
            {
                if (window.GetType().ToString() == "UnityEditor.ProjectBrowser")
                {
                    var viewModeField = window.GetType().GetField("m_ViewMode", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (viewModeField != null)
                    {
                        object viewModeValue = viewModeField.GetValue(window);
                        return viewModeValue.ToString() == "TwoColumns";
                    }
                }
            }
            return false;
        }

        public static VisualElement GetProjectBrowserUI()
        {
            var projectBrowsers = Resources.FindObjectsOfTypeAll(typeof(EditorWindow));
            foreach (EditorWindow window in projectBrowsers.Cast<EditorWindow>())
            {
                if (window.GetType().ToString() == "UnityEditor.ProjectBrowser")
                    return window.rootVisualElement;
            }

            return new();
        }

        public static EditorWindow GetWin()
        {
            var projectBrowsers = Resources.FindObjectsOfTypeAll(typeof(EditorWindow));
            foreach (EditorWindow window in projectBrowsers.Cast<EditorWindow>())
            {
                if (window.GetType().ToString() == "UnityEditor.ProjectBrowser")
                    return window;
            }

            return new();
        }

        public static string GetActiveFolderPath()
        {
            if (window == null) window = GetWin();

            MethodInfo method = window.GetType().GetMethod("GetActiveFolderPath", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method != null)
            {
                string result = method.Invoke(window, null) as string;
                if (!string.IsNullOrEmpty(result))
                    return result;
            }

            return "Assets/";
        }

        public static void OpenFolder(string folderPath)
        {
            if (window == null) window = GetWin();
            if (winType == null) winType = window.GetType();

            Object obj = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
            if (obj == null)
                return;

            if (showFolderContents == null)
            {
                showFolderContents = winType.GetMethod(
                    "ShowFolderContents",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(int), typeof(bool) },
                    null
                );

            }

            if (showFolderContents != null && showFolderContents.GetParameters()[0].ParameterType == typeof(int))
            {
                showFolderContents.Invoke(window, new object[] { obj.GetInstanceID(), true });
                return;
            }


            if (showFolderContents == null)
            {
                if (entityIdType == null)
                    entityIdType = typeof(Object).Assembly.GetType("UnityEngine.EntityId");

                if (entityIdType != null)
                {
                    showFolderContents = winType.GetMethod(
                        "ShowFolderContents",
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new[] { entityIdType, typeof(bool) },
                        null
                    );
                }
            }

            if (showFolderContents != null && showFolderContents.GetParameters()[0].ParameterType == entityIdType)
            {
                object param1 = Activator.CreateInstance(entityIdType);
                entityIdType.GetField("m_Data", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(param1, obj.GetInstanceID());

                showFolderContents.Invoke(window, new object[] { param1, true });
                return;
            }


            AssetDatabase.OpenAsset(obj);
        }


        public static void FocusAssetByObj(Object asset)
        {
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorUtility.FocusProjectWindow();
            }
        }

        public static List<string> CleanEntries(List<string> items)
        {
            // HashSet for fast lookup
            HashSet<string> set = new HashSet<string>(items);

            List<string> output = new List<string>();

            foreach (var item in items)
            {
                bool isParent = items.Any(child =>
                    child != item &&
                    child.StartsWith(item + "/")
                );

                if (!isParent)
                    output.Add(item);
            }

            return output;
        }


        public static void OnCreateMenuEntrySelected(string iconName, string fullEntry)
        {
            CreatorButton button = new(fullEntry, iconName);

            creatorButtons.AddButton(button, UI.creatorButtonAsset);

            FillCreatorButtons();
        }

        public static void OnEditMenuEntry(CreatorButton btnProp, string newEntry)
        {
            btnProp.menuEntry = newEntry;

            FillCreatorButtons();
        }



        // placeholder helper
        public static int PlaceholderNeedleAtPos(VisualElement area, float mouseX)
        {
            bool containsPlaceholder = area.Contains(UI.placeholderNeedle);

            // getting drop index
            int dropIndex = area.childCount;
            for (int i = area.childCount - 1; i >= 0; i--)
            {
                var child = area[i];

                if (containsPlaceholder && child == UI.placeholderNeedle) continue;

                if (true)
                {
                    if (mouseX > child.layout.center.x) dropIndex--;
                    else break;
                }
            }
            if (containsPlaceholder) dropIndex--;

            if (dropIndex != placeHolderIndex)
            {
                placeHolderIndex = dropIndex;

                UI.placeholderNeedle.RemoveFromHierarchy();
                area.Insert(dropIndex, UI.placeholderNeedle);
            }

            return dropIndex;
        }


        // asset create menu items fetcher
        public static List<string> GetAssetCreateMenuEntries()
        {
            var list = new List<string>();

            var menuType = typeof(Editor).Assembly.GetType("UnityEditor.Menu");
            if (menuType == null) return list;

            var method = menuType.GetMethod(
                "GetMenuItems",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new[] { typeof(string), typeof(bool), typeof(bool) },
                null
            );

            if (method == null) return list;
            var raw = method.Invoke(null, new object[] { "Assets/Create/", true, false });
            if (raw == null) return list;

            foreach (var item in (Array)raw)
            {
                string path = item.GetType().GetProperty("path")?.GetValue(item) as string;

                if (!string.IsNullOrEmpty(path))
                    list.Add(path);
            }

            return list;
        }


        // loader
        public static VisualTreeAsset LoadUXML(string relativePath) =>
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{PathUtility.GetPieTabsPath()}/PinePie/PieTabs/editor/Core/UI/{relativePath}");

        public static Texture2D LoadTex(string relativePath) =>
            AssetDatabase.LoadAssetAtPath<Texture2D>($"{PathUtility.GetPieTabsPath()}/PinePie/PieTabs/editor/Core/UI/{relativePath}");


        // button setup 
        public static void SetShadeProperties(VisualElement button, bool isMinimal, string Label, Texture2D iconTexture)
        {
            // shade
            var buttonShade = button.Q<VisualElement>("shade");
            buttonShade.Q<Label>("buttonLabel").text = isMinimal ? "" : Label;
            buttonShade.Q<VisualElement>("buttonIcon").style.backgroundImage = new StyleBackground(iconTexture);
            button.RegisterCallback<MouseEnterEvent>(_ =>
            {
                buttonShade.style.backgroundColor = new Color(255, 255, 255, 0.06f);
            });
            button.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                buttonShade.style.backgroundColor = new Color(255, 255, 255, 0f);
            });
        }

        public static void SetupButtonProperties(VisualElement button, bool isMinimal, string Label, string color)
        {
            // color and tooltip
            Color col = HexToColor(color);

            button.style.backgroundColor = col;
            button.tooltip = isMinimal ? Label : null;

            // label props
            var labelElement = button.Q<Label>("buttonLabel");
            if (labelElement != null)
            {
                labelElement.text = isMinimal ? "" : Label;

                labelElement.style.color = IsColorDark(col)
                ? HexToColor("#f7f7f7")
                : HexToColor("#2e2e2e");
            }
        }


        // color setup helpers
        public static bool IsColorDark(Color color)
        {
            float brightness = (color.r * 0.299f) + (color.g * 0.587f) + (color.b * 0.114f);
            return brightness < 0.5f;
        }

        public static string ColorToHex(Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }

        public static Color HexToColor(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return HexToColor("#3E3E3E");

            if (!hex.StartsWith("#"))
                hex = "#" + hex;

            hex = hex.ToUpperInvariant();

            if (ColorUtility.TryParseHtmlString(hex, out Color color))
                return color;

            return HexToColor("#3E3E3E");
        }

        public enum PieAssetType
        {
            Folder,
            Scene,
            Prefab,
            Script,
            ShaderGraph,
            VisualScriptingGraph,
            Other
        }

        public static PieAssetType GetAssetType(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return PieAssetType.Folder;

            Type t = AssetDatabase.GetMainAssetTypeAtPath(path);

            if (t == typeof(SceneAsset))
                return PieAssetType.Scene;

            if (t == typeof(GameObject))
                return PieAssetType.Prefab;

            if (t == typeof(MonoScript))
                return PieAssetType.Script;

            if (t.FullName == "UnityEditor.ShaderGraph.GraphData" ||
                t.FullName?.Contains("ShaderGraph") == true)
                return PieAssetType.ShaderGraph;

            if (t.FullName?.Contains("VisualScripting") == true)
                return PieAssetType.VisualScriptingGraph;

            return PieAssetType.Other;
        }


    }
}

#endif