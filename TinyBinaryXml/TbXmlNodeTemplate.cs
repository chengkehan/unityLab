using System.Collections.Generic;

namespace TinyBinaryXml
{
	public class TbXmlNodeTemplate
	{
		public static ushort idInc = 0;
		
		public ushort id = 0;
		
		public string name = null;
		
		public List<string> attributeNames = null;

		public Dictionary<string, int> attributeNameIndexMapping = null;
		
		public List<TB_XML_ATTRIBUTE_TYPE> attributeTypes = null;
	}
}

