using IFix;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

//补丁之前的配置文件
//必须添加[Configure]标签
//放在Editor文件夹下

//static变量，[IFix]标签
[Configure]
public class MyConfig
{
    [IFix]
    private static IEnumerable<Type> ToProcess
    {
        get
        {
            Debug.Log("MyConfig");
            return (from type in Assembly.Load("Assembly-CSharp").GetTypes()
                    where type.Namespace == "PatchTest" && !type.Name.Contains("<")
                    select type);
        }
    }


    //[IFix.Filter]
    //private static bool Filter(System.Reflection.MethodInfo methodInfo)
    //{
    //    return methodInfo.DeclaringType.FullName == "IFix.Test.Calculator"
    //        && (methodInfo.Name == "Div" || methodInfo.Name == "Mult");
    //}
}
