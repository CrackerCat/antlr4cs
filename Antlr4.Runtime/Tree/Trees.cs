/*
 * [The "BSD license"]
 *  Copyright (c) 2013 Terence Parr
 *  Copyright (c) 2013 Sam Harwell
 *  All rights reserved.
 *
 *  Redistribution and use in source and binary forms, with or without
 *  modification, are permitted provided that the following conditions
 *  are met:
 *
 *  1. Redistributions of source code must retain the above copyright
 *     notice, this list of conditions and the following disclaimer.
 *  2. Redistributions in binary form must reproduce the above copyright
 *     notice, this list of conditions and the following disclaimer in the
 *     documentation and/or other materials provided with the distribution.
 *  3. The name of the author may not be used to endorse or promote products
 *     derived from this software without specific prior written permission.
 *
 *  THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 *  IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 *  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 *  IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 *  INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 *  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 *  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 *  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 *  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 *  THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
using System.Collections.Generic;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime.Tree.Gui;
using Sharpen;

namespace Antlr4.Runtime.Tree
{
	/// <summary>A set of utility routines useful for all kinds of ANTLR trees.</summary>
	/// <remarks>A set of utility routines useful for all kinds of ANTLR trees.</remarks>
	public class Trees
	{
		public static string GetPS(ITree t, IList<string> ruleNames, string fontName, int
			 fontSize)
		{
			TreePostScriptGenerator psgen = new TreePostScriptGenerator(ruleNames, t, fontName
				, fontSize);
			return psgen.GetPS();
		}

		public static string GetPS(ITree t, IList<string> ruleNames)
		{
			return GetPS(t, ruleNames, "Helvetica", 11);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public static void WritePS(ITree t, IList<string> ruleNames, string fileName, string
			 fontName, int fontSize)
		{
			string ps = GetPS(t, ruleNames, fontName, fontSize);
			FileWriter f = new FileWriter(fileName);
			BufferedWriter bw = new BufferedWriter(f);
			bw.Write(ps);
			bw.Close();
		}

		/// <exception cref="System.IO.IOException"></exception>
		public static void WritePS(ITree t, IList<string> ruleNames, string fileName)
		{
			WritePS(t, ruleNames, fileName, "Helvetica", 11);
		}

		/// <summary>Print out a whole tree in LISP form.</summary>
		/// <remarks>
		/// Print out a whole tree in LISP form.
		/// <see cref="GetNodeText(ITree, Antlr4.Runtime.Parser)">GetNodeText(ITree, Antlr4.Runtime.Parser)
		/// 	</see>
		/// is used on the
		/// node payloads to get the text for the nodes.  Detect
		/// parse trees and extract data appropriately.
		/// </remarks>
		public static string ToStringTree(ITree t)
		{
			return ToStringTree(t, (IList<string>)null);
		}

		/// <summary>Print out a whole tree in LISP form.</summary>
		/// <remarks>
		/// Print out a whole tree in LISP form.
		/// <see cref="GetNodeText(ITree, Antlr4.Runtime.Parser)">GetNodeText(ITree, Antlr4.Runtime.Parser)
		/// 	</see>
		/// is used on the
		/// node payloads to get the text for the nodes.  Detect
		/// parse trees and extract data appropriately.
		/// </remarks>
		public static string ToStringTree(ITree t, Parser recog)
		{
			string[] ruleNames = recog != null ? recog.GetRuleNames() : null;
			IList<string> ruleNamesList = ruleNames != null ? Arrays.AsList(ruleNames) : null;
			return ToStringTree(t, ruleNamesList);
		}

		/// <summary>Print out a whole tree in LISP form.</summary>
		/// <remarks>
		/// Print out a whole tree in LISP form.
		/// <see cref="GetNodeText(ITree, Antlr4.Runtime.Parser)">GetNodeText(ITree, Antlr4.Runtime.Parser)
		/// 	</see>
		/// is used on the
		/// node payloads to get the text for the nodes.  Detect
		/// parse trees and extract data appropriately.
		/// </remarks>
		public static string ToStringTree(ITree t, IList<string> ruleNames)
		{
			string s = Utils.EscapeWhitespace(GetNodeText(t, ruleNames), false);
			if (t.GetChildCount() == 0)
			{
				return s;
			}
			StringBuilder buf = new StringBuilder();
			buf.Append("(");
			s = Utils.EscapeWhitespace(GetNodeText(t, ruleNames), false);
			buf.Append(s);
			buf.Append(' ');
			for (int i = 0; i < t.GetChildCount(); i++)
			{
				if (i > 0)
				{
					buf.Append(' ');
				}
				buf.Append(ToStringTree(t.GetChild(i), ruleNames));
			}
			buf.Append(")");
			return buf.ToString();
		}

		public static string GetNodeText(ITree t, Parser recog)
		{
			string[] ruleNames = recog != null ? recog.GetRuleNames() : null;
			IList<string> ruleNamesList = ruleNames != null ? Arrays.AsList(ruleNames) : null;
			return GetNodeText(t, ruleNamesList);
		}

		public static string GetNodeText(ITree t, IList<string> ruleNames)
		{
			if (ruleNames != null)
			{
				if (t is IRuleNode)
				{
					int ruleIndex = ((IRuleNode)t).GetRuleContext().GetRuleIndex();
					string ruleName = ruleNames[ruleIndex];
					return ruleName;
				}
				else
				{
					if (t is IErrorNode)
					{
						return t.ToString();
					}
					else
					{
						if (t is ITerminalNode)
						{
							object symbol = ((ITerminalNode)t).GetSymbol();
							if (symbol is IToken)
							{
								string s = ((IToken)symbol).GetText();
								return s;
							}
						}
					}
				}
			}
			// no recog for rule names
			object payload = t.GetPayload();
			if (payload is IToken)
			{
				return ((IToken)payload).GetText();
			}
			return t.GetPayload().ToString();
		}

		/// <summary>Return a list of all ancestors of this node.</summary>
		/// <remarks>
		/// Return a list of all ancestors of this node.  The first node of
		/// list is the root and the last is the parent of this node.
		/// </remarks>
		[NotNull]
		public static IList<ITree> GetAncestors(ITree t)
		{
			if (t.GetParent() == null)
			{
				return Sharpen.Collections.EmptyList();
			}
			IList<ITree> ancestors = new List<ITree>();
			t = t.GetParent();
			while (t != null)
			{
				ancestors.Add(0, t);
				// insert at start
				t = t.GetParent();
			}
			return ancestors;
		}

		public Trees()
		{
		}
	}
}