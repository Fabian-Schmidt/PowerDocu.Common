using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace PowerDocu.Common
{
    public class OptionSet(XmlNode xmlEntity)
    {
        private readonly XmlNode xmlEntity = xmlEntity;

        public string getName()
        {
            return xmlEntity.Attributes.GetNamedItem("Name")?.InnerText ?? "";
        }

        public string getLocalizedName()
        {
            return xmlEntity.Attributes.GetNamedItem("localizedName")?.InnerText ?? "";
        }
        public string getOptionSetType()
        {
            return xmlEntity.SelectSingleNode("OptionSetType")?.InnerText ?? "";
        }

        public IEnumerable<OptionSetValue> GetOptions()
        {
            return xmlEntity.SelectNodes("options/option").Cast<XmlNode>().Select(s => new OptionSetValue(s));
        }
    }

    public class OptionSetValue(XmlNode xmlEntity)
    {
        private readonly XmlNode xmlEntity = xmlEntity;

        public int getValue()
        {
            return int.TryParse(xmlEntity.Attributes.GetNamedItem("value")?.InnerText, out var val) ? val : -1;
        }

        public string GetLabel()
        {
            return xmlEntity.SelectSingleNode("(labels/label/@description)[1]")?.InnerText ?? "";
        }

        public string GetDescription()
        {
            return xmlEntity.SelectSingleNode("(Descriptions/Description/@description)[1]")?.InnerText ?? "";
        }
    }
}
