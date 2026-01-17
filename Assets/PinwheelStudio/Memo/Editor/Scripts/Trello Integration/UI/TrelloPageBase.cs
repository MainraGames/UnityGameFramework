using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Memo.UI;

namespace Pinwheel.Memo.Trello.UI
{
    public class TrelloPageBase : Page
    {
        public TrelloPageBase(IMultipageWindow hostWindow) : base(hostWindow) { }

        public override void DrawHeader()
        {
            //if (TrelloUser.me.syncCount == 0 && !TrelloUser.me.isGettingUserInfo)
            //{
            //    TrelloUser.me.UpdateInfoLazy();
            //}

            //Rect headerRect = EditorGUILayout.BeginHorizontal(Styles.headerContainer);

            //GUI.enabled = hostWindow.pageCount > 1;
            //if (GUILayout.Button("←", EditorStyles.toolbarButton))
            //{
            //    hostWindow.PopPage();
            //}
            //GUI.enabled = true;
            //EditorGUILayout.LabelField(this.GetType().Name, Styles.p1);

            //GUILayout.FlexibleSpace();
            //if (GUILayout.Button(TrelloUser.me.fullName, EditorStyles.toolbarButton))
            //{
            //    Application.OpenURL($"https://trello.com/u/{TrelloUser.me.name}/boards");
            //}
            //EditorGUILayout.EndHorizontal();
        }
    }
}
