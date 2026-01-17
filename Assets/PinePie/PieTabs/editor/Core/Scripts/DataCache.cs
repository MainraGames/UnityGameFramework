// Copyright (c) 2025 PinePie. All rights reserved.

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PinePie.PieTabs
{
    public static class ButtonCache<T> where T : IButtonData
    {
        private static readonly string saveFilePath = $"{PathUtility.GetPieTabsPath()}/PinePie/PieTabs/editor/Core/Data_{typeof(T).Name}.json";

        public static void Save(List<T> buttons)
        {
            string json = JsonUtility.ToJson(new Wrapper { items = buttons });
            File.WriteAllText(saveFilePath, json);
        }

        public static List<T> Load()
        {
            if (!File.Exists(saveFilePath))
                return new List<T>();

            string json = File.ReadAllText(saveFilePath);
            return JsonUtility.FromJson<Wrapper>(json).items;
        }

        [System.Serializable]
        private class Wrapper
        {
            public List<T> items;
        }
    }
}


#endif