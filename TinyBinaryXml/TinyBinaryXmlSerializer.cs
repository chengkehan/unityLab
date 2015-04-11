using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;

namespace TinyBinaryXml
{
	public class TinyBinaryXmlSerializer
	{
		private List<TinyBinaryXmlNodeTemplate> nodeTemplates = new List<TinyBinaryXmlNodeTemplate>();

		private List<TinyBinaryXmlNodeInstance> nodeInstances = new List<TinyBinaryXmlNodeInstance>();

		public byte[] SerializeXmlString(string xmlString)
		{
			if(string.IsNullOrEmpty(xmlString))
			{
				return null;
			}

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlString);

			TinyBinaryXmlNodeInstance docNodeInstance = new TinyBinaryXmlNodeInstance();
			docNodeInstance.childrenIds = new List<ushort>();

			XmlNodeList xmlNodeList = doc.ChildNodes;
			foreach(XmlNode xmlNode in xmlNodeList)
			{
				if(xmlNode.NodeType == XmlNodeType.Element)
				{
					ProcessXmlNode(docNodeInstance, xmlNode);
				}
			}

			BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(), Encoding.UTF8);
			Serialize(binaryWriter);

			byte[] buffer = new byte[binaryWriter.BaseStream.Length];
			binaryWriter.BaseStream.Position = 0;
			binaryWriter.BaseStream.Read(buffer, 0, (int)binaryWriter.BaseStream.Length);
			binaryWriter.Close();
			binaryWriter.Dispose();

			return buffer;
		}

		private void ProcessXmlNode(TinyBinaryXmlNodeInstance parentTinyBinaryXmlNodeInstance, XmlNode xmlNode)
		{
			TinyBinaryXmlNodeTemplate nodeTemplate = GetNodeTemplate(xmlNode);
			if(nodeTemplate == null)
			{
				nodeTemplate = new TinyBinaryXmlNodeTemplate();
				nodeTemplates.Add(nodeTemplate);
				nodeTemplate.attributeNames = new List<string>();
				nodeTemplate.attributeTypes = new List<TINY_BINARY_XML_ATTRIBUTE_TYPE>();
				nodeTemplate.id = TinyBinaryXmlNodeTemplate.idInc++;
				nodeTemplate.name = xmlNode.Name;
				foreach(XmlAttribute xmlAttribute in xmlNode.Attributes)
				{
					string attributeString = xmlAttribute.Value;
					float attributeFloat;
					if(float.TryParse(attributeString, out attributeFloat))
					{
						nodeTemplate.attributeTypes.Add(TINY_BINARY_XML_ATTRIBUTE_TYPE.FLOAT);
					}
					else
					{
						nodeTemplate.attributeTypes.Add(TINY_BINARY_XML_ATTRIBUTE_TYPE.STRING);
					}
					nodeTemplate.attributeNames.Add(xmlAttribute.Name);
				}
			}

			TinyBinaryXmlNodeInstance nodeInstance = new TinyBinaryXmlNodeInstance();
			nodeInstances.Add(nodeInstance);
			nodeInstance.attributeValues = new List<object>();
			nodeInstance.childrenIds = new List<ushort>();
			nodeInstance.id = TinyBinaryXmlNodeInstance.idInc++;
			nodeInstance.templateId = nodeTemplate.id;
			parentTinyBinaryXmlNodeInstance.childrenIds.Add(nodeInstance.id);
			foreach(XmlAttribute xmlAttribute in xmlNode.Attributes)
			{
				string attributeString = xmlAttribute.Value;
				float attributeFloat;
				if(float.TryParse(attributeString, out attributeFloat))
				{
					nodeInstance.attributeValues.Add(attributeFloat);
				}
				else
				{
					nodeInstance.attributeValues.Add(attributeString);
				}
			}

			foreach(XmlNode subXmlNode in xmlNode.ChildNodes)
			{
				if(subXmlNode.NodeType == XmlNodeType.Element)
				{
					ProcessXmlNode(nodeInstance, subXmlNode);
				}
				else if(subXmlNode.NodeType == XmlNodeType.Text || subXmlNode.NodeType == XmlNodeType.CDATA)
				{
					if(nodeInstance.text == null)
					{
						nodeInstance.text = subXmlNode.Value;
					}
					else
					{
						nodeInstance.text += subXmlNode.Value;
					}
				}
			}
		}

		private TinyBinaryXmlNodeTemplate GetNodeTemplate(ushort templateId)
		{
			foreach(TinyBinaryXmlNodeTemplate nodeTemplate in nodeTemplates)
			{
				if(nodeTemplate.id == templateId)
				{
					return nodeTemplate;
				}
			}
			return null;
		}

		private TinyBinaryXmlNodeTemplate GetNodeTemplate(XmlNode xmlNode)
		{
			foreach(TinyBinaryXmlNodeTemplate nodeTemplate in nodeTemplates)
			{
				if(XmlNodeMatchTemplate(xmlNode, nodeTemplate))
				{
					return nodeTemplate;
				}
			}
			return null;
		}

		private bool XmlNodeMatchTemplate(XmlNode xmlNode, TinyBinaryXmlNodeTemplate nodeTemplate)
		{
			if(nodeTemplate.name != xmlNode.Name)
			{
				return false;
			}

			foreach(XmlAttribute xmlAttribute in xmlNode.Attributes)
			{
				if(nodeTemplate.attributeNames != null && !nodeTemplate.attributeNames.Contains(xmlAttribute.Name))
				{
					return false;
				}
			}
			return xmlNode.Attributes.Count == nodeTemplate.attributeNames.Count;
		}

		private void Serialize(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((ushort)nodeTemplates.Count);
			foreach(TinyBinaryXmlNodeTemplate nodeTemplate in nodeTemplates)
			{
				SerializeNodeTemplate(binaryWriter, nodeTemplate);
			}

			binaryWriter.Write((ushort)nodeInstances.Count);
			foreach(TinyBinaryXmlNodeInstance nodeInstance in nodeInstances)
			{
				SerializeNodeInstance(binaryWriter, nodeInstance);
			}
		}

		private void SerializeNodeTemplate(BinaryWriter binaryWriter, TinyBinaryXmlNodeTemplate nodeTemplate)
		{
			binaryWriter.Write(nodeTemplate.id);

			binaryWriter.Write(nodeTemplate.name);

			binaryWriter.Write((ushort)nodeTemplate.attributeNames.Count);
			foreach(string attributeName in nodeTemplate.attributeNames)
			{
				binaryWriter.Write(attributeName);
			}

			foreach(TINY_BINARY_XML_ATTRIBUTE_TYPE attributeType in nodeTemplate.attributeTypes)
			{
				binaryWriter.Write((byte)attributeType);
			}
		}

		private void SerializeNodeInstance(BinaryWriter binaryWriter, TinyBinaryXmlNodeInstance nodeInstance)
		{
			TinyBinaryXmlNodeTemplate nodeTemplate = GetNodeTemplate(nodeInstance.templateId);

			binaryWriter.Write(nodeInstance.id);

			binaryWriter.Write(nodeInstance.templateId);

			binaryWriter.Write((ushort)nodeInstance.childrenIds.Count);
			foreach(ushort childId in nodeInstance.childrenIds)
			{
				binaryWriter.Write(childId);
			}
			
			int attributeIndex = 0;
			foreach(object attributeValue in nodeInstance.attributeValues)
			{
				if(nodeTemplate.attributeTypes[attributeIndex] == TINY_BINARY_XML_ATTRIBUTE_TYPE.FLOAT)
				{
					binaryWriter.Write((float)attributeValue);
				}
				else
				{
					binaryWriter.Write((string)attributeValue);
				}
				++attributeIndex;
			}

			if(string.IsNullOrEmpty(nodeInstance.text))
			{
				binaryWriter.Write((byte)0);
			}
			else
			{
				binaryWriter.Write((byte)1);
				binaryWriter.Write(nodeInstance.text);
			}
		}
	}
}

