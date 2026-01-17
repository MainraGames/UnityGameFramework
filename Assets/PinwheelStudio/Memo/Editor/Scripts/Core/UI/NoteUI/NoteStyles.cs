using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Memo.UI
{
    public static class NoteStyles
    {
        public static Vector2 windowSize
        {
            get
            {
                return new Vector2(400, 400);
            }
        }

        private static GUIStyle m_noteNameSceneView;
        public static GUIStyle noteNameSceneView
        {
            get
            {
                if (m_noteNameSceneView == null)
                {
                    m_noteNameSceneView = new GUIStyle(Styles.p1);
                    m_noteNameSceneView.normal.textColor = colorTextNormal;
                    m_noteNameSceneView.wordWrap = false;
                    m_noteNameSceneView.clipping = TextClipping.Clip;
                }
                return m_noteNameSceneView;
            }
        }


        private static GUIStyle m_noteBodySceneView;
        public static GUIStyle noteBodySceneView
        {
            get
            {
                if (m_noteBodySceneView == null)
                {
                    m_noteBodySceneView = new GUIStyle();
                    m_noteBodySceneView.padding = new RectOffset(2, 2, 2, 2);
                }
                return m_noteBodySceneView;
            }
        }

        private static GUIStyle m_noteBody;
        public static GUIStyle noteBody
        {
            get
            {
                if (m_noteBody == null)
                {
                    m_noteBody = new GUIStyle();
                    m_noteBody.padding = new RectOffset(8, 8, 8, 8);
                }
                return m_noteBody;
            }
        }

        private static GUIStyle m_topHandles;
        public static GUIStyle topHandles
        {
            get
            {
                if (m_topHandles == null)
                {
                    m_topHandles = new GUIStyle();
                    m_topHandles.margin = new RectOffset(0, 0, 0, 0);
                    m_topHandles.padding = new RectOffset(0, 0, 4, 4);
                }
                return m_topHandles;
            }
        }

        private static GUIStyle m_h2;
        public static GUIStyle h2
        {
            get
            {
                if (m_h2 == null)
                {
                    m_h2 = new GUIStyle(Styles.h2);
                    m_h2.normal.textColor = colorTextNormal;
                    m_h2.hover.textColor = colorTextNormal;
                    m_h2.focused.textColor = colorTextNormal;
                    m_h2.wordWrap = true;
                }
                return m_h2;
            }
        }

        private static GUIStyle m_h3;
        public static GUIStyle h3
        {
            get
            {
                if (m_h3 == null)
                {
                    m_h3 = new GUIStyle(Styles.h3);
                    m_h3.normal.textColor = colorTextNormal;
                    m_h3.hover.textColor = colorTextNormal;
                    m_h3.focused.textColor = colorTextNormal;
                    m_h3.wordWrap = true;
                }
                return m_h3;
            }
        }

        private static GUIStyle m_noteName;
        public static GUIStyle noteName
        {
            get
            {
                if (m_noteName == null)
                {
                    m_noteName = new GUIStyle(h2);
                }
                return m_noteName;
            }
        }

        private static GUIStyle m_noteNameNoWrap;
        public static GUIStyle noteNameNoWrap
        {
            get
            {
                if (m_noteNameNoWrap == null)
                {
                    m_noteNameNoWrap = new GUIStyle(noteName);
                    m_noteNameNoWrap.wordWrap = false;
                }
                return m_noteNameNoWrap;
            }
        }

        private static GUIStyle m_p1;
        public static GUIStyle p1
        {
            get
            {
                if (m_p1 == null)
                {
                    m_p1 = new GUIStyle(Styles.p1);
                    m_p1.normal.textColor = colorTextNormal;
                    m_p1.hover.textColor = colorTextNormal;
                    m_p1.focused.textColor = colorTextNormal;
                    m_p1.active.textColor = colorTextNormal;
                    m_p1.wordWrap = true;
                }
                return m_p1;
            }
        }

        private static GUIStyle m_p2;
        public static GUIStyle p2
        {
            get
            {
                if (m_p2 == null)
                {
                    m_p2 = new GUIStyle(Styles.p2);
                    m_p2.normal.textColor = colorTextNormal;
                    m_p2.hover.textColor = colorTextNormal;
                    m_p2.focused.textColor = colorTextNormal;
                    m_p2.active.textColor = colorTextNormal;
                    m_p2.wordWrap = true;
                }
                return m_p2;
            }
        }

        private static GUIStyle m_noteDescription;
        public static GUIStyle noteDescription
        {
            get
            {
                if (m_noteDescription == null)
                {
                    m_noteDescription = new GUIStyle(p1);
                }
                return m_noteDescription;
            }
        }

        private static GUIStyle m_placeholderTextH2;
        public static GUIStyle placeholderTextH2
        {
            get
            {
                if (m_placeholderTextH2 == null)
                {
                    m_placeholderTextH2 = new GUIStyle(h2);
                    m_placeholderTextH2.normal.textColor = colorTextPlaceholder;
                    m_placeholderTextH2.hover.textColor = colorTextPlaceholder;
                    m_placeholderTextH2.focused.textColor = colorTextPlaceholder;
                    m_placeholderTextH2.wordWrap = false;
                    m_placeholderTextH2.fontStyle = FontStyle.BoldAndItalic;
                }
                return m_placeholderTextH2;
            }
        }

        private static GUIStyle m_placeholderTextP1;
        public static GUIStyle placeholderTextP1
        {
            get
            {
                if (m_placeholderTextP1 == null)
                {
                    m_placeholderTextP1 = new GUIStyle(p1);
                    m_placeholderTextP1.normal.textColor = colorTextPlaceholder;
                    m_placeholderTextP1.hover.textColor = colorTextPlaceholder;
                    m_placeholderTextP1.focused.textColor = colorTextPlaceholder;
                    m_placeholderTextP1.wordWrap = false;
                    m_placeholderTextP1.fontStyle = FontStyle.Italic;
                }
                return m_placeholderTextP1;
            }
        }

        private static GUIStyle m_dragger;
        public static GUIStyle dragger
        {
            get
            {
                if (m_dragger == null)
                {
                    m_dragger = new GUIStyle(EditorStyles.label);
                    m_dragger.fixedHeight = 12;
                    m_dragger.fontSize = 12;
                    m_dragger.alignment = TextAnchor.MiddleCenter;
                    m_dragger.normal.textColor = new Color32(45, 45, 45, 255);
                    m_dragger.hover.textColor = new Color32(45, 45, 45, 255);
                    m_dragger.focused.textColor = new Color32(45, 45, 45, 255);
                    m_dragger.active.textColor = new Color32(45, 45, 45, 255);
                    m_dragger.margin = new RectOffset(2, 2, 2, 2);
                    m_dragger.padding = new RectOffset(2, 2, 2, 2);
                }
                return m_dragger;
            }
        }

        private static GUIStyle m_noteSettingsButton;
        public static GUIStyle noteSettingsButton
        {
            get
            {
                if (m_noteSettingsButton == null)
                {
                    m_noteSettingsButton = new GUIStyle(EditorStyles.iconButton);
                    m_noteSettingsButton.fixedWidth = 12;
                    m_noteSettingsButton.fixedHeight = 12;
                    m_noteSettingsButton.fontSize = 12;
                    m_noteSettingsButton.alignment = TextAnchor.MiddleCenter;
                    m_noteSettingsButton.normal.textColor = colorTextNormal;
                    m_noteSettingsButton.hover.textColor = colorTextNormal;
                    m_noteSettingsButton.focused.textColor = colorTextNormal;
                    m_noteSettingsButton.active.textColor = colorTextNormal;
                    m_noteSettingsButton.margin = new RectOffset(2, 2, 2, 2);
                    m_noteSettingsButton.padding = new RectOffset(2, 2, 2, 2);
                }

                return m_noteSettingsButton;
            }
        }

        private static GUIStyle m_checklistName;
        public static GUIStyle checklistName
        {
            get
            {
                if (m_checklistName == null)
                {
                    m_checklistName = new GUIStyle(h3);
                }
                return m_checklistName;
            }
        }

        private static GUIStyle m_checklistProgress;
        public static GUIStyle checklistProgress
        {
            get
            {
                if (m_checklistProgress == null)
                {
                    m_checklistProgress = new GUIStyle(p2);
                    m_checklistProgress.padding = new RectOffset(2, 2, 2, 2);
                }
                return m_checklistProgress;
            }
        }

        private static GUIStyle m_checkItemCheckbox;
        public static GUIStyle checkItemCheckbox
        {
            get
            {
                if (m_checkItemCheckbox == null)
                {
                    m_checkItemCheckbox = new GUIStyle(noteDescription);
                    m_checkItemCheckbox.fontSize = 20;
                    m_checkItemCheckbox.padding = new RectOffset(0, 0, -7, 0);
                }
                return m_checkItemCheckbox;
            }
        }

        private static GUIStyle m_checkItemCheckboxFaded;
        public static GUIStyle checkItemCheckboxFaded
        {
            get
            {
                if (m_checkItemCheckboxFaded == null)
                {
                    m_checkItemCheckboxFaded = new GUIStyle(checkItemCheckbox);
                    Color32 textColor = colorTextFaded;
                    m_checkItemCheckboxFaded.normal.textColor = textColor;
                    m_checkItemCheckboxFaded.hover.textColor = textColor;
                    m_checkItemCheckboxFaded.focused.textColor = textColor;
                    m_checkItemCheckboxFaded.active.textColor = textColor;
                }
                return m_checkItemCheckboxFaded;
            }
        }

        private static GUIStyle m_checkItemName;
        public static GUIStyle checkItemName
        {
            get
            {
                if (m_checkItemName == null)
                {
                    m_checkItemName = new GUIStyle(p1);
                }
                return m_checkItemName;
            }
        }

        private static GUIStyle m_checkItemNameFaded;
        public static GUIStyle checkItemNameFaded
        {
            get
            {
                if (m_checkItemNameFaded == null)
                {
                    m_checkItemNameFaded = new GUIStyle(p1);
                    Color32 textColor = colorTextFaded;
                    m_checkItemNameFaded.normal.textColor = textColor;
                    m_checkItemNameFaded.hover.textColor = textColor;
                    m_checkItemNameFaded.focused.textColor = textColor;
                    m_checkItemNameFaded.active.textColor = textColor;
                    m_checkItemNameFaded.fontStyle = FontStyle.Italic;
                }
                return m_checkItemNameFaded;
            }
        }

        private static GUIStyle m_button1;
        public static GUIStyle button1
        {
            get
            {
                if (m_button1 == null)
                {
                    m_button1 = new GUIStyle(p1);
                    m_button1.alignment = TextAnchor.MiddleLeft;
                    m_button1.wordWrap = false;
                    m_button1.padding = new RectOffset(8, 8, 0, 0);
                    m_button1.fontStyle = FontStyle.Bold;
                    m_button1.normal.textColor = colorTextNormal;
                    m_button1.hover.textColor = colorTextNormal;
                    m_button1.focused.textColor = colorTextNormal;
                    m_button1.active.textColor = colorTextNormal;
                }
                return m_button1;
            }
        }

        private static GUIStyle m_button2;
        public static GUIStyle button2
        {
            get
            {
                if (m_button2 == null)
                {
                    m_button2 = new GUIStyle(noteDescription);
                    m_button2.alignment = TextAnchor.MiddleLeft;
                    m_button2.wordWrap = false;
                    m_button2.padding = new RectOffset(1, 1, 0, 0);
                    m_button2.fontStyle = FontStyle.Italic;
                    m_button2.normal.textColor = colorTextNormal;
                    m_button2.hover.textColor = colorTextNormal;
                    m_button2.focused.textColor = colorTextNormal;
                    m_button2.active.textColor = colorTextNormal;
                }
                return m_button2;
            }
        }

        private static GUIStyle m_button3;
        public static GUIStyle button3
        {
            get
            {
                if (m_button3 == null)
                {
                    m_button3 = new GUIStyle(noteDescription);
                    m_button3.alignment = TextAnchor.MiddleLeft;
                    m_button3.wordWrap = false;
                    m_button3.padding = new RectOffset(1, 1, 0, 0);
                    m_button3.normal.textColor = colorTextNormal;
                    m_button3.hover.textColor = colorTextNormal;
                    m_button3.focused.textColor = colorTextNormal;
                    m_button3.active.textColor = colorTextNormal;
                }
                return m_button3;
            }
        }

        private static GUIStyle m_buttonLink;
        public static GUIStyle buttonLink
        {
            get
            {
                if (m_buttonLink == null)
                {
                    m_buttonLink = new GUIStyle(button3);
                    m_buttonLink.normal.textColor = colorTextLink;
                    m_buttonLink.hover.textColor = colorTextLink;
                    m_buttonLink.focused.textColor = colorTextLink;
                    m_buttonLink.active.textColor = colorTextLink;
                }
                return m_buttonLink;
            }
        }

        private static GUIStyle m_buttonMini;
        public static GUIStyle buttonMini
        {
            get
            {
                if (m_buttonMini == null)
                {
                    m_buttonMini = new GUIStyle(p2);
                    m_buttonMini.alignment = TextAnchor.MiddleLeft;
                    m_buttonMini.wordWrap = false;
                    m_buttonMini.padding = new RectOffset(1, 1, 0, 0);
                    m_buttonMini.normal.textColor = colorTextNormal;
                    m_buttonMini.hover.textColor = colorTextNormal;
                    m_buttonMini.focused.textColor = colorTextNormal;
                    m_buttonMini.active.textColor = colorTextNormal;
                }
                return m_buttonMini;
            }
        }

        private static GUIStyle m_buttonIcon;
        public static GUIStyle buttonIcon
        {
            get
            {
                if (m_buttonIcon == null)
                {
                    m_buttonIcon = new GUIStyle(EditorStyles.iconButton);
                    m_buttonIcon.alignment = TextAnchor.MiddleCenter;
                    m_buttonIcon.fixedHeight = 0;
                    m_buttonIcon.fixedWidth = 0;
                }
                return m_buttonIcon;
            }
        }

        private static GUIStyle m_noteContentScrollView;
        public static GUIStyle noteContentScrollView
        {
            get
            {
                if (m_noteContentScrollView == null)
                {
                    m_noteContentScrollView = new GUIStyle();
                    m_noteContentScrollView.padding = new RectOffset(8, 8, 0, 8);
                }
                return m_noteContentScrollView;
            }
        }

        private static GUIStyle m_tagBody;
        public static GUIStyle tagBody
        {
            get
            {
                if (m_tagBody == null)
                {
                    m_tagBody = new GUIStyle(p2);
                    m_tagBody.padding = new RectOffset(2, 2, 0, 0);
                    m_tagBody.margin = new RectOffset(0, 4, 2, 2);
                }
                return m_tagBody;
            }
        }

        public static Color32 colorOverlayLighter = new Color32(255, 255, 255, 50);
        public static Color32 colorOverlayDarker = new Color32(0, 0, 0, 20);
        public static Color32 colorTopHandles = new Color32(0, 0, 0, 35);
        public static Color32 colorOutlines = new Color32(0, 0, 0, 50);
        public static Color32 colorOutlinesDarker = new Color32(0, 0, 0, 150);
        public static Color32 colorTextNormal = new Color32(45, 45, 45, 255);
        public static Color32 colorTextPlaceholder = new Color32(45, 45, 45, 100);
        public static Color32 colorTextFaded = new Color32(45, 45, 45, 64);
        public static Color32 colorTextNormalInverted = new Color32(210, 210, 210, 255);
        public static Color32 colorTextLink = new Color32(70, 70, 255, 255);
        public static Color32 colorTextboxUnderline = new Color32(0, 0, 0, 50);
        public static Color32 colorButtonPressed = new Color32(0, 0, 0, 50);
        public static Color32 colorButtonHover = new Color32(0, 0, 0, 20);
        public static Color32 colorAccomplished = new Color32(110, 255, 110, 175);

        public static Color32 GetNoteBackgroundColor(Colors color)
        {
            switch (color)
            {
                case Colors.Red: return new Color32(255, 175, 165, 255);
                case Colors.Orange: return new Color32(255, 200, 145, 255);
                case Colors.Yellow: return new Color32(255, 255, 155, 255);
                case Colors.Green: return new Color32(185, 255, 185, 255);
                case Colors.Cyan: return new Color32(180, 255, 255, 255);
                case Colors.Blue: return new Color32(150, 210, 255, 255);
                case Colors.Pink: return new Color32(255, 215, 255, 255);
                case Colors.Purple: return new Color32(215, 205, 255, 255);
                case Colors.Gray: return new Color32(235, 235, 235, 255);
                default: return new Color32(255, 255, 255, 255);
            }
        }

        public static Color32 GetTagBackgroundColor(Colors color)
        {
            switch (color)
            {
                case Colors.Red: return new Color32(255, 105, 105, 255);
                case Colors.Orange: return new Color32(255, 150, 95, 255);
                case Colors.Yellow: return new Color32(255, 225, 85, 255);
                case Colors.Green: return new Color32(115, 235, 115, 255);
                case Colors.Cyan: return new Color32(90, 235, 235, 255);
                case Colors.Blue: return new Color32(100, 160, 255, 255);
                case Colors.Pink: return new Color32(235, 165, 235, 255);
                case Colors.Purple: return new Color32(150, 145, 235, 255);
                case Colors.Gray: return new Color32(185, 185, 185, 255);
                default: return new Color32(255, 255, 255, 255);
            }
        }
    }
}
