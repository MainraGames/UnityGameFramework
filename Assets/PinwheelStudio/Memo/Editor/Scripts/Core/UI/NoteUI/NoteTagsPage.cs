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
    public class NoteTagsPage : NoteUIPage
    {
        private const string TAG_NAME_PLACEHOLDER = "Add tag name";

        public NoteTagsPage(IMultipageWindow window, Note note) : base(window, note)
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
            NoteUtils.CleanUpTags(note);
            if (!string.IsNullOrEmpty(TrelloIntegration.GetDefaultBoard()) && TrelloIntegration.IsAuthorized())
            {
                TrelloIntegration.PushLabelsToDefaultBoard(true);
            }
        }

        private float m_addedTagsAreaHeight = EditorGUIUtility.singleLineHeight;
        private float m_availableTagsAreaHeight = EditorGUIUtility.singleLineHeight;

        public override void DrawBody()
        {
            EditorGUILayout.BeginVertical(NoteStyles.noteBody);
            DrawNoteNameAndBackButton();

            EditorGUILayout.LabelField("Added Tags", NoteStyles.h3);
            List<Tag> addedTags = new List<Tag>();
            foreach (string idTag in note.idTags)
            {
                Tag tag = NoteManager.instance.GetTagById(idTag);
                if (tag != null && !addedTags.Contains(tag))
                {
                    addedTags.Add(tag);
                }
            }

            if (addedTags.Count > 0)
            {
                Rect tagsAreaRect = EditorGUILayout.BeginVertical();
                List<string> tagNames = addedTags.Where(t => !t.isDeleted).Select(t => t.name).ToList();
                List<Rect> tagRects = EditorGUIUtility.GetFlowLayoutedRects(tagsAreaRect, NoteStyles.tagBody, 2, 2, tagNames);
                for (int i = 0; i < tagNames.Count; ++i)
                {
                    Rect rect = tagRects[i];
                    Tag tag = addedTags[i];
                    if (ButtonTag(rect, tag))
                    {
                        NoteManager.instance.SetDirty();
                        NoteManager.instance.RecordUndo("Remove tag from note");
                        note.idTags.RemoveAll(tagId => string.Equals(tagId, tag.id));
                    }
                }
                if (Event.current.type == EventType.Repaint)
                {
                    Rect lastRect = tagRects[^1];
                    Rect firstRect = tagRects[0];
                    m_addedTagsAreaHeight = lastRect.yMax - firstRect.yMin;
                }
                EditorGUILayout.GetControlRect(GUILayout.Height(m_addedTagsAreaHeight));
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.LabelField("No tag", NoteStyles.p2);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Available Tags", NoteStyles.h3);
            List<Tag> availableTags =
                NoteManager.instance.GetTags()
                .Where((t) => !note.idTags.Contains(t.id))
                .ToList();
            if (availableTags.Count > 0)
            {
                Rect tagsAreaRect = EditorGUILayout.BeginVertical();
                List<string> tagNames = availableTags.Where(t => !t.isDeleted).Select(t => t.name).ToList();
                List<Rect> tagRects = EditorGUIUtility.GetFlowLayoutedRects(tagsAreaRect, NoteStyles.tagBody, 2, 2, tagNames);
                for (int i = 0; i < tagNames.Count; ++i)
                {
                    Rect rect = tagRects[i];
                    Tag tag = availableTags[i];
                    if (ButtonTag(rect, tag))
                    {
                        if (!note.idTags.Contains(tag.id))
                        {
                            NoteManager.instance.SetDirty();
                            NoteManager.instance.RecordUndo("Add tag to note");
                            note.idTags.Add(tag.id);
                        }
                    }
                }
                if (Event.current.type == EventType.Repaint)
                {
                    Rect lastRect = tagRects[^1];
                    Rect firstRect = tagRects[0];
                    m_availableTagsAreaHeight = lastRect.yMax - firstRect.yMin;
                }
                EditorGUILayout.GetControlRect(GUILayout.Height(m_availableTagsAreaHeight));
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
            if (NoteUI.ButtonMini("Manage Tags"))
            {
                hostWindow.PushPage(new ManageTagsPage(hostWindow, note));
            }

            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.MouseDown)
            {
                GUI.FocusControl(string.Empty);
            }
        }

        public static bool ButtonTag(Rect rect, Tag tag)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            EditorGUI.DrawRect(rect, NoteStyles.GetTagBackgroundColor(tag.color));

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
