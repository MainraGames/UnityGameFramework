using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Networking;
using System;
using System.IO;

namespace Pinwheel.Memo.Trello
{
    [ExecuteInEditMode]
    [InitializeOnLoad]
    //[CreateAssetMenu(menuName = "Memo/Trello Settings")]
    public class TrelloSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Api Key of Memo power up on Trello. This can be publicly available in code.
        /// </summary>
        public string apiKey => "1448434e355adfa43be579252470aa06";
        public string authId { get; internal set; }
        public string apiToken { get; internal set; }

        public TrelloApi.TrelloToken tokenInfo { get; set; }

        protected string m_idDefaultOrganization;
        public string idDefaultOrganization
        {
            get
            {
                return m_idDefaultOrganization;
            }
            internal set
            {
                m_idDefaultOrganization = value;
            }
        }
                
        protected string m_idDefaultBoard;
        public string idDefaultBoard
        {
            get
            {
                return m_idDefaultBoard;
            }
            internal set
            {
                m_idDefaultBoard = value;
            }
        }

        protected TrelloMe m_me;
        /// <summary>
        /// The current logged in Trello user.
        /// Declared here for managing life cycle.
        /// Use the alias <see cref="TrelloUser.me"/>TrelloUser.me</c>
        /// </summary>
        internal TrelloMe me
        {
            get
            {
                if (m_me == null)
                {
                    m_me = new TrelloMe();
                }
                return m_me;
            }
            set
            {
                m_me = value;
            }
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            Resources.Load<TrelloSettings>("Memo/TrelloSettings");
        }

        private void OnEnable()
        {
            LoadAuthorizationInfo();
            if (me == null)
            {
                me = new TrelloMe();
            }
        }

        private void OnDisable()
        {
            SaveAuthorizationInfo();
        }

        private const string KEY_AUTH_INFO = "memo-trello-auth-info";
        public void SaveAuthorizationInfo()
        {
            string value = $"{authId};{apiToken}";
            EditorPrefs.SetString(KEY_AUTH_INFO, value);
        }

        public void LoadAuthorizationInfo()
        {
            authId = string.Empty;
            apiToken = string.Empty;
            if (EditorPrefs.HasKey(KEY_AUTH_INFO))
            {
                string value = EditorPrefs.GetString(KEY_AUTH_INFO);
                string[] splits = value.Split(';');
                if (splits.Length == 2)
                {
                    authId = splits[0];
                    apiToken = splits[1];
                }
            }

            if (!string.IsNullOrEmpty(authId) & !string.IsNullOrEmpty(apiToken))
            {
                TrelloApi.GetTokenInfo(apiToken, (request, response) =>
                {
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        tokenInfo = response;
                        TrelloIntegration.NotifyAuthorizationResult(true);
                    }
                    else
                    {
                        authId = string.Empty;
                        apiToken = string.Empty;
                        TrelloIntegration.NotifyAuthorizationResult(false);
                    }
                });
            }
            else
            {
                TrelloIntegration.NotifyAuthorizationResult(false);
            }
        }

        public void ResetAuthorizationAndUserInfo()
        {
            authId = null;
            apiToken = null;
            me = null;
        }

        public bool IsAuthorized()
        {
            return !string.IsNullOrEmpty(authId) && !string.IsNullOrEmpty(apiToken);
        }

        [System.Serializable]
        public class ApiCallInfo
        {
            public string localPath;
            public string[] query;
            public string json;
        }

        //public List<ApiCallInfo> apiCallsInfo = new List<ApiCallInfo>();

        public void AddApiCallInfoForDebug(UnityWebRequest request)
        {
            //ApiCallInfo info = new ApiCallInfo();
            //info.localPath = $"{request.method} {request.uri.LocalPath}";
            //info.query = request.uri.Query.Split(new char[] { '?', '&' }, StringSplitOptions.RemoveEmptyEntries);

            //if (request.result == UnityWebRequest.Result.Success)
            //{
            //    info.json = request.downloadHandler.text;
            //}
            //else
            //{
            //    info.json = request.error;
            //}
            //apiCallsInfo.Add(info);
        }

        [SerializeField]
        [HideInInspector]
        protected string DATA_DIRECTORY;
        const string FILE_NAME = "data.memotrello";

        [Serializable]
        private class TrelloDefaultSelection
        {
            public string idDefaultOrganization;
            public string idDefaultBoard;
        }

        public void OnBeforeSerialize()
        {
            TrelloDefaultSelection defaultSelection = new TrelloDefaultSelection()
            {
                idDefaultOrganization = this.m_idDefaultOrganization,
                idDefaultBoard = this.m_idDefaultBoard
            };
            string json = JsonUtility.ToJson(defaultSelection);

            DATA_DIRECTORY = Utilities.GetMemoDataFolder();
            if (!Directory.Exists(DATA_DIRECTORY))
            {
                Directory.CreateDirectory(DATA_DIRECTORY);
            }
            File.WriteAllText(Path.Combine(DATA_DIRECTORY, FILE_NAME), json);
        }

        public void OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(DATA_DIRECTORY))
            {
                return;
            }

            string filePath = Path.Combine(DATA_DIRECTORY, FILE_NAME);
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                TrelloDefaultSelection defaultSelection = new TrelloDefaultSelection();
                JsonUtility.FromJsonOverwrite(json, defaultSelection);
                this.m_idDefaultOrganization = defaultSelection.idDefaultOrganization;
                this.m_idDefaultBoard = defaultSelection.idDefaultBoard;
            }
        }
    }
}
