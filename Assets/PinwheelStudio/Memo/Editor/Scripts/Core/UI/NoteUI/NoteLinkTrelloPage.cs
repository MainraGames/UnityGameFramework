using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Memo.UI;
using Pinwheel.Memo.Trello;
using Pinwheel.Memo.Trello.UI;

namespace Pinwheel.Memo.UI
{
    public class NoteLinkTrelloPage : NoteUIPage
    {
        protected string m_selectedListId;
        protected string m_selectedCardId;
        protected string m_searchText;
        protected Vector2 m_cardListScrollPos;

        public NoteLinkTrelloPage(IMultipageWindow window, Note note) : base(window, note)
        {
            m_searchText = null;
        }

        public override void OnPushed()
        {
            base.OnPushed();
            m_searchText = null;

            if (TrelloApi.settings.IsAuthorized())
            {
                string defaultBoardId = TrelloIntegration.GetDefaultBoard();
                TrelloApi.Board board = TrelloUser.me.GetOpenBoardById(defaultBoardId);
                if (board != null)
                {
                    TrelloUser.me.PullListsInBoardLazy(board.id);
                    TrelloUser.me.PullCardsInBoardLazy(board.id);
                }
            }
        }

        public override void DrawBody()
        {
            EditorGUILayout.BeginVertical(NoteStyles.noteBody);
            DrawNoteNameAndBackButton();

            if (!TrelloApi.settings.IsAuthorized())
            {
                EditorGUILayout.LabelField("Waiting for authorization...", NoteStyles.p1);
                TrelloWindow trelloWindow = EditorWindow.GetWindow<TrelloWindow>();
                trelloWindow.Show();
            }
            else
            {
                if (TrelloUser.me.syncCount == 0 && !TrelloUser.me.isGettingUserInfo)
                {
                    TrelloUser.me.UpdateInfoLazy();
                }

                string selectedOrgId = TrelloIntegration.GetDefaultOrganization();
                TrelloApi.Organization organization = TrelloUser.me.GetOrganizationById(selectedOrgId);
                if (organization == null)
                {
                    EditorGUILayout.LabelField("Workspace", NoteStyles.h3);
                    EditorGUILayout.LabelField("Select a workspace to link notes in this project.", NoteStyles.p1);
                    Rect selectWorkspaceRect = EditorGUILayout.BeginVertical(NoteStyles.noteBody);
                    GUIUtils.DrawOutline(selectWorkspaceRect, NoteStyles.colorOutlines);
                    foreach (TrelloApi.Organization org in TrelloUser.me.GetOrganizations())
                    {
                        if (NoteUI.Button3(org.displayName))
                        {
                            TrelloIntegration.SetDefaultOrganization(org.id);
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUIContent workspaceGUIContent = new GUIContent($"<b>Workspace:</b> ");
                    Vector2 workspaceRectSize = NoteStyles.p1.CalcSize(workspaceGUIContent);
                    EditorGUILayout.LabelField(workspaceGUIContent, NoteStyles.p1, GUILayout.Width(workspaceRectSize.x));
                    if (NoteUI.Button3($"{organization.displayName}"))
                    {
                        if (EditorUtility.DisplayDialog("Link project to other workspace", $"This will unlink all notes from the current workspace/board. Process with caution!",
                                   "Unlink notes and change workspace", "Cancel"))
                        {
                            int unlinkCount = NoteUtils.UnlinkNotesFromTrelloCard();
                            Debug.Log($"Unlink {unlinkCount} note(s) from Trello");
                            TrelloIntegration.SetDefaultOrganization(string.Empty);
                            TrelloIntegration.SetDefaultBoard(string.Empty);
                        }
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }

                string selectedBoardId = TrelloIntegration.GetDefaultBoard();
                TrelloApi.Board board = TrelloUser.me.GetOpenBoardById(selectedBoardId);
                if (organization != null)
                {
                    if (board != null && !string.Equals(board.idOrganization, organization.id))
                    {
                        //Selected board is not belong to the selected organization, reset the board selection
                        TrelloIntegration.SetDefaultBoard(string.Empty);
                        board = null;
                    }

                    if (board == null)
                    {
                        EditorGUILayout.LabelField("Board", NoteStyles.h3);
                        EditorGUILayout.LabelField("Select a board to link notes in this project. Notes can only be linked to cards of the same board.", NoteStyles.p1);
                        Rect selectBoardRect = EditorGUILayout.BeginVertical(NoteStyles.noteBody);
                        GUIUtils.DrawOutline(selectBoardRect, NoteStyles.colorOutlines);
                        List<TrelloApi.Board> boards = TrelloUser.me.GetOpenBoards(organization.id);
                        if (boards.Count == 0)
                        {
                            EditorGUILayout.LabelField("No board.", NoteStyles.p1);
                        }
                        foreach (TrelloApi.Board b in boards)
                        {
                            if (NoteUI.Button3(b.name))
                            {
                                TrelloIntegration.SetDefaultBoard(b.id);
                                TrelloUser.me.PullListsInBoardLazy(b.id);
                                TrelloUser.me.PullCardsInBoardLazy(b.id);
                                Utilities.IncreaseObjectSyncCount(b.id);
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUIContent boardGUIContent = new GUIContent($"<b>Board:</b> ");
                        Vector2 boardRectSize = NoteStyles.p1.CalcSize(boardGUIContent);
                        EditorGUILayout.LabelField(boardGUIContent, NoteStyles.p1, GUILayout.Width(boardRectSize.x));
                        if (NoteUI.Button3($"{board.name}"))
                        {
                            if (EditorUtility.DisplayDialog("Link project to other board", $"This will unlink all notes from the current board {board.name}. Process with caution!",
                                "Unlink notes and change board", "Cancel"))
                            {
                                int unlinkCount = NoteUtils.UnlinkNotesFromTrelloCard();
                                Debug.Log($"Unlink {unlinkCount} note(s) from Trello");
                                TrelloIntegration.SetDefaultBoard(string.Empty);
                            }
                        }

                        if (Utilities.GetObjectSyncCount(board.id) == 0)
                        {
                            TrelloUser.me.PullListsInBoardLazy(board.id);
                            TrelloUser.me.PullCardsInBoardLazy(board.id);
                            Utilities.IncreaseObjectSyncCount(board.id);
                        }

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                    }
                }

                TrelloApi.Card card = TrelloUser.me.GetCardById(m_selectedCardId);
                if (organization != null &&
                    board != null)
                {
                    if (card != null && card.idBoard != board.id)
                    {
                        //selected card is not belong to selected board, reset selection
                        m_selectedCardId = null;
                        card = null;
                    }

                    if (string.IsNullOrEmpty(m_selectedCardId) &&
                        string.IsNullOrEmpty(m_selectedListId))
                    {
                        EditorGUILayout.LabelField("Card:", NoteStyles.h3);
                        Rect searchBoxRect = EditorGUILayout.GetControlRect();
                        m_searchText = EditorGUI.TextField(searchBoxRect, m_searchText, NoteStyles.p1);
                        if (string.IsNullOrEmpty(m_searchText))
                        {
                            EditorGUI.LabelField(searchBoxRect, "Search", NoteStyles.placeholderTextP1);
                        }
                        GUIUtils.DrawOutline(searchBoxRect, NoteStyles.colorOutlines);

                        Rect selectCardRect = EditorGUILayout.BeginVertical();
                        GUIUtils.DrawOutline(selectCardRect, NoteStyles.colorOutlines);
                        m_cardListScrollPos = EditorGUILayout.BeginScrollView(m_cardListScrollPos);
                        List<TrelloApi.List> lists = TrelloUser.me.GetOpenLists(board.id);
                        foreach (TrelloApi.List l in lists)
                        {
                            List<TrelloApi.Card> cards = TrelloUser.me.GetOpenCardsInList(l.id);
                            if (!string.IsNullOrEmpty(m_searchText))
                            {
                                cards.RemoveAll(c => !c.name.Contains(m_searchText, System.StringComparison.InvariantCultureIgnoreCase));
                            }

                            //if (cards.Count > 0)
                            {
                                Rect listNameRect = EditorGUILayout.BeginVertical();
                                EditorGUI.DrawRect(listNameRect, NoteStyles.colorOutlines);
                                EditorGUILayout.LabelField(l.name, NoteStyles.p1);
                                EditorGUILayout.EndVertical();
                            }
                            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                            foreach (TrelloApi.Card c in cards)
                            {
                                if (NoteUI.Button3(c.name, false))
                                {
                                    m_selectedCardId = c.id;
                                    m_selectedListId = null;
                                }
                            }
                            if (NoteUI.Button3("<i>+ New card</i>", false))
                            {
                                m_selectedCardId = null;
                                m_selectedListId = l.id;
                            }
                            EditorGUILayout.EndVertical();

                        }
                        EditorGUILayout.EndScrollView();
                        EditorGUILayout.EndVertical();
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUIContent cardGUIContent = new GUIContent($"<b>Card:</b> ");
                        Vector2 cardRectSize = NoteStyles.p1.CalcSize(cardGUIContent);
                        EditorGUILayout.LabelField(cardGUIContent, NoteStyles.p1, GUILayout.Width(cardRectSize.x));
                        string cardSelectionText = string.Empty;
                        if (!string.IsNullOrEmpty(m_selectedCardId))
                        {
                            cardSelectionText = TrelloUser.me.GetCardById(m_selectedCardId).name;
                        }
                        else if (m_selectedListId != null)
                        {
                            cardSelectionText = $"New card in '{TrelloUser.me.GetListById(m_selectedListId).name}'";
                        }

                        if (NoteUI.Button3(cardSelectionText))
                        {
                            m_selectedCardId = null;
                            m_searchText = null;
                        }
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                    }
                }

                if (organization != null && board != null)
                {
                    if (!string.IsNullOrEmpty(m_selectedCardId))
                    {
                        if (NoteUI.Button1("Link Card"))
                        {
                            Undo.RecordObject(NoteManager.instance, "Link note to Trello card");
                            EditorUtility.SetDirty(NoteManager.instance);

                            TrelloLinkage trelloLinkage = new TrelloLinkage()
                            {
                                idCard = m_selectedCardId
                            };
                            note.linkage.Set(trelloLinkage);
                            hostWindow.PopPage();
                        }
                    }
                    else if (!string.IsNullOrEmpty(m_selectedListId))
                    {
                        if (NoteUI.Button1("Create Card"))
                        {
                            Undo.RecordObject(NoteManager.instance, "Link note to new Trello card");
                            EditorUtility.SetDirty(NoteManager.instance);

                            TrelloLinkage trelloLinkage = new TrelloLinkage()
                            {
                                idList = m_selectedListId
                            };
                            note.linkage.Set(trelloLinkage);
                            hostWindow.PopPage();
                        }
                    }
                }

            }
            EditorGUILayout.EndVertical();
        }
    }
}
