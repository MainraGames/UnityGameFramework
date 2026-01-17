using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Memo.Trello.UI;
using Pinwheel.Memo.UI;

namespace Pinwheel.Memo.Trello.UI
{
    public class BoardPage : TrelloPageBase
    {
        private string m_boardId;
        private Vector2 m_scrollPos;

        public BoardPage(IMultipageWindow window, string boardId) : base(window)
        {
            m_boardId = boardId;
        }

        public override void OnPushed()
        {
            base.OnPushed();
            //TrelloBoard board = TrelloUser.me.GetOpenBoardById(m_boardId);
            //if (board != null)
            //{
            //    TrelloUser.me.PullListsInBoardLazy(board.id);
            //    TrelloUser.me.PullCardsInBoardLazy(board.id);
            //}

            hostWindow.window.wantsMouseMove = true;
        }

        public override void OnPopped()
        {
            base.OnPopped();
            hostWindow.window.wantsMouseMove = false;
        }

        public override void DrawBody()
        {
            //EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            //TrelloBoard board = TrelloUser.me.GetOpenBoardById(m_boardId);
            //if (board == null)
            //{
            //    EditorGUILayout.LabelField("Board not found");
            //    return;
            //}

            //EditorGUILayout.LabelField(board.name, Styles.h2);
            //if (!string.IsNullOrEmpty(board.description))
            //{
            //    EditorGUILayout.LabelField(board.description, Styles.p1);
            //}
            //EditorGUILayout.EndVertical();

            //m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
            //EditorGUILayout.BeginHorizontal(EditorStyles.inspectorDefaultMargins);
            //List<TrelloList> lists = TrelloUser.me.GetOpenLists(board.id);
            //foreach (TrelloList list in lists)
            //{
            //    Rect listRect = EditorGUILayout.BeginVertical(TrelloStyle.listBody, GUILayout.Width(256));
            //    EditorGUI.DrawRect(listRect, TrelloStyle.listBodyBackgroundColor);
            //    EditorGUILayout.BeginVertical(TrelloStyle.listNameContainer);
            //    EditorGUILayout.LabelField(list.name, TrelloStyle.listName);
            //    EditorGUILayout.EndVertical();
            //    List<TrelloCard> cards = TrelloUser.me.GetOpenCardsInList(list.id);
            //    foreach (TrelloCard card in cards)
            //    {
            //        Rect cardRect = EditorGUILayout.BeginVertical(TrelloStyle.cardBody);
            //        EditorGUI.DrawRect(cardRect, TrelloStyle.cardBodyBackgroundColor);
            //        EditorGUILayout.LabelField(card.name, Styles.p1);
            //        if (!string.IsNullOrEmpty(card.description))
            //        {
            //            EditorGUILayout.LabelField("...", Styles.p1);
            //        }
            //        EditorGUILayout.EndVertical();

            //        if (cardRect.Contains(Event.current.mousePosition))
            //        {
            //            GUIUtils.DrawOutline(cardRect, TrelloStyle.cardHoverOutlineColor);
            //        }
            //    }

            //    EditorGUILayout.EndVertical();
            //}
            //EditorGUILayout.EndHorizontal();
            //GUILayout.FlexibleSpace();
            //EditorGUILayout.EndScrollView();

            //if (Event.current.isMouse)
            //{
            //    hostWindow.window.Repaint();
            //}
        }
    }
}
