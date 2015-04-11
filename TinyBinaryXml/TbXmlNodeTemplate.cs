using System.Collections.Generic;

namespace TinyBinaryXml
{
	public class TbXmlNodeTemplate
	{	
		internal ushort id = 0;
		
		internal string name = null;
		
		internal List<string> attributeNames = null;

		internal Dictionary<string, int> attributeNameIndexMapping = null;
		
		internal List<TB_XML_ATTRIBUTE_TYPE> attributeTypes = null;
	}
}

