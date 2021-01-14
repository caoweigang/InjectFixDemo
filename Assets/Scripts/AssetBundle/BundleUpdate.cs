using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using LitJson;

/**
 * 资源增量更新
 * 1.检查本地Application.persistentDataPath目录中是否有bundle_list文件
 * 2.如果没有，则从Resources目录中读取bundle_list文件
 * 3.从服务器上下载bundle_list文件，判断版本是否一致，如果一致就不用更新
 * 4.版本不一致，需要更新，更新
 * 5.将最新的bundle_list存入Application.persistentDataPath目录中
 **/
public class BundleUpdate : MonoBehaviour
{
    private static readonly string VERSION_FILE = "bundle_list";
    private string SERVER_RES_URL = "";
    private string LOCAL_RES_URL = "";
    private string LOCAL_RES_PATH = "";
    /// <summary>
    /// 本地版本json对象
    /// </summary>
    private JsonData jdLocalFile;
    /// <summary>
    /// 服务端版本json对象
    /// </summary>
    private JsonData jdServerFile;
    /// <summary>
    /// 本地资源名和路径字典
    /// </summary>
    private Dictionary<string, string> LocalBundleVersion;
    /// <summary>
    /// 服务器资源名和路径字典
    /// </summary>
    private Dictionary<string, string> ServerBundleVersion;
    /// <summary>
    /// 需要下载的文件List
    /// </summary>
    private List<string> NeedDownFiles;
    /// <summary>
    /// 是否需要更新本地版本文件
    /// </summary>
    private bool NeedUpdateLocalVersionFile = false;
    /// <summary>
    /// 下载完成委托
    /// </summary>
    /// <param name="www"></param>
    public delegate void HandleFinishDownload(WWW www);
    /// <summary>
    /// 本次一共需要更新的资源数
    /// </summary>
    int totalUpdateFileCount = 0;

    void Start()
    {
#if UNITY_EDITOR && UNITY_ANDROID
        SERVER_RES_URL = "file:///" + Application.streamingAssetsPath + "/android/";
        LOCAL_RES_URL = "file:///" + Application.persistentDataPath + "/res/";
        LOCAL_RES_PATH = Application.persistentDataPath + "/res/";
#elif UNITY_EDITOR && UNITY_IOS
        SERVER_RES_URL = "file://" + Application.streamingAssetsPath + "/ios/";
        LOCAL_RES_URL =  "file:///" + Application.persistentDataPath + "/res/";
        LOCAL_RES_PATH =  Application.persistentDataPath + "/res/";
#elif UNITY_ANDROID
        //安卓下需要使用www加载StreamingAssets里的文件,Streaming Assets目录在安卓下的路径为 "jar:file://" + Application.dataPath + "!/assets/"
        SERVER_RES_URL =  "jar:file://" + Application.dataPath + "!/assets/" + "android/";
        LOCAL_RES_URL =  "jar:file://" + Application.persistentDataPath + "!/assets/" + "/res/";
        //LOCAL_RES_URL =  "file://" + Application.persistentDataPath + "/res/";
        LOCAL_RES_PATH =  Application.persistentDataPath + "/res/";
#elif UNITY_IOS
        SERVER_RES_URL = "http://127.0.0.1/resource/ios/"
        LOCAL_RES_URL =  "file:///" + Application.persistentDataPath + "/res/";
        LOCAL_RES_PATH =  Application.persistentDataPath + "/res/";
#endif

        //初始化    
        LocalBundleVersion = new Dictionary<string, string>();
        ServerBundleVersion = new Dictionary<string, string>();
        NeedDownFiles = new List<string>();

        //加载本地version配置    
        string tmpLocalVersion = "";
        if (!File.Exists(LOCAL_RES_PATH + VERSION_FILE))
        {
            TextAsset text = Resources.Load(VERSION_FILE) as TextAsset;
            tmpLocalVersion = text.text;
        }
        else
        {
            tmpLocalVersion = File.ReadAllText(LOCAL_RES_PATH + VERSION_FILE);
        }

        //保存本地的version    
        ParseVersionFile(tmpLocalVersion, LocalBundleVersion, 0);
        //加载服务端version配置    
        StartCoroutine(this.DownLoad(SERVER_RES_URL + VERSION_FILE, delegate (WWW serverVersion)
        {
            //保存服务端version    
            ParseVersionFile(serverVersion.text, ServerBundleVersion, 1);
            //计算出需要重新加载的资源    
            CompareVersion();
            //加载需要更新的资源    
            DownLoadRes();
        }));
    }
    //依次加载需要更新的资源    
    private void DownLoadRes()
    {
        if (NeedDownFiles.Count == 0)
        {
            UpdateLocalVersionFile();
            return;
        }

        string file = NeedDownFiles[0];
        NeedDownFiles.RemoveAt(0);

        StartCoroutine(this.DownLoad(SERVER_RES_URL + file, delegate (WWW w)
        {
            //将下载的资源替换本地就的资源    
            ReplaceLocalRes(file, w.bytes);
            DownLoadRes();
        }));
    }
    private void ReplaceLocalRes(string fileName, byte[] data)
    {
        try
        {
            string filePath = LOCAL_RES_PATH + fileName;
            if (!File.Exists(filePath))
            {
                string p = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(p))
                    Directory.CreateDirectory(p);
            }
            File.WriteAllBytes(filePath, data);
        }
        catch (System.Exception e)
        {
            Debug.Log("e is " + e.Message);
        }
    }
    //更新本地的version配置    
    private void UpdateLocalVersionFile()
    {
        if (NeedUpdateLocalVersionFile)
        {
            if (!Directory.Exists(LOCAL_RES_PATH))
                Directory.CreateDirectory(LOCAL_RES_PATH);
            StringBuilder versions = new StringBuilder(jdServerFile.ToJson());
            FileStream stream = new FileStream(LOCAL_RES_PATH + VERSION_FILE, FileMode.Create);
            byte[] data = Encoding.UTF8.GetBytes(versions.ToString());
            stream.Write(data, 0, data.Length);
            stream.Flush();
            stream.Close();
        }
    }

    private void CompareVersion()
    {
        int localVersionId;
        int serverVersionId;
        if (jdLocalFile != null && jdLocalFile.Keys.Contains("id"))
            localVersionId = (int)jdLocalFile["id"];
        if (jdServerFile != null && jdServerFile.Keys.Contains("id"))
            serverVersionId = (int)jdServerFile["id"];

#if UNITY_ANDROID || UNITY_EDITOR
        NeedDownFiles.Add("android");
#endif

#if UNITY_IOS
#endif
        foreach (var version in ServerBundleVersion)
        {
            string fileName = version.Key;
            string serverHash = version.Value;
            //新增的资源    
            if (!LocalBundleVersion.ContainsKey(fileName))
            {
                NeedDownFiles.Add(fileName);
            }
            else
            {
                //需要替换的资源    
                string localHash;
                LocalBundleVersion.TryGetValue(fileName, out localHash);
                if (!serverHash.Equals(localHash))
                {
                    NeedDownFiles.Add(fileName);
                }
            }
        }
        totalUpdateFileCount = NeedDownFiles.Count;

        //本次有更新，同时更新本地的version.txt    
        NeedUpdateLocalVersionFile = NeedDownFiles.Count > 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="content"></param>
    /// <param name="dict"></param>
    /// <param name="flag">0表示本地版本文件，1表示服务器版本文件</param>
    private void ParseVersionFile(string content, Dictionary<string, string> dict, int flag)
    {
        if (content == null || content.Length == 0)
        {
            return;
        }
        JsonData jd = null;
        try
        {
            jd = JsonMapper.ToObject(content);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            return;
        }
        if (flag == 0)//本地
        {
            jdLocalFile = jd;
        }
        else if (flag == 1)//服务器
        {
            jdServerFile = jd;
        }
        else
            return;
        //获取资源对象
        JsonData resObjs = null;
        if (jd.Keys.Contains("resource"))
            resObjs = jd["resource"];
        if (resObjs != null && resObjs.IsObject && resObjs.Count > 0)
        {
            string[] resNames = new string[resObjs.Count];
            resObjs.Keys.CopyTo(resNames, 0);
            for (int i = 0; i < resNames.Length; i++)
            {
                if (resObjs.Keys.Contains(resNames[i]))
                    dict.Add(resNames[i], resObjs[resNames[i]].ToString());
            }
        }
    }

    private IEnumerator DownLoad(string url, HandleFinishDownload finishFun)
    {
        WWW www = new WWW(url);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError("www.error is " + www.error);
            yield break;
        }
        if (finishFun != null)
        {
            finishFun(www);
        }
        www.Dispose();
    }
}