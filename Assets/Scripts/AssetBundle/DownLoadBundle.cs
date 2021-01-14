using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DownLoadBundle : MonoBehaviour
{
    public static DownLoadBundle Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private TextAsset assetDatas;

    private const string URL = "http://localhost/hotfix/";

    public void LoadDataFromServer(string fileName, Action<byte[]> callBack)
    {
        StartCoroutine(DownLoadAsset(AssetType.Byte, fileName, (obj) =>
        {
            callBack.Invoke(obj as byte[]);
        }));
    }

    private IEnumerator DownLoadAsset(AssetType type, string filename, Action<object> DownLoad = null)
    {
        //服务器上的文件路径
        string originPath = URL + filename;

        if (type == AssetType.Byte)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(originPath))
            {
                yield return request.SendWebRequest();

                if (request.isNetworkError)
                {
                    Debug.LogError($"URL = {request.url}\nError = {request.error}");
                }

                if (request.isDone)
                {
                    Debug.Log($"URL = {request.url}");
                    if (request.downloadHandler != null)
                    {
                        var file = request.downloadHandler.data;
                        DownLoad.Invoke(file);
                    }

                }
            }
        }
    }

    public enum AssetType
    {
        Byte = 0,
        //Object = 1,
        //Text = 2,
        //Texture2D = 3,
        //AudioClip = 4,
        //Video = 5,
        //AssetBundle = 6,
    }
}