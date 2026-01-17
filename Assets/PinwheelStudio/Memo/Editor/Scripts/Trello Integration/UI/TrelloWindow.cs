using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Networking;
using System;
using Pinwheel.Memo.UI;

namespace Pinwheel.Memo.Trello.UI
{
    public class TrelloWindow : MultipageEditorWindow
    {       
        [MenuItem("Window/Memo/Trello Integration")]
        public static void ShowWindow()
        {
            TrelloWindow window = GetWindow<TrelloWindow>();
            window.titleContent = new GUIContent("Trello Integration");
            window.Show();
        }

        public void OnGUI()
        {
            if (m_pageStack.Count == 0)
            {
                PushPage(new MainPage(this));
            }

            if (!TrelloIntegration.IsAuthorized())
            {
                Page topPage = m_pageStack.Peek();
                if (!(topPage is AuthorizationPage))
                {
                    PushPage(new AuthorizationPage(this));
                }
            }

            Page currentPage = null;
            if (m_pageStack.TryPeek(out currentPage))
            {
                currentPage.DrawHeader();
                currentPage.DrawBody();
            }
            else
            {
                EditorGUILayout.LabelField("Error: no page");
            }
        }        
    }
}
