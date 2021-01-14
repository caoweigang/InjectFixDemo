using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CommonConfirm : MonoBehaviour
{
    public Text m_Title;
    public Text m_Des;
    public Button m_ConfirmBtn;
    public Button m_CancleBtn;

    public void Show(string title, string des, UnityAction confirmEvent, UnityAction cancleEvent)
    {
        m_Title.text = title;
        m_Des.text = des;
        AddButtonClickListener(m_ConfirmBtn, () =>
        {
            confirmEvent();
            Destroy(gameObject);
        });
        AddButtonClickListener(m_CancleBtn, () =>
        {
            cancleEvent();
            Destroy(gameObject);
        });
    }

    public void AddButtonClickListener(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
            btn.onClick.AddListener(BtnPlaySound);
        }
    }

    /// <summary>
    /// Toggle事件监听
    /// </summary>
    /// <param name="toggle"></param>
    /// <param name="action"></param>
    public void AddToggleClickListener(Toggle toggle, UnityEngine.Events.UnityAction<bool> action)
    {
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(action);
            toggle.onValueChanged.AddListener(TogglePlaySound);
        }
    }

    /// <summary>
    /// 播放button声音
    /// </summary>
    void BtnPlaySound()
    {
    }

    /// <summary>
    /// 播放toggle声音
    /// </summary>
    /// <param name="isOn"></param>
    void TogglePlaySound(bool isOn)
    {
    }
}