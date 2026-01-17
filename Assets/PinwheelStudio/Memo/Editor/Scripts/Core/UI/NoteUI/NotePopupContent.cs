using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

namespace Pinwheel.Memo.UI
{
    public class NotePopupContent : PopupWindowContent
    {
        protected string m_noteId;
        protected Vector2 m_scrollPosDescription;
        public NoteUI noteUI { get; protected set; }

        public NotePopupContent(string noteId)
        {
            this.m_noteId = noteId;
            this.noteUI = new NoteUI(noteId);
        }

        public override void OnOpen()
        {
            base.OnOpen();
            noteUI.window = editorWindow;
            editorWindow.wantsMouseMove = true;
            NoteManager.instance.BeginNoteEditing(m_noteId);
        }

        public override void OnClose()
        {
            base.OnClose();
            noteUI.OnClosed();
            NoteManager.instance.EndNoteEditing(m_noteId);
        }

        public override Vector2 GetWindowSize()
        {
            return NoteStyles.windowSize;
        }

        public override void OnGUI(Rect rect)
        {            
            noteUI.OnGUI(rect);
        }
    }
}
