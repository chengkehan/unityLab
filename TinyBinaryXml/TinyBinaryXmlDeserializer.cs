using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TinyBinaryXml
{
	public class TinyBinaryXmlDeserializer
	{
		public TinyBinaryXml DeserializeXmlBytes(byte[] xmlBytes)
		{
			if(xmlBytes == null || xmlBytes.Length == 0)
			{
				return null;
			}

			TinyBinaryXml tinyBinaryXml = new TinyBinaryXml();
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(xmlBytes));

			ushort numNodeTemplates = binaryReader.ReadUInt16();
			tinyBinaryXml.nodeTemplates = new List<TinyBinaryXmlNodeTemplate>(numNodeTemplates);
			for(ushort i = 0; i < numNodeTemplates; ++i)
			{
				DeserializeNodeTemplate(binaryReader, i, tinyBinaryXml);
			}

			ushort numNodeInstances = binaryReader.ReadUInt16();
			tinyBinaryXml.nodeInstances = new List<TinyBinaryXmlNodeInstance>(numNodeInstances);
			for(ushort i = 0; i < numNodeInstances; ++i)
			{
				DeserializeNodeInstance(binaryReader, i, tinyBinaryXml);
				tinyBinaryXml.nodeInstances[i].tinyBinaryXml = tinyBinaryXml;
			}

			tinyBinaryXml.docNodeInstance = new TinyBinaryXmlNodeInstance();
			tinyBinaryXml.docNodeInstance.childrenIds = new List<ushort>();
			tinyBinaryXml.docNodeInstance.childrenIds.Add(0);
			tinyBinaryXml.docNodeInstance.tinyBinaryXml = tinyBinaryXml;

			binaryReader.Close();
			binaryReader.Dispose();

			return tinyBinaryXml;
		}

		private void DeserializeNodeTemplate(BinaryReader binaryReader, ushort index, TinyBinaryXml tinyBinaryXml)
		{
			TinyBinaryXmlNodeTemplate nodeTemplate = new TinyBinaryXmlNodeTemplate();
			tinyBinaryXml.nodeTemplates.Add(nodeTemplate);

			nodeTemplate.id = binaryReader.ReadUInt16();
			nodeTemplate.name = binaryReader.ReadString();

			ushort numAttributes = binaryReader.ReadUInt16();

			if(numAttributes > 0)
			{
				nodeTemplate.attributeNames = new List<string>(numAttributes);
				nodeTemplate.attributeNameIndexMapping = new Dictionary<string, int>(numAttributes);
				for(int i = 0; i < numAttributes; ++i)
				{
					string attributeName = binaryReader.ReadString();
					nodeTemplate.attributeNames.Add(attributeName);
					nodeTemplate.attributeNameIndexMapping[attributeName] = i;
				}

				nodeTemplate.attributeTypes = new List<TINY_BINARY_XML_ATTRIBUTE_TYPE>(numAttributes);
				for(int i = 0; i < numAttributes; ++i)
				{
					nodeTemplate.attributeTypes.Add((TINY_BINARY_XML_ATTRIBUTE_TYPE)binaryReader.ReadByte());
				}
			}
		}

		private void DeserializeNodeInstance(BinaryReader binaryReader, ushort index, TinyBinaryXml tinyBinaryXml)
		{
			TinyBinaryXmlNodeInstance nodeInstance = new TinyBinaryXmlNodeInstance();
			tinyBinaryXml.nodeInstances.Add(nodeInstance);

			nodeInstance.id = binaryReader.ReadUInt16();
			nodeInstance.templateId = binaryReader.ReadUInt16();

			ushort numChildren = binaryReader.ReadUInt16();
			if(numChildren > 0)
			{
				nodeInstance.childrenIds = new List<ushort>(numChildren);
				for(int i = 0; i < numChildren; ++i)
				{
					nodeInstance.childrenIds.Add(binaryReader.ReadUInt16());
				}
			}

			TinyBinaryXmlNodeTemplate nodeTemplate = tinyBinaryXml.nodeTemplates[nodeInstance.templateId];
			ushort numAttributes = (ushort)(nodeTemplate.attributeNames == null ? 0 : nodeTemplate.attributeNames.Count);
			if(numAttributes > 0)
			{
				nodeInstance.attributeValues = new List<object>(numAttributes);
				for(ushort i = 0; i < numAttributes; ++i)
				{
					if(nodeTemplate.attributeTypes[i] == TINY_BINARY_XML_ATTRIBUTE_TYPE.FLOAT)
					{
						nodeInstance.attributeValues.Add(binaryReader.ReadSingle());
					}
					else
					{
						nodeInstance.attributeValues.Add(binaryReader.ReadString());
					}
				}
			}

			byte hasText = binaryReader.ReadByte();
			if(hasText == 1)
			{
				nodeInstance.text = binaryReader.ReadString();
			}
		}
	}
}

