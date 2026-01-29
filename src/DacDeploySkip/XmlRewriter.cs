using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace DacDeploySkip
{
    /// <summary>
    /// This class is for internal use only.
    /// </summary>
    public class XmlRewriter
    {
        public Task RewriteXmlMetadataAsync(string modelFile)
        {
            const string fileKey = "FileName";
            const string symbolsKey = "AssemblySymbolsName";
            var contents = File.ReadAllText(modelFile);

            if (contents.IndexOf($"<Metadata Name=\"{fileKey}\" ", StringComparison.Ordinal) < 0
                && contents.IndexOf($"<Metadata Name=\"{symbolsKey}\" ", StringComparison.Ordinal) < 0)
            {
                return Task.CompletedTask;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(contents);

            var nodes = xmlDoc.GetElementsByTagName("Metadata");

            foreach (XmlNode node in nodes)
            {
                if (node.Attributes == null)
                {
                    continue;
                }

                ReplaceValue(fileKey, node);

                ReplaceValue(symbolsKey, node);
            }

            xmlDoc.Save(modelFile);

            return Task.CompletedTask;
        }

        private static void ReplaceValue(string key, XmlNode metaData)
        {
            if (metaData.Attributes != null
                && metaData.Attributes.Count == 2
                && metaData.Attributes[0].Name == "Name"
                && metaData.Attributes[0].Value == key)
            {
                var originalValue = metaData.Attributes[1].Value;
                metaData.Attributes[1].Value = Path.GetFileName(originalValue);
            }
        }
    }
}
