using IFix;
using IFix.Core;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace PatchTest
{
    //[IFix.Interpret]
    //public class NewClass
    //{
    //    [Interpret]
    //    public static string GetString()
    //    {
    //        return "nnnnnnnnnnnnnn";
    //    }
    //}


    public class MainView : MonoBehaviour
    {
        public string fileName = "Assembly-CSharp.patch.bytes";
        public Text title;
        public Image bg;
        public Text content;

        
        private string titleStr = "IFix";
        private string contentStr = "虚拟机负责新逻辑的解析执行；注入代码负责把调用重定向到虚拟机；";
        private Color32 color = new Color32(123,123,123,200);

        
        void Start()
        {
            LoadPatch();
        }

        private void LoadPatch()
        {
            DownLoadBundle.Instance.LoadDataFromServer(fileName, (obj) =>
            {
                if (obj != null)
                {
                    Debug.Log("已下载");
                    PatchManager.Load(new MemoryStream(obj));//加载});
                }
                SetView();
            });
            //   if (File.Exists(path))
            //{
            //    Debug.Log("LoadPatch");
            //    PatchManager.Load(new FileStream(path, FileMode.Open));
            //}


        }

        [Patch]
        private void SetView()
        {
            title.text = GetString();
            content.text = contentStr;
            bg.color = color;
        }

        [Interpret]
        private string GetString()
        {
            return "123";
        }

        void Update()
        {

        }
    }
}

