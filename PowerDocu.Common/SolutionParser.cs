using System.IO;
using System.IO.Compression;
using System.Xml;

namespace PowerDocu.Common
{
    public class SolutionParser
    {
        public SolutionEntity solution;
        public SolutionParser(string filename)
        {
            NotificationHelper.SendNotification(" - Processing " + filename);
            if (filename.EndsWith(".zip"))
            {
                using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                //process solution.xml
                var solutionDefinition = ZipHelper.getSolutionDefinitionFileFromZip(stream);
                if (solutionDefinition != null)
                {
                    var tempFile = Path.GetDirectoryName(filename) + @"\" + solutionDefinition.Name;
                    solutionDefinition.ExtractToFile(tempFile, true);
                    NotificationHelper.SendNotification("  - Processing solution ");
                    using (var appDefinition = new FileStream(tempFile, FileMode.Open))
                    {
                        {
                            parseSolutionDefinition(appDefinition);
                        }
                    }
                    File.Delete(tempFile);
                }
                //process customizations.xml
                var customizationsDefinition = ZipHelper.getCustomizationsDefinitionFileFromZip(stream);
                if (customizationsDefinition != null)
                {
                    var tempFile = Path.GetDirectoryName(filename) + @"\" + customizationsDefinition.Name;
                    customizationsDefinition.ExtractToFile(tempFile, true);
                    NotificationHelper.SendNotification("  - Processing customizations.xml ");
                    using (var customizations = new FileStream(tempFile, FileMode.Open))
                    {
                        solution.Customizations = CustomizationsParser.parseCustomizationsDefinition(customizations);
                    }
                    File.Delete(tempFile);
                }
            }
            else
            {
                NotificationHelper.SendNotification("No solution definition found in " + filename);
            }
        }

        private void parseSolutionDefinition(Stream solutionArchive)
        {
            using var reader = new StreamReader(solutionArchive);
            var solutionXML = reader.ReadToEnd();
            var solutionXmlDoc = new XmlDocument
            {
                XmlResolver = null
            };
            solutionXmlDoc.LoadXml(solutionXML);
            var solutionManifest = solutionXmlDoc.SelectSingleNode("/ImportExportXml/SolutionManifest");
            solution = new SolutionEntity
            {
                UniqueName = solutionManifest.SelectSingleNode("UniqueName").InnerText,
                Version = solutionManifest.SelectSingleNode("Version").InnerText,
                isManaged = solutionManifest.SelectSingleNode("Managed").InnerText.Equals("1"),
                Publisher = new SolutionPublisher()
                {
                    UniqueName = solutionManifest.SelectSingleNode("Publisher/UniqueName").InnerText,
                    EMailAddress = solutionManifest.SelectSingleNode("Publisher/EMailAddress").InnerText,
                    SupportingWebsiteUrl = solutionManifest.SelectSingleNode("Publisher/SupportingWebsiteUrl").InnerText,
                    CustomizationPrefix = solutionManifest.SelectSingleNode("Publisher/CustomizationPrefix").InnerText,
                    CustomizationOptionValuePrefix = solutionManifest.SelectSingleNode("Publisher/CustomizationOptionValuePrefix").InnerText
                }
            };
            var publisherLocalizedNames = solutionManifest.SelectSingleNode("Publisher/LocalizedNames");
            if (publisherLocalizedNames != null)
            {
                foreach (XmlNode localizedName in publisherLocalizedNames.ChildNodes)
                {
                    solution.Publisher.LocalizedNames.Add(localizedName.Attributes.GetNamedItem("languagecode")?.InnerText,
                                                localizedName.Attributes.GetNamedItem("description")?.InnerText);
                }
            }
            var publisherAddresses = solutionManifest.SelectSingleNode("Publisher/Addresses");
            if (publisherAddresses != null)
            {
                foreach (XmlNode xmlAddress in publisherAddresses.ChildNodes)
                {
                    var address = new Address()
                    {
                        AddressNumber = xmlAddress.SelectSingleNode("AddressNumber")?.InnerText,
                        AddressTypeCode = xmlAddress.SelectSingleNode("AddressTypeCode")?.InnerText,
                        City = xmlAddress.SelectSingleNode("City")?.InnerText,
                        County = xmlAddress.SelectSingleNode("County")?.InnerText,
                        Country = xmlAddress.SelectSingleNode("Country")?.InnerText,
                        Fax = xmlAddress.SelectSingleNode("Fax")?.InnerText,
                        FreightTermsCode = xmlAddress.SelectSingleNode("FreightTermsCode")?.InnerText,
                        ImportSequenceNumber = xmlAddress.SelectSingleNode("ImportSequenceNumber")?.InnerText,
                        Latitude = xmlAddress.SelectSingleNode("Latitude")?.InnerText,
                        Line1 = xmlAddress.SelectSingleNode("Line1")?.InnerText,
                        Line2 = xmlAddress.SelectSingleNode("Line2")?.InnerText,
                        Line3 = xmlAddress.SelectSingleNode("Line3")?.InnerText,
                        Longitude = xmlAddress.SelectSingleNode("Longitude")?.InnerText,
                        Name = xmlAddress.SelectSingleNode("Name")?.InnerText,
                        PostalCode = xmlAddress.SelectSingleNode("PostalCode")?.InnerText,
                        PostOfficeBox = xmlAddress.SelectSingleNode("PostOfficeBox")?.InnerText,
                        PrimaryContactName = xmlAddress.SelectSingleNode("PrimaryContactName")?.InnerText,
                        ShippingMethodCode = xmlAddress.SelectSingleNode("ShippingMethodCode")?.InnerText,
                        StateOrProvince = xmlAddress.SelectSingleNode("StateOrProvince")?.InnerText,
                        Telephone1 = xmlAddress.SelectSingleNode("Telephone1")?.InnerText,
                        Telephone2 = xmlAddress.SelectSingleNode("Telephone2")?.InnerText,
                        Telephone3 = xmlAddress.SelectSingleNode("Telephone3")?.InnerText,
                        TimeZoneRuleVersionNumber = xmlAddress.SelectSingleNode("TimeZoneRuleVersionNumber")?.InnerText,
                        UPSZone = xmlAddress.SelectSingleNode("UPSZone")?.InnerText,
                        UTCOffset = xmlAddress.SelectSingleNode("UTCOffset")?.InnerText,
                        UTCConversionTimeZoneCode = xmlAddress.SelectSingleNode("UTCConversionTimeZoneCode")?.InnerText
                    };
                    solution.Publisher.Addresses.Add(address);
                }
            }
            var publisherDescriptions = solutionManifest.SelectSingleNode("Publisher/Descriptions");
            if (publisherDescriptions != null)
            {
                foreach (XmlNode description in publisherDescriptions.ChildNodes)
                {
                    solution.Publisher.Descriptions.Add(description.Attributes.GetNamedItem("languagecode")?.InnerText,
                                                description.Attributes.GetNamedItem("description")?.InnerText);
                }
            }
            //parsing the components
            var rootComponents = solutionManifest.SelectSingleNode("RootComponents");
            if (rootComponents != null)
            {
                foreach (XmlNode component in rootComponents.ChildNodes)
                {
                    var solutionComponent = new SolutionComponent()
                    {
                        SchemaName = component.Attributes.GetNamedItem("schemaName")?.InnerText,
                        ID = component.Attributes.GetNamedItem("id")?.InnerText,
                        Type = SolutionComponentHelper.GetComponentType(component.Attributes.GetNamedItem("type")?.InnerText)
                    };
                    solution.Components.Add(solutionComponent);
                }
            }
            //parsing the dependencies
            var missingDependencies = solutionManifest.SelectSingleNode("MissingDependencies");
            if (missingDependencies != null)
            {
                foreach (XmlNode component in missingDependencies.ChildNodes)
                {
                    var required = new SolutionComponent()
                    {
                        SchemaName = component["Required"].Attributes.GetNamedItem("schemaName")?.InnerText,
                        reqdepDisplayName = component["Required"].Attributes.GetNamedItem("displayName")?.InnerText,
                        reqdepSolution = component["Required"].Attributes.GetNamedItem("solution")?.InnerText,
                        ID = component["Required"].Attributes.GetNamedItem("id")?.InnerText,
                        reqdepParentDisplayName = component["Required"].Attributes.GetNamedItem("parentDisplayName")?.InnerText,
                        reqdepParentSchemaName = component["Required"].Attributes.GetNamedItem("parentSchemaName")?.InnerText,
                        reqdepIdSchemaName = component["Required"].Attributes.GetNamedItem("id.schemaname")?.InnerText,
                        Type = SolutionComponentHelper.GetComponentType(component["Required"].Attributes.GetNamedItem("type")?.InnerText)
                    };
                    var dependent = new SolutionComponent()
                    {
                        SchemaName = component["Dependent"].Attributes.GetNamedItem("schemaName")?.InnerText,
                        reqdepDisplayName = component["Dependent"].Attributes.GetNamedItem("displayName")?.InnerText,
                        Type = SolutionComponentHelper.GetComponentType(component["Dependent"].Attributes.GetNamedItem("type")?.InnerText),
                        ID = component["Dependent"].Attributes.GetNamedItem("id")?.InnerText,
                        reqdepParentDisplayName = component["Dependent"].Attributes.GetNamedItem("parentDisplayName")?.InnerText,
                        reqdepParentSchemaName = component["Dependent"].Attributes.GetNamedItem("parentSchemaName")?.InnerText,
                        reqdepIdSchemaName = component["Dependent"].Attributes.GetNamedItem("id.schemaname")?.InnerText,
                        reqdepSolution = component["Dependent"].Attributes.GetNamedItem("solution")?.InnerText
                    };
                    solution.Dependencies.Add(new SolutionDependency(required, dependent));
                }
            }

            //LocalizedNames
            var localizedNames = solutionManifest.SelectSingleNode("LocalizedNames");
            if (localizedNames != null)
            {
                foreach (XmlNode localizedName in localizedNames.ChildNodes)
                {
                    solution.LocalizedNames.Add(localizedName.Attributes.GetNamedItem("languagecode")?.InnerText,
                                                localizedName.Attributes.GetNamedItem("description")?.InnerText);
                }
            }
            //Descriptions
            var descriptions = solutionManifest.SelectSingleNode("LocalizedNames");
            if (descriptions != null)
            {
                foreach (XmlNode description in descriptions.ChildNodes)
                {
                    solution.Descriptions.Add(description.Attributes.GetNamedItem("languagecode")?.InnerText,
                                                description.Attributes.GetNamedItem("description")?.InnerText);
                }
            }
        }
    }
}