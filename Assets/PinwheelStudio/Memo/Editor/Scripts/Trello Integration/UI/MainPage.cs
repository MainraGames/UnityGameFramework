using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Pinwheel.Memo;
using Pinwheel.Memo.UI;

namespace Pinwheel.Memo.Trello.UI
{
    public class MainPage : TrelloPageBase
    {
        public bool m_isLoggingOut;

        public MainPage(IMultipageWindow window) : base(window) { }

        public override void DrawBody()
        {
            if (TrelloUser.me.syncCount == 0 && !TrelloUser.me.isGettingUserInfo)
            {
                TrelloUser.me.UpdateInfoLazy();
            }

            TrelloUI.DrawMemoTrelloBanner();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField($"Logged in as: <b>{TrelloUser.me.name}</b>", TrelloStyle.p1Centered);

            GUI.enabled = !m_isLoggingOut;
            if (GUILayout.Button("Log out and revoke access"))
            {
                m_isLoggingOut = true;
                TrelloIntegration.LogoutAndRevokeToken((error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogWarning($"Logout failed: {error}");
                        m_isLoggingOut = false;
                    }
                    else
                    {
                        hostWindow.PopAllPages();
                    }
                });
            }
            GUI.enabled = true;

            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }
}
