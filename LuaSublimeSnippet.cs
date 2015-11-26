using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Reflection;
using SpaceCitizen;
using System.Xml;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class LuaSublimeSnippet
{
	private static string saveToFolder = "/Users/jimCheng/Library/Application Support/Sublime Text 2/Packages/Lua/SpaceCitizen";

	private static string extention = ".sublime-snippet";

	[MenuItem("Space Citizen/Lua Sublime Snippet")]
	private static void Execute()
	{
		DeleteSnippets();

		ProcessCSharp();

		ProcessLua();
	}

	private static void ProcessLua()
	{
		if(!Directory.Exists(Application.streamingAssetsPath))
		{
			return;
		}

		string[] files = Directory.GetFiles(Application.streamingAssetsPath);
		foreach(var file in files)
		{
			if(file.EndsWith(".lua"))
			{
				string fileContent = File.ReadAllText(file);
				using(StringReader reader = new StringReader(fileContent))
				{
					ProcessLuaScript(reader);
				}
			}
		}
	}

	private static void ProcessLuaScript(StringReader reader)
	{
		while(true)
		{
			string line = reader.ReadLine();
			if(line == null)
			{
				break;
			}
			line = line.TrimStart().TrimEnd();

			if(line.StartsWith("--"))
			{
				continue;
			}

			{
				string className = null;
				if(CheckClassDeclaration(line, out className))
				{

					continue;
				}
			}

			{
				string className = null;
				string methodName = null;
				string[] args = null;
				if(CheckFunctionDefinition(line, out className, out methodName, out args))
				{

					continue;
				}
			}

			{
				string type = null;
				string description = null;
				if(CheckImportType(line, out type, out description))
				{

					continue;
				}
			}

			{
				string msgName = null;
				string description = null;
				if(CheckNotificationCenterMessageID(line, out msgName, out description))
				{

					continue;
				}
			}
		}
	}

	private static bool CheckClassDeclaration(string line, out string className)
	{
		Regex regex = new Regex(@".+=.*class(.*)");
		if(regex.IsMatch(line))
		{
			className = line.Split('=')[0].Trim();
			return true;
		}
		else
		{
			className = null;
			return false;
		}
	}

	private static bool CheckFunctionDefinition(string line, out string className, out string methodName, out string[] args)
	{
		Regex regex1 = new Regex(@"^function.+:.+(.*)");
		Regex regex2 = new Regex(@"^function.+\..+(.*)");
		bool match1 = regex1.IsMatch(line);
		bool match2 = regex2.IsMatch(line);
		if(match1 || match2)
		{
			char flag = match1 ? ':' : '.';
			className = line.Substring("function".Length, line.IndexOf(flag) - "function".Length).Trim();
			methodName = line.Substring(line.IndexOf(flag) + 1, line.IndexOf('(') - line.IndexOf(flag) - 1).Trim();
			string newArgs = line.Substring(line.IndexOf('(') + 1, line.IndexOf(')') - line.IndexOf('(') - 1).Trim();
			if(string.IsNullOrEmpty(newArgs))
			{
				args = null;
			}
			else if(!newArgs.Contains(","))
			{
				args = new string[]{ newArgs };
			}
			else
			{
				args = newArgs.Split(',');
				int numArgs = args == null ? 0 : args.Length;
				for(int i = 0; i < numArgs; ++i)
				{
					args[i] = args[i].Trim();
				}
			}
			// Debug Print
//			string info = className + ":" + methodName + "(";
//			int argsCount = args == null ? 0 : args.Length;
//			for(int i = 0; i < argsCount; ++i)
//			{
//				info += args[i];
//				if(i != argsCount - 1)
//				{
//					info += ", ";
//				}
//			}
//			info += ")";
//			Debug.LogError(info);
			return true;
		}
		else
		{
			className = null;
			methodName = null;
			args = null;
			return false;
		}
	}

	private static bool CheckImportType(string line, out string type, out string description)
	{
		Regex regex = new Regex(@".+=.*luanet\.import_type(.+)");
		if(regex.IsMatch(line))
		{
			string[] blocks = line.Split('=');
			type = blocks[0].Trim();
			description = blocks[1].Trim();
			return true;
		}
		else
		{
			type = null;
			description = null;
			return false;
		}
	}

	private static bool CheckNotificationCenterMessageID(string line, out string msgName, out string description)
	{
		Regex regex = new Regex(@"^[A-Z_]+\s=\s\d");
		if(regex.IsMatch(line))
		{
			string[] blocks = line.Split('=');
			msgName = blocks[0].Trim();
			description = line;
			return true;
		}
		else
		{
			msgName = null;
			description = null;
			return false;
		}
	}

	private static void ProcessCSharp()
	{
		Assembly assemably = typeof(IamInAssemblyCSharp).Assembly;
		Type[] types = assemably.GetTypes();
		foreach(var type in types)
		{
			if(type.Namespace == "SpaceCitizen" && !type.IsNested)
			{
				NewSnippet(type.Name, type.Name, type.ToString(), type.ToString());
				
				//				PropertyInfo[] properties = type.GetProperties();
				//				MemberInfo[] members = type.GetMembers();
				
				MethodInfo[] methods = type.GetMethods();
				int numMethods = methods == null ? 0 : methods.Length;
				for(int methodIndex = 0; methodIndex < numMethods; ++methodIndex)
				{
					var method = methods[methodIndex];
					if(method.IsPublic && method.DeclaringType == type)
					{
						string content = method.Name + "(";
						string tabTrigger = method.Name;
						string description = type.Name + ":" + method.Name + "(";
						string fileName = type.ToString() + "." + method.Name + "." + methodIndex;
						
						ParameterInfo[] parameters = method.GetParameters();
						int numParameters = parameters == null ? 0 : parameters.Length;
						for(int parameterIndex = 0; parameterIndex < numParameters; ++parameterIndex)
						{
							var parameter = parameters[parameterIndex];
							description += parameter.Name;
							content += "${" + (parameterIndex + 1) + ":" + parameter.Name + "}";
							if(parameterIndex != numParameters - 1)
							{
								description += ", ";
								content += ", ";
							}
						}
						description += ")";
						content += ")";
						
						NewSnippet(content, tabTrigger, description, fileName);
					}
				}
			}
		}
	}

	private static void DeleteSnippets()
	{
		if(Directory.Exists(saveToFolder))
		{
			string[] files = Directory.GetFiles(saveToFolder);
			foreach(var file in files)
			{
				if(file.EndsWith(extention))
				{
					File.Delete(file);
				}
			}
		}
	}

	private static void NewSnippet(string content, string tabTrigger, string description, string fileName)
	{
		XmlDocument doc = new XmlDocument();
		{
			XmlElement rootNode = doc.CreateElement("snippet");
			doc.AppendChild(rootNode);
			{
				XmlElement contentNode = doc.CreateElement("content");
				rootNode.AppendChild(contentNode);
				{
					XmlCDataSection cdataNode = doc.CreateCDataSection(content);
					contentNode.AppendChild(cdataNode);
				}

				XmlElement tabTriggerNode = doc.CreateElement("tabTrigger");
				rootNode.AppendChild(tabTriggerNode);
				{
					XmlText textNode = doc.CreateTextNode(tabTrigger);
					tabTriggerNode.AppendChild(textNode);
				}

				XmlElement scopeNode = doc.CreateElement("scope");
				rootNode.AppendChild(scopeNode);
				{
					XmlText textNode = doc.CreateTextNode("source.lua");
					scopeNode.AppendChild(textNode);
				}

				XmlElement descriptionNode = doc.CreateElement("description");
				rootNode.AppendChild(descriptionNode);
				{
					XmlText textNode = doc.CreateTextNode(description);
					descriptionNode.AppendChild(textNode);
				}
			}
		}

		StringBuilder sb = new StringBuilder();
		StringWriter sw = new StringWriter(sb);
		XmlTextWriter xtw = null;
		try
		{
			xtw = new XmlTextWriter(sw);
			xtw.Formatting = Formatting.Indented;
			xtw.Indentation = 1;
			xtw.IndentChar = '\t';
			doc.WriteTo(xtw);
		}
		catch(Exception e)
		{
			throw e;
		}
		finally
		{
			if(xtw != null)
			{
				xtw.Close();
				xtw = null;
			}
		}

		File.WriteAllText(saveToFolder + "/" + fileName + ".sublime-snippet", sb.ToString());
	}
}
