using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;   //保存二进制文件必须使用的命名空间
using System.Collections.Generic;                       

public static class FileUtility
{
	public enum RootDirectoryLocation { GameDataPath, StreamingAssetsPath, PersistentDataPath, AbsolutePath }

	public static void WriteLinesToFile(string[] lines, string fileName, RootDirectoryLocation rootLocation = RootDirectoryLocation.GameDataPath)
    {
		string path = GetFullPath(rootLocation, fileName);

		//将lines字符串数组写入path指定的文件，如果文件已经存在，将被覆盖
		File.WriteAllLines(path, lines);
	}

	public static string[] ReadLinesFromFile(string fileName, RootDirectoryLocation rootLocation = RootDirectoryLocation.GameDataPath)
    {
		string path = GetFullPath(rootLocation, fileName);

		//如果要读取的文件存在，就读取里面的内容，按行写入字符串数组
	    if (File.Exists(path))
        {	
			string[] lines = File.ReadAllLines(path);
		    return lines;
        }
        else
        {
			throw new System.Exception("文件不存在！");
        }
    }

	public static void WriteToBinaryFile(object data, string fileName, RootDirectoryLocation rootLocation = RootDirectoryLocation.GameDataPath)
	{
		string fullPathFileName = GetFullPath(rootLocation, fileName);
		BinaryFormatter bf = new BinaryFormatter();                         //声明一个BinaryFormatter对象实例bf

		FileStream file = File.Create(fullPathFileName);   					//创建要保存的文件，返回对这个文件进行操作的文件流file   

		bf.Serialize(file,data);                                            //将data对象的信息转换成二进制信息流，存放入file所对应的文件
		file.Close();                                                       //关闭file所对应的文件
	}

	public static object ReadFromBinaryFile(string fileName, RootDirectoryLocation rootLocation = RootDirectoryLocation.GameDataPath)
	{
		string fullPathFileName = GetFullPath(rootLocation,fileName);

		if (File.Exists (fullPathFileName) ) {	    						//判断要读取的二进制文件是否存在
			BinaryFormatter bf = new BinaryFormatter();                     //声明一个BinaryFormatter对象实例bf
			FileStream file = File.Open(fullPathFileName, FileMode.Open);   //打开文件，返回对这个文件进行操作的文件流file
			object load = bf.Deserialize (file);							//将文件数据反序列化，转换为对象
			file.Close();              										//关闭file所对应的文件

			return load;		//返回从文件数据转换而来的对象
		}
		else 
		{
			throw new System.Exception("文件不存在！");
		}
	}

	public static string GetFullPath(RootDirectoryLocation rootLocation, string subPath)
	{
		if(rootLocation == RootDirectoryLocation.AbsolutePath)
		{
			return subPath;
		}

		string rootPath;
		switch (rootLocation)
		{
			case RootDirectoryLocation.GameDataPath:
			rootPath = Application.dataPath;
			break;
			case RootDirectoryLocation.StreamingAssetsPath:
			rootPath = Application.streamingAssetsPath;
			break;
			case RootDirectoryLocation.PersistentDataPath:
			rootPath = Application.persistentDataPath;
			break;
			default:
				throw new ArgumentOutOfRangeException ();
		}

		return rootPath + "/" + subPath;
	}
}