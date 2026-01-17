using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using UnityEditor;

namespace Pinwheel.Memo
{
    public static class NoteUtils
    {
        public static bool IsNoteConnectedToTrelloCard(Note note)
        {
            bool isNoteLinkedToTrelloCard =
                note.linkage != null &&
                note.linkage.remote == NoteLinkage.Remote.TrelloCard &&
                !string.IsNullOrEmpty(note.linkage.json);
            return isNoteLinkedToTrelloCard;
        }

        public static int UnlinkNotesFromTrelloCard()
        {
            int count = 0;
            foreach (Note n in NoteManager.instance.GetNotes())
            {
                if (n.linkage != null &&
                    n.linkage.remote == NoteLinkage.Remote.TrelloCard)
                {
                    n.linkage.json = null;
                    count += 1;
                }
            }
            return count;
        }

        public static string CalculateContentHash(Note note)
        {
            string json = JsonUtility.ToJson(note);
            MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(json));
            string hashString = string.Concat(hashBytes.Select(x => x.ToString("X2")));
            return hashString;
        }

        public static void CleanUpTags(Note note)
        {
            note.idTags.RemoveAll(idTag => { return !NoteManager.instance.HasTag(idTag); });
        }

        public static bool IsNoteAttached(Note note)
        {
            return note.attachTarget.targetType != AttachTarget.TargetType.None &&
                !string.IsNullOrEmpty(note.attachTarget.id);
        }

        public static bool AttachNote(Note note, Object target)
        {
            if (AssetDatabase.Contains(target))
            {
                note.attachTarget.targetType = AttachTarget.TargetType.ProjectAsset;
                note.attachTarget.id = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(target));
            }
            else if (target is GameObject go)
            {
                if (go.scene.IsValid())
                {
                    note.attachTarget.targetType = AttachTarget.TargetType.SceneObject;
                    GameObject noteAttacherGO = new GameObject("~Note");
                    noteAttacherGO.hideFlags = HideFlags.DontSaveInBuild;
                    GameObjectUtility.SetParentAndAlign(noteAttacherGO, go);
                    Undo.RegisterCreatedObjectUndo(noteAttacherGO, "Create note");

                    NoteAttacher attacher = Undo.AddComponent<NoteAttacher>(noteAttacherGO);
                    attacher.hideFlags = HideFlags.DontSaveInBuild;

                    note.attachTarget.targetType = AttachTarget.TargetType.SceneObject;
                    note.attachTarget.id = attacher.id;
                }
            }

            return false;
        }

        public static void DetachNote(Note note)
        {
            if (note.attachTarget.targetType == AttachTarget.TargetType.SceneObject)
            {
                NoteAttacher attacher = NoteAttacher.GetById(note.attachTarget.id);
                if (attacher != null)
                {
                    GameObject.DestroyImmediate(attacher.gameObject);
                }
            }

            note.attachTarget.targetType = AttachTarget.TargetType.None;
            note.attachTarget.id = string.Empty;
        }
    }
}
