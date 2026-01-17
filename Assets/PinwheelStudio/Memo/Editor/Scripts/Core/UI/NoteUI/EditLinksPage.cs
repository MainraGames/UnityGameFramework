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
    public class EditLinksPage : NoteUIPage
    {
        protected Vector2 m_scrollPos;

        public EditLinksPage(IMultipageWindow window, Note note) : base(window, note)
        {
        }

        public override void OnPushed()
        {
            base.OnPushed();
        }

        public override void OnPopped()
        {
            base.OnPopped();
            note.links.RemoveAll(l => string.IsNullOrEmpty(l.url) && string.IsNullOrEmpty(l.idRemote) && l.isDeleted);
        }

        public override void DrawBody()
        {
            EditorGUILayout.BeginVertical(NoteStyles.noteBody);
            DrawNoteNameAndBackButton();

            EditorGUILayout.LabelField("Edit Links", NoteStyles.h3);
            EditorGUILayout.EndVertical();

            m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, NoteStyles.noteContentScrollView);
            List<Link> links = note.links.Where(l => !l.isDeleted).ToList();
            Link deletedLink = null;
            for (int i = 0; i < links.Count; ++i)
            {
                Rect linkRect = EditorGUILayout.BeginVertical(NoteStyles.noteBody);
                GUIUtils.DrawOutline(linkRect, NoteStyles.colorOutlines);
                Link link = links[i];

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                string url = NoteUI.TextboxWithPlaceholder("Add URL", link.url, NoteStyles.placeholderTextP1, NoteStyles.buttonLink);
                string displayText = NoteUI.TextboxWithPlaceholder("Add display text (optional)", link.displayText, NoteStyles.placeholderTextP1, NoteStyles.p1);
                EditorGUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                {
                    NoteManager.instance.SetDirty();
                    NoteManager.instance.RecordUndo("Edit link");
                    link.url = url;
                    link.displayText = displayText;
                }
                if (NoteUI.ButtonIcon(Icons.X))
                {
                    deletedLink = link;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            if (deletedLink != null)
            {
                NoteManager.instance.SetDirty();
                NoteManager.instance.RecordUndo("Delete link");
                deletedLink.isDeleted = true;
            }

            if (NoteUI.ButtonMini("Add Link"))
            {
                NoteManager.instance.SetDirty();
                NoteManager.instance.RecordUndo("Add link");
                note.links.Add(new Link());
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
