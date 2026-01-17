using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;

namespace Pinwheel.Memo
{
    //[CreateAssetMenu(menuName = "Memo/News Checker")]
    [ExecuteInEditMode]
    [InitializeOnLoad]
    public class NewsChecker : ScriptableObject
    {
        [System.Serializable]
        public class NewsEntry
        {
            public string title;
            public string description;
            public string link;
        }

        [System.Serializable]
        public class NewsCollection
        {
            public NewsEntry[] entries;
        }

        private static List<NewsEntry> m_news = new List<NewsEntry>();

        [InitializeOnLoadMethod]
        private static void Init()
        { 
            Resources.Load<NewsChecker>("Memo/NewsChecker");
        }

        private void OnEnable()
        {
            GetNews((request, response) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    m_news = new List<NewsEntry>(response.entries);
                } 
            });
        }

        private void OnDisable()
        {

        }

        protected UnityWebRequest GetNews(ApiCallback<NewsCollection> callback)
        {
            string url = "https://api.pinwheelstud.io/news";
            UnityWebRequest request = UnityWebRequest.Get(url);
            EditorCoroutineUtility.StartCoroutine(NetUtils.SendRequest(request, callback), this);
            return request;
        }

        public static NewsEntry GetFeaturedNews()
        {
            return m_news.Find(n => n.description.Contains("#mm"));
        }
    }
}
