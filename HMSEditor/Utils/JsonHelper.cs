using System.Text;
using System.Xml;

namespace HMSEditorNS.Utils {
    public class JsonHelper {
        private const string INDENT_STRING = "  ";
        public static string FormatJson(string str) {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            string sq = "";
            for (var i = 0; i < str.Length; i++) {
                var ch = str[i];
                int n;
                switch (ch) {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted) {
                            sb.AppendLine();
                            for(n = ++indent; n > 0; n--) sb.Append(INDENT_STRING);
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted) {
                            sb.AppendLine();
                            for (n = --indent; n > 0; n--) sb.Append(INDENT_STRING);
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && str[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped) quoted = !quoted;
                        if (!quoted) {
                            sb = sb.Replace(sq+'"', System.Text.RegularExpressions.Regex.Unescape(sq)+'"');
                            sq = "";
                        }
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted) {
                            sb.AppendLine();
                            for (n = indent; n > 0; n--) sb.Append(INDENT_STRING);
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
                if (quoted) sq += ch;
            }
            return sb.ToString();
        }
    }
    
    /// <summary>
    /// Supports formatting for XML in a format that is easily human-readable.
    /// </summary>
    public static class PrettyXmlFormatter {

        /// <summary>
        /// Generates formatted UTF-8 XML for the content in the <paramref name="doc"/>
        /// </summary>
        /// <param name="doc">XmlDocument for which content will be returned as a formatted string</param>
        /// <returns>Formatted (indented) XML string</returns>
        public static string GetPrettyXml(XmlDocument doc) {
            // Configure how XML is to be formatted
            XmlWriterSettings settings = new XmlWriterSettings {
                Indent = true
                , IndentChars = "  "
                , NewLineChars = System.Environment.NewLine
                , NewLineHandling = NewLineHandling.Replace
                //,NewLineOnAttributes = true
                //,OmitXmlDeclaration = false
            };

            // Use wrapper class that supports UTF-8 encoding
            StringWriterWithEncoding sw = new StringWriterWithEncoding(Encoding.UTF8);

            // Output formatted XML to StringWriter
            using (XmlWriter writer = XmlWriter.Create(sw, settings)) {
                doc.Save(writer);
            }

            // Get formatted text from writer
            return sw.ToString();
        }



        /// <summary>
        /// Wrapper class around <see cref="StringWriter"/> that supports encoding.
        /// Attribution: http://stackoverflow.com/a/427737/3063884
        /// </summary>
        private sealed class StringWriterWithEncoding: System.IO.StringWriter {
            private readonly Encoding encoding;

            /// <summary>
            /// Creates a new <see cref="PrettyXmlFormatter"/> with the specified encoding
            /// </summary>
            /// <param name="encoding"></param>
            public StringWriterWithEncoding(Encoding encoding) {
                this.encoding = encoding;
            }

            /// <summary>
            /// Encoding to use when dealing with text
            /// </summary>
            public override Encoding Encoding
            {
                get { return encoding; }
            }
        }
    }
}
