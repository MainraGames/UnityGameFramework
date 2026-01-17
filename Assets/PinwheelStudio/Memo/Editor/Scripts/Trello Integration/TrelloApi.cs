using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;
using System;

namespace Pinwheel.Memo.Trello
{
    public static class TrelloApi
    {
        private static TrelloSettings s_settings;
        public static TrelloSettings settings
        {
            get
            {
                if (s_settings == null)
                {
                    s_settings = Resources.Load<TrelloSettings>("Memo/TrelloSettings");
                }
                if (s_settings == null)
                {
                    throw new System.NullReferenceException("Fail to load Trello Settings.");
                }
                return s_settings;
            }
        }

        [System.Serializable]
        public class AuthorizationOptions
        {
            public enum Scope
            {
                Read, Write, Account
            }

            public enum Expiration
            {
                Hour1, Day1, Days30, Never
            }

            //public Scope scope;
            public Expiration expiration;
        }

        public static string OpenBrowserForAuthorization(AuthorizationOptions r)
        {
            //string scope =
            //    r.scope == AuthorizationOptions.Scope.Read ? "read" :
            //    r.scope == AuthorizationOptions.Scope.Write ? "write" :
            //    r.scope == AuthorizationOptions.Scope.Account ? "account" : "";
            string expiration =
                r.expiration == AuthorizationOptions.Expiration.Hour1 ? "1hour" :
                r.expiration == AuthorizationOptions.Expiration.Day1 ? "1day" :
                r.expiration == AuthorizationOptions.Expiration.Days30 ? "30days" :
                r.expiration == AuthorizationOptions.Expiration.Never ? "never" : "";

            string authId = Guid.NewGuid().ToString().Replace("-", "");
            string redirectUrl = UnityWebRequest.EscapeURL($"https://api.pinwheelstud.io/memo/trello/fragment?authId={authId}");

            string url = $"https://trello.com/1/authorize?" +
                $"expiration={expiration}" +
                $"&scope=read,write" +
                $"&response_type=token" +
                $"&callback_method=fragment" +
                $"&return_url={redirectUrl}" +
                $"&key={settings.apiKey}";

            Application.OpenURL(url);
            return authId;
        }

        private static IEnumerator SendRequest<T>(UnityWebRequest request, ApiCallback<T> callback) where T : class, new()
        {
            yield return request.SendWebRequest();

            T response = new T();
            if (request.result == UnityWebRequest.Result.Success)
            {
                settings.AddApiCallInfoForDebug(request);
                JsonUtility.FromJsonOverwrite(request.downloadHandler.text, response);
            }
            else if (request.responseCode == 401)
            {
                settings.AddApiCallInfoForDebug(request);
                Debug.LogWarning("Unauthorized: Trello account not connected or token has expired.");
                settings.ResetAuthorizationAndUserInfo();
            }
            else
            {
                settings.AddApiCallInfoForDebug(request);
            }
            callback.Invoke(request, response);
        }

        private static IEnumerator SendRequest<T>(UnityWebRequest request, ApiCallbackArray<T> callback) where T : class, new()
        {
            yield return request.SendWebRequest();

            T[] response = null;
            if (request.result == UnityWebRequest.Result.Success)
            {
                settings.AddApiCallInfoForDebug(request);
                response = Utilities.ParseJsonToArray<T>(request.downloadHandler.text);
            }
            else if (request.responseCode == 401)
            {
                settings.AddApiCallInfoForDebug(request);
                Debug.LogWarning("Unauthorized: Trello account not connected or token has expired.");
                settings.ResetAuthorizationAndUserInfo();
            }
            else
            {
                settings.AddApiCallInfoForDebug(request);
            }
            callback.Invoke(request, response);
        }

        private static IEnumerator SendRequest(UnityWebRequest request, ApiCallbackTexture callback)
        {
            yield return request.SendWebRequest();

            Texture2D texture = null;
            if (request.result == UnityWebRequest.Result.Success)
            {
                settings.AddApiCallInfoForDebug(request);
                texture = DownloadHandlerTexture.GetContent(request);
            }
            else if (request.responseCode == 401)
            {
                settings.AddApiCallInfoForDebug(request);
                Debug.LogWarning("Unauthorized: Trello account not connected or token has expired.");
                settings.ResetAuthorizationAndUserInfo();
            }
            else
            {
                settings.AddApiCallInfoForDebug(request);
            }
            callback.Invoke(request, texture);
        }

        [System.Serializable]
        public class GetTokenResponse
        {
            public string authId;
            public string token;
            public string error;
        }

        public static UnityWebRequest GetUserTokenFromAuthId(string authId, ApiCallback<GetTokenResponse> callback)
        {
            string url = $"https://api.pinwheelstud.io/memo/trello/token?authId={authId}";
            UnityWebRequest request = UnityWebRequest.Get(url);
            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        [System.Serializable]
        public class TrelloToken
        {
            public string id;
            public string identifier;
            public string idMember;
            public string dateCreated;
            public string dateExpires;
            public Permission[] permissions;

            [System.Serializable]
            public class Permission
            {
                public string idModel;
                public string modelType;
                public bool read;
                public bool write;
            }
        }

        public static UnityWebRequest GetTokenInfo(string token, ApiCallback<TrelloToken> callback)
        {
            string url = $"https://api.trello.com/1/tokens/{token}?key={settings.apiKey}&token={settings.apiToken}";
            UnityWebRequest request = UnityWebRequest.Get(url);
            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        public static UnityWebRequest DeleteToken(string token, ApiCallback<TrelloToken> callback)
        {
            string url = $"https://api.trello.com/1/tokens/{token}?key={settings.apiKey}&token={settings.apiToken}";
            UnityWebRequest request = new UnityWebRequest(url, "DELETE");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        [System.Serializable]
        public class UserDescription
        {
            //https://developer.atlassian.com/cloud/trello/rest/api-group-members/#api-members-id-get
            public string id;
            public string username;
            public string fullName;
            public string initials;
            public string[] idBoards;
            public string[] idOrganizations;
        }

        public static UnityWebRequest GetUser(string userId, ApiCallback<UserDescription> callback)
        {
            string url = $"https://api.trello.com/1/members/{userId}?key={settings.apiKey}&token={settings.apiToken}";
            UnityWebRequest request = UnityWebRequest.Get(url);
            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        [System.Serializable]
        public class GetFieldResponse
        {
            public string _value;
        }

        [System.Serializable]
        public class Organization
        {
            public string id;
            public string name;
            public string displayName;
            public string url;
            public string logoUrl;
        }

        public static UnityWebRequest GetOrganization(string orgId, ApiCallback<Organization> callback)
        {
            string url = $"https://api.trello.com/1/organizations/{orgId}/?key={settings.apiKey}&token={settings.apiToken}";
            UnityWebRequest request = UnityWebRequest.Get(url);
            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        public static UnityWebRequest GetOrganizations(string userId, ApiCallbackArray<Organization> callback)
        {
            string url = $"https://api.trello.com/1/members/{userId}/organizations?key={settings.apiKey}&token={settings.apiToken}";
            UnityWebRequest request = UnityWebRequest.Get(url);
            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        public static UnityWebRequest GetImage(string imageUrl, ApiCallbackTexture callback)
        {
            string url = imageUrl;
            UnityWebRequest request = UnityWebRequest.Get(url);
            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        [System.Serializable]
        public class Board
        {
            //https://developer.atlassian.com/cloud/trello/rest/api-group-boards/#api-boards-id-get
            public string id;
            public string name;
            public string desc;
            public bool closed;
            public string idOrganization;
            public bool starred;
        }

        public static UnityWebRequest GetBoard(string boardId, ApiCallback<Board> callback)
        {
            string url = $"https://api.trello.com/1/boards/{boardId}?key={settings.apiKey}&token={settings.apiToken}";
            UnityWebRequest request = UnityWebRequest.Get(url);
            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        public static UnityWebRequest GetBoards(string userId, ApiCallbackArray<Board> callback)
        {
            string url = $"https://api.trello.com/1/members/{userId}/boards?key={settings.apiKey}&token={settings.apiToken}";
            UnityWebRequest request = UnityWebRequest.Get(url);
            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        [System.Serializable]
        public class List
        {
            //https://developer.atlassian.com/cloud/trello/rest/api-group-lists/#api-lists-id-get
            public string id;
            public string name;
            public bool closed;
            public int pos;
            public string idBoard;
        }

        public static UnityWebRequest GetListsInABoard(string boardId, ApiCallbackArray<List> callback)
        {
            string url = $"https://api.trello.com/1/boards/{boardId}/lists?key={settings.apiKey}&token={settings.apiToken}";
            UnityWebRequest request = UnityWebRequest.Get(url);
            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        [System.Serializable]
        public class Card
        {
            //https://developer.atlassian.com/cloud/trello/rest/api-group-cards/#api-cards-id-get
            public string id;
            public bool closed;
            public string dateLastActivity;
            public string desc;
            public string idBoard;
            public string idList;
            public string name;
            public int pos;
            public string shortUrl;
            public string[] idChecklists;
            public Checklist[] checklists;
            public CardCover cover;
            public Label[] labels;
            public Attachment[] attachments;
        }

        [System.Serializable]
        public class CardCover
        {
            public string color;
        }

        [System.Serializable]
        public class Checklist
        {
            //https://developer.atlassian.com/cloud/trello/rest/api-group-checklists/#api-checklists-id-get
            public string id;
            public string name;
            public string idBoard;
            public string idCard;
            public int pos;
            public CheckItem[] checkItems;
        }

        [System.Serializable]
        public class CheckItem
        {
            public const string STATE_COMPLETE = "complete";
            public const string STATE_INCOMPLETE = "incomplete";

            public string id;
            public string name;
            public int pos;
            public string state;
            public string idChecklist;
        }

        [System.Serializable]
        public class CardCreationQuery
        {
            public string name;
            public string desc;
            public string idList;
            public string[] idLabels;
        }

        public static UnityWebRequest CreateCard(CardCreationQuery query, ApiCallback<Card> callback)
        {
            string url = $"https://api.trello.com/1/cards?key={settings.apiKey}&token={settings.apiToken}" +
                $"&name={query.name}" +
                $"&desc={query.desc}" +
                $"&idList={query.idList}" +
                $"&idLabels={string.Join(',', query.idLabels)}" +
                $"&pos=bottom";
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        public static UnityWebRequest GetCardsInABoard(string boardId, ApiCallbackArray<Card> callback)
        {
            string url = $"https://api.trello.com/1/boards/{boardId}/cards?key={settings.apiKey}&token={settings.apiToken}" +
                $"&checklists=all" +
                $"&checkItemStates=true";
            UnityWebRequest request = UnityWebRequest.Get(url);
            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        public static UnityWebRequest GetCard(string cardId, ApiCallback<Card> callback)
        {
            string url = $"https://api.trello.com/1/cards/{cardId}?key={settings.apiKey}&token={settings.apiToken}" +
                $"&checklists=all" +
                $"&checkItemStates=true" +
                $"&attachments=true";
            UnityWebRequest request = UnityWebRequest.Get(url);
            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        [System.Serializable]
        public class CardUpdateQuery
        {
            //https://developer.atlassian.com/cloud/trello/rest/api-group-cards/#api-cards-id-put
            public string name;
            public string desc;
            public CardCover cover;
            public string[] idLabels;
        }

        public static UnityWebRequest UpdateCard(string cardId, CardUpdateQuery cardUpdate, ApiCallback<Card> callback)
        {
            string url = $"https://api.trello.com/1/cards/{cardId}?key={settings.apiKey}&token={settings.apiToken}" +
                $"&checklists=all" +
                $"&checkItemStates=true";
            string updateJson = JsonUtility.ToJson(cardUpdate);

            UnityWebRequest request = new UnityWebRequest(url, "PUT");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(updateJson);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        [System.Serializable]
        public class ChecklistCreationQuery
        {
            //https://developer.atlassian.com/cloud/trello/rest/api-group-checklists/#api-checklists-post
            public string cardId;
            public string name;
        }

        public static UnityWebRequest CreateChecklist(ChecklistCreationQuery query, ApiCallback<Checklist> callback)
        {
            string url = $"https://api.trello.com/1/checklists?key={settings.apiKey}&token={settings.apiToken}" +
                $"&idCard={query.cardId}" +
                $"&name={query.name}" +
                $"&pos=bottom";
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        [System.Serializable]
        public class ChecklistUpdateQuery
        {
            //https://developer.atlassian.com/cloud/trello/rest/api-group-checklists/#api-checklists-id-put
            public string name;
            //public int pos;
        }

        public static UnityWebRequest UpdateChecklist(string checklistId, ChecklistUpdateQuery query, ApiCallback<Checklist> callback)
        {
            string url = $"https://api.trello.com/1/checklists/{checklistId}?key={settings.apiKey}&token={settings.apiToken}" +
                $"&name={query.name}";
            UnityWebRequest request = new UnityWebRequest(url, "PUT");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        public static UnityWebRequest DeleteChecklist(string checklistId, ApiCallback<Checklist> callback)
        {
            string url = $"https://api.trello.com/1/checklists/{checklistId}?key={settings.apiKey}&token={settings.apiToken}";
            UnityWebRequest request = new UnityWebRequest(url, "DELETE");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        [System.Serializable]
        public class CheckItemCreationQuery
        {
            //https://developer.atlassian.com/cloud/trello/rest/api-group-checklists/#api-checklists-id-checkitems-post
            public string name;
            public bool isChecked;
        }

        public static UnityWebRequest CreateCheckItem(string checklistId, CheckItemCreationQuery query, ApiCallback<CheckItem> callback)
        {
            string url = $"https://api.trello.com/1/checklists/{checklistId}/checkItems?key={settings.apiKey}&token={settings.apiToken}" +
                $"&name={(string.IsNullOrEmpty(query.name) ? " " : query.name)}" +
                $"&checked={query.isChecked.ToString().ToLower()}" +
                $"&pos=bottom";
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        [System.Serializable]
        public class CheckItemUpdateQuery
        {
            //https://developer.atlassian.com/cloud/trello/rest/api-group-cards/#api-cards-id-checkitem-idcheckitem-put
            public string name;
            public bool isChecked;
        }

        public static UnityWebRequest UpdateCheckItem(string cardId, string checkItemId, CheckItemUpdateQuery query, ApiCallback<CheckItem> callback)
        {
            string url = $"https://api.trello.com/1/cards/{cardId}/checkItem/{checkItemId}?key={settings.apiKey}&token={settings.apiToken}" +
                $"&name={query.name}" +
                $"&state={(query.isChecked ? "complete" : "incomplete")}";
            UnityWebRequest request = new UnityWebRequest(url, "PUT");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        public static UnityWebRequest DeleteCheckItem(string checklistId, string checkItemId, ApiCallback<CheckItem> callback)
        {
            string url = $"https://api.trello.com/1/checklists/{checklistId}/checkItems/{checkItemId}?key={settings.apiKey}&token={settings.apiToken}";
            UnityWebRequest request = new UnityWebRequest(url, "DELETE");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        [System.Serializable]
        public class Label
        {
            public string id;
            public string idBoard;
            public string name;
            public string color;
        }

        [System.Serializable]
        public class LabelCreationQuery
        {
            public string idBoard;
            public string name;
            public string color;
        }

        public static UnityWebRequest CreateLabel(LabelCreationQuery query, ApiCallback<Label> callback)
        {
            string url = $"https://api.trello.com/1/labels?key={settings.apiKey}&token={settings.apiToken}" +
                $"&idBoard={query.idBoard}" +
                $"&name={(string.IsNullOrEmpty(query.name) ? " " : query.name)}" +
                $"&color={query.color}";

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        public static UnityWebRequest GetLabelsOfBoard(string boardId, ApiCallbackArray<Label> callback)
        {
            string url = $"https://api.trello.com/1/boards/{boardId}/labels?key={settings.apiKey}&token={settings.apiToken}";
            UnityWebRequest request = UnityWebRequest.Get(url);
            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        [System.Serializable]
        public class LabelUpdateRequest
        {
            public string name;
            public string color;
        }

        public static UnityWebRequest UpdateLabel(string labelId, LabelUpdateRequest query, ApiCallback<Label> callback)
        {
            string url = $"https://api.trello.com/1/labels/{labelId}?key={settings.apiKey}&token={settings.apiToken}" +
                $"&name={query.name}" +
                $"&color={query.color}";
            UnityWebRequest request = new UnityWebRequest(url, "PUT");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        public static UnityWebRequest DeleteLabel(string labelId, ApiCallback<Label> callback)
        {
            string url = $"https://api.trello.com/1/labels/{labelId}?key={settings.apiKey}&token={settings.apiToken}";
            UnityWebRequest request = new UnityWebRequest(url, "DELETE");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        [System.Serializable]
        public class Attachment
        {
            public string id;
            public bool isUpload;
            public string name;
            public string url;
        }

        [System.Serializable]
        public class AttachmentCreationQuery
        {
            public string name;
            public string url;
        }

        public static UnityWebRequest CreateAttachment(string idCard, AttachmentCreationQuery query, ApiCallback<Attachment> callback)
        {
            string url = $"https://api.trello.com/1/cards/{idCard}/attachments?key={settings.apiKey}&token={settings.apiToken}" +
                $"&name={query.name}" +
                $"&url={query.url}";

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        [System.Serializable]
        public class AttachmentUpdateQuery
        {
            public string name;
            public string url;
        }

        public static UnityWebRequest UpdateAttachment(string idCard, string idAttachment, AttachmentUpdateQuery query, ApiCallback<Attachment> callback)
        {
            string url = $"https://api.trello.com/1/cards/{idCard}/attachments/{idAttachment}?key={settings.apiKey}&token={settings.apiToken}" +
                 $"&name={query.name}" +
                 $"&url={query.url}";
            UnityWebRequest request = new UnityWebRequest(url, "PUT");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }

        public static UnityWebRequest DeleteAttachment(string idCard, string idAttachment, ApiCallback<Attachment> callback)
        {
            string url = $"https://api.trello.com/1/cards/{idCard}/attachments/{idAttachment}?key={settings.apiKey}&token={settings.apiToken}";
            UnityWebRequest request = new UnityWebRequest(url, "DELETE");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            EditorCoroutineUtility.StartCoroutine(SendRequest(request, callback), settings);
            return request;
        }
    }
}
