using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

namespace HotFixTest
{
    public class GameStart : MonoBehaviour
    {
        //public Image processImage;
        //public Text messageText;

        public Transform uiparent;

        private void Awake()
        {
#if UNITY_EDITOR&&!SIMULATE
            StartOnFinish();
#else
            if (HotPatchManager.Instance.ComputeUnPackFile())
            {
                //messageText.text = "解压中...";

                HotPatchManager.Instance.StartUnackFile(() =>
               {
                   HotFix();
               });
            }
            else
            {
                HotFix();
            }
#endif
        }

        private void StartOnFinish()
        {
            StartCoroutine(StartGame());
        }

        private void HotFix()
        {
            //检查网络
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                //网络异常
                OpenCommonConfirm("网络连接失败", "网络连接失败,请检查网络是否正常.", Application.Quit, Application.Quit);
            }
            else
            {
                CheckVersion();
            }

            //检查热更版本
        }
        private void CheckVersion()
        {
            Debug.LogError("CheckVersion");
            HotPatchManager.Instance.CheckVersion(hot =>
            {
                Debug.LogError("hot==" + hot);
                if (hot)
                {
                    //提示玩家是否确定热更下载
                    OpenCommonConfirm("热更确定",
                        string.Format("当前版本为{0},有{1:F}M大小的热更包,是否确定下载?", HotPatchManager.Instance.CurVersion,
                            HotPatchManager.Instance.LoadSumSize / 1024.0f), OnClickStartDownload, OnClickCancelDownload);
                }
                else
                {
                    StartOnFinish();
                }
            });
        }
        void OnClickStartDownload()
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
                {
                    OpenCommonConfirm("确认下载", "现在使用的是数据流量，是否继续下载?", StartDownload, OnClickCancelDownload);
                }
            }
            else
            {
                StartDownload();
            }
        }

        void StartDownload()
        {
            //m_Panel.m_SliderTopText.text = "下载中...";
            //m_Panel.m_InfoPanel.SetActive(true);
            //m_Panel.m_hotContentText.text = HotPatchManager.Instance.CurrentPatches.Des;

            StartCoroutine(HotPatchManager.Instance.StartDownLoadAB(StartOnFinish));
        }

        void OnClickCancelDownload()
        {
            Application.Quit();
        }

        public IEnumerator StartGame()
        {
            //processImage.fillAmount = 0f;
            yield return null;
            //messageText.text = "加载本地数据...";
            AssetBundleManager.Instance.LoadAssetBundleConfig();

            //image.fillAmount = 0.1f;
            //yield return null;
            //text.text = "加载dll...";
            //ILRuntimeManager.Instance.Init();

            //image.fillAmount = 0.2f;
            //yield return null;
            //text.text = "加载数据表...";
            //LoadConfiger();

            //image.fillAmount = 0.6f;
            //yield return null;
            //text.text = "加载配置文件...";

            //image.fillAmount = 0.9f;
            //yield return null;
            //text.text = "初始化场景...";
            //GameMapManager.Instance.Init(this);

            //processImage.fillAmount = 1f;
            yield return null;
        }


        public void OpenCommonConfirm(string title, string des, UnityAction confirmAction, UnityAction cancleAction)
        {
            GameObject commonObj = Instantiate(Resources.Load<GameObject>("CommonConfirm"));
            commonObj.transform.SetParent(uiparent, false);
            CommonConfirm commonItem = commonObj.GetComponent<CommonConfirm>();
            commonItem.Show(title, des, confirmAction, cancleAction);
        }
    }
}