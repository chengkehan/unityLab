using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TinyBinaryXml
{
	public class TbXml
	{
		internal List<TbXmlNodeTemplate> nodeTemplates = null;

		internal List<TbXmlNode> nodes = null;

		public TbXmlNode docNode = null;

		public static TbXml Load(byte[] xmlBytes)
		{
			TbXmlDeserializer deserializer = new TbXmlDeserializer();
			return deserializer.DeserializeXmlBytes(xmlBytes);
		}
	}
}

