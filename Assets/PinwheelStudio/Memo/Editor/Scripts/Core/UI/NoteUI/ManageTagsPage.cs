using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Memo.UI;
using Pinwheel.Memo.Trello;
using Pinwheel.Memo.Trello.UI;
using System.Linq;

namespace Pinwheel.Memo.UI
{
    public class ManageTagsPage : NoteUIPage
    {
        protected Vector2 m_scrollPos;

        public ManageTagsPage(IMultipageWindow window, Note note) : base(window, note)
        {
        }

        public override void OnPushed()
        {
            base.OnPushed();
            if (!string.IsNullOrEmpty(TrelloIntegration.GetDefaultBoard()) && TrelloIntegration.IsAuthorized())
            {
                TrelloIntegration.PullLabelsOfDefaultBoard();
            }
        }

        public override void OnPopped()
        {
            base.OnPopped();
            NoteManager.instance.CleanUpTags();
            if (!string.IsNullOrEmpty(TrelloIntegration.GetDefaultBoard()) && TrelloIntegration.IsAuthorized())
            {
                TrelloIntegration.PushLabelsToDefaultBoard();
            }
        }

        public override void DrawBody()
        {
            EditorGUILayout.BeginVertical(NoteStyles.noteBody);
            DrawNoteNameAndBackButton();

            EditorGUILayout.LabelField("Manage Tags", NoteStyles.h3);
            EditorGUILayout.EndVertical();

            m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, NoteStyles.noteContentScrollView);
            List<Tag> tags = NoteManager.instance.GetTags().Where(t => !t.isDeleted).ToList();
            Tag tobeDeletedTag = null;
            foreach (Tag t in tags)
            {
                Rect tagRect = EditorGUILayout.BeginHorizontal();
                EditorGUI.DrawRect(tagRect, NoteStyles.GetTagBackgroundColor(t.color));
                if (NoteUI.ButtonIcon(Icons.COLORS))
                {
                    GenericMenu colorMenu = new GenericMenu();
                    foreach (Colors c in System.Enum.GetValues(typeof(Colors)))
                    {
                        colorMenu.AddItem(
                            new GUIContent(c.ToString()),
                            false,
                            () =>
                            {
                                NoteManager.instance.SetDirty();
                                NoteManager.instance.RecordUndo("Edit tag");
                                t.color = c;
                            });
                    }
                    colorMenu.ShowAsContext();
                }
                EditorGUI.BeginChangeCheck();
                string tagName = NoteUI.TextboxWithPlaceholder("Add tag name", t.name, NoteStyles.placeholderTextP1, NoteStyles.tagBody);
                if (EditorGUI.EndChangeCheck())
                {
                    NoteManager.instance.SetDirty();
                    NoteManager.instance.RecordUndo("Edit tag");
                    t.name = tagName;
                }

                if (NoteUI.ButtonIcon(Icons.X))
                {
                    tobeDeletedTag = t;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (tobeDeletedTag != null)
            {
                NoteManager.instance.SetDirty();
                NoteManager.instance.RecordUndo("Delete tag");
                NoteManager.instance.RemoveTag(tobeDeletedTag.id);
                tobeDeletedTag = null;
            }

            if (NoteUI.ButtonMini("Add Tag"))
            {
                NoteManager.instance.SetDirty();
                NoteManager.instance.RecordUndo("Add tag");
                NoteManager.instance.AddTag(Colors.Gray, "");
            }

            EditorGUILayout.EndScrollView();
            if (Event.current.type == EventType.MouseDown)
            {
                GUI.FocusControl(string.Empty);
            }
        }


        public static bool ButtonTag(Rect rect, Tag tag)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            EditorGUI.DrawRect(rect, NoteStyles.GetNoteBackgroundColor(tag.color));

            const string PREF_BUTTON_ACTIVE_RECT = "button-active-rect";
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                SessionState.SetString(PREF_BUTTON_ACTIVE_RECT, rect.ToString());
            }
            if (Event.current.type == EventType.MouseUp)
            {
                SessionState.SetString(PREF_BUTTON_ACTIVE_RECT, null);
            }
            if (rect.Contains(Event.current.mousePosition))
            {
                string activeRect = SessionState.GetString(PREF_BUTTON_ACTIVE_RECT, null);
                if (string.Equals(rect.ToString(), activeRect))
                {
                    EditorGUI.DrawRect(rect, NoteStyles.colorButtonPressed);
                }
                else
                {
                    EditorGUI.DrawRect(rect, NoteStyles.colorButtonHover);
                }
            }

            bool clicked = GUI.Button(rect, tag.name, NoteStyles.tagBody);

            return clicked;
        }
    }
}
