using System;
using Excel;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
public class CSVEditor : EditorWindow
{
    [MenuItem("Tools/Csv生成c#类")]
    static void CreateCSharpCode()
    {
        CSVEditor window = EditorWindow.CreateWindow<CSVEditor>();
        window.Show();
    }
    public string PathFileName;
    string biaoti = @"/************************************************************************
该文件是通过自动生成的，禁止手动修改
作者：
日期：#1
*************************************************************************/";

    string csvpath;
    string text;
    string textname;
    string CreatPath;
    //TextAsset text;
    private void OnGUI()
    {
        GUILayout.Label("温馨小提示，用前必看：");
        GUILayout.Label("1.要生成的CSV文件的编码格式必须为UTF-8格式");
        GUILayout.Label("2.请确保要解析的CSV文件内不存在英文符号的逗号");
        GUILayout.Label("3.会自动生成管理类，请在Scripts下创建一个CSVScripts文件夹用来存放管理类");
        GUILayout.Label("请选择一个CSV文件");
        if (GUILayout.Button("选择一个CSV文件"))
        {
            csvpath = EditorUtility.OpenFilePanel("", Application.dataPath, "csv");
            text = Path.GetFileName(csvpath);
            textname = Path.GetFileNameWithoutExtension(csvpath);
        }
        if (GUILayout.Button("选择生成的路径"))
        {
            CreatPath = EditorUtility.OpenFolderPanel("", Application.dataPath, "");
        }
        if (GUILayout.Button("选择好了"))
        {
            if (string.IsNullOrEmpty(text))
            {
                UnityEngine.Debug.Log("未选择文件");
                return;
            }
            else if (string.IsNullOrEmpty(CreatPath))
            {
                UnityEngine.Debug.Log("未选择生成路径");
                return;
            }
            else
            {
                ReaderConfigFile(csvpath, textname );
                CSVManger.AddDataCSV(textname );
                CSVManger.CsvManger();
                AssetDatabase.Refresh();

            }
        }

    }

    /// <summary>
    /// 读取配置文件
    /// 将所有的类型及数据名取出
    /// </summary>
    /// <param name="path">文件路径</param>
    void ReaderConfigFile(string path,string JsonFileName)
    {
        string[] fileStr = File.ReadAllLines(path);
        PathFileName = path;
        UnityEngine.Debug.Log(fileStr);
        CreateCS(fileStr, JsonFileName);
    }

    void CreateCS(string[] reflectFileName,string JsonFileName)
    {
        /************ 写入配置路径位置与创建的文件写入流 ************/
        string CSPath = $"{CreatPath}/{JsonFileName}.cs";
        StreamWriter sw = new StreamWriter(CSPath);

        /************ 设置一些写入的格式符与变量 ************/
        //写入的行以\为换行符  \t==tab
        string tabKey = "\t";
        //参数类型
        string[] argumentType = reflectFileName[1].Split(',');
        for (int i = 0; i < argumentType.Length; i++)
        {
            argumentType[i] = argumentType[i].ToLower();
        }
        //参数名称
        string[] argumentName = reflectFileName[0].Split(',');

        //string[] argumentList = reflectFileName[1].Split(',');

        string time = DateTime.Now.ToString();
        sw.WriteLine(biaoti.Replace("#1", time));
        sw.WriteLine(GetImport());
        /************ 正式在配置流文件里开始写入代码配置 ************/
        sw.WriteLine($"public class {JsonFileName}");
        sw.WriteLine("{");
        //遍历参数列表，生成配置
        for (int i = 0; i < argumentType.Length; i++)
        {
            sw.WriteLine($"{tabKey}public {argumentType[i]} {argumentName[i]};");
        }

        sw.WriteLine("}");

        //生成解析csv文件函数
        sw.WriteLine($"public class CSVTOC{JsonFileName}");
        sw.WriteLine("{");

        sw.WriteLine($"{tabKey}public List<{JsonFileName}> {JsonFileName}_list = new List<{JsonFileName}>();");
        sw.WriteLine($"{tabKey}public void CSVTOCOpen()");
        sw.WriteLine($"{tabKey}" + "{");
        sw.WriteLine($"{tabKey}{tabKey}string json = \"{PathFileName}\";");
        sw.WriteLine($"{tabKey}{tabKey}string[] fileStr = File.ReadAllLines(json);");
        sw.WriteLine($"{tabKey}{tabKey}for (int i = 3; i < fileStr.Length; i++)" + "{");
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}string[] list_open = fileStr[i].Split(',');");
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}{JsonFileName} jsons = new {JsonFileName}();");
        for (int i = 0; i < argumentType.Length; i++)
        {
            sw.WriteLine($"{tabKey}{tabKey}if (!string.IsNullOrEmpty(list_open[{i}]))");
            sw.WriteLine($"{tabKey}{tabKey}"+"{");
            //当前不同类型表头定义不同类型
            if (argumentType[i] == "int")
            {
                sw.WriteLine($"{tabKey}{tabKey}{tabKey}jsons.{argumentName[i]} = int.Parse(list_open[{i}]);");
            }
            else if (argumentType[i] == "string")
            {
                sw.WriteLine($"{tabKey}{tabKey}{tabKey}jsons.{argumentName[i]} = list_open[{i}];");
            }
            else if (argumentType[i] == "float")
            {
                sw.WriteLine($"{tabKey}{tabKey}{tabKey}jsons.{argumentName[i]} = float.Parse(list_open[{i}]);");
            }
            sw.WriteLine($"{tabKey}{tabKey}" + "}");
        }
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}{JsonFileName}_list.Add(jsons);" + "}");

        sw.WriteLine($"{tabKey}" + "}");


        sw.WriteLine($"{tabKey}public CSVTOC{JsonFileName}()");
        sw.WriteLine($"{tabKey}" + "{");
        sw.WriteLine($"{tabKey}{tabKey}CSVTOCOpen();");
        sw.WriteLine($"{tabKey}" + "}");




        //生成调用数据并返回集合
        sw.WriteLine($"{tabKey}public List<{JsonFileName}> GetData()");
        sw.WriteLine($"{tabKey}"+"{");
        sw.WriteLine($"{tabKey}{tabKey}return {JsonFileName}_list;");

        sw.WriteLine($"{tabKey}" + "}");



        //查
        sw.WriteLine($"{tabKey}" + "//查找数据");
        sw.WriteLine($"{tabKey}public {JsonFileName} GetDataById({argumentType[0]} {argumentName[0]})");
        sw.WriteLine($"{tabKey}" + "{");
        sw.WriteLine($"{tabKey}{tabKey}for(int i=0;i<{JsonFileName}_list.Count;i++)");
        sw.WriteLine($"{tabKey}{tabKey}" + "{");
        //此处点的id需要放置在配置表的第一个
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}if({JsonFileName}_list[i].{argumentName[0]}=={argumentName[0]})");
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}" + "{");
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}{tabKey}" + $"return {JsonFileName}_list[i];");
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}" + "}");
        sw.WriteLine($"{tabKey}{tabKey}" + "}");
        sw.WriteLine($"{tabKey}{tabKey}" + "return null;");
        sw.WriteLine($"{tabKey}" + "}");

        //增
        sw.WriteLine($"{tabKey}" + "//添加数据");
        sw.WriteLine($"{tabKey}public void AddData({JsonFileName} JsonFile)");
        sw.WriteLine($"{tabKey}" + "{");
        sw.WriteLine($"{tabKey}{tabKey}{JsonFileName}_list.Add(JsonFile);");
        sw.WriteLine($"{tabKey}" + "}");


        //删除
        sw.WriteLine($"{tabKey}" + "//删除数据");
        sw.WriteLine($"{tabKey}public void DelData({argumentType[0]} {argumentName[0]})");
        sw.WriteLine($"{tabKey}" + "{");
        sw.WriteLine($"{tabKey}{tabKey}for(int i=0;i<{JsonFileName}_list.Count;i++)");
        sw.WriteLine($"{tabKey}{tabKey}" + "{");
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}if({JsonFileName}_list[i].{argumentName[0]}=={argumentName[0]})");
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}" + "{");
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}{tabKey}" + $"{JsonFileName}_list.Remove({JsonFileName}_list[i]);");
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}{tabKey}" + "break;");
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}" + "}");
        sw.WriteLine($"{tabKey}{tabKey}" + "}");
        sw.WriteLine($"{tabKey}" + "}");


        //改
        sw.WriteLine($"{tabKey}" + "//修改数据");
        sw.WriteLine($"{tabKey}public void RevampData({JsonFileName} JsonFile)");
        sw.WriteLine($"{tabKey}" + "{");
        sw.WriteLine($"{tabKey}{tabKey}for(int i=0;i<{JsonFileName}_list.Count;i++)");
        sw.WriteLine($"{tabKey}{tabKey}" + "{");
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}if({JsonFileName}_list[i].{argumentName[0]}==JsonFile.{argumentName[0]})");
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}" + "{");
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}{tabKey}" + $"{JsonFileName}_list[i]=JsonFile;");
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}{tabKey}" + "break;");
        sw.WriteLine($"{tabKey}{tabKey}{tabKey}" + "}");
        sw.WriteLine($"{tabKey}{tabKey}" + "}");
        sw.WriteLine($"{tabKey}" + "}");
        sw.WriteLine("}");

        sw.Flush();
        sw.Close();
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 加载调用数据
    /// </summary>
    /// <returns></returns>
    string GetImport()
    {
        string importStr = null;
        importStr += $"using UnityEngine;\r\n";
        importStr += $"using UnityEngine.UI;\r\n";
        importStr += $"using System;\r\n";
        importStr += $"using System.Collections;\r\n";
        importStr += $"using UnityEditor;\r\n";
        importStr += $"using System.IO;\r\n";
        importStr += $"using System.Collections.Generic;\r\n";
        return importStr;
    }
}
