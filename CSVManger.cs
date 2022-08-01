using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
public class CSVManger : EditorWindow
{
    static StreamWriter sw;
    static string str1;
    static string str2;
    static string str3;
    static string Space = "\t";
    
    public static void AddDataCSV(string name) 
    {
        str2 += $"{Space}"+ "CSVTOC" + name + $" {name}_Data;" + "\r\n"; 
        str3 += $"{Space}{Space}{name}_Data=new CSVTOC{name}();" + "\r\n";
        str1 += $"{Space}public " + "CSVTOC" + name + " Get" + "CSVTOC" + name + "()"+"\r\n";
        str1 += $"{Space}" + "{" + "\r\n";
        str1 += $"{Space}{Space}" + "return "+$"{name}_Data;" + "\r\n";
        str1 += $"{Space}" + "}" + "\r\n";
        if (File.Exists("Assets/str1Data.txt"))
        {
            string data1 = File.ReadAllText("Assets/str1Data.txt");
            str1 += data1;
            string data2 = File.ReadAllText("Assets/str2Data.txt");
            str2 += data2;
            string data3 = File.ReadAllText("Assets/str3Data.txt");
            str3 += data3;
        }
        File.WriteAllText("Assets/str1Data.txt", str1);
        File.WriteAllText("Assets/str2Data.txt", str2);
        File.WriteAllText("Assets/str3Data.txt", str3);
    }
    public static void CsvManger()
    {
        string CSPath = $"{Application.dataPath + "/Scripts/CSVScripts"}/CSVDataManger.cs";
        sw = new StreamWriter(CSPath);
        
        string Import = GetImport();
        sw.WriteLine(Import);
        
        sw.WriteLine("public class CSVDataManger");
        sw.WriteLine("{");
        sw.WriteLine(str2);
        sw.WriteLine($"{Space}private static CSVDataManger instance;");
        sw.WriteLine($"{Space}public static CSVDataManger Instance");
        sw.WriteLine($"{Space}" + "{");
        sw.WriteLine($"{Space}{Space}" + "get");
        sw.WriteLine($"{Space}{Space}" + "{");
        sw.WriteLine($"{Space}{Space}{Space}" + "if(instance==null)");
        sw.WriteLine($"{Space}{Space}{Space}" + "{");
        sw.WriteLine($"{Space}{Space}{Space}{Space}" + "instance=new CSVDataManger();");
        sw.WriteLine($"{Space}{Space}{Space}" + "}");
        sw.WriteLine($"{Space}{Space}{Space}" + "return instance;");
        sw.WriteLine($"{Space}{Space}" + "}");
        sw.WriteLine($"{Space}" + "}");
        sw.WriteLine($"{Space}" + "public CSVDataManger()");
        sw.WriteLine($"{Space}" + "{");
        sw.WriteLine(str3);
        sw.WriteLine($"{Space}" + "}");
        sw.WriteLine(str1);
        sw.WriteLine("}");
        sw.Flush();
        sw.Close();
    }
    static string GetImport()
    {
        string importStr = null;
        importStr += $"using UnityEngine;\r\n";
        importStr += $"using System;\r\n";
        importStr += $"using System.Collections;\r\n";
        return importStr;
    }
}
