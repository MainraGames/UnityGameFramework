using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;

namespace Pinwheel.Memo.UI
{
    public class NoteUIPage : Page
    {
        public Note note { get; private set; }
        public NoteUIPage(IMultipageWindow window, Note note) : base(window)
        {
            this.note = note;
        }

        protected float m_labelWithBackButtonWidth;
        public void DrawNoteNameAndBackButton()
        {
            EditorGUILayout.BeginHorizontal();
            if (NoteUI.Button3("←"))
            {
                hostWindow.PopPage();
            }
            Rect labelRect = EditorGUILayout.BeginVertical();
            if (Event.current.type == EventType.Repaint)
            {
                m_labelWithBackButtonWidth = labelRect.width;
            }
            Vector2 labelSize = NoteStyles.noteNameNoWrap.CalcSize(EditorGUIUtility.TrTempContent(note.name));
            float ellipsisRatio = m_labelWithBackButtonWidth / labelSize.x;
            ellipsisRatio = Mathf.Floor(ellipsisRatio * 100f) / 100f;
            int charCount = Mathf.Clamp((int)(note.name.Length * ellipsisRatio) - 3, 0, note.name.Length);
            if (charCount == note.name.Length)
            {
                EditorGUILayout.LabelField(note.name, NoteStyles.noteNameNoWrap);
            }
            else
            {
                EditorGUILayout.LabelField(note.name.Substring(0, charCount) + "…", NoteStyles.noteNameNoWrap);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
    }
}
