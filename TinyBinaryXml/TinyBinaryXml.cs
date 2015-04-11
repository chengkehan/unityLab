using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TinyBinaryXml
{
	public class TinyBinaryXml
	{
		public List<TinyBinaryXmlNodeTemplate> nodeTemplates = null;

		public List<TinyBinaryXmlNodeInstance> nodeInstances = null;

		public TinyBinaryXmlNodeInstance docNodeInstance = null;

		public List<TinyBinaryXmlNodeInstance> GetNodes(string path)
		{
			if(string.IsNullOrEmpty(path))
			{
				return null;
			}

			TinyBinaryXmlNodeInstance currentNodeInstance = docNodeInstance.childrenIds == null || docNodeInstance.childrenIds.Count == 0 ? null : nodeInstances[docNodeInstance.childrenIds[0]];
			if(currentNodeInstance == null)
			{
				return null;
			}

			string[] pathBlocks = path.Split('/');
			List<TinyBinaryXmlNodeInstance> resultNodes = null;
			GetNodesRecursive(pathBlocks, 0, ref pathBlocks[0], currentNodeInstance, ref resultNodes);

			return resultNodes;
		}

		private void GetNodesRecursive(string[] pathBlocks, int pathBlockIndex, ref string pathBlock, TinyBinaryXmlNodeInstance currentNodeInstance, ref List<TinyBinaryXmlNodeInstance> resultNodes)
		{
			if(nodeTemplates[currentNodeInstance.templateId].name.Equals(pathBlock))
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
						GetNodesRecursive(pathBlocks, pathBlockIndex + 1, ref pathBlocks[pathBlockIndex + 1], nodeInstances[childrenIds[childIndex]], ref resultNodes);
					}
				}
			}
		}
	}
}

