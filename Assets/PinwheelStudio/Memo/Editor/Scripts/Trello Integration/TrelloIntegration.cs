using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Memo.Trello.UI;
using UnityEditor;
using System;
using Unity.EditorCoroutines.Editor;
using UnityEngine.Networking;
using System.Text;
using System.Linq;

namespace Pinwheel.Memo.Trello
{
    public static class TrelloIntegration
    {
        public delegate void AuthorizationResultHandler(bool success);
        public static event AuthorizationResultHandler onNotifyAuthorizationResult;

        public static bool IsAuthorized()
        {
            return TrelloApi.settings.IsAuthorized();
        }

        public static void StartAuthorizationFlow()
        {
            TrelloWindow trelloWindow = EditorWindow.GetWindow<TrelloWindow>();
            trelloWindow.Show();
        }

        internal static void NotifyAuthorizationResult(bool isSuccessful)
        {
            onNotifyAuthorizationResult?.Invoke(isSuccessful);
        }

        public static AsyncTask LogoutAndRevokeToken(AsyncTaskCallback callback = null)
        {
            AsyncTask taskHandle = new AsyncTask();
            EditorCoroutineUtility.StartCoroutine(ILogoutAndRevokeToken(taskHandle, callback), NoteManager.instance);
            return taskHandle;
        }

        private static IEnumerator ILogoutAndRevokeToken(AsyncTask taskHandle, AsyncTaskCallback callback = null)
        {
            if (string.IsNullOrEmpty(TrelloApi.settings.apiToken))
            {
                taskHandle.Complete();
                callback?.Invoke("Trello token is null or empty, skip deleting");
                yield break;
            }
            string error = null;
            UnityWebRequest deleteTokenRequest = TrelloApi.DeleteToken(TrelloApi.settings.apiToken, (request, response) =>
            {
                if (request.result == UnityWebRequest.Result.Success ||
                    request.responseCode == 404)
                {
                    TrelloApi.settings.authId = null;
                    TrelloApi.settings.apiToken = null;
                }
                else
                {
                    error = request.error;
                }
            });

            yield return new WaitUntil(()=>Utilities.WebRequestIsCompleted(deleteTokenRequest));
            yield return null;

            taskHandle.Complete();
            callback?.Invoke(error);
        }

        public static void SetDefaultOrganization(string orgId)
        {
            TrelloApi.settings.idDefaultOrganization = orgId;
            TrelloApi.settings.MarkDirty();
        }

        public static string GetDefaultOrganization()
        {
            return TrelloApi.settings.idDefaultOrganization;
        }

        public static void SetDefaultBoard(string boardId)
        {
            TrelloApi.settings.idDefaultBoard = boardId;
            TrelloApi.settings.MarkDirty();
        }

        public static string GetDefaultBoard()
        {
            return TrelloApi.settings.idDefaultBoard;
        }

        public static AsyncTask PushNote(Note note, AsyncTaskCallback callback = null)
        {
            AsyncTask taskHandle = new AsyncTask();
            EditorCoroutineUtility.StartCoroutine(IPushToRemote(taskHandle, note, callback), NoteManager.instance);
            return taskHandle;
        }

        private static IEnumerator IPushToRemote(AsyncTask taskHandle, Note note, AsyncTaskCallback callback = null)
        {
            if (!TrelloApi.settings.IsAuthorized())
            {
                taskHandle.Complete();
                callback?.Invoke("Not authorized, skip pushing note to Trello card.");
                yield break;
            }

            TrelloLinkage linkage = new TrelloLinkage();
            EditorJsonUtility.FromJsonOverwrite(note.linkage.json, linkage);
            string listId = linkage.idList;
            string cardId = linkage.idCard;
            StringBuilder error = new StringBuilder();

            yield return EditorCoroutineUtility.StartCoroutine(IPushCard(listId, cardId, note, error), NoteManager.instance);
            //Note linkage can changed after card sync (create new card -> cardId change)
            EditorJsonUtility.FromJsonOverwrite(note.linkage.json, linkage);
            cardId = linkage.idCard;

            yield return EditorCoroutineUtility.StartCoroutine(IPushAttachments(cardId, note, error), NoteManager.instance);
            yield return EditorCoroutineUtility.StartCoroutine(IPushChecklists(cardId, note, error), NoteManager.instance);
            yield return EditorCoroutineUtility.StartCoroutine(IPushCheckItems(cardId, note, error), NoteManager.instance);

            taskHandle.Complete();
            callback?.Invoke(error.ToString());
        }

        private static IEnumerator IPushCard(string listId, string cardId, Note note, StringBuilder error)
        {
            UnityWebRequest syncCardRequest = null;
            if (string.IsNullOrEmpty(listId) && !string.IsNullOrEmpty(cardId)) //already linked to a card
            {
                TrelloApi.CardUpdateQuery query = new TrelloApi.CardUpdateQuery();
                query.name = note.name;
                query.desc = note.description;
                query.cover = GetCardCoverFromNoteColor(note.color);
                query.idLabels = note.idTags.Where(idTag => !Utilities.IsLocalId(idTag)).ToArray();

                syncCardRequest = TrelloApi.UpdateCard(cardId, query,
                (request, response) =>
                {
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        TrelloUser.me.HandleIncomingCard(response);
                        TrelloUser.me.SetDirty();
                    }
                    else
                    {
                        error.Append($"Sync (update) card failed: {request.error}");
                    }
                    request.Dispose();
                });
            }
            else if (!string.IsNullOrEmpty(listId) && string.IsNullOrEmpty(cardId)) //not linked to a card, but user want to create a new one 
            {
                TrelloApi.CardCreationQuery query = new TrelloApi.CardCreationQuery();
                query.idList = listId;
                query.name = note.name;
                query.desc = note.description;
                query.idLabels = note.idTags.Where(idTag => !Utilities.IsLocalId(idTag)).ToArray();

                syncCardRequest = TrelloApi.CreateCard(query, (request, response) =>
                {
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        TrelloUser.me.HandleIncomingCard(response);
                        TrelloUser.me.SetDirty();

                        NoteLinkage newLinkage = new NoteLinkage();
                        newLinkage.remote = NoteLinkage.Remote.TrelloCard;
                        TrelloLinkage trelloLinkage = new TrelloLinkage();
                        trelloLinkage.idCard = response.id;
                        newLinkage.json = JsonUtility.ToJson(trelloLinkage);
                        note.linkage = newLinkage;
                        NoteManager.instance.SetDirty();
                    }
                    else
                    {
                        error.Append($"Sync (create) card failed: {request.error}");
                    }
                });
                //Card cover won't matched at this point, we can make another UpdateCard request
                //That's not quite important anyway so let it sync later.
            }
            else
            {
                //Not sure what to do. ListId and cardId cannot be null or nonempty at the same time
                error.Append($"Sync card failed: Invalid note linkage info.");
            }


            yield return new WaitUntil(() => Utilities.WebRequestIsCompleted(syncCardRequest));
            yield return null;
        }

        private static IEnumerator IPushAttachments(string idCard, Note note, StringBuilder error)
        {
            IEnumerable<Link> links = note.links.Where(l => !string.IsNullOrEmpty(l.url));
            int semaphore = 0;
            int linkCount = links.Count();
            foreach (Link l in links)
            {
                UnityWebRequest syncLinkRequest = null;
                if (Utilities.IsLocalId(l.idRemote) && !l.isDeleted) //not linked, not deleted => create new attachment
                {
                    TrelloApi.AttachmentCreationQuery query = new TrelloApi.AttachmentCreationQuery();
                    query.name = l.displayText;
                    query.url = l.url;
                    syncLinkRequest = TrelloApi.CreateAttachment(idCard, query, (request, response) =>
                    {
                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            l.idRemote = response.id;
                            l.displayText = response.name;
                            l.url = response.url;
                        }
                        else
                        {
                            error.Append($"Sync link (create) failed: {request.error}");
                        }
                        request.Dispose();
                        semaphore += 1;
                    });
                }
                else if (!Utilities.IsLocalId(l.idRemote) && !l.isDeleted) //linked, not deleted => update attachment
                {
                    TrelloApi.AttachmentUpdateQuery query = new TrelloApi.AttachmentUpdateQuery();
                    query.name = l.displayText;
                    query.url = l.url;
                    syncLinkRequest = TrelloApi.UpdateAttachment(idCard, l.idRemote, query, (request, response) =>
                    {
                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            l.idRemote = response.id;
                            l.displayText = response.name;
                            l.url = response.url;
                        }
                        else
                        {
                            if (request.responseCode == 404) //linked to a nonexist resource, probably the attachment was deleted on remote
                            {
                                l.isDeleted = true; //marked as deleted locally for clean up later
                            }
                            else
                            {
                                error.Append($"Sync link (update) failed: {request.error}");
                            }
                        }
                        request.Dispose();
                        semaphore += 1;
                    });
                }
                else if (!Utilities.IsLocalId(l.idRemote) && l.isDeleted) //linked, deleted => delete attachment
                {
                    syncLinkRequest = TrelloApi.DeleteAttachment(idCard, l.idRemote, (request, response) =>
                    {
                        if (request.result == UnityWebRequest.Result.Success || //deleted on remote by this request
                            request.responseCode == 404) //deleted on remote by someone else
                        {

                        }
                        else
                        {
                            error.Append($"Sync link (delete) failed: {request.error}");
                        }
                        request.Dispose();
                        semaphore += 1;
                    });

                }
                else //not linked, deleted => remove locally
                {
                    semaphore += 1;
                }
            }

            yield return new WaitUntil(() => semaphore >= linkCount);
            yield return null;

            note.links.RemoveAll(l => l == null || string.IsNullOrEmpty(l.url) || l.isDeleted);
        }

        private static IEnumerator IPushChecklists(string cardId, Note note, StringBuilder error)
        {
            //Sync checklists
            List<Checklist> checklistsAfter = new List<Checklist>();
            foreach (Checklist checklist in note.checklists)
            {
                UnityWebRequest syncChecklistRequest = null;
                //Empty idRemote means the checklist was created locally, need to create a new one on remote
                if (string.IsNullOrEmpty(checklist.idRemote) && !checklist.isDeleted)
                {
                    TrelloApi.ChecklistCreationQuery query = new TrelloApi.ChecklistCreationQuery()
                    {
                        cardId = cardId,
                        name = checklist.name
                    };
                    syncChecklistRequest = TrelloApi.CreateChecklist(query, (request, response) =>
                    {
                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            checklist.idRemote = response.id;
                            checklist.name = response.name;
                        }
                        else
                        {
                            error.Append($"Sync (create) checklist failed: {request.error}");
                        }

                        checklistsAfter.Add(checklist);
                        request.Dispose();
                    });
                }
                else if (!string.IsNullOrEmpty(checklist.idRemote) && !checklist.isDeleted)
                {
                    TrelloApi.ChecklistUpdateQuery query = new TrelloApi.ChecklistUpdateQuery()
                    {
                        name = checklist.name
                    };
                    syncChecklistRequest = TrelloApi.UpdateChecklist(checklist.idRemote, query, (request, response) =>
                    {
                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            checklist.idRemote = response.id;
                            checklist.name = response.name;
                        }
                        else
                        {
                            error.Append($"Sync (update) checklist failed: {request.error}");
                            if (request.responseCode == 404)
                            {
                                //For some reason the checklist was linked to a nonexisted object on remote
                                //Unlink for retry next time
                                checklist.idRemote = null;
                            }
                        }
                        checklistsAfter.Add(checklist);
                        request.Dispose();
                    });
                }
                else if (!string.IsNullOrEmpty(checklist.idRemote) && checklist.isDeleted) //Checklist was linked to remote and deleted locally
                {
                    syncChecklistRequest = TrelloApi.DeleteChecklist(checklist.idRemote, (request, response) =>
                    {
                        if (request.result == UnityWebRequest.Result.Success ||
                            request.responseCode == 404)
                        {
                            //Remove locally by not adding to the checklistAfter list
                        }
                        else
                        {
                            //Keep the checklist locally to try again later
                            checklistsAfter.Add(checklist);
                            error.Append($"Sync (delete) checklist failed: {request.error}");
                        }
                        request.Dispose();
                    });
                }
                else //not linked to remote and deleted locally
                {
                    //Don't add to checklistAfter anyway!
                }
                yield return new WaitUntil(() => Utilities.WebRequestIsCompleted(syncChecklistRequest));
                yield return null;
            }

            note.checklists.Clear();
            note.checklists.AddRange(checklistsAfter);
        }

        private static IEnumerator IPushCheckItems(string cardId, Note note, StringBuilder error)
        {
            foreach (Checklist checklist in note.checklists)
            {
                if (checklist.isDeleted)
                    continue;

                List<Checklist.Item> itemsAfter = new List<Checklist.Item>();
                foreach (Checklist.Item item in checklist.items)
                {
                    UnityWebRequest syncCheckItemRequest = null;
                    if (string.IsNullOrEmpty(item.idRemote) && !item.isDeleted)
                    {
                        TrelloApi.CheckItemCreationQuery query = new TrelloApi.CheckItemCreationQuery()
                        {
                            name = item.name,
                            isChecked = item.isChecked
                        };
                        syncCheckItemRequest = TrelloApi.CreateCheckItem(checklist.idRemote, query, (request, response) =>
                        {
                            if (request.result == UnityWebRequest.Result.Success)
                            {
                                item.idRemote = response.id;
                                item.name = response.name;
                                item.isChecked = string.Equals(response.state, "complete");
                            }
                            else
                            {
                                error.Append($"Sync (create) check item failed {request.error}");
                            }
                            itemsAfter.Add(item);
                            request.Dispose();
                        });
                    }
                    else if (!string.IsNullOrEmpty(item.idRemote) && !item.isDeleted)
                    {
                        TrelloApi.CheckItemUpdateQuery query = new TrelloApi.CheckItemUpdateQuery()
                        {
                            name = item.name,
                            isChecked = item.isChecked
                        };
                        syncCheckItemRequest = TrelloApi.UpdateCheckItem(cardId, item.idRemote, query, (request, response) =>
                        {
                            if (request.result == UnityWebRequest.Result.Success)
                            {
                                item.idRemote = response.id;
                                item.name = response.name;
                                item.isChecked = string.Equals(response.state, "complete");
                            }
                            else
                            {
                                error.Append($"Sync (update) check item failed {request.error}");
                            }
                            itemsAfter.Add(item);
                            request.Dispose();
                        });
                    }
                    else if (!string.IsNullOrEmpty(item.idRemote) && item.isDeleted)
                    {
                        syncCheckItemRequest = TrelloApi.DeleteCheckItem(checklist.idRemote, item.idRemote, (request, response) =>
                        {
                            if (request.result == UnityWebRequest.Result.Success ||
                                request.responseCode == 404)
                            {
                                //Remove locally by not adding to the itemAfter list
                            }
                            else
                            {
                                //Keep the checklist locally to try again later
                                itemsAfter.Add(item);
                                error.Append($"Sync (delete) check item failed: {request.error}");
                            }
                            request.Dispose();
                        });
                    }
                    else //not linked to remote and deleted locally
                    {
                        //Don't add to itemAfter anyway!
                    }
                    yield return new WaitUntil(() => Utilities.WebRequestIsCompleted(syncCheckItemRequest));
                    yield return null;
                }
            }
        }

        public static AsyncTask PullNote(Note note, AsyncTaskCallback callback = null)
        {
            AsyncTask taskHandle = new AsyncTask();
            EditorCoroutineUtility.StartCoroutine(IPullFromRemote(taskHandle, note, callback), NoteManager.instance);
            return taskHandle;
        }

        private static IEnumerator IPullFromRemote(AsyncTask taskHandle, Note note, AsyncTaskCallback callback = null)
        {
            if (!TrelloApi.settings.IsAuthorized())
            {
                taskHandle.Complete();
                callback?.Invoke("Not authorized, skip pulling from Trello card.");
                yield break;
            }

            TrelloLinkage linkage = new TrelloLinkage();
            EditorJsonUtility.FromJsonOverwrite(note.linkage.json, linkage);
            string cardId = linkage.idCard;

            string error = null;
            UnityWebRequest request = TrelloApi.GetCard(cardId, (UnityWebRequest request, TrelloApi.Card response) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    TrelloUser.me.RecordUndo("Pull Trello card");
                    TrelloUser.me.HandleIncomingCard(response);
                    TrelloUser.me.SetDirty();

                    NoteManager.instance.RecordUndo("Pull Trello card");
                    CopyFromTrelloCard(response, note);
                    NoteManager.instance.SetDirty();
                }
                else
                {
                    error = "Pull card failed: " + request.error;
                }
                request.Dispose();
            });
            yield return new WaitUntil(() => Utilities.WebRequestIsCompleted(request));

            taskHandle.Complete();
            callback?.Invoke(error);
        }

        private static void CopyFromTrelloCard(TrelloApi.Card from, Note to)
        {
            to.name = from.name;
            to.description = from.desc;
            if (from.checklists != null)
            {
                to.checklists.Clear();
                foreach (TrelloApi.Checklist tc in from.checklists)
                {
                    Checklist checklist = new Checklist();
                    checklist.idRemote = tc.id;
                    checklist.name = tc.name;
                    foreach (TrelloApi.CheckItem tci in tc.checkItems)
                    {
                        Checklist.Item checkItem = new Checklist.Item();
                        checkItem.idRemote = tci.id;
                        checkItem.isChecked = string.Equals(tci.state, TrelloApi.CheckItem.STATE_COMPLETE);
                        checkItem.name = tci.name;
                        checklist.items.Add(checkItem);
                    }
                    to.checklists.Add(checklist);
                }
            }
            to.color = GetNoteColorFromCardCover(from.cover);

            to.idTags.Clear();
            to.idTags.AddRange(from.labels.Select(l => l.id));

            if (from.attachments != null)
            {
                to.links.Clear();
                foreach (TrelloApi.Attachment att in from.attachments)
                {
                    if (att.isUpload)
                        continue;
                    Link link = new Link();
                    link.url = att.url;
                    link.displayText = att.name;
                    link.idRemote = att.id;
                    to.links.Add(link);
                }
            }
        }

        private static Colors GetNoteColorFromCardCover(TrelloApi.CardCover cover)
        {
            string colorName = cover.color;
            switch (colorName)
            {
                case null: return Colors.Yellow;
                case "pink": return Colors.Pink;
                case "yellow": return Colors.Yellow;
                case "lime": return Colors.Green;
                case "blue": return Colors.Blue;
                case "black": return Colors.Gray;
                case "orange": return Colors.Orange;
                case "red": return Colors.Red;
                case "purple": return Colors.Purple;
                case "sky": return Colors.Cyan;
                case "green": return Colors.Green;

                default: return Colors.Yellow;
            }
        }

        private static TrelloApi.CardCover GetCardCoverFromNoteColor(Colors color)
        {
            TrelloApi.CardCover cover = new TrelloApi.CardCover();
            switch (color)
            {
                case Colors.Red: cover.color = "red"; break;
                case Colors.Orange: cover.color = "orange"; break;
                case Colors.Yellow: cover.color = "yellow"; break;
                case Colors.Green: cover.color = "green"; break;
                case Colors.Cyan: cover.color = "sky"; break;
                case Colors.Blue: cover.color = "blue"; break;
                case Colors.Pink: cover.color = "pink"; break;
                case Colors.Purple: cover.color = "purple"; break;
                case Colors.Gray: cover.color = "black"; break;
                default: cover.color = null; break;
            }
            return cover;
        }

        public static string GetCardUrl(Note note)
        {
            TrelloLinkage linkage = new TrelloLinkage();
            EditorJsonUtility.FromJsonOverwrite(note.linkage.json, linkage);
            string cardId = linkage.idCard;

            TrelloApi.Card card = TrelloUser.me.GetCardById(cardId);
            if (card == null)
            {
                return null;
            }
            else
            {
                return card.shortUrl;
            }
        }

        public static AsyncTask PullLabelsOfDefaultBoard(AsyncTaskCallback callback = null)
        {
            AsyncTask taskHandle = new AsyncTask();
            EditorCoroutineUtility.StartCoroutine(IPullLabelsOfDefaultBoard(taskHandle, callback), NoteManager.instance);
            return taskHandle;
        }

        private static IEnumerator IPullLabelsOfDefaultBoard(AsyncTask taskHandle, AsyncTaskCallback callback = null)
        {
            if (string.IsNullOrEmpty(TrelloApi.settings.idDefaultBoard))
            {
                taskHandle.Complete();
                callback?.Invoke("Default board not set, skip pulling labels from Trello board.");
                yield break;
            }
            if (!TrelloApi.settings.IsAuthorized())
            {
                taskHandle.Complete();
                callback?.Invoke("Not authorized, skip pulling labels from Trello board.");
                yield break;
            }

            TrelloApi.GetLabelsOfBoard(GetDefaultBoard(), (request, response) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    MergeTagsAndLabels(response);
                }
                else
                {
                    Debug.LogWarning($"Pull labels failed: {request.error}");
                }
            });
        }

        private static void MergeTagsAndLabels(params TrelloApi.Label[] labels)
        {
            foreach (TrelloApi.Label l in labels)
            {
                if (string.IsNullOrEmpty(l.name))
                    continue;

                Tag t = NoteManager.instance.GetTagByName(l.name);
                if (t != null)
                {
                    NoteManager.instance.ChangeTagId(t, l.id);
                    t.name = l.name;
                    t.color = FromLabelColor(l.color);
                }
                else
                {
                    Tag newTag = NoteManager.instance.AddTag(FromLabelColor(l.color), l.name);
                    newTag.id = l.id;
                }
            }
        }

        public static Colors FromLabelColor(string labelColor)
        {
            if (labelColor.StartsWith("yellow")) return Colors.Yellow;
            if (labelColor.StartsWith("purple")) return Colors.Purple;
            if (labelColor.StartsWith("blue")) return Colors.Blue;
            if (labelColor.StartsWith("red")) return Colors.Red;
            if (labelColor.StartsWith("green")) return Colors.Green;
            if (labelColor.StartsWith("orange")) return Colors.Orange;
            if (labelColor.StartsWith("black")) return Colors.Gray;
            if (labelColor.StartsWith("sky")) return Colors.Cyan;
            if (labelColor.StartsWith("pink")) return Colors.Pink;
            if (labelColor.StartsWith("lime")) return Colors.Green;

            return Colors.Gray;
        }

        public static string ToLabelColor(Colors color)
        {
            switch (color)
            {
                case Colors.Red: return "red";
                case Colors.Orange: return "orange";
                case Colors.Yellow: return "yellow";
                case Colors.Green: return "green";
                case Colors.Cyan: return "sky";
                case Colors.Blue: return "blue";
                case Colors.Pink: return "pink";
                case Colors.Purple: return "purple";
                case Colors.Gray: return "black";
                default: return "black";
            }
        }

        public static AsyncTask PushLabelsToDefaultBoard(bool excludeLinkedTag = false, AsyncTaskCallback callback = null)
        {
            AsyncTask taskHandle = new AsyncTask();
            EditorCoroutineUtility.StartCoroutine(IPushLabelsToDefaultBoard(taskHandle, excludeLinkedTag, callback), NoteManager.instance);
            return taskHandle;
        }

        private static IEnumerator IPushLabelsToDefaultBoard(AsyncTask taskHandle, bool excludeLinkedTag = false, AsyncTaskCallback callback = null)
        {
            if (string.IsNullOrEmpty(TrelloApi.settings.idDefaultBoard))
            {
                taskHandle.Complete();
                callback?.Invoke("Default board not set, skip pushing tags to Trello board.");
                yield break;
            }
            if (!TrelloApi.settings.IsAuthorized())
            {
                taskHandle.Complete();
                callback?.Invoke("Not authorized, skip pushing tags to Trello board.");
                yield break;
            }
            StringBuilder error = new StringBuilder();
            IEnumerable<Tag> tags = NoteManager.instance.GetTags();
            if (excludeLinkedTag)
            {
                tags = tags.Where(t => Utilities.IsLocalId(t.id));
            }

            int semaphore = 0;
            int tagCount = tags.Count();
            foreach (Tag t in tags)
            {
                UnityWebRequest syncTagRequest = null;
                if (Utilities.IsLocalId(t.id) && !t.isDeleted) //not linked, not deleted => create new label
                {
                    TrelloApi.LabelCreationQuery query = new TrelloApi.LabelCreationQuery();
                    query.idBoard = GetDefaultBoard();
                    query.name = t.name;
                    query.color = ToLabelColor(t.color);
                    syncTagRequest = TrelloApi.CreateLabel(query, (request, response) =>
                    {
                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            MergeTagsAndLabels(response);
                        }
                        else
                        {
                            error.Append($"Sync tag (create '{t.name}') failed: {request.error}");
                        }
                        request.Dispose();
                        semaphore += 1;
                    });
                }
                else if (!Utilities.IsLocalId(t.id) && !t.isDeleted) //linked, not deleted => update label
                {
                    TrelloApi.LabelUpdateRequest query = new TrelloApi.LabelUpdateRequest();
                    query.name = t.name;
                    query.color = ToLabelColor(t.color);
                    syncTagRequest = TrelloApi.UpdateLabel(t.id, query, (request, response) =>
                    {
                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            t.name = response.name;
                            t.color = FromLabelColor(response.color);
                        }
                        else
                        {
                            if (request.responseCode == 404) //linked to a nonexist resource, probably the label was deleted on remote
                            {
                                t.isDeleted = true; //marked as deleted locally for clean up later
                            }
                            else
                            {
                                error.Append($"Sync tag (update '{t.name}' failed: {request.error}");
                            }
                        }
                        request.Dispose();
                        semaphore += 1;
                    });
                }
                else if (!Utilities.IsLocalId(t.id) && t.isDeleted) //linked, deleted => delete label
                {
                    syncTagRequest = TrelloApi.DeleteLabel(t.id, (request, response) =>
                    {
                        if (request.result == UnityWebRequest.Result.Success || //deleted on remote by this request
                            request.responseCode == 404) //deleted on remote by someone else
                        {

                        }
                        else
                        {
                            error.Append($"Sync tag (delete '{t.name}' failed: {request.error}");
                        }
                        request.Dispose();
                        semaphore += 1;
                    });

                }
                else //not linked, deleted => remove locally
                {
                    semaphore += 1;
                }
            }

            yield return new WaitUntil(() => semaphore >= tagCount);
            yield return null;
            NoteManager.instance.CleanUpTags();
            callback?.Invoke(error.ToString());
        }
    }
}
