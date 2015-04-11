using System.Collections.Generic;

namespace TinyBinaryXml
{
	public class TbXmlNode
	{
		public static ushort idInc = 0;
		
		public ushort id = 0;
		
		public List<ushort> childrenIds = null;
		
		public ushort templateId = 0;
		
		public List<object> attributeValues = null;

		public TbXml tbXml = null;

		public string text = null;

		public string GetStringValue(string name)
		{
			return GetValue(ref name).ToString();
		}

		public float GetFloatValue(string name)
		{
			return (float)GetValue(ref name);
		}

		public int GetIntValue(string name)
		{
			return (int)(float)GetValue(ref name);
		}

		public uint GetUIntValue(string name)
		{
			return (uint)(float)GetValue(ref name);
		}

		public byte GetByteValue(string name)
		{
			return (byte)(float)GetValue(ref name);
		}

		public ushort GetUShortValue(string name)
		{
			return (ushort)(float)GetValue(ref name);
		}

		public short GetShortValue(string name)
		{
			return (short)(float)GetValue(ref name);
		}

		private object GetValue(ref string name)
		{
			TbXmlNodeTemplate nodeTemplate = tbXml.nodeTemplates[templateId];
			int attributeIndex;
			if(nodeTemplate.attributeNameIndexMapping.TryGetValue(name, out attributeIndex))
			{
				return attributeValues[attributeIndex];
			}
			else
			{
				return string.Empty;
			}
		}

		public List<TbXmlNode> GetNodes(string path)
		{
			if(string.IsNullOrEmpty(path))
			{
				return null;
			}

			List<TbXmlNode> resultNodes = null;
			int numChildren = childrenIds == null ? 0 : childrenIds.Count;
			string[] pathBlocks = path.Split('/');
			for(int childIndex = 0; childIndex < numChildren; ++childIndex)
			{
				TbXmlNode childNode = tbXml.nodes[childrenIds[childIndex]];
				GetNodesRecursive(pathBlocks, 0, ref pathBlocks[0], childNode, ref resultNodes);
			}
			
			return resultNodes;
		}

		private void GetNodesRecursive(string[] pathBlocks, int pathBlockIndex, ref string pathBlock, TbXmlNode currentNode, ref List<TbXmlNode> resultNodes)
		{
			if(tbXml.nodeTemplates[currentNode.templateId].name.Equals(pathBlock))
			{
				if(pathBlockIndex == pathBlocks.Length - 1)
				{
					if(resultNodes == null)
					{
						resultNodes = new List<TbXmlNode>();
					}
					resultNodes.Add(currentNode);
				}
				else
				{
					List<ushort> childrenIds = currentNode.childrenIds;
					int numChildren = childrenIds == null ? 0 : childrenIds.Count;
					for(int childIndex = 0; childIndex < numChildren; ++childIndex)
					{
						GetNodesRecursive(pathBlocks, pathBlockIndex + 1, ref pathBlocks[pathBlockIndex + 1], tbXml.nodes[childrenIds[childIndex]], ref resultNodes);
					}
				}
			}
		}
	}
}

