using System.Collections.Generic;

namespace TinyBinaryXml
{
	public class TinyBinaryXmlNodeInstance
	{
		public static ushort idInc = 0;
		
		public ushort id = 0;
		
		public List<ushort> childrenIds = null;
		
		public ushort templateId = 0;
		
		public List<object> attributeValues = null;

		public TinyBinaryXml tinyBinaryXml = null;

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
			TinyBinaryXmlNodeTemplate nodeTemplate = tinyBinaryXml.nodeTemplates[templateId];
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

		public List<TinyBinaryXmlNodeInstance> GetNodes(string path)
		{
			if(string.IsNullOrEmpty(path))
			{
				return null;
			}

			List<TinyBinaryXmlNodeInstance> resultNodes = null;
			int numChildren = childrenIds == null ? 0 : childrenIds.Count;
			string[] pathBlocks = path.Split('/');
			for(int childIndex = 0; childIndex < numChildren; ++childIndex)
			{
				TinyBinaryXmlNodeInstance childNodeInstance = tinyBinaryXml.nodeInstances[childrenIds[childIndex]];
				GetNodesRecursive(pathBlocks, 0, ref pathBlocks[0], childNodeInstance, ref resultNodes);
			}
			
			return resultNodes;
		}

		private void GetNodesRecursive(string[] pathBlocks, int pathBlockIndex, ref string pathBlock, TinyBinaryXmlNodeInstance currentNodeInstance, ref List<TinyBinaryXmlNodeInstance> resultNodes)
		{
			if(tinyBinaryXml.nodeTemplates[currentNodeInstance.templateId].name.Equals(pathBlock))
			{
				if(pathBlockIndex == pathBlocks.Length - 1)
				{
					if(resultNodes == null)
					{
						resultNodes = new List<TinyBinaryXmlNodeInstance>();
					}
					resultNodes.Add(currentNodeInstance);
				}
				else
				{
					List<ushort> childrenIds = currentNodeInstance.childrenIds;
					int numChildren = childrenIds == null ? 0 : childrenIds.Count;
					for(int childIndex = 0; childIndex < numChildren; ++childIndex)
					{
						GetNodesRecursive(pathBlocks, pathBlockIndex + 1, ref pathBlocks[pathBlockIndex + 1], tinyBinaryXml.nodeInstances[childrenIds[childIndex]], ref resultNodes);
					}
				}
			}
		}
	}
}

