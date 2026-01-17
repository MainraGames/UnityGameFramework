// Copyright (c) 2025 PinePie. All rights reserved.

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace PinePie.PieTabs
{
    public static partial class Navigator
    {
        // UI Setup

        public static void OnDrag(float mouseX)
        {
            VisualElement area = UI.creatorButtonArea;

            List<VisualElement> tabs = area.Children().Where(c => c.name != "line").ToList();

            int newIndex = tabs.Count;
            for (int i = tabs.Count - 1; i >= 0; i--)
            {
                var child = tabs[i];
                if (child == UI.placeholderNeedle) continue;

                Rect rect = child.worldBound;
                float midX = rect.x + rect.width / 2;

                if (mouseX > midX) newIndex--;
            }

            if (newIndex != placeHolderIndex)
            {
                placeHolderIndex = newIndex;

                if (area.Contains(UI.placeholderNeedle)) UI.placeholderNeedle.RemoveFromHierarchy();
                area.Insert(newIndex, UI.placeholderNeedle);
            }

            if (!area.Contains(UI.placeholderNeedle)) // on over inserting section
                area.Insert(newIndex, UI.placeholderNeedle);
        }

        public static void EndDrag(VisualElement btn)
        {
            VisualElement area = UI.creatorButtonArea;

            if (placeHolderIndex < 0)
                return;

            UI.placeholderNeedle.RemoveFromHierarchy();

            int oldIndex = area.IndexOf(btn);

            MoveItem(creatorButtons.buttons, oldIndex, placeHolderIndex);
            creatorButtons.SaveToJson();

            placeHolderIndex = -1;
        }

        public static void MoveItem<T>(List<T> list, int fromIndex, int toIndex)
        {
            toIndex = Mathf.Clamp(toIndex, 0, list.Count);

            if (fromIndex == toIndex || (fromIndex == list.Count - 1 && toIndex == list.Count))
                return;

            T item = list[fromIndex];
            list.RemoveAt(fromIndex);

            if (toIndex > fromIndex) toIndex--;

            if (toIndex >= list.Count) list.Add(item);
            else list.Insert(toIndex, item);
        }


        // split bars
        public static void SetupDragAreaStyling()
        {
            foreach (var view in new VisualElement[] { UI.shortcutButtonArea, UI.creatorButtonArea })
            {
                view.contentContainer.style.flexDirection = FlexDirection.RowReverse;
                view.contentContainer.style.justifyContent = Justify.FlexStart;
            }
        }

        public static void SetupSplitter()
        {
            VisualElement splitter = UI.mainUI.Q<VisualElement>("splitter");

            bool isDragging = false;
            int pointerId = -1;
            float distFromMouse = 0;

            UI.shortcutButtonArea.style.flexGrow = 0;
            UI.shortcutButtonArea.style.width = LastCreatorWidth;

            splitter.RegisterCallback<PointerDownEvent>(evt =>
            {
                isDragging = true;
                pointerId = evt.pointerId;

                distFromMouse = evt.position.x - splitter.worldBound.x - 5f;

                splitter.CapturePointer(pointerId);

                evt.StopPropagation();
            });

            splitter.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (!isDragging || evt.pointerId != pointerId) return;

                UI.shortcutButtonArea.style.width = evt.position.x - 70f - distFromMouse;
                UI.creatorButtonArea.style.flexGrow = 1;

                evt.StopPropagation();
            });

            splitter.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (evt.pointerId != pointerId) return;

                LastCreatorWidth = UI.shortcutButtonArea.resolvedStyle.width;
                isDragging = false;

                splitter.ReleasePointer(pointerId);
                evt.StopPropagation();
            });
        }

        public static void SearchBarAndCreatorTabBtnCallbacks()
        {
            VisualElement splitter = UI.mainUI.Q<VisualElement>("splitter");
            splitter.style.backgroundImage = LoadTex("grip.png");

            Button searchBtn = UI.mainUI.Q<Button>("searchToggle");
            searchBtn.Q<VisualElement>("icon").style.backgroundImage = LoadTex("magnifying-glass (1).png");
            Button creatorTabBtn = UI.mainUI.Q<Button>("addCreatorBtn");
            creatorTabBtn.style.backgroundImage = LoadTex("plus 1.png");

            ScrollView AssetCreatorButtonArea = UI.mainUI.Q<ScrollView>("CreationMenuDragArea");
            ScrollView shortcutTabsArea = UI.mainUI.Q<ScrollView>("shorcutsDragArea");

            Button sceneMenu = UI.mainUI.Q<Button>("sceneSel");

            if (IsSearchBarOpen)
                OpenSearchBar(sceneMenu, creatorTabBtn, splitter, searchBtn, AssetCreatorButtonArea);
            else
                CloseSearchBar(sceneMenu, creatorTabBtn, splitter, searchBtn, AssetCreatorButtonArea, shortcutTabsArea);


            sceneMenu.clicked += () =>
            {
                var menu = new GenericMenu();

                string[] guids = AssetDatabase.FindAssets("t:Scene");
                string activeScenePath = EditorSceneManager.GetActiveScene().path;

                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.StartsWith("Assets/"))
                        continue;

                    string name = Path.GetFileNameWithoutExtension(path);

                    string capturedPath = path;
                    menu.AddItem(new GUIContent(name), capturedPath == activeScenePath, () =>
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            EditorSceneManager.OpenScene(capturedPath);
                    });
                }

                menu.ShowAsContext();
            };


            searchBtn.clicked += () =>
            {
                if (IsSearchBarOpen)
                    CloseSearchBar(sceneMenu, creatorTabBtn, splitter, searchBtn, AssetCreatorButtonArea, shortcutTabsArea);
                else
                    OpenSearchBar(sceneMenu, creatorTabBtn, splitter, searchBtn, AssetCreatorButtonArea);
            };

            creatorTabBtn.clicked += () =>
            {
                List<string> items = GetAssetCreateMenuEntries();

                var menu = new GenericMenu();
                foreach (var item in CleanEntries(items))
                {
                    var trimmedItem = item;
                    const string prefix = "Assets/Create/";

                    if (item.StartsWith(prefix))
                        trimmedItem = item.Substring(prefix.Length);

                    menu.AddItem(new GUIContent(trimmedItem), false, () =>
                    {
                        CreateMenuIconUtility.GetIconFromCreateMenu(item, iconName =>
                        {
                            OnCreateMenuEntrySelected(iconName, item);
                        });
                    });
                }
                menu.DropDown(new Rect(creatorTabBtn.worldBound.position, Vector2.zero));
            };
        }

        private static void CloseSearchBar(Button sceneBtn, Button creatorBtn, VisualElement splitter, Button openSearchButton, ScrollView AssetCreatorButtonArea, ScrollView shortcutTabsArea)
        {
            if (isTwoColumnMode)
            {
                splitter.style.display = DisplayStyle.Flex;
                creatorBtn.style.display = DisplayStyle.Flex;
                sceneBtn.style.display = DisplayStyle.Flex;
                shortcutTabsArea.scrollOffset = Vector2.zero;
            }

            openSearchButton.style.width = 30f;

            AssetCreatorButtonArea.style.width = LastCreatorWidth;
            AssetCreatorButtonArea.style.marginRight = 0f;

            IsSearchBarOpen = false;
        }

        private static void OpenSearchBar(Button sceneBtn, Button creatorBtn, VisualElement splitter, Button openSearchButton, ScrollView AssetCreatorButtonArea)
        {
            if (!isTwoColumnMode)
            {
                openSearchButton.style.width = 20f;
                sceneBtn.style.display = DisplayStyle.None;
            }

            splitter.style.display = DisplayStyle.None;
            creatorBtn.style.display = DisplayStyle.None;

            AssetCreatorButtonArea.style.width = 0f;
            AssetCreatorButtonArea.style.marginRight = 425;

            IsSearchBarOpen = true;
        }


        // address copy from bottom bar
        public static void SetupBottomBarMargin()
        {
            VisualElement bottomAddressBar = UI.mainUI.Q<VisualElement>("bottomAddressBar");

            bottomAddressBar.style.marginLeft = GetSideRectWidth(GetWin());
        }

        public static void RegisterAddressCopyCallbacks()
        {
            VisualElement bottomAddressBar = UI.mainUI.Q<VisualElement>("bottomAddressBar");
            bottomAddressBar.RegisterCallback<MouseDownEvent>((evt) =>
            {
                if (isTwoColumnMode) bottomAddressBar.style.marginLeft = GetSideRectWidth(GetWin());

                string copyingStr = "";

                if (evt.button == 0)
                {
                    copyingStr = !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(Selection.activeObject))
                        ? AssetDatabase.GetAssetPath(Selection.activeObject)
                        : GetActiveFolderPath();
                }
                else if (evt.button == 1)
                {
                    string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                    copyingStr = !string.IsNullOrEmpty(assetPath)
                        ? Path.GetFileName(assetPath)
                        : "";
                }

                EditorGUIUtility.systemCopyBuffer = copyingStr;

                ShowCopiedNotification(evt.mousePosition, UI.mainUI);
            });

            if (isTwoColumnMode)
                Selection.selectionChanged += () =>
                {
                    SetupBottomBarMargin();
                };
        }

        public static void ShowCopiedNotification(Vector2 position, VisualElement root)
        {
            UI.copiedText.style.left = position.x - 20;

            UI.copiedText.style.display = DisplayStyle.Flex;

            root.schedule.Execute(() =>
            {
                UI.copiedText.style.display = DisplayStyle.None;
            }).ExecuteLater(1000);
        }


        // icon popup
        public static void ShowBoxAtPos(VisualElement box, float posX)
        {
            UI.mainUI.pickingMode = PickingMode.Position;

            float rightOffset = UI.mainUI.resolvedStyle.width - 200;

            box.style.left = Mathf.Clamp(posX, 0, rightOffset);

            box.style.display = DisplayStyle.Flex;
        }

        public static void CallbacksForPopupBoxes()
        {
            UI.mainUI.RegisterCallback<MouseDownEvent>((evt) =>
            {
                CloseAllPopups();
            });
        }

        private static void CloseAllPopups()
        {
            UI.colorPopup.style.display = DisplayStyle.None;

            ColorPopup.popupIsOpen = false;

            UI.mainUI.pickingMode = PickingMode.Ignore;
        }

        public static void CallbacksForColorPopup()
        {
            var colorButtons = UI.colorPopup.Query<Button>("icon").ToList();
            var removeColorBtn = UI.colorPopup.Q<Button>("removeColorBtn");

            foreach (Button clrBtn in colorButtons)
            {
                var buttonColor = clrBtn.resolvedStyle.backgroundColor;

                clrBtn.clicked += () =>
                {
                    if (!ColorPopup.popupIsOpen) return;

                    ApplyColorToVisualElement(ColorToHex(buttonColor));
                };
            }

            removeColorBtn.style.backgroundImage = LoadTex("cross icon.png");
            removeColorBtn.clicked += () =>
            {
                if (!ColorPopup.popupIsOpen) return;

                ApplyColorToVisualElement("#3E3E3E");
            };

            UI.colorPopup.RegisterCallback<MouseDownEvent>((evt) => evt.StopPropagation());
        }

        public static void ApplyColorToVisualElement(string hex)
        {
            if (ColorPopup.isForCreator)
            {
                ColorPopup.activeCreatorButton.color = hex;
                creatorButtons.SaveToJson();
            }
            else
            {
                ColorPopup.activeNavButton.color = hex;
                navButtons.SaveToJson();
            }

            ColorPopup.activeVisualItem.style.backgroundColor = HexToColor(hex);
            CloseAllPopups();
        }

    }
}

#endif