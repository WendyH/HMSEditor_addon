using System;
using System.IO;
using System.Xml;
using whYamlParser;

namespace HMSEditorNS {
	public static class PlistParser {

		public static YamlObject LoadFromFile(string file) {
			YamlObject result = new YamlObject();

			if (!File.Exists(file))
				file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(file));

			if (File.Exists(file)) {
				XmlDocument xml = new XmlDocument();
				xml.XmlResolver = null;
				try {
					xml.Load(file);
					result = readXml(xml);
				} catch (Exception e) {
					HMS.LogError("Ошибка загрузки "+file+". Причина: "+ e.Message);
				}
			}
			return result;
		}

		private static YamlObject readXml(XmlDocument xml) {
			XmlNode rootNode = xml.DocumentElement.ChildNodes[0];
			return parse(rootNode);
		}

		private static YamlObject parseDictionary(XmlNode node) {
			XmlNodeList children = node.ChildNodes;
			YamlObject result = new YamlObject(node.Name);
			result.Type = YamlObjectType.Mapping;

			int count = children.Count;
			if (count < 2) return result;

			for (int i = 0; i < count; i += 2) {
				XmlNode keynode = children[i];
				if (keynode.Name == "#comment") { i--; continue; }
				XmlNode valnode = children[i + 1];
				if (valnode.Name == "#comment") { i++; valnode = children[i + 1]; }

				if (keynode.Name != "key") continue;

				YamlObject r = parse(valnode);
				r.Name = keynode.InnerText;
                result.ChildItems.Add(r);
			}
			return result;
		}

		private static YamlObject parseArray(XmlNode node) {
			YamlObject result = new YamlObject(node.Name);
			result.Type = YamlObjectType.Sequence;
			int i = 0;
			foreach (XmlNode child in node.ChildNodes) {
                YamlObject r = parse(child);
				r.Name = i.ToString();
                result.ChildItems.Add(r);
				i++;
			}
			return result;
		}

		private static YamlObject parse(XmlNode node) {
			switch (node.Name) {
				case "dict" : return parseDictionary(node);
				case "array": return parseArray(node);
			}
            return new YamlObject(node.Name, node.InnerText);
		}
	}
}