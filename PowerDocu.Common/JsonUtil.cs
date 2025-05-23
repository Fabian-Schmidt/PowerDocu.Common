using Newtonsoft.Json;
using System;
using System.IO;

namespace PowerDocu.Common
{
    public static class JsonUtil
    {
        public static string JsonPrettify(string json)
        {
            try
            {
                using StringReader stringReader = new StringReader(json);
                using var stringWriter = new StringWriter();
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
                jsonWriter.WriteToken(jsonReader);
                return stringWriter.ToString();
            }
            catch (Exception e)
            {
                //if the attempt to beautify the string failed we simply return the string itself
                return json;
            }
        }
    }
}