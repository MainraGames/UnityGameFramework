// Copyright (c) 2025 PinePie. All rights reserved.

#if UNITY_EDITOR
using UnityEditor;
using System;

namespace PinePie.PieTabs
{
    public static class CreateMenuIconUtility
    {
        private static Action<string> _done;
        private static bool fetchingIcon;

        private class Tracker : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(
                string[] imported,
                string[] deleted,
                string[] moved,
                string[] movedFrom)
            {
                if (imported == null || imported.Length == 0) return;

                if (fetchingIcon)
                {
                    Selection.activeObject = null;
                    fetchingIcon = false;

                    foreach (var path in imported)
                    {
                        var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                        if (obj == null) continue;

                        var icon = EditorGUIUtility.ObjectContent(obj, obj.GetType()).image;
                        string iconName = icon != null ? icon.name : "DefaultAsset Icon";

                        AssetDatabase.DeleteAsset(path);

                        _done?.Invoke(iconName);
                        _done = null;

                        break;
                    }
                }

            }
        }

        public static void GetIconFromCreateMenu(string menuPath, Action<string> onDone)
        {
            fetchingIcon = true;
            _done = onDone;

            EditorApplication.ExecuteMenuItem(menuPath);

            if (PieTabsPrefs.InstantCreatorTab) Navigator.ForceEndRenaming();
        }
    }
}
#endif
