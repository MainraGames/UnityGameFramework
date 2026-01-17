using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine.Networking;

namespace Pinwheel.Memo
{
    //[CreateAssetMenu(menuName = "Memo/Version Checker")]
    [ExecuteInEditMode]
    [InitializeOnLoad]
    public class VersionChecker : ScriptableObject
    {
        public VersionInfo newestVersion;

        private static readonly string PREF_PREFIX = "memo-check-update-";

        [InitializeOnLoadMethod] 
        private static void Init()
        {
            Resources.Load<VersionChecker>("Memo/VersionChecker");
        }

        internal static bool CheckedToday()
        {
            string dateString = DateTime.Now.ToString("yyyy-MM-dd");
            return EditorPrefs.HasKey(PREF_PREFIX + dateString);
        }

        internal static void CheckForUpdate()
        {
            string dateString = DateTime.Now.ToString("yyyy-MM-dd");
            EditorPrefs.SetBool(PREF_PREFIX + dateString, true);
            NetUtils.GetVersionInfo((request, response) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (response.major > VersionInfo.current.major ||
                    response.minor > VersionInfo.current.minor ||
                    response.patch > VersionInfo.current.patch)
                    {
                        Debug.Log($"MEMO: New version {response.major}.{response.minor}.{response.patch} is available, please update the asset.");
                    }
                }
            });
        }

        private void OnEnable()
        {
            if (!CheckedToday())
            {
                CheckForUpdate();
            }
        }

        private void OnDisable()
        {

        }
    }
}
