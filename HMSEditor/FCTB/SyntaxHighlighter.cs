﻿//#define debug

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Timer = System.Windows.Forms.Timer;
using HMSEditorNS;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace FastColoredTextBoxNS {
    public sealed class SyntaxHighlighter: IDisposable {
        //styles
        private static readonly NativeMethods.Platform platformType = NativeMethods.GetOperationSystemPlatform();
        public readonly Style BlueBoldStyle = new TextStyle(Brushes.Blue   , null, FontStyle.Bold   );
        public readonly Style BlueStyle     = new TextStyle(Brushes.Blue   , null, FontStyle.Regular);
        public readonly Style BoldStyle     = new TextStyle(null           , null, FontStyle.Bold   );
        public readonly Style BrownStyle    = new TextStyle(Brushes.Brown  , null, FontStyle.Italic );
        public readonly Style GrayStyle     = new TextStyle(Brushes.Gray   , null, FontStyle.Regular);
        public readonly Style GreenStyle    = new TextStyle(Brushes.Green  , null, FontStyle.Italic );
        public readonly Style MagentaStyle  = new TextStyle(Brushes.Magenta, null, FontStyle.Regular);
        public readonly Style MaroonStyle   = new TextStyle(Brushes.Maroon , null, FontStyle.Regular);
        public readonly Style RedStyle      = new TextStyle(Brushes.Red    , null, FontStyle.Regular);
        public readonly Style BlackStyle    = new TextStyle(Brushes.Black  , null, FontStyle.Regular);
        // By WendyH < ----------------------------------------------------------------------------
        public readonly Style BoldStyle2       = new TextStyle(null, null, FontStyle.Bold | FontStyle.Underline);
        public readonly Style WendyHsLiteStyle = new TextStyle(new SolidBrush(Color.FromArgb(0, 0x40, 0x80)), null, FontStyle.Regular);
        public readonly Style DarkOrangeStyle  = new TextStyle(Brushes.DarkOrange, null, FontStyle.Regular);
        public readonly Style DarkRedStyle     = new TextStyle(Brushes.DarkRed, null, FontStyle.Regular);
        public readonly Style VSClassStyle     = new TextStyle(new SolidBrush(Color.FromArgb(255, 43, 145, 175)), null, FontStyle.Regular);
        public Style DefaultStyle;
        public bool AltPascalKeywordsHighlight = true;
        public bool RedStringsHighlight        = false;
        public string HmsClasses  = "";
        public string HmsKeywords = "";
        public Theme  StyleTheme  = null;
        const int MaxLength4FastWork = 15000;
        const int MaxLineLength      = 1000;
        const int AbsoluteMaxRangeLength = 150000;
        // CONSTRUCTOR
        public SyntaxHighlighter() {
        }

        // By WendyH > ----------------------------------------------------------------------------
        //
        private readonly Dictionary<string, SyntaxDescriptor> descByXMLfileNames = new Dictionary<string, SyntaxDescriptor>();

        #region REGEX FIELDS
        private static Regex CPPStringAndCommentsRegex    = new Regex(@"""(?>(?:\\[^\r\n]|[^""\r\n])*)""?|'(?>(?:\\[^\r\n]|[^'\r\n])*)'?|(//.*)|(?<mc>\/\*[\s\S]*?((?<mcend>\*\/)|$))|(?<mcend2>\*\/)", RegexOptions.ExplicitCapture | RegexCompiledOption); // By WendyH
        private static Regex CSharpStringAndCommentsRegex = new Regex(@"
                            # Character definitions:
                            '
                            (?> # disable backtracking
                              (?:
                                \\[^\r\n]|    # escaped meta char
                                [^'\r\n]      # any character except '
                              )*
                            )
                            '?
                            |
                            # Normal string & verbatim strings definitions:
                            (?<verbatimIdentifier>@)?         # this group matches if it is an verbatim string
                            ""
                            (?> # disable backtracking
                              (?:
                                # match and consume an escaped character including escaped double quote ("") char
                                (?(verbatimIdentifier)        # if it is a verbatim string ...
                                  """"|                         #   then: only match an escaped double quote ("") char
                                  \\.                         #   else: match an escaped sequence
                                )
                                | # OR
            
                                # match any char except double quote char ("")
                                [^""]
                              )*
                            )
                            ""
                        |(//.*?(\n|$)|\/\*.*?(\*\/|$))", RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace); //thanks to rittergig for this regex
        private Regex CSharpStringRegex = new Regex(@"
                            # Character definitions:
                            '
                            (?> # disable backtracking
                              (?:
                                \\[^\r\n]|    # escaped meta char
                                [^'\r\n]      # any character except '
                              )*
                            )
                            '?
                            |
                            # Normal string & verbatim strings definitions:
                            (?<verbatimIdentifier>@)?         # this group matches if it is an verbatim string
                            ""
                            (?> # disable backtracking
                              (?:
                                # match and consume an escaped character including escaped double quote ("") char
                                (?(verbatimIdentifier)        # if it is a verbatim string ...
                                  """"|                         #   then: only match an escaped double quote ("") char
                                  \\.                         #   else: match an escaped sequence
                                )
                                | # OR
            
                                # match any char except double quote char ("")
                                [^""]
                              )*
                            )
                            ""
                        ", RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace); //thanks to rittergig for this regex

        private Regex CSharpNumberRegex    = new Regex(@"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b");
        private Regex CSharpAttributeRegex = new Regex(@"^\s*(?<range>\[.+?\])\s*$", RegexOptions.Multiline);
        private Regex CSharpClassNameRegex = new Regex(@"\b(class|struct|enum|interface)\s+(?<range>\w+?)\b");
        private Regex CSharpKeywordRegex   = new Regex(@"\b(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|void|volatile|while|add|alias|ascending|descending|dynamic|from|get|global|group|into|join|let|orderby|partial|remove|select|set|value|var|where|yield)\b|#region\b|#endregion\b", RegexCompiledOption);
        private Regex CSharpCommentRegex1  = new Regex(@"//.*$", RegexOptions.Multiline | RegexCompiledOption);
        private Regex CSharpCommentRegex2  = new Regex(@"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline | RegexCompiledOption);
        private Regex CSharpCommentRegex3  = new Regex(@"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft | RegexCompiledOption);
        private Regex HTMLAttrRegex  , HTMLAttrValRegex, HTMLCommentRegex1  , HTMLCommentRegex2;
        private Regex HTMLEndTagRegex, HTMLEntityRegex , HTMLTagContentRegex, HTMLTagNameRegex, HTMLTagRegex;

        private Regex XMLAttrRegex,
                      XMLAttrValRegex,
                      XMLCommentRegex1,
                      XMLCommentRegex2;

        private Regex XMLEndTagRegex;

        private Regex XMLEntityRegex;

        private Regex XMLTagNameRegex;
        private Regex XMLTagRegex;
        private Regex XMLCDataRegex;
        private Regex XMLFoldingRegex;

        private Regex JScriptKeywordRegex;
        private Regex JScriptNumberRegex;

        private Regex LuaCommentRegex1,
                      LuaCommentRegex2,
                      LuaCommentRegex3;

        private Regex LuaKeywordRegex;
        private Regex LuaNumberRegex;
        private Regex LuaStringRegex;
        private Regex LuaFunctionsRegex;

        private Regex PHPKeywordRegex1,
                      PHPKeywordRegex2,
                      PHPKeywordRegex3;

        private Regex PHPNumberRegex;
        private Regex PHPStringRegex;
        private Regex PHPVarRegex;

        private Regex SQLCommentRegex1,
                        SQLCommentRegex2,
                        SQLCommentRegex3;

        private Regex SQLFunctionsRegex;
        private Regex SQLKeywordsRegex;
        private Regex SQLNumberRegex;
        private Regex SQLStatementsRegex;
        private Regex SQLStringRegex;
        private Regex SQLTypesRegex;
        private Regex SQLVarRegex;
        private Regex VBClassNameRegex;
        private Regex VBKeywordRegex;
        private Regex VBNumberRegex;
        private Regex VBStringRegex;
        #endregion REGEX FIELDS

        public static RegexOptions RegexCompiledOption {
            get {
                if (platformType == NativeMethods.Platform.X86)
                    return RegexOptions.Compiled;
                else
                    return RegexOptions.None;
            }
        }

        #region IDisposable Members

        public void Dispose() {
            foreach (SyntaxDescriptor desc in descByXMLfileNames.Values)
                desc.Dispose();
            WendyHsLiteStyle.Dispose();
            VSClassStyle    .Dispose();
            RedStyle        .Dispose();
            MaroonStyle     .Dispose();
            MagentaStyle    .Dispose();
            GreenStyle      .Dispose();
            GrayStyle       .Dispose();
            DarkOrangeStyle .Dispose();
            DarkRedStyle    .Dispose();
            BrownStyle      .Dispose();
            BoldStyle       .Dispose();
            BoldStyle2      .Dispose();
            BlueBoldStyle   .Dispose();
            BlueStyle       .Dispose();
            BlackStyle      .Dispose();
            if (Worker4BigText != null) Worker4BigText.Dispose();
        }

        #endregion

        /// <summary>
        /// Highlights syntax for given language
        /// </summary>
        public void HighlightSyntax(Language language, Range range) {
#if debug
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
#endif
            currentLanguage = language;
            if (Worker4BigText == null) {
                Worker4BigText = new BackgroundWorker();
                Worker4BigText.DoWork             += Worker4BigText_DoWork;
                Worker4BigText.RunWorkerCompleted += Worker4BigText_RunWorkerCompleted;
            }
            switch (language) {
                case Language.CSharp: CSharpSyntaxHighlight (range); break;
                case Language.VB    : VBSyntaxHighlight     (range); break;
                case Language.HTML  : HTMLSyntaxHighlight   (range); break;
                case Language.XML   : XMLSyntaxHighlight    (range); break;
                case Language.SQL   : SQLSyntaxHighlight    (range); break;
                case Language.PHP   : PHPSyntaxHighlight    (range); break;
                case Language.JS    : JScriptSyntaxHighlight(range); break;
                case Language.Lua   : LuaSyntaxHighlight    (range); break;
                // By WendyH < -------------------------------------------------------
                case Language.CPPScript   : CPPScriptSyntaxHighlight   (range); break;
                case Language.PascalScript: PascalScriptSyntaxHighlight(range); break;
                case Language.BasicScript : BasicSyntaxHighlight       (range); break;
                case Language.JScript     : HmsJScriptSyntaxHighlight  (range); break;
                case Language.YAML        : YAMLSyntaxHighlight        (range); break;
                case Language.Custom      : break;
                // By WendyH > -------------------------------------------------------
            }
#if debug
            Console.WriteLine("OnSyntaxHighlight: " + sw.ElapsedMilliseconds);
#endif
        }

        /// <summary>
        /// Highlights syntax for given XML description file
        /// </summary>
        public void HighlightSyntax(string XMLdescriptionFile, Range range) {
            SyntaxDescriptor desc;
            if (!descByXMLfileNames.TryGetValue(XMLdescriptionFile, out desc)) {
                var doc = new XmlDocument();
                string file = XMLdescriptionFile;
                if (!File.Exists(file))
                    file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(file));

                doc.LoadXml(File.ReadAllText(file));
                desc = ParseXmlDescription(doc);
                descByXMLfileNames[XMLdescriptionFile] = desc;
            }

            HighlightSyntax(desc, range);
        }

        public void AutoIndentNeeded(object sender, AutoIndentEventArgs args) {
            if ((args.LineText.Length > MaxLineLength) || (args.PrevLineText.Length > MaxLineLength)) return;
            var tb = (sender as FastColoredTextBox);
            if (tb != null) {
                Language language = tb.Language;
                switch (language) {
                    case Language.CSharp: CSharpAutoIndentNeeded(args); break;
                    case Language.VB    : VBAutoIndentNeeded    (args); break;
                    case Language.PHP   : PHPAutoIndentNeeded   (args); break;
                    case Language.JS    : CSharpAutoIndentNeeded(args); break; //JS like C#
                    case Language.Lua   : LuaAutoIndentNeeded   (args); break;
                    case Language.HTML  : HTMLAutoIndentNeeded(sender, args); break;
                    case Language.XML   : XMLAutoIndentNeeded (sender, args); break;
                    case Language.SQL   : SQLAutoIndentNeeded (sender, args); break;
                    // By WendyH < -------------------------------------------------------
                    case Language.CPPScript   : CSharpAutoIndentNeeded(args); break;
                    case Language.PascalScript: PascalAutoIndentNeeded(args); break;
                    case Language.BasicScript : VBAutoIndentNeeded    (args); break;
                    case Language.JScript     : CSharpAutoIndentNeeded(args); break;
                    case Language.YAML        : CSharpAutoIndentNeeded(args); break;
                    case Language.Custom      : break;
                    // By WendyH > -------------------------------------------------------
                }
            }
        }

        private void PHPAutoIndentNeeded(AutoIndentEventArgs args) {
            /*
            FastColoredTextBox tb = sender as FastColoredTextBox;
            tb.CalcAutoIndentShiftByCodeFolding(sender, args);*/
            //block {}
            if (Regex.IsMatch(args.LineText, @"^[^""']*\{.*\}[^""']*$"))
                return;
            //start of block {}
            if (Regex.IsMatch(args.LineText, @"^[^""']*\{")) {
                args.ShiftNextLines = args.TabLength;
                return;
            }
            //end of block {}
            if (Regex.IsMatch(args.LineText, @"}[^""']*$")) {
                args.Shift = -args.TabLength;
                args.ShiftNextLines = -args.TabLength;
                return;
            }
            //is unclosed operator in previous line ?
            if (Regex.IsMatch(args.PrevLineText, @"^\s*(if|for|foreach|while|[\}\s]*else)\b[^{]*$"))
                if (!Regex.IsMatch(args.PrevLineText, @"(;\s*$)|(;\s*//)")) //operator is unclosed
                {
                    args.Shift = args.TabLength;
                }
        }

        private void SQLAutoIndentNeeded(object sender, AutoIndentEventArgs args) {
            var tb = sender as FastColoredTextBox;
            tb?.CalcAutoIndentShiftByCodeFolding(sender, args);
        }

        private void HTMLAutoIndentNeeded(object sender, AutoIndentEventArgs args) {
            var tb = sender as FastColoredTextBox;
            tb?.CalcAutoIndentShiftByCodeFolding(sender, args);
        }

        private void XMLAutoIndentNeeded(object sender, AutoIndentEventArgs args) {
            var tb = sender as FastColoredTextBox;
            tb?.CalcAutoIndentShiftByCodeFolding(sender, args);
        }

        // < By WendyH ------------------------------------------
        private void PascalAutoIndentNeeded(AutoIndentEventArgs args) {
            Match m;
            string beginBlock = "\\b(Begin|Try|Repeat|Case)\\b";
            string endBlock   = "\\b(End|Until)\\b";
            if (Regex.IsMatch(args.LineText, @"^\s*(Begin)\b", RegexOptions.IgnoreCase)) {
                if (!Regex.IsMatch(args.PrevLineText, @"\b(Then|Do)[\s#]*$", RegexOptions.IgnoreCase)) {
                    args.AbsoluteIndentation = 0;
                    args.ShiftNextLines = args.TabLength;
                    return;
                }
            }
            m = Regex.Match(args.PrevLineText, @"^\s*(Function|Procedure)[^\(]+\(\s*([^\)]+)$", RegexOptions.IgnoreCase);
            if (m.Success) {
                args.Shift = m.Groups[2].Index;
                args.ShiftNextLines = m.Groups[2].Index;
                return;
            }
            if (Regex.IsMatch(args.LineText, @"^\s*(Var)[\s#]*$", RegexOptions.IgnoreCase)) {
                args.ShiftNextLines = args.TabLength;
                return;
            }
            if (Regex.IsMatch(args.LineText, @"^\s*(Program|Function|Procedure)\b", RegexOptions.IgnoreCase)) {
                args.AbsoluteIndentation = 0;
                //args.Shift = args.TabLength;
                return;
            }
            if (Regex.IsMatch(args.LineText, @"^\s*(Var)\s*\w", RegexOptions.IgnoreCase)) {
                if (Regex.IsMatch(args.PrevLineText, @"(^\s*(Program|Function|Procedure)\b|\)([\s;#]*|\s*:\s*\w+\s*;[\s#]*)$)", RegexOptions.IgnoreCase)) {
                    args.Shift = args.TabLength;
                    args.ShiftNextLines = args.TabLength;
                    return;
                }
            }
            int opened = DetectStartOrEndBlock(args.LineText, beginBlock, endBlock); // By WendyH
            //end of block
//            if (Regex.IsMatch(args.LineText, @"^\s*(End|EndIf|Next|Loop)\b", RegexOptions.IgnoreCase)) {
            if (opened < 0) {
                args.Shift = -args.TabLength;
                args.ShiftNextLines = -args.TabLength;
                return;
            }
            //start of declaration
            //if (Regex.IsMatch(args.LineText, @"\b(Begin)\b", RegexOptions.IgnoreCase)) {
            if (opened > 0) {
                args.ShiftNextLines = args.TabLength;
                return;
            }
            // end block in begining and start block in end (if opened == 0)
            if (Regex.IsMatch(args.LineText, "^\\s+" + endBlock.Replace("End|", "End|Finally|"), RegexOptions.IgnoreCase)) {
                args.Shift = -args.TabLength;
                return;
            }
            // then ...
            if (Regex.IsMatch(args.LineText, @"\b(Then)\s*\S+", RegexOptions.IgnoreCase)) {
                return;
            }
            ////start of operator block
            //if (Regex.IsMatch(args.LineText, @"^\s*(If|While|For|Do|Try|With|Using|Select)\b", RegexOptions.IgnoreCase)) {
            //    args.ShiftNextLines = args.TabLength;
            //    return;
            //}
            ////Statements else, elseif, case etc
            //if (Regex.IsMatch(args.LineText, @"^\s*(Else|ElseIf|Else If|Case|Catch|Finally)\b", RegexOptions.IgnoreCase)) {
            //    args.AbsoluteIndentation = 0;
            //    args.Shift = -args.TabLength;
            //    return;
            //}
            if (Regex.IsMatch(args.PrevLineText, @"\b(AND|OR|NOT)[\s#]*$", RegexOptions.IgnoreCase)) {
                args.Shift = args.TabLength;
                return;
            }
            if (Regex.IsMatch(args.PrevLineText, @"\b(Then|Else|Do)[\s#]*$", RegexOptions.IgnoreCase)) {
                args.Shift = args.TabLength;
                return;
            }
            // Indent with '+' in the end of line --- By WendyH 
            m = Regex.Match(args.PrevLineText, @"=.*?\+[\s#]*$");
            if (m.Success) {
                args.Shift = m.Index + 2 - args.AbsoluteIndentation;
                return;
            }
            m = Regex.Match(args.PrevLineText, @"\S.*?\+[\s#]*$");
            if (m.Success) {
                args.Shift = m.Index - args.AbsoluteIndentation;
                return;
            }
            // -------------------------------------------------

            //Char _
            if (args.PrevLineText.TrimEnd().EndsWith("_")) {
                args.Shift = args.TabLength;
            }
        }

        // > By WendyH ------------------------------------------

        private void VBAutoIndentNeeded(AutoIndentEventArgs args) {
            //end of block
            if (Regex.IsMatch(args.LineText, @"^\s*(End|EndIf|Next|Loop)\b", RegexOptions.IgnoreCase)) {
                args.Shift = -args.TabLength;
                args.ShiftNextLines = -args.TabLength;
                return;
            }
            //start of declaration
            if (Regex.IsMatch(args.LineText, @"\b(Class|Property|Enum|Structure|Sub|Function|Namespace|Interface|Get)\b|(Set\s*\()", RegexOptions.IgnoreCase)) {
                args.ShiftNextLines = args.TabLength;
                return;
            }
            // then ...
            if (Regex.IsMatch(args.LineText, @"\b(Then)\s*\S+", RegexOptions.IgnoreCase))
                return;
            //start of operator block
            if (Regex.IsMatch(args.LineText, @"^\s*(If|While|For|Do|Try|With|Using|Select)\b", RegexOptions.IgnoreCase)) {
                args.ShiftNextLines = args.TabLength;
                return;
            }

            //Statements else, elseif, case etc
            if (Regex.IsMatch(args.LineText, @"^\s*(Else|ElseIf|Case|Catch|Finally)\b", RegexOptions.IgnoreCase)) {
                args.Shift = -args.TabLength;
                return;
            }

            //Char _
            if (args.PrevLineText.TrimEnd().EndsWith("_")) {
                args.Shift = args.TabLength;
                return;
            }
            if (args.PrevLineText.TrimEnd().EndsWith(",")) {
                args.Shift = args.TabLength;
            }
        }

        // By WendyH
        private static int DetectStartOrEndBlock(string text, string startBlock, string endBlock) {
            if (string.IsNullOrEmpty(text)) return 0;
            int result = 0;
            MatchCollection mc = Regex.Matches(text, "(" + startBlock + "|" + endBlock + ")", RegexOptions.IgnoreCase);
            foreach (Match m in mc) {
                if (Regex.IsMatch(m.Value, startBlock, RegexOptions.IgnoreCase)) result++;
                if (Regex.IsMatch(m.Value, endBlock  , RegexOptions.IgnoreCase)) result--;
            }
            return result;
        }

        private void CSharpAutoIndentNeeded(AutoIndentEventArgs args) {
            int opened = DetectStartOrEndBlock(args.LineText, "{", "}");
            //block {}
            //if (Regex.IsMatch(args.LineText, @"^[^""']*\{.*\}[^""']*$"))
            //    return;
            //start of block {}
            //if (Regex.IsMatch(args.LineText, @"^[^""']*\{")) {
            if (opened > 0) {
                args.ShiftNextLines = args.TabLength;
                return;
            }
            //end of block {}
            //if (Regex.IsMatch(args.LineText, @"}[^""']*$")) {
            if (opened < 0) {
                args.Shift = -args.TabLength;
                args.ShiftNextLines = -args.TabLength;
                return;
            }
            if (Regex.IsMatch(args.LineText, @"^\s+}")) {
                args.Shift = -args.TabLength;
                return;
            }
            // Indent with '+' in the end of line --- By WendyH 
            Match m = Regex.Match(args.PrevLineText, @"=.*?\+\s*?$");
            if (m.Success) {
                args.Shift = m.Index + 2 - args.AbsoluteIndentation;
                return;
            }
            m = Regex.Match(args.PrevLineText, @"\S.*?\+\s*?$");
            if (m.Success) {
                args.Shift = m.Index - args.AbsoluteIndentation;
                return;
            }
            // -------------------------------------------------
            //label
            if (Regex.IsMatch(args.LineText, @"^\s*\w+\s*:\s*($|//)") && !Regex.IsMatch(args.LineText, @"^\s*default\s*:")) {
                args.Shift = -args.TabLength;
                return;
            }
            //some statements: case, default
            if (Regex.IsMatch(args.LineText, @"^\s*(case|default)\b.*:[\s#]*($|//)")) {
                args.Shift = -args.TabLength/2;
                return;
            }
            //is unclosed operator in previous line ?
            if (Regex.IsMatch(args.PrevLineText, @"^\s*(if|for|foreach|while|[\}\s]*else)\b[^{]*$"))
                if (!Regex.IsMatch(args.PrevLineText, @"(;\s*$)|(;[\s#]*//)")) //operator is unclosed
                {
                    args.Shift = args.TabLength;
                }
        }

        public static SyntaxDescriptor ParseXmlDescription(XmlDocument doc) {
            var desc = new SyntaxDescriptor();
            XmlNode brackets = doc.SelectSingleNode("doc/brackets");
            if (brackets != null) {
                if (brackets.Attributes != null && (brackets.Attributes["left"] == null || brackets.Attributes["right"] == null || string.IsNullOrEmpty(brackets.Attributes["left"].Value) || brackets.Attributes["right"].Value == "")) {
                    desc.leftBracket  = '\x0';
                    desc.rightBracket = '\x0';
                }
                else {
                    if (brackets.Attributes != null) {
                        desc.leftBracket  = brackets.Attributes["left" ].Value[0];
                        desc.rightBracket = brackets.Attributes["right"].Value[0];
                    }
                }

                if (brackets.Attributes != null && (brackets.Attributes["left2"] == null || brackets.Attributes["right2"] == null || brackets.Attributes["left2"].Value == "" || brackets.Attributes["right2"].Value == "")) {
                    desc.leftBracket2  = '\x0';
                    desc.rightBracket2 = '\x0';
                }
                else {
                    if (brackets.Attributes != null) {
                        desc.leftBracket2  = brackets.Attributes["left2" ].Value[0];
                        desc.rightBracket2 = brackets.Attributes["right2"].Value[0];
                    }
                }

                if (brackets.Attributes != null && (brackets.Attributes["strategy"] == null || brackets.Attributes["strategy"].Value == ""))
                    desc.bracketsHighlightStrategy = BracketsHighlightStrategy.Strategy2;
                else if (brackets.Attributes != null)
                    desc.bracketsHighlightStrategy = (BracketsHighlightStrategy) Enum.Parse(typeof(BracketsHighlightStrategy), brackets.Attributes["strategy"].Value);
            }

            var styleByName = new Dictionary<string, Style>();

            var xmlNodeList = doc.SelectNodes("doc/style");
            if (xmlNodeList != null)
                foreach (XmlNode style in xmlNodeList) {
                    var s = ParseStyle(style);
                    if (s == null) continue;
                    if (style.Attributes != null) styleByName[style.Attributes["name"].Value] = s;
                    desc.styles.Add(s);
                }
            var selectNodes = doc.SelectNodes("doc/rule");
            if (selectNodes != null)
                foreach (XmlNode rule in selectNodes)
                    desc.rules.Add(ParseRule(rule, styleByName));
            foreach (XmlNode folding in doc.SelectNodes("doc/folding"))
                desc.foldings.Add(ParseFolding(folding));

            return desc;
        }

        private static FoldingDesc ParseFolding(XmlNode foldingNode) {
            var folding = new FoldingDesc();
            //regex
            if (foldingNode.Attributes != null) {
                folding.startMarkerRegex  = foldingNode.Attributes["start"].Value;
                folding.finishMarkerRegex = foldingNode.Attributes["finish"].Value;
                //options
                XmlAttribute optionsA = foldingNode.Attributes["options"];
                if (optionsA != null)
                    folding.options = (RegexOptions) Enum.Parse(typeof(RegexOptions), optionsA.Value);
            }

            return folding;
        }

        private static RuleDesc ParseRule(XmlNode ruleNode, Dictionary<string, Style> styles) {
            var rule = new RuleDesc {pattern = ruleNode.InnerText};
            //
            if (ruleNode.Attributes != null) {
                XmlAttribute styleA   = ruleNode.Attributes["style"];
                XmlAttribute optionsA = ruleNode.Attributes["options"];
                //Style
                if (styleA == null)
                    throw new Exception("Rule must contain style name.");
                if (!styles.ContainsKey(styleA.Value))
                    throw new Exception("Style '" + styleA.Value + "' is not found.");
                rule.style = styles[styleA.Value];
                //options
                if (optionsA != null)
                    rule.options = (RegexOptions) Enum.Parse(typeof(RegexOptions), optionsA.Value);
            }

            return rule;
        }

        private static Style ParseStyle(XmlNode styleNode) {
            if (styleNode.Attributes != null) {
                XmlAttribute colorA     = styleNode.Attributes["color"    ];
                XmlAttribute backColorA = styleNode.Attributes["backColor"];
                XmlAttribute fontStyleA = styleNode.Attributes["fontStyle"];
                //colors
                SolidBrush foreBrush = null;
                if (colorA != null)
                    foreBrush = new SolidBrush(ParseColor(colorA.Value));
                SolidBrush backBrush = null;
                if (backColorA != null)
                    backBrush = new SolidBrush(ParseColor(backColorA.Value));
                //fontStyle
                FontStyle fontStyle = FontStyle.Regular;
                if (fontStyleA != null)
                    fontStyle = (FontStyle) Enum.Parse(typeof(FontStyle), fontStyleA.Value);

                return new TextStyle(foreBrush, backBrush, fontStyle);
            }
            return null;
        }

        private static Color ParseColor(string s) {
            if (s.StartsWith("#")) {
                if (s.Length <= 7)
                    return Color.FromArgb(255, Color.FromArgb(Int32.Parse(s.Substring(1), NumberStyles.AllowHexSpecifier)));
                else
                    return Color.FromArgb(Int32.Parse(s.Substring(1), NumberStyles.AllowHexSpecifier));
            }
            else
                return Color.FromName(s);
        }

        public static void HighlightSyntax(SyntaxDescriptor desc, Range range) {
            //set style order
            range.tb.ClearStylesBuffer();
            for (int i = 0; i < desc.styles.Count; i++)
                range.tb.Styles[i] = desc.styles[i];
            //brackets
            char[] oldBrackets     = RememberBrackets(range.tb);
            range.tb.LeftBracket   = desc.leftBracket;
            range.tb.RightBracket  = desc.rightBracket;
            range.tb.LeftBracket2  = desc.leftBracket2;
            range.tb.RightBracket2 = desc.rightBracket2;
            //clear styles of range
            range.ClearStyle(desc.styles.ToArray());
            //highlight syntax
            foreach (RuleDesc rule in desc.rules)
                range.SetStyle(rule.style, rule.Regex);
            //clear folding
            range.ClearFoldingMarkers();
            //folding markers
            foreach (FoldingDesc folding in desc.foldings)
                range.SetFoldingMarkers(folding.startMarkerRegex, folding.finishMarkerRegex, folding.options);

            //
            RestoreBrackets(range.tb, oldBrackets);
        }

        private static void RestoreBrackets(FastColoredTextBox tb, char[] oldBrackets) {
            tb.LeftBracket   = oldBrackets[0];
            tb.RightBracket  = oldBrackets[1];
            tb.LeftBracket2  = oldBrackets[2];
            tb.RightBracket2 = oldBrackets[3];
        }

        private static char[] RememberBrackets(FastColoredTextBox tb) {
            return new[] {tb.LeftBracket, tb.RightBracket, tb.LeftBracket2, tb.RightBracket2};
        }

        // < By WendyH ------------------------------------------
        public void InitStyleTheme(Language lang) {
            if (StyleTheme != null) {
                if (StyleTheme.StringStyle  != null) StringStyle  = StyleTheme.StringStyle;
                if (StyleTheme.CommentStyle != null) CommentStyle = StyleTheme.CommentStyle;
                NumberStyle       = StyleTheme.NumberStyle;
                ClassNameStyle    = StyleTheme.ClassNameStyle;
                KeywordStyle      = StyleTheme.KeywordStyle;
                TagBracketStyle   = StyleTheme.TagBracketStyle;
                CommentTagStyle   = StyleTheme.CommentTagStyle;
                FunctionsStyle    = StyleTheme.FunctionsStyle;
                VariableStyle     = StyleTheme.VariableStyle;
                ConstantsStyle    = StyleTheme.ConstantsStyle;
                DeclFunctionStyle = StyleTheme.DeclFunctionStyle;
                TypesStyle        = StyleTheme.TypesStyle;
                PunctuationSyle   = StyleTheme.PunctuationStyle;
                if (TypesStyle      == null) TypesStyle      = KeywordStyle;
                if (ClassNameStyle  == null) ClassNameStyle  = TypesStyle;
                if (PunctuationSyle == null) PunctuationSyle = KeywordStyle;
                if (StyleTheme.Name == "Стандартная") {
                    if (Array.IndexOf(clike, lang) >= 0) KeywordStyle = TypesStyle;
                }



            }
        }

        private Language[] clike = { Language.CPPScript, Language.JScript, Language.JS, Language.CSharp, Language.PHP };
        // > By WendyH ------------------------------------------

        public void InitStyleSchema(Language lang) {
            FunctionsStyle    = null;
            DeclFunctionStyle = null;
            VariableStyle     = null;
            ConstantsStyle    = null;
            PunctuationSyle   = null;
            switch (lang) {
                case Language.CSharp:
                    StringStyle     = BrownStyle;
                    CommentStyle    = GreenStyle;
                    NumberStyle     = MagentaStyle;
                    AttributeStyle  = GreenStyle;
                    ClassNameStyle  = VSClassStyle;
                    KeywordStyle    = BlueStyle;
                    TypesStyle      = BlueStyle;
                    CommentTagStyle = GrayStyle;
                    break;
                case Language.VB:
                    StringStyle     = BrownStyle;
                    CommentStyle    = GreenStyle;
                    NumberStyle     = MagentaStyle;
                    ClassNameStyle  = VSClassStyle;
                    KeywordStyle    = BlueStyle;
                    TypesStyle      = BlueStyle;
                    break;
                case Language.HTML:
                    CommentStyle        = GreenStyle;
                    TagBracketStyle     = BlueStyle;
                    TagNameStyle        = MaroonStyle;
                    AttributeStyle      = RedStyle;
                    AttributeValueStyle = BlueStyle;
                    HtmlEntityStyle     = RedStyle;
                    break;
                case Language.XML:
                    CommentStyle        = GreenStyle;
                    XmlTagBracketStyle  = BlueStyle;
                    XmlTagNameStyle     = MaroonStyle;
                    XmlAttributeStyle   = RedStyle;
                    XmlAttributeValueStyle = BlueStyle;
                    XmlEntityStyle      = RedStyle;
                    XmlCDataStyle       = BlackStyle;
                    break;
                case Language.JS:
                    StringStyle     = BrownStyle;
                    CommentStyle    = GreenStyle;
                    NumberStyle     = MagentaStyle;
                    KeywordStyle    = BlueStyle;
                    TypesStyle      = BlueStyle;
                    break;
                case Language.Lua:
                    StringStyle     = BrownStyle;
                    CommentStyle    = GreenStyle;
                    NumberStyle     = MagentaStyle;
                    KeywordStyle    = BlueBoldStyle;
                    TypesStyle      = BlueStyle;
                    FunctionsStyle  = MaroonStyle;
                    break;
                case Language.PHP:
                    StringStyle     = RedStyle;
                    CommentStyle    = GreenStyle;
                    NumberStyle     = RedStyle;
                    VariableStyle   = MaroonStyle;
                    KeywordStyle    = MagentaStyle;
                    KeywordStyle2   = BlueStyle;
                    KeywordStyle3   = GrayStyle;
                    TypesStyle      = BlueStyle;
                    break;
                case Language.SQL:
                    StringStyle     = RedStyle;
                    CommentStyle    = GreenStyle;
                    NumberStyle     = MagentaStyle;
                    KeywordStyle    = BlueBoldStyle;
                    StatementsStyle = BlueBoldStyle;
                    FunctionsStyle  = MaroonStyle;
                    VariableStyle   = MaroonStyle;
                    TypesStyle      = BrownStyle;
                    break;
                // By WendyH < -------------------------------
                case Language.CPPScript:
                    StringStyle     = DarkRedStyle;
                    CommentStyle    = GreenStyle;
                    NumberStyle     = MagentaStyle;
                    AttributeStyle  = GreenStyle;
                    ClassNameStyle  = VSClassStyle;
                    KeywordStyle    = BlueStyle;
                    CommentTagStyle = GrayStyle;
                    TypesStyle      = BlueStyle;
                    break;
                case Language.PascalScript:
                    StringStyle     = DarkRedStyle;
                    CommentStyle    = GreenStyle;
                    NumberStyle     = MagentaStyle;
                    ClassNameStyle  = VSClassStyle;
                    KeywordStyle    = new TextStyle(null, null, AltPascalKeywordsHighlight ? FontStyle.Bold | FontStyle.Underline : FontStyle.Bold);
                    TypesStyle      = BlueStyle;
                    break;
                case Language.BasicScript:
                    StringStyle     = DarkRedStyle;
                    CommentStyle    = GreenStyle;
                    NumberStyle     = MagentaStyle;
                    ClassNameStyle  = VSClassStyle;
                    KeywordStyle    = BlueStyle;
                    TypesStyle      = BlueStyle;
                    break;
                case Language.JScript:
                    StringStyle     = DarkRedStyle;
                    CommentStyle    = GreenStyle;
                    NumberStyle     = MagentaStyle;
                    KeywordStyle    = BlueStyle;
                    TypesStyle      = BlueStyle;
                    break;
                case Language.YAML:
                    StringStyle     = DarkRedStyle;
                    CommentStyle    = GreenStyle;
                    NumberStyle     = MagentaStyle;
                    ClassNameStyle  = WendyHsLiteStyle;
                    KeywordStyle    = BlueBoldStyle;
                    TypesStyle      = BlueStyle;
                    TagBracketStyle = DarkOrangeStyle;
                    break;
                case Language.Custom:
                    StringStyle     = BrownStyle;
                    CommentStyle    = GreenStyle;
                    NumberStyle     = MagentaStyle;
                    AttributeStyle  = GreenStyle;
                    ClassNameStyle  = VSClassStyle;
                    KeywordStyle    = BlueStyle;
                    TypesStyle      = BlueStyle;
                    CommentTagStyle = GrayStyle;
                    break;
                // By WendyH > -------------------------------
            }
            InitStyleTheme(lang);
            if ((lang == Language.PascalScript) && (KeywordStyle != null))
                KeywordStyle = new TextStyle(((TextStyle)KeywordStyle).ForeBrush, ((TextStyle)KeywordStyle).BackgroundBrush, ((TextStyle)KeywordStyle).FontStyle | FontStyle.Bold | (AltPascalKeywordsHighlight ? FontStyle.Underline : FontStyle.Regular));
            if ((lang == Language.BasicScript) && (KeywordStyle != null))
                KeywordStyle = new TextStyle(((TextStyle)KeywordStyle).ForeBrush, ((TextStyle)KeywordStyle).BackgroundBrush, ((TextStyle)KeywordStyle).FontStyle | FontStyle.Bold);
            StringStyle = RedStringsHighlight ? RedStyle : StringStyle; // By WendyH
        }

        /// <summary>
        /// Highlights C# code
        /// </summary>
        /// <param name="range"></param>
        public void CSharpSyntaxHighlight(Range range) {
            range.tb.CommentPrefix = "//";
            range.tb.LeftBracket   = '(';
            range.tb.RightBracket  = ')';
            range.tb.LeftBracket2  = '{';
            range.tb.RightBracket2 = '}';
            range.tb.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy1;

            range.tb.AutoIndentCharsPatterns = @"^\s*[\w\.]+(\s\w+)?\s*(?<range>=)\s*(?<range>[^;]+);^\s*(case|default)\s*[^:]*(?<range>:)\s*(?<range>[^;]+);";
            range.ClearStyle(StringStyle, CommentStyle, NumberStyle, AttributeStyle, ClassNameStyle, KeywordStyle);
            range.SetStyle(NumberStyle, CSharpNumberRegex);
            range.SetStyle(AttributeStyle, CSharpAttributeRegex);
            range.SetStyle(ClassNameStyle, CSharpClassNameRegex);
            range.SetStyle(KeywordStyle, CSharpKeywordRegex);
            range.SetStylesStringsAndComments(CSharpStringAndCommentsRegex, StringStyle, CommentStyle); // By WendyH

            //find document comments
            foreach (Range r in range.GetRanges(@"^\s*///.*$", RegexOptions.Multiline)) {
                //remove C# highlighting from this fragment
                r.ClearStyle(StyleIndex.All);
                //do XML highlighting
                if (HTMLTagRegex == null)
                    InitHTMLRegex();
                //
                r.SetStyle(CommentStyle);
                //tags
                foreach (Range rr in r.GetRanges(HTMLTagContentRegex)) {
                    rr.ClearStyle(StyleIndex.All);
                    rr.SetStyle(CommentTagStyle);
                }
                //prefix '///'
                foreach (Range rr in r.GetRanges(@"^\s*///", RegexOptions.Multiline)) {
                    rr.ClearStyle(StyleIndex.All);
                    rr.SetStyle(CommentTagStyle);
                }
            }

            //clear folding markers
            range.ClearFoldingMarkers();
            //set folding markers
            range.SetFoldingMarkers("{", "}"); //allow to collapse brackets block
            range.SetFoldingMarkers(@"#region\b", @"#endregion\b"); //allow to collapse #region blocks
            range.SetFoldingMarkers(@"/\*", @"\*/"); //allow to collapse comment block
        }

        private void InitVBRegex() {
//            VBStringRegex    = new Regex(@"""(\\[\s\S]|[^""])*""|('.*)", RegexCompiledOption);  // By WendyH
            VBStringRegex    = new Regex(@"""(\\""|[^""])*""|('.*)", RegexCompiledOption); // By WendyH
            VBNumberRegex    = new Regex(@"\b\d+[\.]?\d*([eE]\-?\d+)?\b", RegexCompiledOption);
            VBClassNameRegex = new Regex(@"\b(Class|Structure|Enum|Interface)[ ]+(?<range>\w+?)\b", RegexOptions.IgnoreCase | RegexCompiledOption);
            VBKeywordRegex   = new Regex(@"\b(AddHandler|AddressOf|Alias|And|AndAlso|As|Boolean|ByRef|Byte|ByVal|Call|Case|Catch|CBool|CByte|CChar|CDate|CDbl|CDec|Char|CInt|Class|CLng|CObj|Const|Continue|CSByte|CShort|CSng|CStr|CType|CUInt|CULng|CUShort|Date|Decimal|Declare|Default|Delegate|Dim|DirectCast|Do|Double|Each|Else|ElseIf|End|EndIf|Enum|Erase|Error|Event|Exit|False|Finally|For|Friend|Function|Get|GetType|GetXMLNamespace|Global|GoSub|GoTo|Handles|If|Implements|Imports|In|Inherits|Integer|Interface|Is|IsNot|Let|Lib|Like|Long|Loop|Me|Mod|Module|MustInherit|MustOverride|MyBase|MyClass|Namespace|Narrowing|New|Next|Not|Nothing|NotInheritable|NotOverridable|Object|Of|On|Operator|Option|Optional|Or|OrElse|Overloads|Overridable|Overrides|ParamArray|Partial|Private|Property|Protected|Public|RaiseEvent|ReadOnly|ReDim|REM|RemoveHandler|Resume|Return|SByte|Select|Set|Shadows|Shared|Short|Single|Static|Step|Stop|String|Structure|Sub|SyncLock|Then|Throw|To|True|Try|TryCast|TypeOf|UInteger|ULong|UShort|Using|Variant|Wend|When|While|Widening|With|WithEvents|WriteOnly|Xor|Region)\b|(#Const|#Else|#ElseIf|#End|#If|#Region)\b", RegexOptions.IgnoreCase | RegexCompiledOption);
        }

        /// <summary>
        /// Highlights VB code
        /// </summary>
        /// <param name="range"></param>
        public void VBSyntaxHighlight(Range range) {
            range.tb.CommentPrefix = "'";
            range.tb.LeftBracket   = '(';
            range.tb.RightBracket  = ')';
            range.tb.LeftBracket2  = '\x0';
            range.tb.RightBracket2 = '\x0';

            range.tb.AutoIndentCharsPatterns = @"^\s*[\w\.\(\)]+\s*(?<range>=)\s*(?<range>.+)";
            range.ClearStyle(StringStyle, CommentStyle, NumberStyle, ClassNameStyle, KeywordStyle);
            if (VBStringRegex == null) InitVBRegex();
            range.SetStyle(NumberStyle, VBNumberRegex);
            range.SetStyle(ClassNameStyle, VBClassNameRegex);
            range.SetStyle(KeywordStyle, VBKeywordRegex);
            range.SetStylesStringsAndComments(VBStringRegex, StringStyle, CommentStyle, false);

            //clear folding markers
            range.ClearFoldingMarkers();
            //set folding markers
            range.SetFoldingMarkers(@"#Region\b", @"#End\s+Region\b", RegexOptions.IgnoreCase);
            range.SetFoldingMarkers(@"\b(Class|Property|Enum|Structure|Interface)[ \t]+\S+", @"\bEnd (Class|Property|Enum|Structure|Interface)\b", RegexOptions.IgnoreCase);
            range.SetFoldingMarkers(@"^\s*(?<range>While)[ \t]+\S+", @"^\s*(?<range>End While)\b", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            range.SetFoldingMarkers(@"\b(Sub|Function)[ \t]+[^\s']+", @"\bEnd (Sub|Function)\b", RegexOptions.IgnoreCase);
            //this declared separately because Sub and Function can be unclosed
            range.SetFoldingMarkers(@"(\r|\n|^)[ \t]*(?<range>Get|Set)[ \t]*(\r|\n|$)", @"\bEnd (Get|Set)\b", RegexOptions.IgnoreCase);
            range.SetFoldingMarkers(@"^\s*(?<range>For|For\s+Each)\b", @"^\s*(?<range>Next)\b", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            range.SetFoldingMarkers(@"^\s*(?<range>Do)\b", @"^\s*(?<range>Loop)\b", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        }

        private void InitHTMLRegex() {
            HTMLCommentRegex1   = new Regex(@"(<!--.*?-->)|(<!--.*)", RegexOptions.Singleline | RegexCompiledOption);
            HTMLCommentRegex2   = new Regex(@"(<!--.*?-->)|(.*-->)" , RegexOptions.Singleline | RegexOptions.RightToLeft | RegexCompiledOption);
            HTMLTagRegex        = new Regex(@"<|/>|</|>", RegexCompiledOption);
            HTMLTagNameRegex    = new Regex(@"<(?<range>[!\w:]+)"   , RegexCompiledOption);
            HTMLEndTagRegex     = new Regex(@"</(?<range>[\w:]+)>"  , RegexCompiledOption);
            HTMLTagContentRegex = new Regex(@"<[^>]+>", RegexCompiledOption);
            HTMLAttrRegex       = new Regex(@"(?<range>[\w\d\-]{1,20}?)='[^']*'|(?<range>[\w\d\-]{1,20})=""[^""]*""|(?<range>[\w\d\-]{1,20})=[\w\d\-]{1,20}", RegexCompiledOption);
            HTMLAttrValRegex    = new Regex(@"[\w\d\-]{1,20}?=(?<range>'[^']*')|[\w\d\-]{1,20}=(?<range>""[^""]*"")|[\w\d\-]{1,20}=(?<range>[\w\d\-]{1,20})", RegexCompiledOption);
            HTMLEntityRegex     = new Regex(@"\&(amp|gt|lt|nbsp|quot|apos|copy|reg|#[0-9]{1,8}|#x[0-9a-f]{1,8});", RegexCompiledOption | RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Highlights HTML code
        /// </summary>
        /// <param name="range"></param>
        public void HTMLSyntaxHighlight(Range range) {
            range.tb.CommentPrefix = null;
            range.tb.LeftBracket   = '<';
            range.tb.RightBracket  = '>';
            range.tb.LeftBracket2  = '(';
            range.tb.RightBracket2 = ')';
            range.tb.AutoIndentCharsPatterns = @"";
            //clear style of changed range
            range.ClearStyle(CommentStyle, TagBracketStyle, TagNameStyle, AttributeStyle, AttributeValueStyle, HtmlEntityStyle);
            //
            if (HTMLTagRegex == null)
                InitHTMLRegex();
            //comment highlighting
            range.SetStyle(CommentStyle, HTMLCommentRegex1);
            range.SetStyle(CommentStyle, HTMLCommentRegex2);
            //tag brackets highlighting
            range.SetStyle(TagBracketStyle, HTMLTagRegex);
            //tag name
            range.SetStyle(TagNameStyle, HTMLTagNameRegex);
            //end of tag
            range.SetStyle(TagNameStyle, HTMLEndTagRegex);
            //attributes
            range.SetStyle(AttributeStyle, HTMLAttrRegex);
            //attribute values
            range.SetStyle(AttributeValueStyle, HTMLAttrValRegex);
            //html entity
            range.SetStyle(HtmlEntityStyle, HTMLEntityRegex);

            //clear folding markers
            range.ClearFoldingMarkers();
            ////set folding markers
            //range.SetFoldingMarkers("<head"  , "</head>"  , RegexOptions.IgnoreCase);
            //range.SetFoldingMarkers("<body"  , "</body>"  , RegexOptions.IgnoreCase);
            //range.SetFoldingMarkers("<table" , "</table>" , RegexOptions.IgnoreCase);
            //range.SetFoldingMarkers("<form"  , "</form>"  , RegexOptions.IgnoreCase);
            //range.SetFoldingMarkers("<div"   , "</div>"   , RegexOptions.IgnoreCase);
            //range.SetFoldingMarkers("<script", "</script>", RegexOptions.IgnoreCase);
            //range.SetFoldingMarkers("<tr"    , "</tr>"    , RegexOptions.IgnoreCase);
        }

        private void InitXMLRegex() {
            XMLCommentRegex1 = new Regex(@"(<!--.*?-->)|(<!--.*)", RegexOptions.Singleline | RegexCompiledOption);
            XMLCommentRegex2 = new Regex(@"(<!--.*?-->)|(.*-->)", RegexOptions.Singleline | RegexOptions.RightToLeft | RegexCompiledOption);
            XMLTagRegex      = new Regex(@"<\?|<|/>|</|>|\?>", RegexCompiledOption);
            XMLTagNameRegex  = new Regex(@"<[?](?<range1>[x][m][l]{1})|<(?<range>[!\w:]+)", RegexCompiledOption);
            XMLEndTagRegex   = new Regex(@"</(?<range>[\w:]+)>", RegexCompiledOption);
            XMLAttrRegex     = new Regex(@"(?<range>[\w\d\-\:]+)[ ]*=[ ]*'[^']*'|(?<range>[\w\d\-\:]+)[ ]*=[ ]*""[^""]*""|(?<range>[\w\d\-\:]+)[ ]*=[ ]*[\w\d\-\:]+", RegexCompiledOption);
            XMLAttrValRegex  = new Regex(@"[\w\d\-]+?=(?<range>'[^']*')|[\w\d\-]+[ ]*=[ ]*(?<range>""[^""]*"")|[\w\d\-]+[ ]*=[ ]*(?<range>[\w\d\-]+)", RegexCompiledOption);
            XMLEntityRegex   = new Regex(@"\&(amp|gt|lt|nbsp|quot|apos|copy|reg|#[0-9]{1,8}|#x[0-9a-f]{1,8});", RegexCompiledOption | RegexOptions.IgnoreCase);
            XMLCDataRegex    = new Regex(@"<!\s*\[CDATA\s*\[(?<text>(?>[^]]+|](?!]>))*)]]>", RegexCompiledOption | RegexOptions.IgnoreCase); // http://stackoverflow.com/questions/21681861/i-need-a-regex-that-matches-cdata-elements-in-html
            XMLFoldingRegex  = new Regex(@"<(?<range>/?\w+)\s[^>]*?[^/]>|<(?<range>/?\w+)\s*>", RegexOptions.Singleline | RegexCompiledOption);
        }

        /// <summary>
        /// Highlights XML code
        /// </summary>
        /// <param name="range"></param>
        public void XMLSyntaxHighlight(Range range) {
            range.tb.CommentPrefix = null;
            range.tb.LeftBracket   = '<';
            range.tb.RightBracket  = '>';
            range.tb.LeftBracket2  = '(';
            range.tb.RightBracket2 = ')';
            range.tb.AutoIndentCharsPatterns = @"";
            //clear style of changed range
            range.ClearStyle(CommentStyle, XmlTagBracketStyle, XmlTagNameStyle, XmlAttributeStyle, XmlAttributeValueStyle, XmlEntityStyle, XmlCDataStyle);

            //
            if (XMLTagRegex == null) {
                InitXMLRegex();
            }

            //xml CData
            range.SetStyle(XmlCDataStyle, XMLCDataRegex);

            //comment highlighting
            range.SetStyle(CommentStyle, XMLCommentRegex1);
            range.SetStyle(CommentStyle, XMLCommentRegex2);

            //tag brackets highlighting
            range.SetStyle(XmlTagBracketStyle, XMLTagRegex);

            //tag name
            range.SetStyle(XmlTagNameStyle, XMLTagNameRegex);

            //end of tag
            range.SetStyle(XmlTagNameStyle, XMLEndTagRegex);

            //attributes
            range.SetStyle(XmlAttributeStyle, XMLAttrRegex);

            //attribute values
            range.SetStyle(XmlAttributeValueStyle, XMLAttrValRegex);

            //xml entity
            range.SetStyle(XmlEntityStyle, XMLEntityRegex);

            //clear folding markers
            range.ClearFoldingMarkers();

            //set folding markers
            XmlFolding(range);
        }

        private void XmlFolding(Range range) {
            var stack = new Stack<XmlFoldingTag>();
            var id = 0;
            var fctb = range.tb;
            //extract opening and closing tags (exclude open-close tags: <TAG/>)
            foreach (var r in range.GetRanges(XMLFoldingRegex)) {
                var tagName = r.Text;
                var iLine = r.Start.iLine;
                //if it is opening tag...
                if (tagName[0] != '/') {
                    // ...push into stack
                    var tag = new XmlFoldingTag {Name = tagName, id = id++, startLine = r.Start.iLine};
                    stack.Push(tag);
                    // if this line has no markers - set marker
                    if (string.IsNullOrEmpty(fctb[iLine].FoldingStartMarker))
                        fctb[iLine].FoldingStartMarker = tag.Marker;
                }
                else {
                    //if it is closing tag - pop from stack
                    if (stack.Count > 0) {
                        var tag = stack.Pop();
                        //compare line number
                        if (iLine == tag.startLine) {
                            //remove marker, because same line can not be folding
                            if (fctb[iLine].FoldingStartMarker == tag.Marker) //was it our marker?
                                fctb[iLine].FoldingStartMarker = null;
                        }
                        else {
                            //set end folding marker
                            if (string.IsNullOrEmpty(fctb[iLine].FoldingEndMarker))
                                fctb[iLine].FoldingEndMarker = tag.Marker;
                        }
                    }
                }
            }
        }

        class XmlFoldingTag {
            public string Name;
            public int    id;
            public int    startLine;
            public string Marker => Name + id;
        }

        private void InitSQLRegex() {
            SQLStringRegex     = new Regex(@"""""|''|"".*?[^\\]""|'.*?[^\\]'", RegexCompiledOption);
            SQLNumberRegex     = new Regex(@"\b\d+[\.]?\d*([eE]\-?\d+)?\b", RegexCompiledOption);
            SQLCommentRegex1   = new Regex(@"--.*$", RegexOptions.Multiline | RegexCompiledOption);
            SQLCommentRegex2   = new Regex(@"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline | RegexCompiledOption);
            SQLCommentRegex3   = new Regex(@"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft | RegexCompiledOption);
            SQLVarRegex        = new Regex(@"@[a-zA-Z_\d]*\b", RegexCompiledOption);
            SQLStatementsRegex = new Regex(@"\b(ALTER APPLICATION ROLE|ALTER ASSEMBLY|ALTER ASYMMETRIC KEY|ALTER AUTHORIZATION|ALTER BROKER PRIORITY|ALTER CERTIFICATE|ALTER CREDENTIAL|ALTER CRYPTOGRAPHIC PROVIDER|ALTER DATABASE|ALTER DATABASE AUDIT SPECIFICATION|ALTER DATABASE ENCRYPTION KEY|ALTER ENDPOINT|ALTER EVENT SESSION|ALTER FULLTEXT CATALOG|ALTER FULLTEXT INDEX|ALTER FULLTEXT STOPLIST|ALTER FUNCTION|ALTER INDEX|ALTER LOGIN|ALTER MASTER KEY|ALTER MESSAGE TYPE|ALTER PARTITION FUNCTION|ALTER PARTITION SCHEME|ALTER PROCEDURE|ALTER QUEUE|ALTER REMOTE SERVICE BINDING|ALTER RESOURCE GOVERNOR|ALTER RESOURCE POOL|ALTER ROLE|ALTER ROUTE|ALTER SCHEMA|ALTER SERVER AUDIT|ALTER SERVER AUDIT SPECIFICATION|ALTER SERVICE|ALTER SERVICE MASTER KEY|ALTER SYMMETRIC KEY|ALTER TABLE|ALTER TRIGGER|ALTER USER|ALTER VIEW|ALTER WORKLOAD GROUP|ALTER XML SCHEMA COLLECTION|BULK INSERT|CREATE AGGREGATE|CREATE APPLICATION ROLE|CREATE ASSEMBLY|CREATE ASYMMETRIC KEY|CREATE BROKER PRIORITY|CREATE CERTIFICATE|CREATE CONTRACT|CREATE CREDENTIAL|CREATE CRYPTOGRAPHIC PROVIDER|CREATE DATABASE|CREATE DATABASE AUDIT SPECIFICATION|CREATE DATABASE ENCRYPTION KEY|CREATE DEFAULT|CREATE ENDPOINT|CREATE EVENT NOTIFICATION|CREATE EVENT SESSION|CREATE FULLTEXT CATALOG|CREATE FULLTEXT INDEX|CREATE FULLTEXT STOPLIST|CREATE FUNCTION|CREATE INDEX|CREATE LOGIN|CREATE MASTER KEY|CREATE MESSAGE TYPE|CREATE PARTITION FUNCTION|CREATE PARTITION SCHEME|CREATE PROCEDURE|CREATE QUEUE|CREATE REMOTE SERVICE BINDING|CREATE RESOURCE POOL|CREATE ROLE|CREATE ROUTE|CREATE RULE|CREATE SCHEMA|CREATE SERVER AUDIT|CREATE SERVER AUDIT SPECIFICATION|CREATE SERVICE|CREATE SPATIAL INDEX|CREATE STATISTICS|CREATE SYMMETRIC KEY|CREATE SYNONYM|CREATE TABLE|CREATE TRIGGER|CREATE TYPE|CREATE USER|CREATE VIEW|CREATE WORKLOAD GROUP|CREATE XML INDEX|CREATE XML SCHEMA COLLECTION|DELETE|DISABLE TRIGGER|DROP AGGREGATE|DROP APPLICATION ROLE|DROP ASSEMBLY|DROP ASYMMETRIC KEY|DROP BROKER PRIORITY|DROP CERTIFICATE|DROP CONTRACT|DROP CREDENTIAL|DROP CRYPTOGRAPHIC PROVIDER|DROP DATABASE|DROP DATABASE AUDIT SPECIFICATION|DROP DATABASE ENCRYPTION KEY|DROP DEFAULT|DROP ENDPOINT|DROP EVENT NOTIFICATION|DROP EVENT SESSION|DROP FULLTEXT CATALOG|DROP FULLTEXT INDEX|DROP FULLTEXT STOPLIST|DROP FUNCTION|DROP INDEX|DROP LOGIN|DROP MASTER KEY|DROP MESSAGE TYPE|DROP PARTITION FUNCTION|DROP PARTITION SCHEME|DROP PROCEDURE|DROP QUEUE|DROP REMOTE SERVICE BINDING|DROP RESOURCE POOL|DROP ROLE|DROP ROUTE|DROP RULE|DROP SCHEMA|DROP SERVER AUDIT|DROP SERVER AUDIT SPECIFICATION|DROP SERVICE|DROP SIGNATURE|DROP STATISTICS|DROP SYMMETRIC KEY|DROP SYNONYM|DROP TABLE|DROP TRIGGER|DROP TYPE|DROP USER|DROP VIEW|DROP WORKLOAD GROUP|DROP XML SCHEMA COLLECTION|ENABLE TRIGGER|EXEC|EXECUTE|REPLACE|FROM|INSERT|MERGE|OPTION|OUTPUT|SELECT|TOP|TRUNCATE TABLE|UPDATE|UPDATE STATISTICS|WHERE|WITH|INTO|IN|SET)\b", RegexOptions.IgnoreCase | RegexCompiledOption);
            SQLKeywordsRegex   = new Regex(@"\b(ADD|ALL|AND|ANY|AS|ASC|AUTHORIZATION|BACKUP|BEGIN|BETWEEN|BREAK|BROWSE|BY|CASCADE|CHECK|CHECKPOINT|CLOSE|CLUSTERED|COLLATE|COLUMN|COMMIT|COMPUTE|CONSTRAINT|CONTAINS|CONTINUE|CROSS|CURRENT|CURRENT_DATE|CURRENT_TIME|CURSOR|DATABASE|DBCC|DEALLOCATE|DECLARE|DEFAULT|DENY|DESC|DISK|DISTINCT|DISTRIBUTED|DOUBLE|DUMP|ELSE|END|ERRLVL|ESCAPE|EXCEPT|EXISTS|EXIT|EXTERNAL|FETCH|FILE|FILLFACTOR|FOR|FOREIGN|FREETEXT|FULL|FUNCTION|GOTO|GRANT|GROUP|HAVING|HOLDLOCK|IDENTITY|IDENTITY_INSERT|IDENTITYCOL|IF|INDEX|INNER|INTERSECT|IS|JOIN|KEY|KILL|LIKE|LINENO|LOAD|NATIONAL|NOCHECK|NONCLUSTERED|NOT|NULL|OF|OFF|OFFSETS|ON|OPEN|OR|ORDER|OUTER|OVER|PERCENT|PIVOT|PLAN|PRECISION|PRIMARY|PRINT|PROC|PROCEDURE|PUBLIC|RAISERROR|READ|READTEXT|RECONFIGURE|REFERENCES|REPLICATION|RESTORE|RESTRICT|RETURN|REVERT|REVOKE|ROLLBACK|ROWCOUNT|ROWGUIDCOL|RULE|SAVE|SCHEMA|SECURITYAUDIT|SHUTDOWN|SOME|STATISTICS|TABLE|TABLESAMPLE|TEXTSIZE|THEN|TO|TRAN|TRANSACTION|TRIGGER|TSEQUAL|UNION|UNIQUE|UNPIVOT|UPDATETEXT|USE|USER|VALUES|VARYING|VIEW|WAITFOR|WHEN|WHILE|WRITETEXT)\b", RegexOptions.IgnoreCase | RegexCompiledOption);
            SQLFunctionsRegex  = new Regex(@"(@@CONNECTIONS|@@CPU_BUSY|@@CURSOR_ROWS|@@DATEFIRST|@@DATEFIRST|@@DBTS|@@ERROR|@@FETCH_STATUS|@@IDENTITY|@@IDLE|@@IO_BUSY|@@LANGID|@@LANGUAGE|@@LOCK_TIMEOUT|@@MAX_CONNECTIONS|@@MAX_PRECISION|@@NESTLEVEL|@@OPTIONS|@@PACKET_ERRORS|@@PROCID|@@REMSERVER|@@ROWCOUNT|@@SERVERNAME|@@SERVICENAME|@@SPID|@@TEXTSIZE|@@TRANCOUNT|@@VERSION)\b|\b(ABS|ACOS|APP_NAME|ASCII|ASIN|ASSEMBLYPROPERTY|AsymKey_ID|ASYMKEY_ID|asymkeyproperty|ASYMKEYPROPERTY|ATAN|ATN2|AVG|CASE|CAST|CEILING|Cert_ID|Cert_ID|CertProperty|CHAR|CHARINDEX|CHECKSUM_AGG|COALESCE|COL_LENGTH|COL_NAME|COLLATIONPROPERTY|COLLATIONPROPERTY|COLUMNPROPERTY|COLUMNS_UPDATED|COLUMNS_UPDATED|CONTAINSTABLE|CONVERT|COS|COT|COUNT|COUNT_BIG|CRYPT_GEN_RANDOM|CURRENT_TIMESTAMP|CURRENT_TIMESTAMP|CURRENT_USER|CURRENT_USER|CURSOR_STATUS|DATABASE_PRINCIPAL_ID|DATABASE_PRINCIPAL_ID|DATABASEPROPERTY|DATABASEPROPERTYEX|DATALENGTH|DATALENGTH|DATEADD|DATEDIFF|DATENAME|DATEPART|DAY|DB_ID|DB_NAME|DECRYPTBYASYMKEY|DECRYPTBYCERT|DECRYPTBYKEY|DECRYPTBYKEYAUTOASYMKEY|DECRYPTBYKEYAUTOCERT|DECRYPTBYPASSPHRASE|DEGREES|DENSE_RANK|DIFFERENCE|ENCRYPTBYASYMKEY|ENCRYPTBYCERT|ENCRYPTBYKEY|ENCRYPTBYPASSPHRASE|ERROR_LINE|ERROR_MESSAGE|ERROR_NUMBER|ERROR_PROCEDURE|ERROR_SEVERITY|ERROR_STATE|EVENTDATA|EXP|FILE_ID|FILE_IDEX|FILE_NAME|FILEGROUP_ID|FILEGROUP_NAME|FILEGROUPPROPERTY|FILEPROPERTY|FLOOR|fn_helpcollations|fn_listextendedproperty|fn_servershareddrives|fn_virtualfilestats|fn_virtualfilestats|FORMATMESSAGE|FREETEXTTABLE|FULLTEXTCATALOGPROPERTY|FULLTEXTSERVICEPROPERTY|GETANSINULL|GETDATE|GETUTCDATE|GROUPING|HAS_PERMS_BY_NAME|HOST_ID|HOST_NAME|IDENT_CURRENT|IDENT_CURRENT|IDENT_INCR|IDENT_INCR|IDENT_SEED|IDENTITY\(|INDEX_COL|INDEXKEY_PROPERTY|INDEXPROPERTY|IS_MEMBER|IS_OBJECTSIGNED|IS_SRVROLEMEMBER|ISDATE|ISDATE|ISNULL|ISNUMERIC|Key_GUID|Key_GUID|Key_ID|Key_ID|KEY_NAME|KEY_NAME|LEFT|LEN|LOG|LOG10|LOWER|LTRIM|MAX|MIN|MONTH|NCHAR|NEWID|NTILE|NULLIF|OBJECT_DEFINITION|OBJECT_ID|OBJECT_NAME|OBJECT_SCHEMA_NAME|OBJECTPROPERTY|OBJECTPROPERTYEX|OPENDATASOURCE|OPENQUERY|OPENROWSET|OPENXML|ORIGINAL_LOGIN|ORIGINAL_LOGIN|PARSENAME|PATINDEX|PATINDEX|PERMISSIONS|PI|POWER|PUBLISHINGSERVERNAME|PWDCOMPARE|PWDENCRYPT|QUOTENAME|RADIANS|RAND|RANK|REPLICATE|REVERSE|RIGHT|ROUND|ROW_NUMBER|ROWCOUNT_BIG|RTRIM|SCHEMA_ID|SCHEMA_ID|SCHEMA_NAME|SCHEMA_NAME|SCOPE_IDENTITY|SERVERPROPERTY|SESSION_USER|SESSION_USER|SESSIONPROPERTY|SETUSER|SIGN|SignByAsymKey|SignByCert|SIN|SOUNDEX|SPACE|SQL_VARIANT_PROPERTY|SQRT|SQUARE|STATS_DATE|STDEV|STDEVP|STR|STUFF|SUBSTRING|SUM|SUSER_ID|SUSER_NAME|SUSER_SID|SUSER_SNAME|SWITCHOFFSET|SYMKEYPROPERTY|symkeyproperty|sys\.dm_db_index_physical_stats|sys\.fn_builtin_permissions|sys\.fn_my_permissions|SYSDATETIME|SYSDATETIMEOFFSET|SYSTEM_USER|SYSTEM_USER|SYSUTCDATETIME|TAN|TERTIARY_WEIGHTS|TEXTPTR|TODATETIMEOFFSET|TRIGGER_NESTLEVEL|TYPE_ID|TYPE_NAME|TYPEPROPERTY|UNICODE|UPDATE\(|UPPER|USER_ID|USER_NAME|USER_NAME|VAR|VARP|VerifySignedByAsymKey|VerifySignedByCert|XACT_STATE|YEAR)\b", RegexOptions.IgnoreCase | RegexCompiledOption);
            SQLTypesRegex      = new Regex(@"\b(BIGINT|NUMERIC|BIT|SMALLINT|DECIMAL|SMALLMONEY|INT|TINYINT|MONEY|FLOAT|REAL|DATE|DATETIMEOFFSET|DATETIME2|SMALLDATETIME|DATETIME|TIME|CHAR|VARCHAR|TEXT|NCHAR|NVARCHAR|NTEXT|BINARY|VARBINARY|IMAGE|TIMESTAMP|HIERARCHYID|TABLE|UNIQUEIDENTIFIER|SQL_VARIANT|XML)\b", RegexOptions.IgnoreCase | RegexCompiledOption);
        }

        /// <summary>
        /// Highlights SQL code
        /// </summary>
        /// <param name="range"></param>
        public void SQLSyntaxHighlight(Range range) {
            range.tb.CommentPrefix = "--";
            range.tb.LeftBracket   = '(';
            range.tb.RightBracket  = ')';
            range.tb.LeftBracket2  = '\x0';
            range.tb.RightBracket2 = '\x0';

            range.tb.AutoIndentCharsPatterns = @"";
            //clear style of changed range
            range.ClearStyle(CommentStyle, StringStyle, NumberStyle, VariableStyle, StatementsStyle, KeywordStyle, FunctionsStyle, TypesStyle);
            //
            if (SQLStringRegex == null) InitSQLRegex();
            //comment highlighting
            range.SetStyle(CommentStyle, SQLCommentRegex1);
            range.SetStyle(CommentStyle, SQLCommentRegex2);
            range.SetStyle(CommentStyle, SQLCommentRegex3);
            //string highlighting
            range.SetStyle(StringStyle, SQLStringRegex);
            //number highlighting
            range.SetStyle(NumberStyle, SQLNumberRegex);
            //types highlighting
            range.SetStyle(TypesStyle, SQLTypesRegex);
            //var highlighting
            range.SetStyle(VariableStyle, SQLVarRegex);
            //statements
            range.SetStyle(StatementsStyle, SQLStatementsRegex);
            //keywords
            range.SetStyle(KeywordStyle, SQLKeywordsRegex);
            //functions
            range.SetStyle(FunctionsStyle, SQLFunctionsRegex);

            //clear folding markers
            range.ClearFoldingMarkers();
            //set folding markers
            range.SetFoldingMarkers(@"\bBEGIN\b", @"\bEND\b", RegexOptions.IgnoreCase);
            //allow to collapse BEGIN..END blocks
            range.SetFoldingMarkers(@"/\*", @"\*/"); //allow to collapse comment block
        }

        private void InitPHPRegex() {
            PHPStringRegex   = new Regex(@"""(\\""|[^""])*""|'(\\'|[^'])*'|((//|#).*|\/\*[\s\S]*?\*\/)", RegexCompiledOption); // By WendyH
            PHPNumberRegex   = new Regex(@"\b\d+[\.]?\d*\b", RegexCompiledOption);
            PHPVarRegex      = new Regex(@"\$[a-zA-Z_\d]*\b", RegexCompiledOption);
            PHPKeywordRegex1 = new Regex(@"\b(die|echo|empty|exit|eval|include|include_once|isset|list|require|require_once|return|print|unset)\b", RegexCompiledOption);
            PHPKeywordRegex2 = new Regex(@"\b(abstract|and|array|as|break|case|catch|cfunction|class|clone|const|continue|declare|default|do|else|elseif|enddeclare|endfor|endforeach|endif|endswitch|endwhile|extends|final|for|foreach|function|global|goto|if|implements|instanceof|interface|namespace|new|or|private|protected|public|static|switch|throw|try|use|var|while|xor)\b", RegexCompiledOption);
            PHPKeywordRegex3 = new Regex(@"__CLASS__|__DIR__|__FILE__|__LINE__|__FUNCTION__|__METHOD__|__NAMESPACE__", RegexCompiledOption);
        }

        /// <summary>
        /// Highlights PHP code
        /// </summary>
        /// <param name="range"></param>
        public void PHPSyntaxHighlight(Range range) {
            range.tb.CommentPrefix = "//";
            range.tb.LeftBracket   = '(';
            range.tb.RightBracket  = ')';
            range.tb.LeftBracket2  = '{';
            range.tb.RightBracket2 = '}';
            range.tb.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy1;
            //clear style of changed range
            range.ClearStyle(StringStyle, CommentStyle, NumberStyle, VariableStyle, KeywordStyle, KeywordStyle2, KeywordStyle3);

            range.tb.AutoIndentCharsPatterns = @"^\s*\$[\w\.\[\]\'\""]+\s*(?<range>=)\s*(?<range>[^;]+);";

            //
            if (PHPStringRegex == null) InitPHPRegex();
            range.SetStyle(NumberStyle  , PHPNumberRegex  );
            range.SetStyle(VariableStyle, PHPVarRegex     );
            range.SetStyle(KeywordStyle , PHPKeywordRegex1); // 
            range.SetStyle(KeywordStyle2, PHPKeywordRegex2);
            range.SetStyle(KeywordStyle3, PHPKeywordRegex3);
            range.SetStylesStringsAndComments(PHPStringRegex, StringStyle, CommentStyle); // By WendyH

            //clear folding markers
            range.ClearFoldingMarkers();
            //set folding markers
            range.SetFoldingMarkers("{", "}"); //allow to collapse brackets block
            range.SetFoldingMarkers(@"/\*", @"\*/"); //allow to collapse comment block
        }

        private void InitJScriptRegex() {
            JScriptNumberRegex  = new Regex(@"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b", RegexCompiledOption);
            JScriptKeywordRegex = new Regex(@"\b(true|false|break|case|catch|const|continue|default|delete|do|else|export|for|function|if|in|instanceof|new|null|return|switch|this|throw|try|var|void|while|with|typeof)\b", RegexCompiledOption);
        }

        /// <summary>
        /// Highlights JavaScript code
        /// </summary>
        /// <param name="range"></param>
        public void JScriptSyntaxHighlight(Range range) {
            range.tb.CommentPrefix = "//";
            range.tb.LeftBracket   = '(';
            range.tb.RightBracket  = ')';
            range.tb.LeftBracket2  = '{';
            range.tb.RightBracket2 = '}';
            range.tb.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy1;
            range.tb.AutoIndentCharsPatterns = @"^\s*[\w\.]+(\s\w+)?\s*(?<range>=)\s*(?<range>[^;]+);";

            range.ClearStyle(StringStyle, CommentStyle, NumberStyle, KeywordStyle);
            if (JScriptNumberRegex == null) InitJScriptRegex();
            range.SetStyle(NumberStyle , JScriptNumberRegex);
            range.SetStyle(KeywordStyle, JScriptKeywordRegex);
            range.SetStylesStringsAndComments(CPPStringAndCommentsRegex, StringStyle, CommentStyle);
            range.ClearFoldingMarkers();
            range.SetFoldingMarkers("{", "}"); //allow to collapse brackets block
            range.SetFoldingMarkers(@"/\*", @"\*/"); //allow to collapse comment block
        }

        private void InitLuaRegex() {
            LuaStringRegex    = new Regex(@"""""|''|"".*?[^\\]""|'.*?[^\\]'", RegexCompiledOption);
            LuaCommentRegex1  = new Regex(@"--.*$", RegexOptions.Multiline | RegexCompiledOption);
            LuaCommentRegex2  = new Regex(@"(--\[\[.*?\]\])|(--\[\[.*)", RegexOptions.Singleline | RegexCompiledOption);
            LuaCommentRegex3  = new Regex(@"(--\[\[.*?\]\])|(.*\]\])", RegexOptions.Singleline | RegexOptions.RightToLeft | RegexCompiledOption);
            LuaNumberRegex    = new Regex(@"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b", RegexCompiledOption);
            LuaKeywordRegex   = new Regex(@"\b(and|break|do|else|elseif|end|false|for|function|if|in|local|nil|not|or|repeat|return|then|true|until|while)\b", RegexCompiledOption);
            LuaFunctionsRegex = new Regex(@"\b(assert|collectgarbage|dofile|error|getfenv|getmetatable|ipairs|load|loadfile|loadstring|module|next|pairs|pcall|print|rawequal|rawget|rawset|require|select|setfenv|setmetatable|tonumber|tostring|type|unpack|xpcall)\b", RegexCompiledOption);
        }

        /// <summary>
        /// Highlights Lua code
        /// </summary>
        /// <param name="range"></param>
        public void LuaSyntaxHighlight(Range range) {
            range.tb.CommentPrefix = "--";
            range.tb.LeftBracket   = '(';
            range.tb.RightBracket  = ')';
            range.tb.LeftBracket2  = '{';
            range.tb.RightBracket2 = '}';
            range.tb.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy2;

            range.tb.AutoIndentCharsPatterns = @"^\s*[\w\.]+(\s\w+)?\s*(?<range>=)\s*(?<range>.+)";

            //clear style of changed range
            range.ClearStyle(StringStyle, CommentStyle, NumberStyle, KeywordStyle, FunctionsStyle);
            //
            if (LuaStringRegex == null)
                InitLuaRegex();
            //string highlighting
            range.SetStyle(StringStyle, LuaStringRegex);
            //comment highlighting
            range.SetStyle(CommentStyle, LuaCommentRegex1);
            range.SetStyle(CommentStyle, LuaCommentRegex2);
            range.SetStyle(CommentStyle, LuaCommentRegex3);
            //number highlighting
            range.SetStyle(NumberStyle, LuaNumberRegex);
            //keyword highlighting
            range.SetStyle(KeywordStyle, LuaKeywordRegex);
            //functions highlighting
            range.SetStyle(FunctionsStyle, LuaFunctionsRegex);
            //clear folding markers
            range.ClearFoldingMarkers();
            //set folding markers
            range.SetFoldingMarkers("{", "}"); //allow to collapse brackets block
            range.SetFoldingMarkers(@"--\[\[", @"\]\]"); //allow to collapse comment block
        }

        private void LuaAutoIndentNeeded(AutoIndentEventArgs args) {
            //end of block
            if (Regex.IsMatch(args.LineText, @"^\s*(end|until)\b")) {
                args.Shift = -args.TabLength;
                args.ShiftNextLines = -args.TabLength;
                return;
            }
            // then ...
            if (Regex.IsMatch(args.LineText, @"\b(then)\s*\S+"))
                return;
            //start of operator block
            if (Regex.IsMatch(args.LineText, @"^\s*(function|do|for|while|repeat|if)\b")) {
                args.ShiftNextLines = args.TabLength;
                return;
            }

            //Statements else, elseif, case etc
            if (Regex.IsMatch(args.LineText, @"^\s*(else|elseif)\b", RegexOptions.IgnoreCase)) {
                args.Shift = -args.TabLength;
            }
        }

        // By WendyH < ----------------------------------------------------------------------------
        string hmsCommonTypes = "Byte|Word|Integer|Longint|Cardinal|TColor|Boolean|Real|Single|Double|Extended|Currency|TDate|TTime|TDateTime|Char|String|Pointer|Variant|Array";

        static Regex CPPScriptKeywordRegex, CPPScriptTypesRegex, CPPClassNameRegex;
        static Regex regexDeclFunctionCPP = new Regex(@"^(?:\w+)\s+(?<range>\w+)\s*?\("               , RegexCompiledOption | RegexOptions.Multiline);
        static Regex regexDeclFunctionPAS = new Regex(@"^\s*?(?:Function|Procedure)\s+(?<range>\w+)\b", RegexCompiledOption | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex regexDeclFunctionBAS = new Regex(@"^\s*?(?:Function|SUB)\s+(?<range>\w+)\b"      , RegexCompiledOption | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex regexPunctuationCPP  = new Regex(@"[\+-/<>\*\&\^\%\!:=]", RegexCompiledOption);
        static Regex regexPunctuationPAS  = new Regex(@"[\+-/<>\*\&\^\%\!]"  , RegexCompiledOption);
        Regex PascalScriptStringRegex, PascalScriptNumberRegex, PascalScriptKeywordRegex1, PascalScriptKeywordRegex2, PascalScriptClassNameRegex;
        Regex BasicScriptKeywordRegex1, BasicScriptKeywordRegex2;
        Regex HmsJScriptKeywordRegex;
        Regex YAMLStringRegex, YAMLNumberRegex, YAMLKeywordRegex, YAMLObjectNameRegex, YAMLBreaketsRegex;

        void InitCPPScriptRegex() {
            if (HMS.ClassesString.Length > 2)
                HmsClasses = HMS.ClassesString.Substring(1, HMS.ClassesString.Length - 2);
            CPPScriptKeywordRegex = new Regex(@"(\b(new|break|continue|exit|delete|return|if|else|switch|default|case|do|while|for|try|finally|except|in|is)|^\s*?#include|^\s*?#define)\b", RegexCompiledOption | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            CPPScriptTypesRegex   = new Regex(@"\b(byte|word|longint|Cardinal|TColor|Real|Single|Double|Extended|Currency|TDate|TTime|TDateTime|Char|String|Pointer|Variant|Array|bool|float|int|long|void)\b", RegexCompiledOption | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            CPPClassNameRegex     = new Regex(@"\b(" + HmsClasses + @")\b", RegexCompiledOption | RegexOptions.IgnoreCase);
        }

        public void InitPascalScriptRegex() {
            if (HMS.ClassesString.Length > 2)
                HmsClasses = HMS.ClassesString.Substring(1, HMS.ClassesString.Length - 2);
            PascalScriptStringRegex = new Regex(@"""([^""\r])*""?|'([^'\r])*'?|//.*|(?<mc>\{[\s\S]*?((?<mcend>\})|$))|(?<mcend2>\})", RegexCompiledOption);
            PascalScriptNumberRegex = new Regex(@"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b", RegexCompiledOption);
            string keywords = "PROGRAM|USES|CONST|VAR|ARRAY|NOT|IN|IS|OR|XOR|DIV|MOD|AND|SHL|SHR|BREAK|CONTINUE|EXIT|BEGIN|END|IF|THEN|ELSE|CASE|OF|REPEAT|UNTIL|WHILE|DO|FOR|TO|DOWNTO|TRY|FINALLY|EXCEPT|WITH|FUNCTION|PROCEDURE";
            PascalScriptKeywordRegex1  = new Regex(@"\b(" + hmsCommonTypes + @")\b", RegexCompiledOption | RegexOptions.IgnoreCase);
            PascalScriptKeywordRegex2  = new Regex(@"\b(" + keywords   + @")\b"    , RegexCompiledOption | RegexOptions.IgnoreCase);
            PascalScriptClassNameRegex = new Regex(@"\b(" + HmsClasses + @")\b"    , RegexCompiledOption | RegexOptions.IgnoreCase);
        }

        void InitBasicScriptRegex() {
            string keywords = "EOL|IMPORTS|DIM|AS|NOT|IN|IS|OR|XOR|MOD|AND|ADDRESSOF|BREAK|CONTINUE|EXIT|DELETE|SET|RETURN|IF|THEN|END|ELSEIF|ELSE|SELECT|CASE|DO|LOOP|UNTIL|WHILE|WEND|FOR|TO|STEP|NEXT|TRY|FINALLY|CATCH|WITH|SUB|FUNCTION|BYREF|BYVAL";
            BasicScriptKeywordRegex1 = new Regex(@"\b(" + hmsCommonTypes + @")\b", RegexCompiledOption | RegexOptions.IgnoreCase);
            BasicScriptKeywordRegex2 = new Regex(@"\b(" + keywords       + @")\b", RegexCompiledOption | RegexOptions.IgnoreCase);
        }

        void InitHmsJScriptRegex() {
            string keywords = @"import|var|new|in|is|break|continue|exit|delete|return|if|else|switch|default|case|do|while|for|try|finally|except|function|with|" + hmsCommonTypes;
            HmsJScriptKeywordRegex = new Regex(@"\b(" + keywords + @")\b", RegexCompiledOption | RegexOptions.IgnoreCase);
        }

        void InitYAMLRegex() {
            YAMLStringRegex     = new Regex(@"""(\\""|[^""\r])*""|'(\\'|[^'\r])*'|#.*|(?<mc>\/\*[\s\S]*?((?<mcend>\*\/)|$))|(?<mcend2>\*\/)", RegexCompiledOption);
            YAMLNumberRegex     = new Regex(@"[^%]\b(?<range>\d+[\.]?\d*([eE]\-?\d+)?)\b", RegexCompiledOption);
            YAMLKeywordRegex    = new Regex(@"\b(true|false|null)\b"       , RegexCompiledOption);
            YAMLObjectNameRegex = new Regex(@"(?:^|{|,)[\s-]*?(?<range>[\w-]+)\s*?:", RegexCompiledOption | RegexOptions.Multiline);
            YAMLBreaketsRegex   = new Regex(@"[{},]", RegexCompiledOption);
        }

        /// <summary>
        /// Highlights YAML code
        /// </summary>
        /// <param name="range"></param>
        public void YAMLSyntaxHighlight(Range range) {
            if ((range.Size > AbsoluteMaxRangeLength) || (range.ToX > MaxLineLength)) return;
            range.tb.CommentPrefix = "#";
            range.tb.LeftBracket   = '(';
            range.tb.RightBracket  = ')';
            range.tb.LeftBracket2  = '{';
            range.tb.RightBracket2 = '}';
            range.ClearStyle(StringStyle, CommentStyle, NumberStyle, KeywordStyle, FunctionsStyle, TagBracketStyle, VariableStyle, ConstantsStyle);
            if (YAMLStringRegex == null) InitYAMLRegex();
            range.SetStylesStringsAndComments(YAMLStringRegex, StringStyle, CommentStyle);

            range.SetStyle(NumberStyle    , YAMLNumberRegex      );
            range.SetStyle(TagBracketStyle, YAMLBreaketsRegex    );
            range.SetStyle(KeywordStyle   , YAMLKeywordRegex     );
            range.SetStyle(FunctionsStyle , YAMLObjectNameRegex  );
            range.SetStyle(ConstantsStyle , HMS.RegexHmsConstants);

            //range.ClearFoldingMarkers();
            //range.SetFoldingMarkers(@"^[\s-]*?[\w-]+\s*?:", "[IDENT]", RegexOptions.Multiline); // Allow to collapse block
        }

        /// <summary>
        /// Highlights PascalScript code
        /// </summary>
        /// <param name="range"></param>
        public void PascalScriptSyntaxHighlight(Range range) {
            if ((range.Size > AbsoluteMaxRangeLength) || (range.ToX > MaxLineLength)) return;
            range.tb.CommentPrefix = "//";
            range.tb.LeftBracket   = '(';
            range.tb.RightBracket  = ')';
            range.tb.LeftBracket2  = '[';
            range.tb.RightBracket2 = ']';
            range.tb.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy1;
            range.tb.AutoIndentCharsPatterns   = @"^\s*[\w\.]+(\s\w+)?\s*(?<range>=)\s*(?<range>[^;]+);^\s*(case|default)\s*[^:]*(?<range>:)\s*(?<range>[^;]+);";
            if (PascalScriptStringRegex == null) InitPascalScriptRegex();
            bool bigText = range.Size > MaxLength4FastWork;

            range.ClearStyleAndFuncBegin(StringStyle, CommentStyle, NumberStyle, DeclFunctionStyle, ClassNameStyle, KeywordStyle, FunctionsStyle, VariableStyle, ConstantsStyle, TypesStyle, PunctuationSyle);
            range.SetStylesStringsAndComments(PascalScriptStringRegex, StringStyle, CommentStyle);
            range.SetStyle(NumberStyle      , PascalScriptNumberRegex);
            range.SetFunct(DeclFunctionStyle, regexDeclFunctionPAS   );

            range.ClearFoldingMarkers();
            range.SetFoldingMarkers(@"\b(repeat)\b"   , @"\b(until)\b", RegexCompiledOption | RegexOptions.IgnoreCase); //allow to collapse brackets block
            range.SetFoldingMarkers(@"\b(begin|try)\b", @"\b(end)\b"  , RegexCompiledOption | RegexOptions.IgnoreCase); //allow to collapse brackets block

            if (bigText && !Worker4BigText.IsBusy) {
                Worker4BigText.RunWorkerAsync(new Syntax2StepArgs(currentLanguage, range));
            } else {
                PascalScriptSyntaxHighlight2(range);
            }
        }
        public void PascalScriptSyntaxHighlight2(Range range) {
            range.SetStyle(ClassNameStyle , PascalScriptClassNameRegex);
            range.SetStyle(TypesStyle     , PascalScriptKeywordRegex1 );
            range.SetStyle(KeywordStyle   , PascalScriptKeywordRegex2 );
            range.SetStyle(PunctuationSyle, regexPunctuationPAS       );
            range.SetStyle(FunctionsStyle, HMS.RegexHmsFunctions);
            range.SetStyle(VariableStyle , HMS.RegexHmsVariables);
            range.SetStyle(ConstantsStyle, HMS.RegexHmsConstants);
        }

        /// <summary>
        /// Highlights C++Script code
        /// </summary>
        /// <param name="range"></param>
        public void CPPScriptSyntaxHighlight(Range range) {
            if ((range.Size > AbsoluteMaxRangeLength) || (range.ToX > MaxLineLength)) return;
            range.tb.CommentPrefix = "//";
            range.tb.LeftBracket   = '(';
            range.tb.RightBracket  = ')';
            range.tb.LeftBracket2  = '{';
            range.tb.RightBracket2 = '}';
            range.tb.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy1;
            range.tb.AutoIndentCharsPatterns   = @"^\s*[\w\.]+(\s\w+)?\s*(?<range>=)\s*(?<range>[^;]+);^\s*(case|default)\s*[^:]*(?<range>:)\s*(?<range>[^;]+);";
            if (CPPScriptKeywordRegex == null) InitCPPScriptRegex();
            bool bigText = range.Size > MaxLength4FastWork;

            range.ClearStyleAndFuncBegin(StringStyle, CommentStyle, NumberStyle, DeclFunctionStyle, ClassNameStyle, KeywordStyle, FunctionsStyle, VariableStyle, ConstantsStyle, PunctuationSyle, TypesStyle);
            range.SetStylesStringsAndComments(CPPStringAndCommentsRegex, StringStyle, CommentStyle);
            range.SetStyle(NumberStyle      , CSharpNumberRegex    );
            range.SetFunct(DeclFunctionStyle, regexDeclFunctionCPP );

            range.ClearFoldingMarkers();
            range.SetFoldingMarkers("{", "}"); //allow to collapse brackets block

            if (bigText && !Worker4BigText.IsBusy) {
                Worker4BigText.RunWorkerAsync(new Syntax2StepArgs(currentLanguage, range));
            } else {
                CPPScriptSyntaxHighlight2(range);
            }
        }

        private void Worker4BigText_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Error != null || e.Cancelled) return;
            Syntax2StepArgs args = e.Result as Syntax2StepArgs;
            if (args != null) {
                args.Range.tb.Invalidate();
            }
        }

        private void Worker4BigText_DoWork(object sender, DoWorkEventArgs e) {
            Syntax2StepArgs args = e.Argument as Syntax2StepArgs;
            if (args != null) {
                HighlightSyntax2Step(args.Language, args.Range);
            }
            e.Result = args;
        }

        BackgroundWorker Worker4BigText;

        public void CPPScriptSyntaxHighlight2(Range range) {
            range.SetStyle(ClassNameStyle , CPPClassNameRegex    );
            range.SetStyle(KeywordStyle   , CPPScriptKeywordRegex);
            range.SetStyle(TypesStyle     , CPPScriptTypesRegex  );
            range.SetStyle(PunctuationSyle, regexPunctuationCPP  );
            range.SetStyle(FunctionsStyle, HMS.RegexHmsFunctions);
            range.SetStyle(VariableStyle , HMS.RegexHmsVariables);
            range.SetStyle(ConstantsStyle, HMS.RegexHmsConstants);
        }

        private Language currentLanguage;

        public void HighlightSyntax2Step(Language language, Range range) {
            switch (language) {
                case Language.CPPScript   : CPPScriptSyntaxHighlight2   (range); break;
                case Language.PascalScript: PascalScriptSyntaxHighlight2(range); break;
                case Language.BasicScript : BasicSyntaxHighlight2       (range); break;
                case Language.JScript     : HmsJScriptSyntaxHighlight2  (range); break;
            }
        }
        // By WendyH > -------------------------------------------------------


        /// <summary>
        /// Highlights JScript code
        /// </summary>
        /// <param name="range"></param>
        public void HmsJScriptSyntaxHighlight(Range range) {
            if ((range.Size > AbsoluteMaxRangeLength) || (range.ToX > MaxLineLength)) return;
            range.tb.CommentPrefix = "//";
            range.tb.LeftBracket   = '(';
            range.tb.RightBracket  = ')';
            range.tb.LeftBracket2  = '{';
            range.tb.RightBracket2 = '}';
            range.tb.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy1;
            range.tb.AutoIndentCharsPatterns   = @"^\s*[\w\.]+(\s\w+)?\s*(?<range>=)\s*(?<range>[^;]+);";
            if (JScriptNumberRegex     == null) InitJScriptRegex();
            if (HmsJScriptKeywordRegex == null) InitHmsJScriptRegex();
            if (CPPClassNameRegex      == null) InitCPPScriptRegex();
            bool bigText = range.Size > MaxLength4FastWork;

            range.ClearStyleAndFuncBegin(StringStyle, CommentStyle, NumberStyle, DeclFunctionStyle, FunctionsStyle, VariableStyle, ConstantsStyle, PunctuationSyle, KeywordStyle, ClassNameStyle);
            range.SetStylesStringsAndComments(CPPStringAndCommentsRegex, StringStyle, CommentStyle);
            range.SetStyle(NumberStyle      , JScriptNumberRegex  );
            range.SetFunct(DeclFunctionStyle, regexDeclFunctionCPP);

            range.ClearFoldingMarkers();
            range.SetFoldingMarkers("{", "}"); //allow to collapse brackets block

            if (bigText && !Worker4BigText.IsBusy) {
                Worker4BigText.RunWorkerAsync(new Syntax2StepArgs(currentLanguage, range));
            } else {
                HmsJScriptSyntaxHighlight2(range);
            }
        }
        public void HmsJScriptSyntaxHighlight2(Range range) {
            range.SetStyle(ClassNameStyle , CPPClassNameRegex     );
            range.SetStyle(KeywordStyle   , HmsJScriptKeywordRegex);
            range.SetStyle(FunctionsStyle , HMS.RegexHmsFunctions );
            range.SetStyle(VariableStyle  , HMS.RegexHmsVariables );
            range.SetStyle(ConstantsStyle , HMS.RegexHmsConstants );
            range.SetStyle(PunctuationSyle, regexPunctuationCPP   );
        }

        /// <summary>
        /// Highlights BasicScript code
        /// </summary>
        /// <param name="range"></param>
        public void BasicSyntaxHighlight(Range range) {
            if ((range.Size > AbsoluteMaxRangeLength) || (range.ToX > MaxLineLength)) return;
            range.tb.CommentPrefix = "'";
            range.tb.LeftBracket   = '(';
            range.tb.RightBracket  = ')';
            range.tb.LeftBracket2  = '[';
            range.tb.RightBracket2 = ']';
            range.tb.AutoIndentCharsPatterns = @"^\s*[\w\.\(\)]+\s*(?<range>=)\s*(?<range>.+)";
            if (VBStringRegex            == null) InitVBRegex();
            if (BasicScriptKeywordRegex1 == null) InitBasicScriptRegex();
            if (CPPClassNameRegex        == null) InitCPPScriptRegex();
            bool bigText = range.Size > MaxLength4FastWork;

            range.ClearStyleAndFuncBegin(StringStyle, CommentStyle, NumberStyle, DeclFunctionStyle, ClassNameStyle, KeywordStyle, FunctionsStyle, VariableStyle, ConstantsStyle, PunctuationSyle, TypesStyle);
            range.SetStylesStringsAndComments(VBStringRegex, StringStyle, CommentStyle, false);
            range.SetStyle(NumberStyle      , VBNumberRegex);
            range.SetFunct(DeclFunctionStyle, regexDeclFunctionBAS);

            range.ClearFoldingMarkers();
            range.SetFoldingMarkers(@"#Region\b"                                           , @"#End\s+Region\b"                                  , RegexOptions.IgnoreCase);
            range.SetFoldingMarkers(@"\b(Class|Property|Enum|Structure|Interface)[ \t]+\S+", @"\bEnd (Class|Property|Enum|Structure|Interface)\b", RegexOptions.IgnoreCase);
            range.SetFoldingMarkers(@"^\s*(?<range>While)[ \t]+\S+"                        , @"^\s*(?<range>End While)\b"                        , RegexOptions.IgnoreCase | RegexOptions.Multiline);
            //this declared separately because Sub and Function can be unclosed
            range.SetFoldingMarkers(@"\b(Sub|Function)[ \t]+[^\s']+"                       , @"\bEnd (Sub|Function)\b"                           , RegexOptions.IgnoreCase);
            range.SetFoldingMarkers(@"\bIF[ \t][^\n]+THEN\s*?$"                            , @"\bEND IF\b"                                       , RegexOptions.IgnoreCase | RegexOptions.Multiline);
            range.SetFoldingMarkers(@"(\r|\n|^)[ \t]*(?<range>Get|Set)[ \t]*(\r|\n|$)"     , @"\bEnd (Get|Set)\b"                                , RegexOptions.IgnoreCase);
            range.SetFoldingMarkers(@"^\s*(?<range>For|For\s+Each)\b"                      , @"^\s*(?<range>Next)\b"                             , RegexOptions.IgnoreCase | RegexOptions.Multiline);
            range.SetFoldingMarkers(@"^\s*(?<range>Do)\b"                                  , @"^\s*(?<range>Loop)\b"                             , RegexOptions.IgnoreCase | RegexOptions.Multiline);

            if (bigText && !Worker4BigText.IsBusy) {
                Worker4BigText.RunWorkerAsync(new Syntax2StepArgs(currentLanguage, range));
            } else {
                BasicSyntaxHighlight2(range);
            }
        }
        public void BasicSyntaxHighlight2(Range range) {
            range.SetStyle(ClassNameStyle , CPPClassNameRegex    );
            range.SetStyle(TypesStyle     , BasicScriptKeywordRegex1);
            range.SetStyle(KeywordStyle   , BasicScriptKeywordRegex2);
            range.SetStyle(FunctionsStyle , HMS.RegexHmsFunctions);
            range.SetStyle(VariableStyle  , HMS.RegexHmsVariables);
            range.SetStyle(ConstantsStyle , HMS.RegexHmsConstants);
            range.SetStyle(PunctuationSyle, regexPunctuationCPP  );
        }

        public static Language DetectLang(string txt) {
            string firstChar = txt.Substring(0, 1);
            if ((firstChar == "{") || (firstChar == "["))
                return Language.YAML;
            else
            {
                string firstText = txt.Length > 500 ? txt.Substring(0, 500) : txt;
                Language lang = Language.HTML;
                Dictionary<Language, string> langPatterns = new Dictionary<Language, string> {
                    [Language.PHP] = @"^<\?php",
                    [Language.XML] = @"^<",
                    [Language.PascalScript] = @"\b(\w+\s*?:=\s*?[\d'""\w]|ifthen|end;|end\.)",
                    [Language.JS] = @"function\s+\w+\s*?\(",
                    [Language.BasicScript] = @"\b(End\sSub|Dim\s\w+)",
                    [Language.CPPScript] = @"\b(\w+\s+\w+\s*?[,;=]|(void|int)\s+\w+\s*?\()"
                };
                // JSON
                foreach (var item in langPatterns) {
                    string pattern = item.Value;
                    if (Regex.IsMatch(firstText, pattern, RegexOptions.IgnoreCase)) {
                        return item.Key;
                    }
                }
                return lang;
            }

        }

        // By WendyH > ----------------------------------------------------------------------------

        #region Styles

        /// <summary>
        /// String style
        /// </summary>
        public Style StringStyle { get; set; }

        /// <summary>
        /// Comment style
        /// </summary>
        public Style CommentStyle { get; set; }

        /// <summary>
        /// Number style
        /// </summary>
        public Style NumberStyle { get; set; }

        /// <summary>
        /// C# attribute style
        /// </summary>
        public Style AttributeStyle { get; set; }

        /// <summary>
        /// Class name style
        /// </summary>
        public Style ClassNameStyle { get; set; }

        /// <summary>
        /// Keyword style
        /// </summary>
        public Style KeywordStyle { get; set; }

        /// <summary>
        /// Style of tags in comments of C#
        /// </summary>
        public Style CommentTagStyle { get; set; }

        /// <summary>
        /// HTML attribute value style
        /// </summary>
        public Style AttributeValueStyle { get; set; }

        /// <summary>
        /// HTML tag brackets style
        /// </summary>
        public Style TagBracketStyle { get; set; }

        /// <summary>
        /// HTML tag name style
        /// </summary>
        public Style TagNameStyle { get; set; }

        /// <summary>
        /// HTML Entity style
        /// </summary>
        public Style HtmlEntityStyle { get; set; }

        /// <summary>
        /// XML attribute style
        /// </summary>
        public Style XmlAttributeStyle { get; set; }

        /// <summary>
        /// XML attribute value style
        /// </summary>
        public Style XmlAttributeValueStyle { get; set; }

        /// <summary>
        /// XML tag brackets style
        /// </summary>
        public Style XmlTagBracketStyle { get; set; }

        /// <summary>
        /// XML tag name style
        /// </summary>
        public Style XmlTagNameStyle { get; set; }

        /// <summary>
        /// XML Entity style
        /// </summary>
        public Style XmlEntityStyle { get; set; }

        /// <summary>
        /// XML CData style
        /// </summary>
        public Style XmlCDataStyle { get; set; }

        /// <summary>
        /// Variable style
        /// </summary>
        public Style VariableStyle { get; set; }

        /// <summary>
        /// Specific PHP keyword style
        /// </summary>
        public Style KeywordStyle2 { get; set; }

        /// <summary>
        /// Specific PHP keyword style
        /// </summary>
        public Style KeywordStyle3 { get; set; }

        /// <summary>
        /// SQL Statements style
        /// </summary>
        public Style StatementsStyle { get; set; }

        /// <summary>
        /// SQL Functions style
        /// </summary>
        public Style FunctionsStyle { get; set; }

        /// <summary>
        /// SQL Types style
        /// </summary>
        public Style TypesStyle { get; set; }

        // < By WendyH ------------------------------
        public Style ConstantsStyle    { get; set; }
        public Style DeclFunctionStyle { get; set; }
        public Style PunctuationSyle   { get; set; }

        public static string Lang2Str(Language l) {
            if (l == Language.CPPScript) return "C++Script";
            else if (l == Language.JS) return "JavaScript";
            else if (l == Language.VB) return "VisualBasic";
            else if (l == Language.CSharp) return "C#";
            else if (l == Language.Custom) return "";
            else return l.ToString();
        }

        public static Language Str2Lang(string name) {
            Language l;
            switch (name) {
                case "C#"          : l = Language.CSharp      ; break;
                case "VisualBasic" : l = Language.VB          ; break;
                case "HTML"        : l = Language.HTML        ; break;
                case "XML"         : l = Language.XML         ; break;
                case "SQL"         : l = Language.SQL         ; break;
                case "PHP"         : l = Language.PHP         ; break;
                case "JavaScript"  : l = Language.JS          ; break;
                case "Lua"         : l = Language.Lua         ; break;
                case "YAML"        : l = Language.YAML        ; break;
                case "C++Script"   : l = Language.CPPScript   ; break;
                case "PascalScript": l = Language.PascalScript; break;
                case "BasicScript" : l = Language.BasicScript ; break;
                case "JScript"     : l = Language.JScript     ; break;
                default            : l = Language.YAML        ; break;
            }
            return l;
        }
        // > By WendyH ------------------------------
        #endregion
    }

    public class Syntax2StepArgs {
        public Language Language;
        public Range    Range;
        public Syntax2StepArgs(Language lang, Range range) {
            Language = lang;
            Range    = range;
        }
    }

    /// <summary>
    /// Language
    /// </summary>
    public enum Language {
        Custom,
        CSharp,
        VB,
        HTML,
        XML,
        SQL,
        PHP,
        JS,
        Lua,
        YAML,
        CPPScript,
        PascalScript,
        BasicScript,
        JScript
    }

}