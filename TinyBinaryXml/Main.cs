using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;

namespace TinyBinaryXml
{
	class MainClass
	{
		public static void Main (string[] args)
		{	
			Console.WriteLine ("Hello World!");

			{
//				TestInnerText();
			}

			{
				SerializeBuff();
				DeserializeBuff();
			}

			{
//				SerializeInnerText();
//				DeserializeInnerText();
			}
		}

		private static void SerializeInnerText()
		{
			TbXmlSerializer convertor = new TbXmlSerializer();
			byte[] bytes = convertor.SerializeXmlString(File.ReadAllText("/Users/jimCheng/projects/XmlPerformanceTest/Assets/Resources/InnerText.xml"));
			File.WriteAllBytes("/Users/jimCheng/Desktop/BinaryXml.bytes", bytes);
		}

		private static void DeserializeInnerText()
		{
			DateTime time = DateTime.Now;

			TbXml tbXml = TbXml.Load(File.ReadAllBytes("/Users/jimCheng/Desktop/BinaryXml.bytes"));
			List<TbXmlNode> nodes = tbXml.docNode.GetNodes("root/item");
			
			Console.WriteLine(DateTime.Now - time);

			Console.WriteLine(nodes.Count);
			Console.WriteLine(nodes[0].GetStringValue("name"));
			Console.WriteLine(nodes[0].text);
			Console.WriteLine(nodes[0].GetNodes("subItem")[0].GetStringValue("name"));
			Console.WriteLine(nodes[0].GetNodes("subItem")[0].text);
		}

		private static void SerializeBuff()
		{
			TbXmlSerializer convertor = new TbXmlSerializer();
			byte[] bytes = convertor.SerializeXmlString(File.ReadAllText("/Users/jimCheng/projects/XmlPerformanceTest/Assets/Resources/Buff.xml"));
			File.WriteAllBytes("/Users/jimCheng/Desktop/BinaryXml.bytes", bytes);
		}

		private static void DeserializeBuff()
		{
			DateTime time = DateTime.Now;

			TbXml tbXml = TbXml.Load(File.ReadAllBytes("/Users/jimCheng/Desktop/BinaryXml.bytes"));
			List<TbXmlNode> nodes = tbXml.docNode.GetNodes("Buff/Property");
			
			Console.WriteLine(DateTime.Now - time);
			
			Console.WriteLine(nodes.Count);
			Console.WriteLine(nodes[0].GetByteValue("Position"));
		}

		private static void TestInnerText()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(File.ReadAllText("/Users/jimCheng/projects/XmlPerformanceTest/Assets/Resources/InnerText.xml"));

			XmlNodeList nodeList = doc.SelectNodes("root/item");
			foreach(XmlNode node in nodeList)
			{
				Console.WriteLine(node.InnerText);
			}
		}
	}
}
