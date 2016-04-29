using System.Text;

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
}
