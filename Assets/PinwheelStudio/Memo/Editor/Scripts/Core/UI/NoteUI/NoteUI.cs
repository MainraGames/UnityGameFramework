using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Memo.Trello.UI;
using System;
using Pinwheel.Memo.Trello;
using System.Text;

namespace Pinwheel.Memo.UI
{
    public class NoteUI : IMultipageWindow
    {
        public string noteId { get; protected set; }

        private EditorWindow m_hostWindow;
        public EditorWindow window
        {
            get
            {
                return m_hostWindow;
            }
            set
            {
                m_hostWindow = value;
            }
        }

        public int pageCount => m_pageStack.Count;
        protected Stack<Page> m_pageStack = new Stack<Page>();

        public void PushPage(Page page)
        {
            if (this != page.hostWindow)
                throw new ArgumentException("Invalid page host window");
            m_pageStack.Push(page);
            page.OnPushed();
        }

        public void PopPage()
        {
            Page page = m_pageStack.Pop();
            page.OnPopped();
        }

        public NoteUI(string noteId)
        {
            this.noteId = noteId;
        }

        public void OnGUI(Rect rect)
        {
            Note note = NoteManager.instance.GetNoteById(noteId);
            if (note == null)
            {
                EditorGUI.DrawRect(rect, NoteStyles.GetNoteBackgroundColor(Colors.Yellow));
                return;
            }

            EditorGUI.DrawRect(rect, NoteStyles.GetNoteBackgroundColor(note.color));
            DrawHeaderBar(note);
            DrawBody(note);

            if (Event.current.type == EventType.MouseMove)
            {
                window?.Repaint();
            }
            GUI.skin = null;
        }

        public void OnClosed()
        {
            while (pageCount > 0)
            {
                PopPage();
            }
        }

        private void DrawHeaderBar(Note note)
        {
            Rect topHandleRect = EditorGUILayout.BeginHorizontal(NoteStyles.topHandles);
            EditorGUI.DrawRect(topHandleRect, NoteStyles.colorTopHandles);

            Rect draggerRect = EditorGUILayout.GetControlRect();
            EditorGUIUtility.AddCursorRect(draggerRect, MouseCursor.MoveArrow);
            HandleDragging(draggerRect);

            EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(0), GUILayout.Width(10));
            StringBuilder trelloButtonTooltip = new StringBuilder();
            bool isNoteLinkedToTrelloCard = NoteUtils.IsNoteConnectedToTrelloCard(note);
            bool isTrelloAuthorized = TrelloIntegration.IsAuthorized();
            trelloButtonTooltip.Append(isNoteLinkedToTrelloCard ?
                "Note linked to Trello card." :
                "Link note to Trello card.");
            if (isNoteLinkedToTrelloCard && !isTrelloAuthorized)
            {
                trelloButtonTooltip.Append("\nTrello access not authorized or token has expired.");
            }
            EditorGUILayout.BeginVertical();
            if (ButtonIcon(isNoteLinkedToTrelloCard ? Icons.TRELLO : Icons.TRELLO_GRAY, trelloButtonTooltip.ToString()))
            {
                ShowTrelloIntegrationContextMenu(this, note);
            }
            EditorGUILayout.EndVertical();

            if (ButtonIcon(Icons.COLORS, "Change note color"))
            {
                ShowNoteColorContextMenu(this, note);
            }

            if (ButtonIcon(Icons.DISCORD, "Send feedback on Discord"))
            {
                Application.OpenURL("https://discord.gg/btX4pdmZdV");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
        }

        private static void ShowTrelloIntegrationContextMenu(NoteUI caller, Note note)
        {
            bool isNoteConnectedToTrelloCard = NoteUtils.IsNoteConnectedToTrelloCard(note);
            GenericMenu menu = new GenericMenu();
            if (isNoteConnectedToTrelloCard)
            {
                menu.AddItem(
                    new GUIContent("↓ Pull from Trello"),
                    false,
                    () =>
                    {
                        if (!TrelloIntegration.IsAuthorized())
                        {
                            TrelloIntegration.StartAuthorizationFlow();
                        }
                        else
                        {
                            note.PullFromRemote();
                        }
                    });
                menu.AddItem(
                    new GUIContent("↑ Push to Trello"),
                    false,
                    () =>
                    {
                        if (!TrelloIntegration.IsAuthorized())
                        {
                            TrelloIntegration.StartAuthorizationFlow();
                        }
                        else
                        {
                            note.PushToRemote();
                        }
                    });
                menu.AddItem(
                    new GUIContent("View on Trello"),
                    false,
                    () =>
                    {
                        string url = TrelloIntegration.GetCardUrl(note);
                        Application.OpenURL(url);
                    });
                menu.AddSeparator(string.Empty);
                menu.AddItem(
                    new GUIContent("Unlink"),
                    false,
                    () =>
                    {
                        note.linkage = null;
                    });
            }
            else
            {
                menu.AddItem(
                    new GUIContent("Link To Trello Card"),
                    false,
                    () =>
                    {
                        NoteLinkTrelloPage noteLinkTrelloPage = new NoteLinkTrelloPage(caller, note);
                        caller.PushPage(noteLinkTrelloPage);
                    });
            }

            menu.ShowAsContext();
        }

        private static void ShowNoteColorContextMenu(NoteUI caller, Note note)
        {
            GenericMenu menu = new GenericMenu();
            foreach (Colors c in Enum.GetValues(typeof(Colors)))
            {
                menu.AddItem(
                    new GUIContent(c.ToString()),
                    note.color == c,
                    () => { note.color = c; });
            }

            menu.ShowAsContext();
        }

        private bool m_isDraggingWindow;
        private Vector2 m_grabPositionRelative;
        private void HandleDragging(Rect draggerRect)
        {
            if (window == null)
                return;
            if (Event.current.type == EventType.MouseDown &&
                draggerRect.Contains(Event.current.mousePosition))
            {
                m_isDraggingWindow = true;
                m_grabPositionRelative = Event.current.mousePosition;
                Event.current.Use();
            }
            if (Event.current.type == EventType.MouseDrag
                || Event.current.type == EventType.MouseLeaveWindow)
            {
                if (m_isDraggingWindow)
                {
                    Rect windowRect = window.position;
                    Vector2 mouseScreenPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                    windowRect.position = mouseScreenPos - m_grabPositionRelative;
                    window.position = windowRect;
                }
            }
            if (Event.current.type == EventType.MouseUp)
            {
                m_isDraggingWindow = false;
            }
        }

        private void DrawBody(Note note)
        {
            if (m_pageStack.Count == 0)
            {
                NoteContentPage noteContentPage = new NoteContentPage(this, note);
                PushPage(noteContentPage);
            }

            Page page;
            if (m_pageStack.TryPeek(out page))
            {
                page.DrawHeader();
                page.DrawBody();
            }
        }

        public static bool Button1(string text, bool fitText = true)
        {
            Rect rect;
            if (fitText)
            {
                Vector2 controlSize = NoteStyles.button1.CalcSize(new GUIContent(text));
                rect = EditorGUILayout.GetControlRect(GUILayout.Width(controlSize.x), GUILayout.Height(controlSize.y));
            }
            else
            {
                rect = EditorGUILayout.GetControlRect();
            }

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

            bool clicked = GUI.Button(rect, text, NoteStyles.button1);

            GUIUtils.DrawOutline(rect, NoteStyles.button1.normal.textColor);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            return clicked;
        }

        public static bool Button2(string text, bool fitText = true)
        {
            Rect rect;
            if (fitText)
            {
                Vector2 controlSize = NoteStyles.button2.CalcSize(new GUIContent(text));
                rect = EditorGUILayout.GetControlRect(GUILayout.Width(controlSize.x), GUILayout.Height(controlSize.y));
            }
            else
            {
                rect = EditorGUILayout.GetControlRect();
            }

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

            bool clicked = GUI.Button(rect, text, NoteStyles.button2);

            GUIUtils.DrawBottomLineForRect(rect, NoteStyles.button2.normal.textColor);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            return clicked;
        }

        public static bool Button3(string text, bool fitText = true, GUIStyle style = null)
        {
            return Button3(EditorGUIUtility.TrTempContent(text), fitText, style);
        }

        public static bool Button3(GUIContent text, bool fitText = true, GUIStyle style = null, float maxWidth = -1)
        {
            if (style == null)
                style = NoteStyles.button3;

            Rect rect;
            if (fitText)
            {
                Vector2 controlSize = style.CalcSize(text);
                rect = maxWidth > 0 ?
                    EditorGUILayout.GetControlRect(GUILayout.Width(controlSize.x), GUILayout.Height(controlSize.y), GUILayout.MaxWidth(maxWidth)) :
                    EditorGUILayout.GetControlRect(GUILayout.Width(controlSize.x), GUILayout.Height(controlSize.y));
            }
            else
            {
                rect = maxWidth > 0 ?
                    EditorGUILayout.GetControlRect(GUILayout.MaxWidth(maxWidth)) :
                    EditorGUILayout.GetControlRect();
            }

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

            bool clicked = GUI.Button(rect, text, style);

            return clicked;
        }

        public static bool Button3(Rect rect, string text, GUIStyle style = null)
        {
            if (style == null)
                style = NoteStyles.button3;

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

            bool clicked = GUI.Button(rect, text, style);

            return clicked;
        }

        public static bool ButtonMini(string text, bool fitText = true)
        {
            Rect rect;
            if (fitText)
            {
                Vector2 controlSize = NoteStyles.buttonMini.CalcSize(new GUIContent(text));
                rect = EditorGUILayout.GetControlRect(GUILayout.Width(controlSize.x), GUILayout.Height(controlSize.y));
            }
            else
            {
                rect = EditorGUILayout.GetControlRect();
            }

            bool clicked = ButtonMini(rect, text);

            return clicked;
        }

        public static bool ButtonMini(Rect rect, string text, GUIStyle style = null)
        {
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

            if (style == null)
                style = NoteStyles.buttonMini;
            bool clicked = GUI.Button(rect, text, style);

            GUIUtils.DrawBottomLineForRect(rect, style.normal.textColor);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            return clicked;
        }

        public static bool ButtonIcon(Texture icon, string tooltip = null)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, NoteStyles.buttonIcon, GUILayout.Width(EditorGUIUtility.singleLineHeight));

            GUIContent guiContent = EditorGUIUtility.TrIconContent(icon, tooltip);
            EditorGUIUtility.SetIconSize(rect.size);
            bool clicked = GUI.Button(rect, guiContent, NoteStyles.buttonIcon);
            return clicked;
        }

        public static bool ButtonLink(string displayText, string url)
        {
            GUIStyle style = NoteStyles.buttonLink;
            GUIContent content = EditorGUIUtility.TrTextContent(displayText, url);
            Vector2 controlSize = style.CalcSize(content);
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(controlSize.x), GUILayout.Height(controlSize.y));
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

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

            bool clicked = GUI.Button(rect, content, style);
            return clicked;
        }

        public static string TextboxWithPlaceholder(string placeholder, string text, GUIStyle placeholderStyle, GUIStyle textStyle, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.BeginVertical();
            string controlName = GetTextboxName(placeholder, text);
            GUI.SetNextControlName(controlName);
            string modifiedText = EditorGUILayout.TextField(text, textStyle, options);
            if (string.IsNullOrEmpty(modifiedText))
            {
                EditorGUI.LabelField(rect, placeholder, placeholderStyle);
            }
            EditorGUILayout.EndVertical();

            if (string.Equals(controlName, GUI.GetNameOfFocusedControl()))
            {
                EditorGUI.DrawRect(rect, NoteStyles.colorOverlayDarker);
                GUIUtils.DrawBottomLineForRect(rect, NoteStyles.colorTextboxUnderline);
            }

            return modifiedText;
        }

        public static string TextAreaWithPlaceholder(string placeholder, string text, GUIStyle placeholderStyle, GUIStyle textStyle)
        {
            Rect rect = EditorGUILayout.BeginVertical();
            string controlName = GetTextboxName(placeholder, text);
            GUI.SetNextControlName(controlName);
            string modifiedText = EditorGUILayout.TextArea(text, textStyle);
            if (string.IsNullOrEmpty(modifiedText))
            {
                EditorGUI.LabelField(rect, placeholder, placeholderStyle);
            }
            EditorGUILayout.EndVertical();

            if (string.Equals(controlName, GUI.GetNameOfFocusedControl()))
            {
                EditorGUI.DrawRect(rect, NoteStyles.colorOverlayDarker);
                GUIUtils.DrawBottomLineForRect(rect, NoteStyles.colorTextboxUnderline);
            }

            return modifiedText;
        }

        public static string GetTextboxName(string placeholder, string text)
        {
            return placeholder + text;
        }

        public static void DrawTag(Rect rect, Tag tag)
        {
            EditorGUI.DrawRect(rect, NoteStyles.GetTagBackgroundColor(tag.color));
            EditorGUI.LabelField(rect, tag.name, NoteStyles.tagBody);
        }

        public static NoteUI ShowAsPopup(Rect activatorRect, Note note)
        {
            NotePopupContent popupContent = new NotePopupContent(note.id);
            PopupWindow.Show(activatorRect, popupContent);
            return popupContent.noteUI;
        }
    }
}