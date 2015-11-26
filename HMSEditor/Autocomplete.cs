/* This code is released under WTFPL Version 2 (http://www.wtfpl.net/) * Created by WendyH. Copyleft. */
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using FastColoredTextBoxNS;

namespace HMSEditorNS {
	class Autocomplete {
		/// <summary>
		/// This item appears when any part of snippet text is typed
		/// </summary>
		public class DeclarationSnippet : SnippetAutocompleteItem {
			public static string RegexSpecSymbolsPattern = @"[\^\$\[\]\(\)\.\\\*\+\|\?\{\}]";

			public DeclarationSnippet(string snippet)
				: base(snippet) {
			}

			public override CompareResult Compare(string fragmentText) {
				var pattern = Regex.Replace(fragmentText, RegexSpecSymbolsPattern, "\\$0");
				if (Regex.IsMatch(Text, "\\b" + pattern, RegexOptions.IgnoreCase))
					return CompareResult.Visible;
				return CompareResult.Hidden;
			}
		}

		/// <summary>
		/// Divides numbers and words: "123AND456" -> "123 AND 456"
		/// Or "i=2" -> "i = 2"
		/// </summary>
		public class InsertSpaceSnippet : AutocompleteItem {
			string pattern;

			public InsertSpaceSnippet(string pattern)
				: base("") {
				this.pattern = pattern;
			}

			public InsertSpaceSnippet()
				: this(@"^(\d+)([a-zA-Z_]+)(\d*)$") {
			}

			public override CompareResult Compare(string fragmentText) {
				if (Regex.IsMatch(fragmentText, pattern)) {
					Text = InsertSpaces(fragmentText);
					if (Text != fragmentText)
						return CompareResult.Visible;
				}
				return CompareResult.Hidden;
			}

			public string InsertSpaces(string fragment) {
				var m = Regex.Match(fragment, pattern);
				if (m.Groups[1].Value == "" && m.Groups[3].Value == "")
					return fragment;
				return (m.Groups[1].Value + " " + m.Groups[2].Value + " " + m.Groups[3].Value).Trim();
			}

			public override string ToolTipTitle {
				get {
					return Text;
				}
			}
		}

	}
}
