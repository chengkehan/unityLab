using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TinyBinaryXml
{
	public class TbXmlDeserializer
	{
		public TbXml DeserializeXmlBytes(byte[] xmlBytes)
		{
			if(xmlBytes == null || xmlBytes.Length == 0)
			{
				return null;
			}

			TbXml tbXml = new TbXml();
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(xmlBytes));

			ushort numNodeTemplates = binaryReader.ReadUInt16();
			tbXml.nodeTemplates = new List<TbXmlNodeTemplate>(numNodeTemplates);
			for(ushort i = 0; i < numNodeTemplates; ++i)
			{
				DeserializeNodeTemplate(binaryReader, i, tbXml);
			}

			ushort numNodes = binaryReader.ReadUInt16();
			tbXml.nodes = new List<TbXmlNode>(numNodes);
			for(ushort i = 0; i < numNodes; ++i)
			{
				DeserializeNode(binaryReader, i, tbXml);
				tbXml.nodes[i].tbXml = tbXml;
			}

            DeserializeStringPool(binaryReader, tbXml);
            DeserializeValuePool(binaryReader, tbXml);

			tbXml.docNode = new TbXmlNode();
			tbXml.docNode.childrenIds = new List<ushort>();
			tbXml.docNode.childrenIds.Add(0);
			tbXml.docNode.tbXml = tbXml;

			binaryReader.Close();
			binaryReader.Dispose();

			return tbXml;
		}

        private void DeserializeStringPool(BinaryReader binaryReader, TbXml tbXml)
        {
            int numStirngs = binaryReader.ReadInt32();
            tbXml.stringPool = new List<string>(numStirngs);

            for (int i = 0; i < numStirngs; ++i)
            {
                tbXml.stringPool.Add(binaryReader.ReadString());
            }
        }

        private void DeserializeValuePool(BinaryReader binaryReader, TbXml tbXml)
        {
            int numValues = binaryReader.ReadInt32();
            tbXml.valuePool = new List<double>(numValues);

            for (int i = 0; i < numValues; ++i)
            {
                tbXml.valuePool.Add(binaryReader.ReadDouble());
            }
        }

		private void DeserializeNodeTemplate(BinaryReader binaryReader, ushort index, TbXml tbXml)
		{
			TbXmlNodeTemplate nodeTemplate = new TbXmlNodeTemplate();
			tbXml.nodeTemplates.Add(nodeTemplate);

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

				nodeTemplate.attributeTypes = new List<TB_XML_ATTRIBUTE_TYPE>(numAttributes);
				for(int i = 0; i < numAttributes; ++i)
				{
					nodeTemplate.attributeTypes.Add((TB_XML_ATTRIBUTE_TYPE)binaryReader.ReadByte());
				}
			}
		}

		private void DeserializeNode(BinaryReader binaryReader, ushort index, TbXml tbXml)
		{
			TbXmlNode node = new TbXmlNode();
			tbXml.nodes.Add(node);

			node.id = binaryReader.ReadUInt16();
			node.templateId = binaryReader.ReadUInt16();

			ushort numChildren = binaryReader.ReadUInt16();
			if(numChildren > 0)
			{
				node.childrenIds = new List<ushort>(numChildren);
				for(int i = 0; i < numChildren; ++i)
				{
					node.childrenIds.Add(binaryReader.ReadUInt16());
				}
			}

			TbXmlNodeTemplate nodeTemplate = tbXml.nodeTemplates[node.templateId];
			ushort numAttributes = (ushort)(nodeTemplate.attributeNames == null ? 0 : nodeTemplate.attributeNames.Count);
			if(numAttributes > 0)
			{
				node.attributeValues = new List<int>(numAttributes);
				for(ushort i = 0; i < numAttributes; ++i)
				{
                    node.attributeValues.Add(binaryReader.ReadInt32());
				}
			}

			byte hasText = binaryReader.ReadByte();
			if(hasText == 1)
			{
				node.text = binaryReader.ReadInt32();
			}
		}
	}
}

