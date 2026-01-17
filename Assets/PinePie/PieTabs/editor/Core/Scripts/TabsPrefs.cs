// Copyright (c) 2025 PinePie. All rights reserved.

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PinePie.PieTabs
{
    static class PieTabsPrefs
    {
        static readonly EventModifiers[] modifierOptions = new[]
        {
            EventModifiers.None,
            EventModifiers.Control,
            EventModifiers.Shift,
            EventModifiers.Alt,
            EventModifiers.Control | EventModifiers.Shift,
            EventModifiers.Control | EventModifiers.Alt,
            EventModifiers.Shift | EventModifiers.Alt,
        };

        static readonly string[] mouseOptions = { "LMB", "RMB" };

        private const string AskBeforeDeleteKey = "PieTabs_AskBeforeDelete";
        private const string InstantCreatorTabKey = "PieTabs_InstantCreatorTab";
        private const string FastFolderOpenKey = "PieTabs_FastFolderOpen";
        private const string FastSceneOpenKey = "PieTabs_FastSceneOpen";
        private const string FastPrefabOpenKey = "PieTabs_FastPrefabOpen";
        private const string FastScriptOpenKey = "PieTabs_FastScriptOpen";
        private const string FastShaderGraphOpenKey = "PieTabs_FastShaderGraphOpen";
        private const string FastVisScrGraphOpenKey = "PieTabs_FastVisScrGraphOpen";



        public static bool AskBeforeDelete
        {
            get => EditorPrefs.GetBool(AskBeforeDeleteKey, false);
            set => EditorPrefs.SetBool(AskBeforeDeleteKey, value);
        }

        public static bool InstantCreatorTab
        {
            get => EditorPrefs.GetBool(InstantCreatorTabKey, true);
            set => EditorPrefs.SetBool(InstantCreatorTabKey, value);
        }


        public static bool FastFolderOpen
        {
            get => EditorPrefs.GetBool(FastFolderOpenKey, true);
            set => EditorPrefs.SetBool(FastFolderOpenKey, value);
        }

        public static bool FastSceneOpen
        {
            get => EditorPrefs.GetBool(FastSceneOpenKey, false);
            set => EditorPrefs.SetBool(FastSceneOpenKey, value);
        }

        public static bool FastPrefabOpen
        {
            get => EditorPrefs.GetBool(FastPrefabOpenKey, false);
            set => EditorPrefs.SetBool(FastPrefabOpenKey, value);
        }

        public static bool FastScriptOpen
        {
            get => EditorPrefs.GetBool(FastScriptOpenKey, false);
            set => EditorPrefs.SetBool(FastScriptOpenKey, value);
        }

        public static bool FastShaderGraphOpen
        {
            get => EditorPrefs.GetBool(FastShaderGraphOpenKey, false);
            set => EditorPrefs.SetBool(FastShaderGraphOpenKey, value);
        }

         public static bool FastVisScrGraphOpen
        {
            get => EditorPrefs.GetBool(FastVisScrGraphOpenKey, false);
            set => EditorPrefs.SetBool(FastVisScrGraphOpenKey, value);
        }

        // key combiations
        private const string ColorPopupKey = "PieTabs_ColorShortcut";
        private const string DirectOpenKey = "PieTabs_DirectOpen";
        private const string ChangeMenuEntryKey = "PieTabs_ChangeMenuEntry";
        private const string ToggleMinimalKey = "PieTabs_MinimalShortcut";
        private const string TabRemoveKey = "PieTabs_DeleteShortcut";
        public static KeyCombination directOpenShortcut;
        public static KeyCombination changeMenuEntryShortcut;
        public static KeyCombination colorShortcut;
        public static KeyCombination minimalShortcut;
        public static KeyCombination deleteShortcut;

        private const int LabelWidth = 270;


        [SettingsProvider]
        public static SettingsProvider CreatePieTabsProvider()
        {
            var provider = new SettingsProvider("Preferences/PieTabs", SettingsScope.User)
            {
                label = "PieTabs",
                guiHandler = (searchContext) =>
                {
                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Ask Before Removing Tab", GUILayout.Width(LabelWidth));
                    AskBeforeDelete = EditorGUILayout.Toggle("", AskBeforeDelete);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Instant Creator Tab (Experimental)*", "Experimental: Some entry may not work"), GUILayout.Width(LabelWidth));
                    InstantCreatorTab = EditorGUILayout.Toggle(new GUIContent("", "Experimental: Some entry may not work"), InstantCreatorTab);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    // folder
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Open Folder Tab in Only Left click", "Without even using ctrl key"), GUILayout.Width(LabelWidth));
                    FastFolderOpen = EditorGUILayout.Toggle("", FastFolderOpen);
                    EditorGUILayout.EndHorizontal();
                    // scene
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Open Scene Tab in Only Left click", "Without even using ctrl key"), GUILayout.Width(LabelWidth));
                    FastSceneOpen = EditorGUILayout.Toggle("", FastSceneOpen);
                    EditorGUILayout.EndHorizontal();
                    // prefab
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Open Prefab Tab in Only Left click", "Without even using ctrl key"), GUILayout.Width(LabelWidth));
                    FastPrefabOpen = EditorGUILayout.Toggle("", FastPrefabOpen);
                    EditorGUILayout.EndHorizontal();
                    // scene
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Open Script Tab in Only Left click", "Without even using ctrl key"), GUILayout.Width(LabelWidth));
                    FastScriptOpen = EditorGUILayout.Toggle("", FastScriptOpen);
                    EditorGUILayout.EndHorizontal();
                    // shader graph
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("ShaderGraph Tab in Left click", "Without even using ctrl key"), GUILayout.Width(LabelWidth));
                    FastShaderGraphOpen = EditorGUILayout.Toggle("", FastShaderGraphOpen);
                    EditorGUILayout.EndHorizontal();
                    // visual scripting graph
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("VisualScripting Graph Tab in Left click", "Without even using ctrl key"), GUILayout.Width(LabelWidth));
                   FastVisScrGraphOpen = EditorGUILayout.Toggle("",FastVisScrGraphOpen);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    var boldLabel = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold, };
                    EditorGUILayout.LabelField("Key Combinations", boldLabel);

                    LoadKeyComb();
                    colorShortcut = DrawShortcut("To Open Color Popup", colorShortcut);
                    minimalShortcut = DrawShortcut("To Toggle Minimal Mode", minimalShortcut);
                    deleteShortcut = DrawShortcut("To Remove Tab", deleteShortcut);

                    EditorGUILayout.Space();

                    directOpenShortcut = DrawShortcut("To Open Shortcut Tab File", directOpenShortcut);
                    changeMenuEntryShortcut = DrawShortcut("To Change Creator Tab Menu Entry", changeMenuEntryShortcut);

                    ValidateShortcuts();
                    Save();
                }
            };

            return provider;
        }

        static KeyCombination DrawShortcut(string label, KeyCombination shortcut)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(LabelWidth));

            // Modifier (Ctrl/Shift/Alt)
            int modIndex = System.Array.IndexOf(modifierOptions, shortcut.modifier);
            modIndex = EditorGUILayout.Popup(
                modIndex,
                modifierOptions.Select(m => m.ToString()).ToArray(),
                GUILayout.Width(100)
            );
            shortcut.modifier = modifierOptions[modIndex];

            // Mouse button
            shortcut.mouseButton = EditorGUILayout.Popup(
                shortcut.mouseButton,
                mouseOptions,
                GUILayout.Width(120)
            );

            EditorGUILayout.EndHorizontal();

            return shortcut;
        }

        static bool ValidateShortcuts()
        {
            var warnings = new List<string>();

            HashSet<string> collisionSet = new();

            void CheckBare(string name, KeyCombination kc)
            {
                if (IsBareLeftClick(kc))
                    warnings.Add($"{name}: Only LMB is reserved. Use any other combination.");
            }

            CheckBare("Color Popup", colorShortcut);
            CheckBare("Minimal Mode", minimalShortcut);
            CheckBare("Delete", deleteShortcut);
            CheckBare("Direct Open", directOpenShortcut);
            CheckBare("Change Menu Entry", changeMenuEntryShortcut);

            var all = new (string name, KeyCombination kc)[]
            {
                ("Color Popup", colorShortcut),
                ("Minimal Mode", minimalShortcut),
                ("Delete", deleteShortcut),
                ("Direct Open", directOpenShortcut),
                ("Change Menu Entry", changeMenuEntryShortcut),
            };

            for (int i = 0; i < all.Length; i++)
            {
                for (int j = i + 1; j < all.Length; j++)
                {
                    bool allowed = i == all.Length - 2 && j == all.Length - 1;
                    if (allowed) continue;

                    if (Collides(all[i].kc, all[j].kc))
                    {
                        string a = all[i].name;
                        string b = all[j].name;
                        string key = string.Compare(a, b) < 0 ? $"{a} <-> {b}" : $"{b} <-> {a}";

                        if (!collisionSet.Contains(key))
                        {
                            collisionSet.Add(key);
                            warnings.Add($"{a} and {b} use the same shortcut.");
                        }
                    }
                }
            }

            if (warnings.Count > 0)
            {
                EditorGUILayout.HelpBox(string.Join("\n", warnings), MessageType.Warning);
                return false;
            }
            else return true;
        }

        public static bool KeyMatches(PointerUpEvent evt, KeyCombination shortcut)
        {
            if (evt.button != shortcut.mouseButton) return false;

            EventModifiers evtMods = evt.modifiers & (EventModifiers.Control | EventModifiers.Shift | EventModifiers.Alt);

            // // Prevent bare LMB from matching anything that expects modifiers
            // if (shortcut.modifier != EventModifiers.None && evtMods == EventModifiers.None)
            //     return false;

            // // Exact match only

            return evtMods == shortcut.modifier;
        }



        public static bool IsBareLeftClick(KeyCombination kc)
        {
            return kc.mouseButton == 0 && kc.modifier == EventModifiers.None;
        }

        static bool Collides(KeyCombination a, KeyCombination b)
        {
            return a.modifier == b.modifier && a.mouseButton == b.mouseButton;
        }


        // save
        static void Save()
        {
            SaveShortcut(ColorPopupKey, colorShortcut);
            SaveShortcut(ToggleMinimalKey, minimalShortcut);
            SaveShortcut(TabRemoveKey, deleteShortcut);

            SaveShortcut(DirectOpenKey, directOpenShortcut);
            SaveShortcut(ChangeMenuEntryKey, changeMenuEntryShortcut);
        }

        static void SaveShortcut(string prefix, KeyCombination sc)
        {
            EditorPrefs.SetInt(prefix + "_Modifier", (int)sc.modifier);
            EditorPrefs.SetInt(prefix + "_MouseButton", sc.mouseButton);
        }

        // load
        public static void LoadKeyComb()
        {
            colorShortcut = LoadShortcut(ColorPopupKey, EventModifiers.Alt, 0);
            minimalShortcut = LoadShortcut(ToggleMinimalKey, EventModifiers.Shift, 0);
            deleteShortcut = LoadShortcut(TabRemoveKey, EventModifiers.None, 1);

            directOpenShortcut = LoadShortcut(DirectOpenKey, EventModifiers.Control, 0);
            changeMenuEntryShortcut = LoadShortcut(ChangeMenuEntryKey, EventModifiers.Control, 0);
        }

        static KeyCombination LoadShortcut(string prefix, EventModifiers defaultMod, int defaultBtn)
        {
            KeyCombination sc;

            sc.modifier = (EventModifiers)EditorPrefs.GetInt(prefix + "_Modifier", (int)defaultMod);
            sc.mouseButton = EditorPrefs.GetInt(prefix + "_MouseButton", defaultBtn);

            return sc;
        }

    }

    [System.Serializable]
    public struct KeyCombination
    {
        public EventModifiers modifier;
        public int mouseButton;
    }

}
#endif

