using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Test : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.LogError("Onenable");
    }

    public void LogError()
    {
        Debug.LogError("执行Example方法");

    }
}
