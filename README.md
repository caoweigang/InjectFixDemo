# InjectFixDemo

#### 介绍
新项目中使用热修复，使用hfs模拟服务器，补丁上传到模拟服务器中，unity再下载使用

#### 安装Inject到新项目中
1. 示例代码中VSProj下的buile_for_unity.batWenjian ,首行改为Unity的安装目录
2. 运行build_for_unity文件
3. 之后可以在文件夹中看到IFixToolKit文件夹，此文件夹复制到项目的Assets同级目录
4. 示例项目中的IFix文件夹和Plugins文件夹复制到Assets文件夹下


#### 配置类预处理代码

和xLua类似，热补丁的实现依赖于提前做些静态代码插入，对热更的类进行预处理，两种方式：指定类、指定命名空间下的所有类
注意：配置文件必须添加[Configure]标签，放在Editor文件夹下

```
[Configure]
public class MyConfig
{
    [IFix]
    private static IEnumerable<Type> ToProcess
    {
        get
        {
            return (from type in Assembly.Load("Assembly-CSharp").GetTypes()
                    where type.Namespace == "XXXXX" && !type.Name.Contains("<")
                    select type);
        }
    }

    [IFix]
    private static IEnumerable<Type> hotfix
    {
        get
        {
            return new List<Type>()
            {
                typeof(HotFixMgr),
                typeof(MyGame.TestScr),
                //AnotherClass在Pro Standard Assets下，会编译到Assembly-CSharp-firstpass.dll下，用来演示多dll的修复
                //typeof(AnotherClass),
            };
        }
    }

    //过滤：
      [IFix.Filter]
    private static bool Filter(System.Reflection.MethodInfo methodInfo)
    {
        return methodInfo.DeclaringType.FullName == "IFix.Test.Calculator"
            && (methodInfo.Name == "Div" || methodInfo.Name == "Mult");
    }
}
```



#### 加载补丁文件Assembly-CSharp.patch代码

在项目启动时候加载补丁文件
```
private async void DownloadScriptFixPatch()
{
     //#if !UNITY_EDITOR
     var patch = Resources.Load<TextAsset>("Assembly-CSharp.patch");
     if (patch != null)
     {
        //UnityEngine.Debug.Log("loading Assembly-CSharp.patch ...");
        //var sw = Stopwatch.StartNew();
        PatchManager.Load(new MemoryStream(patch.bytes));
        //UnityEngine.Debug.Log("patch Assembly-CSharp.patch, using " + sw.ElapsedMilliseconds + " ms");
    }
    //#endif
    var patch = await AssetCacheService.Instance.LoadByteAssetFromServer("FIX/Assembly-CSharp.patch.bytes");
    if (patch != null)
    {
         PatchManager.Load(new MemoryStream(patch));
    }
}
```
#### 生成补丁并使用的过程（仅修改方法，不添加属性方法和类）

1.  在需要修改的方法打上[Patch]标签
```
[Patch]
private void SetView()
{
    title.text = GetString();
    content.text = contentStr;
    bg.color = color;
}
```
2.  修改过代码之后使用InjectFix/Fix，生成Assembly-CSharp.patch补丁文件，即包含了当前修改内容
3.  上传补丁文件到服务器
4.  在Unity中模拟时放在Resource文件中，并且需要InjectFit/Inject


#### 添加属性、新增方法、新增类使用[Interpret]
```
private string name;//这个name字段是原生的

public string Name
{
    [IFix.Interpret]
    set
    {
    	name = value;    
    }
    [IFix.Interpret]
    get
    {
        return name;
    }
}
        
        
[Interpret]
private string GetString()
{
    return "123";
}

[IFix.Interpret]
public class NewClass
{
    ...类中增加的方法也应打标签
}
```