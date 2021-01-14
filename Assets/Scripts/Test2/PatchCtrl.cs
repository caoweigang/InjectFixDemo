using IFix.Core;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PatchCtrl : MonoBehaviour
{
    public string fileName = "Assembly-CSharp.patch.bytes";


    private void Start()
    {   
        LoadPatch();
    }

    private void LoadPatch()
    {
        DownLoadBundle.Instance.LoadDataFromServer(fileName, (obj) =>
        {
            if (obj!=null)
            {
                Debug.Log("已下载");
                PatchManager.Load(new MemoryStream(obj));//加载});
            }
            
        });
    }
}