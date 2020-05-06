using System;
using System.Collections.Generic;

using Mistware.Utils;

namespace Mistware.Postman
{
    /// Singleton class to do mail merge
    public class MailMerge
    {
        private MailMerge() 
        { 
            Templates = null;
        }

        private static MailMerge _me = new MailMerge();

        /// Singleton Pattern
        public static MailMerge Me { get { return _me; } }

        /// Mail Merge Templates
        public Dictionary<string,string> Templates { get; set; }

        /// Load the templates from a txt file
        public void LoadTemplates(string path, string filename)
        {
            Templates = new Dictionary<string, string>();

            string key      = null; 
            string template = null;
            using (FileRead reader = new FileRead(path, filename))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.Left(2) == "%%")  // Template Key line found
                    {
                        if (key != null)
                        {
                            if (template == null) throw new Exception("Empty template for key '" + key + "'");
                            Templates.Add(key, template);
                        }  
                        key = line.Substring(2).Trim();
                        template = null;
                    }
                    else
                    {
                        if (key == null) throw new Exception("Missing Template Key");
                        if (template == null) template = line;
                        else                  template += "\n" + line;
                    }

                }
                if (key != null)
                {
                    if (template == null) throw new Exception("Empty template for key '" + key + "'");
                    Templates.Add(key, template);
                }
            }
        }

        /// Get the email subject by mail merging the selected template
        public string GetSubject(string deliveryType, Dictionary<string, string> fields)
        {
            if (deliveryType == null) return null;

            string code = deliveryType + "-Subject";

            return ReplaceFields(code, fields);
        }

        /// Get the email body by mail merging the selected template
        public string GetBody(string deliveryType, Dictionary<string, string> fields)
        {
            if (deliveryType == null) return null;

            string code = deliveryType + "-Body";

            return ReplaceFields(code, fields);
        }
        private string ReplaceFields(string code, Dictionary<string, string> fields)
        {
            if (Templates == null) return null;

            if (!Templates.ContainsKey(code))
            {
                Log.Me.Error("Error in ReplaceFields: Have not got a template corresponding to code '" + code + "'");
                return null;
            }

            string result = Templates[code];

            try
            {
                foreach (KeyValuePair<string, string> kv in fields)
                {
                    result = result.Replace("{" + kv.Key + "}", kv.Value);
                }
            }
            catch (Exception ex)
            {
                Log.Me.Error("Error in ReplaceFields: " + ex.Message);
                return null;
            }

            return result;
        }
    }
}