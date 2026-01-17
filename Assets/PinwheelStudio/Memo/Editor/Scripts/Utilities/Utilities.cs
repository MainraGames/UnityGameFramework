using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine.Networking;
using System.IO;

namespace Pinwheel.Memo
{
    public static class Utilities
    {
        public static void CallDelayed(System.Action action)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(ICallDelayed(action));
        }

        private static IEnumerator ICallDelayed(System.Action action)
        {
            yield return null;
            action.Invoke();
        }

        public static T[] ParseJsonToArray<T>(string json)
        {
            json = $"{{\"{nameof(ArrayWrapper<T>.items)}\":{json}}}";
            ArrayWrapper<T> arrayWrapper = new ArrayWrapper<T>();
            arrayWrapper = JsonUtility.FromJson<ArrayWrapper<T>>(json);
            return arrayWrapper.items;
        }

        public static T[] Concat<T>(this T[] array1, T[] array2)
        {
            int count = array1.Length + array2.Length;
            T[] values = new T[count];
            array1.CopyTo(values, 0);
            array2.CopyTo(values, array1.Length);
            return values;
        }

        public static string Ellipsis(string src, int maxLength)
        {
            if (src.Length > maxLength)
            {
                return src.Substring(0, maxLength) + "...";
            }
            else
            {
                return src;
            }
        }

        public static void LogIfError(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogWarning(error);
            }
        }

        public static string MarkdownToRichText(string markdown)
        {
            string richtext = "";

            return richtext;
        }

        private const string PREF_SYNC_COUNT_PREFIX = "memo-object-sync-count-";
        public static int GetObjectSyncCount(string objectId)
        {
            string PREF_KEY = PREF_SYNC_COUNT_PREFIX + objectId;
            return SessionState.GetInt(PREF_KEY, 0);
        }

        public static void IncreaseObjectSyncCount(string objectId)
        {
            string PREF_KEY = PREF_SYNC_COUNT_PREFIX + objectId;
            int currentValue = SessionState.GetInt(PREF_KEY, 0);
            SessionState.SetInt(PREF_KEY, currentValue + 1);
        }

        public static string NewId()
        {
            return $"memo-{System.Guid.NewGuid().ToString()}";
        }

        public static bool IsLocalId(string id)
        {
            return string.IsNullOrEmpty(id) || id.StartsWith("memo");
        }

        public static bool HasSingleObjectSelection()
        {
            return Selection.count == 1;
        }

        public static bool WebRequestIsCompleted(UnityWebRequest request)
        {
            if (request == null)
                return true;

            try
            {
                if (request.isDone)
                    return true;
            }
            catch (System.ArgumentNullException) //_unity_self is null for some reason. The request is not null, but internal unmanaged object is null
            {
                return true;
            }

            return false;
        }

        public static string GetMemoRootFolder()
        {
            string[] guids = AssetDatabase.FindAssets("l:MemoRootFolder");
            string root = null;
            if (guids.Length == 1)
            {
                string folderPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                return Path.GetDirectoryName(folderPath);
            }
            else
            {
                root = Path.Combine("Assets","PinwheelStudio");
            }
            return root;
        }

        public static string GetMemoDataFolder()
        {
            return Path.Combine(GetMemoRootFolder(), "MemoData");
        }

        public static void MarkDirty(this Object o)
        {
            EditorUtility.SetDirty(o);
        }
    }
}
