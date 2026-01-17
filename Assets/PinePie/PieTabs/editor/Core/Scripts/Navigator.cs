// Copyright (c) 2025 PinePie. All rights reserved.

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PinePie.PieTabs
{
    [InitializeOnLoad]
    public static class NavigatorLoader
    {
        static NavigatorLoader() => EditorApplication.update += RunOnceOnLoad;

        static void RunOnceOnLoad()
        {
            EditorApplication.update -= RunOnceOnLoad;

            if (!Directory.Exists($"{PathUtility.GetPieTabsPath()}/PinePie/PieTabs"))
                return;

            Navigator.Setup();
            UI.mainUI.RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                Selection.selectionChanged -= EnsurePieDeskOverlay;

                EditorApplication.delayCall += () =>
                {
                    var projectWindow = Navigator.GetProjectBrowserUI();
                    if (UI.mainUI.panel == null)
                    {
                        Selection.selectionChanged += EnsurePieDeskOverlay;
                    }
                };
            });
        }

        private static void EnsurePieDeskOverlay()
        {
            var lastFocused = EditorWindow.focusedWindow;
            if (lastFocused != null && lastFocused.GetType().Name == "ProjectBrowser")
            {
                Selection.selectionChanged -= EnsurePieDeskOverlay;
                Navigator.Setup();
            }
        }

        [MenuItem("Tools/Refresh PieTabs _F5")]
        public static void RefreshPieDesk()
        {
            Navigator.Setup();
        }
    }

    public static partial class Navigator
    {
        public static ShortcutButtonBundle navButtons = new();
        public static CreatorButtonBundle creatorButtons = new();

        private const string SplitterKey = "PieTabs_LastSplitterSpacing";
        private const string SearchBarOpenKey = "PieTabs_SearchBarOpen";

        private static float LastCreatorWidth
        {
            get => EditorPrefs.GetFloat(SplitterKey, 300f);
            set => EditorPrefs.SetFloat(SplitterKey, value);
        }

        private static bool IsSearchBarOpen
        {
            get => EditorPrefs.GetBool(SearchBarOpenKey, false);
            set => EditorPrefs.SetBool(SearchBarOpenKey, value);
        }

        public static bool isTwoColumnMode = true;
        private static int placeHolderIndex;

        private static Vector2 dragStartPos;
        private static bool isDragging = false;
        private static bool isMouseDown = false;


        public static void Setup()
        {
            UI.projectBrowserUI = GetProjectBrowserUI();
            UI.mainUI = LoadUXML("PieDeskMainUI.uxml").Instantiate().Q<VisualElement>("PieDeskUI");
            UI.mainUI.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>($"{PathUtility.GetPieTabsPath()}/PinePie/PieTabs/editor/Core/UI/PieDeskStyling.uss"));

            UI.shortcutButtonAsset = LoadUXML("shortcutButton.uxml");
            UI.creatorButtonAsset = LoadUXML("creatorButton.uxml");
            UI.placeholderNeedle = LoadUXML("placeholderLine.uxml").Instantiate().Q<VisualElement>("line");

            UI.copiedText = UI.mainUI.Q<VisualElement>("copiedText");

            UI.colorPopup = UI.mainUI.Q<VisualElement>("colorPopup").Q<VisualElement>("colorPopup");

            VisualElement alreadySetUI = UI.projectBrowserUI.Q<VisualElement>("PieDeskUI");
            alreadySetUI?.RemoveFromHierarchy();

            isTwoColumnMode = IsTwoColumnMode();
            PieTabsPrefs.LoadKeyComb();

            UI.shortcutButtonArea = UI.mainUI.Q<ScrollView>("shorcutsDragArea");
            UI.creatorButtonArea = UI.mainUI.Q<ScrollView>("CreationMenuDragArea");
            if (!isTwoColumnMode)
            {
                UI.shortcutButtonArea.style.display = DisplayStyle.None;
                UI.creatorButtonArea.style.flexGrow = 1;

                UI.mainUI.Q<VisualElement>("splitter").style.display = DisplayStyle.None;

                VisualElement bottomBar = UI.mainUI.Q<VisualElement>("bottomAddressBar");
                bottomBar.style.marginRight = 0;
                bottomBar.style.marginLeft = 0;

                UI.shortcutButtonArea = UI.mainUI.Q<ScrollView>("CreationMenuDragArea");
            }
            else // two coloumn mode 
            {
                SetupSplitter();
                SetupBottomBarMargin();
                UI.creatorButtonArea.style.width = LastCreatorWidth;

                // asset creator button 
                creatorButtons.LoadFromJson(UI.creatorButtonAsset);
                FillCreatorButtons();
            }

            CallbacksForPopupBoxes();
            RegisterAddressCopyCallbacks();
            SetupDragAreaStyling();
            SearchBarAndCreatorTabBtnCallbacks();
            CallbacksForColorPopup();

            // shortcut buttons
            navButtons.LoadFromJson(UI.shortcutButtonAsset);
            SetupDragNDropForShortcutArea(UI.shortcutButtonArea, navButtons);
            FillShortcutButtons();

            UI.projectBrowserUI.Add(UI.mainUI);
        }


        // click callbacks
        public static void OnShortcutButtonClicked(
            VisualElement UIbutton,
            ShortcutButton buttonProp)
        {
            // callbacks
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(buttonProp.Path);
            UIbutton.AddManipulator(new ShortcutDragManipulator(obj));

            UIbutton.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == 0)
                {
                    UIbutton.Q<VisualElement>("shade").style.backgroundColor = new Color(255, 255, 255, 0.15f);

                    evt.StopPropagation();
                }
                else if (evt.button == 1)
                {
                    evt.StopPropagation();
                }
            });

            UIbutton.RegisterCallback<PointerUpEvent>(evt =>
            {
                // single click
                if (obj == null) return;

                if (PieTabsPrefs.KeyMatches(evt, PieTabsPrefs.directOpenShortcut))
                {
                    if (AssetDatabase.IsValidFolder(buttonProp.Path))
                        OpenFolder(buttonProp.Path);
                    else
                        AssetDatabase.OpenAsset(obj);

                    evt.StopPropagation();
                }
                else if (PieTabsPrefs.KeyMatches(evt, PieTabsPrefs.minimalShortcut))
                {
                    buttonProp.isMinimal = !buttonProp.isMinimal;

                    UIbutton.Q<Label>("buttonLabel").text = buttonProp.isMinimal ? "" : buttonProp.Label;
                    UIbutton.tooltip = buttonProp.isMinimal ? buttonProp.Label : null;

                    var buttonShade = UIbutton.Q<VisualElement>("shade");
                    buttonShade.Q<Label>("buttonLabel").text = buttonProp.isMinimal ? "" : buttonProp.Label;

                    navButtons.SaveToJson();

                    evt.StopPropagation();
                }
                else if (PieTabsPrefs.KeyMatches(evt, PieTabsPrefs.colorShortcut))
                {
                    ShowBoxAtPos(UI.colorPopup, evt.position.x - 100);

                    ColorPopup.isForCreator = false;
                    ColorPopup.popupIsOpen = true;

                    ColorPopup.activeNavButton = buttonProp;
                    ColorPopup.activeVisualItem = UIbutton;

                    evt.StopPropagation();
                }

                else if (PieTabsPrefs.KeyMatches(evt, PieTabsPrefs.deleteShortcut))
                {
                    if (PieTabsPrefs.AskBeforeDelete)
                    {
                        bool confirm = EditorUtility.DisplayDialog(
                            "Delete Tab",
                            $"Are you sure you want to delete \"{buttonProp.Label}\" Tab?",
                            "Delete", "Cancel"
                        );

                        if (confirm) RemoveButton(buttonProp);
                    }
                    else RemoveButton(buttonProp);

                    evt.StopPropagation();
                }

                else if (evt.button == 0)
                {
                    string path = buttonProp.Path;
                    PieAssetType type = GetAssetType(path);

                    if (type == PieAssetType.Folder && PieTabsPrefs.FastFolderOpen)
                        OpenFolder(path);
                    else if ((type == PieAssetType.Scene && PieTabsPrefs.FastSceneOpen)
                            || (type == PieAssetType.Script && PieTabsPrefs.FastScriptOpen)
                            || (type == PieAssetType.Prefab && PieTabsPrefs.FastPrefabOpen)
                            || (type == PieAssetType.ShaderGraph && PieTabsPrefs.FastShaderGraphOpen)
                            || (type == PieAssetType.VisualScriptingGraph && PieTabsPrefs.FastVisScrGraphOpen))
                        AssetDatabase.OpenAsset(obj);
                    else
                        FocusAssetByObj(obj);


                    evt.StopPropagation();
                }

                UIbutton.Q<VisualElement>("shade").style.backgroundColor = new Color(255, 255, 255, 0.12f);
            });

        }

        public static void OnAssetCreatorButtonClicked(
            VisualElement UIbutton,
            CreatorButton buttonProp)
        {
            // callbacks
            UIbutton.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == 0)
                {
                    UIbutton.Q<VisualElement>("shade").style.backgroundColor = new Color(255, 255, 255, 0.15f);

                    dragStartPos = evt.position;
                    isMouseDown = true;
                    isDragging = false;

                    evt.StopPropagation();
                }
                else if (evt.button == 1)
                {
                    evt.StopPropagation();
                }
            });

            UIbutton.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (!isMouseDown) return;

                if (!isDragging && Vector2.Distance(evt.position, dragStartPos) > 3f)
                {
                    isDragging = true;
                    UIbutton.CaptureMouse();
                }

                if (UIbutton.HasMouseCapture() && isDragging) OnDrag(evt.position.x);
            });

            UIbutton.RegisterCallback<PointerUpEvent>(evt =>
            {
                // single click
                if (evt.button == 0) isMouseDown = false;

                if (isDragging)
                {
                    isDragging = false;
                    UIbutton.ReleaseMouse();

                    EndDrag(UIbutton);
                    FillCreatorButtons();

                    evt.StopPropagation();
                    return;
                }

                if (PieTabsPrefs.KeyMatches(evt, PieTabsPrefs.changeMenuEntryShortcut))
                {
                    List<string> items = GetAssetCreateMenuEntries();

                    var menu = new GenericMenu();
                    foreach (var item in CleanEntries(items))
                    {
                        var trimmedItem = item;
                        const string prefix = "Assets/Create/";

                        if (item.StartsWith(prefix))
                            trimmedItem = item[prefix.Length..];

                        menu.AddItem(new GUIContent(trimmedItem), false, () =>
                        {
                            OnEditMenuEntry(buttonProp, item);
                        });
                    }
                    menu.DropDown(new Rect(evt.position, Vector2.zero));

                    evt.StopPropagation();
                }
                // minimal mode
                else if (PieTabsPrefs.KeyMatches(evt, PieTabsPrefs.minimalShortcut))
                {
                    buttonProp.isMinimal = !buttonProp.isMinimal;

                    UIbutton.Q<Label>("buttonLabel").text = buttonProp.isMinimal ? "" : buttonProp.Label;
                    UIbutton.tooltip = buttonProp.isMinimal ? buttonProp.Label : null;

                    var buttonShade = UIbutton.Q<VisualElement>("shade");
                    buttonShade.Q<Label>("buttonLabel").text = buttonProp.isMinimal ? "" : buttonProp.Label;

                    creatorButtons.SaveToJson();

                    evt.StopPropagation();
                }
                // color setting
                else if (PieTabsPrefs.KeyMatches(evt, PieTabsPrefs.colorShortcut))
                {
                    ShowBoxAtPos(UI.colorPopup, evt.position.x - 100);

                    ColorPopup.isForCreator = true;
                    ColorPopup.popupIsOpen = true;

                    ColorPopup.activeCreatorButton = buttonProp;
                    ColorPopup.activeVisualItem = UIbutton;

                    evt.StopPropagation();
                }
                else if (evt.button == 0)
                {
                    EditorApplication.ExecuteMenuItem(buttonProp.menuEntry);

                    evt.StopPropagation();
                }

                else if (PieTabsPrefs.KeyMatches(evt, PieTabsPrefs.deleteShortcut))
                {
                    if (PieTabsPrefs.AskBeforeDelete)
                    {
                        bool confirm = EditorUtility.DisplayDialog(
                            "Delete Tab",
                            $"Are you sure you want to delete \"{buttonProp.Label}\" Tab?",
                            "Delete",
                            "Cancel"
                        );

                        if (confirm) RemoveButton(buttonProp);
                    }
                    else RemoveButton(buttonProp);

                    evt.StopPropagation();
                }
            });

        }


        // filling and removing
        public static void FillShortcutButtons()
        {
            UI.shortcutButtonArea.Clear();

            foreach (var button in navButtons.buttons)
            {
                UI.shortcutButtonArea.Add(button.UIbutton);
            }
        }
        public static void FillCreatorButtons()
        {
            UI.creatorButtonArea.Clear();

            foreach (var button in creatorButtons.buttons)
            {
                UI.creatorButtonArea.Add(button.UIbutton);
            }
        }

        public static void RemoveButton(ShortcutButton toRemove)
        {
            navButtons.RemoveButton(toRemove);

            FillShortcutButtons();
        }
        public static void RemoveButton(CreatorButton toRemove)
        {
            creatorButtons.RemoveButton(toRemove);

            FillCreatorButtons();
        }


        // shortcut bar dragging
        public static void SetupDragNDropForShortcutArea(VisualElement area, ShortcutButtonBundle navButtonsList)
        {
            area.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                PlaceholderNeedleAtPos(area, evt.localMousePosition.x);

                evt.StopPropagation();
            });

            area.RegisterCallback<DragPerformEvent>(evt =>
            {
                DragAndDrop.AcceptDrag();

                foreach (var obj in DragAndDrop.objectReferences)
                {
                    string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));

                    Texture2D iconTexture = EditorGUIUtility.ObjectContent(obj, obj.GetType()).image as Texture2D;

                    var foundButton = navButtonsList.buttons.FirstOrDefault(b => b.buttonProp.guid == guid);
                    ShortcutButton buttonAtSamePath = foundButton?.buttonProp;

                    var button = new ShortcutButton(obj.name, guid);
                    if (buttonAtSamePath != null) button = new ShortcutButton(obj.name, guid, buttonAtSamePath.isMinimal, buttonAtSamePath.color);

                    if (placeHolderIndex != -1) navButtonsList.InsertAt(placeHolderIndex, button, UI.shortcutButtonAsset);

                    if (buttonAtSamePath != null) navButtonsList.RemoveButton(buttonAtSamePath);

                    UI.placeholderNeedle.RemoveFromHierarchy();
                    placeHolderIndex = -1;
                }

                FillShortcutButtons();

                evt.StopPropagation();
            });

            area.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                if (DragAndDrop.objectReferences.Length > 0)
                {
                    UI.placeholderNeedle.RemoveFromHierarchy();

                    placeHolderIndex = -1;
                }
            });
        }

    }

    static class UI
    {
        public static VisualElement projectBrowserUI;
        public static VisualElement mainUI;

        public static VisualElement placeholderNeedle;
        public static VisualElement copiedText;

        public static VisualElement colorPopup;


        public static VisualTreeAsset shortcutButtonAsset;
        public static VisualTreeAsset creatorButtonAsset;


        public static VisualElement shortcutButtonArea;
        public static VisualElement creatorButtonArea;
    }

    static class ColorPopup
    {
        public static bool isForCreator = false;
        public static bool popupIsOpen = false;

        public static ShortcutButton activeNavButton;
        public static CreatorButton activeCreatorButton;
        public static VisualElement activeVisualItem;
    }

}
#endif
