using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Pinwheel.Memo
{
    public delegate void ApiCallback<ResponseType>(UnityWebRequest request, ResponseType response) where ResponseType : class, new();
    public delegate void ApiCallbackArray<ResponseType>(UnityWebRequest request, ResponseType[] response) where ResponseType : class, new();
    public delegate void ApiCallbackTexture(UnityWebRequest request, Texture2D texture);

    public delegate void AsyncTaskCallback(string error);
}
