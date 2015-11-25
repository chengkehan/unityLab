using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Reflection;
using SpaceCitizen;
using System.Xml;
using System.IO;
using System.Text;

public class LuaSublimeSnippet
{
	private static string saveToFolder = "/Users/jimCheng/Library/Application Support/Sublime Text 2/Packages/Lua/SpaceCitizen";

	private static string extention = ".sublime-snippet";

	[MenuItem("Space Citizen/Lua Sublime Snippet")]
	private static void Execute()
	{
		DeleteSnippets();

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
