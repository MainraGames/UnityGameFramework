using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Memo.Trello.UI;
using Pinwheel.Memo.UI;
using UnityEditor;
using UnityEngine.Networking;

namespace Pinwheel.Memo.Trello.UI
{
    public class AuthorizationPage : TrelloPageBase
    {
        protected string m_newAuthId;

        public AuthorizationPage(IMultipageWindow hostWindow) : base(hostWindow) { }

        public override void DrawHeader()
        {
        }

        public override void DrawBody()
        {
            if (TrelloIntegration.IsAuthorized())
            {
                hostWindow.PopPage();
                return;
            }

            TrelloUI.DrawMemoTrelloBanner();

            EditorGUILayout.LabelField("Please allow Memo to access your Trello workspaces, boards and cards. \nThis will be used to sync notes between devices and team members.", TrelloStyle.p1Centered);

            EditorGUILayout.GetControlRect(GUILayout.Height(32));
            if (string.IsNullOrEmpty(m_newAuthId))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUIContent buttonGUIContent = EditorGUIUtility.TrTextContent(" Continue on Browser → ", "https://trello.com/1/authorize");
                if (GUILayout.Button(buttonGUIContent, GUILayout.Height(32)))
                {
                    TrelloApi.AuthorizationOptions options = new TrelloApi.AuthorizationOptions();
                    options.expiration = TrelloApi.AuthorizationOptions.Expiration.Never;
                    m_newAuthId = TrelloApi.OpenBrowserForAuthorization(options);
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("Waiting for authorization...", TrelloStyle.p1Centered);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Cancel"))
                {
                    m_newAuthId = null;
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        public override void OnFocus()
        {
            base.OnFocus();
            if (!string.IsNullOrEmpty(m_newAuthId))
            {
                TrelloApi.GetUserTokenFromAuthId(m_newAuthId, (request, response) =>
                {
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        if (!string.IsNullOrEmpty(response.authId) &&
                            !string.IsNullOrEmpty(response.token))
                        {
                            hostWindow.PopPage();
                            TrelloApi.settings.authId = response.authId;
                            TrelloApi.settings.apiToken = response.token;
                            TrelloIntegration.NotifyAuthorizationResult(true);
                        }
                        else
                        {
                            Debug.Log($"Trello authorization unsuccessful: " +
                                $"{(!string.IsNullOrEmpty(response.error) ? response.error : "Authorization not completed")}");
                            TrelloApi.settings.authId = null;
                            TrelloApi.settings.apiToken = null;
                            TrelloIntegration.NotifyAuthorizationResult(false);
                        }
                        m_newAuthId = null;
                    }
                    else
                    {
                        Debug.Log("Error getting user token: " + request.error);
                        m_newAuthId = null;
                        TrelloApi.settings.authId = null;
                        TrelloApi.settings.apiToken = null;
                        TrelloIntegration.NotifyAuthorizationResult(false);
                    }
                    request.Dispose();
                });
            }
        }
    }
}
