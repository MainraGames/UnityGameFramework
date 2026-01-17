// Copyright (c) 2025 PinePie. All rights reserved.

#if UNITY_EDITOR

using UnityEditor;
using System.IO;

namespace PinePie.PieTabs
{
    public static class PathUtility
    {
        private static string GetAnchorFilePath()
        {
            string[] guids = AssetDatabase.FindAssets("t:MonoScript PieTabs_AnchorSO");
            if (guids.Length == 0) return null;

            string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            return Path.GetDirectoryName(scriptPath);
        }

        public static string GetPieTabsPath()
        {
            string originalPath = GetAnchorFilePath();

            originalPath = originalPath.Replace("\\", "/");

            string[] parts = originalPath.Split('/');

            // last no. to know how much anchor file is deep in folders
            return string.Join("/", parts, 0, parts.Length - 6);
        }
    }

}
#endif