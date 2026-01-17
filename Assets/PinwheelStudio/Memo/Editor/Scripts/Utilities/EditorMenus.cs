using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Pinwheel.Memo.UI;

namespace Pinwheel.Memo
{
    public static class EditorMenus
    {
        [MenuItem("GameObject/Add Note", true, -100000)]
        [MenuItem("GameObject/Add Note (Link To Trello)", true, -100000)]
        public static bool CreateNoteForSceneObjectValidation()
        {
            return Selection.activeGameObject != null;
        }

        [MenuItem("GameObject/Add Note", false, 100000)]
        public static void CreateNoteForSceneObject()
        {
            GameObject target = Selection.activeGameObject;
            Note note = NoteManager.instance.AddNoteToGameObject(target);
            NoteUI.ShowAsPopup(new Rect(0, 0, 0, 0), note);
        }

        [MenuItem("GameObject/Add Note (Link To Trello)", false, 100000)]
        public static void CreateTrelloNoteForSceneObject()
        {
            GameObject target = Selection.activeGameObject;
            Note note = NoteManager.instance.AddNoteToGameObject(target);
            NoteUI ui = NoteUI.ShowAsPopup(new Rect(0, 0, 0, 0), note);
            ui.PushPage(new NoteLinkTrelloPage(ui, note));
        }

        [MenuItem("Assets/Add Note To This Asset", true, 100000)]
        [MenuItem("Assets/Add Note To This Asset (Link To Trello)", true, 100000)]
        public static bool CreateNoteForSelectedAssetValidation()
        {
            return Selection.count == 1 && Selection.activeObject != null && AssetDatabase.IsMainAsset(Selection.activeObject);
        }


        [MenuItem("Assets/Add Note To This Asset", false, 100000)]
        public static void CreateNoteForSelectedAsset()
        {
            Note note = NoteManager.instance.AddNoteToAsset(Selection.activeObject);
            NoteUI.ShowAsPopup(new Rect(0, 0, 0, 0), note);
        }

        [MenuItem("Assets/Add Note To This Asset (Link To Trello)", false, 100000)]
        public static void CreateTrelloNoteForSelectedAsset()
        {
            Note note = NoteManager.instance.AddNoteToAsset(Selection.activeObject);
            NoteUI ui = NoteUI.ShowAsPopup(new Rect(0, 0, 0, 0), note);
            ui.PushPage(new NoteLinkTrelloPage(ui, note));
        }
    }
}
