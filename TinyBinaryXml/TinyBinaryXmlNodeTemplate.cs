using System.Collections.Generic;

namespace TinyBinaryXml
{
	public class TinyBinaryXmlNodeTemplate
	{
		public static ushort idInc = 0;
		
		public ushort id = 0;
		
		public string name = null;
		
		public List<string> attributeNames = null;

		public Dictionary<string, int> attributeNameIndexMapping = null;
		
		public List<TINY_BINARY_XML_ATTRIBUTE_TYPE> attributeTypes = null;
	}
}

