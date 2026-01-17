using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Memo.UI;
using UnityEditor;

namespace Pinwheel.Memo.Trello.UI
{
    public static class TrelloUI
    {
        public static void DrawMemoTrelloBanner()
        {
            EditorGUILayout.GetControlRect(GUILayout.Height(32));
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Rect memoIconRect = EditorGUILayout.GetControlRect(GUILayout.Width(64), GUILayout.Height(64));
            Rect arrowsIconRect = EditorGUILayout.GetControlRect(GUILayout.Width(64), GUILayout.Height(64));
            Rect trelloIconRect = EditorGUILayout.GetControlRect(GUILayout.Width(64), GUILayout.Height(64));
            GUI.DrawTexture(memoIconRect, Icons.NOTE_GIZMO);
            GUI.DrawTexture(arrowsIconRect, Icons.EXCHANGE);
            GUI.DrawTexture(trelloIconRect, Icons.TRELLO);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.Height(32));
        }
    }
}
