#region License
/* **********************************************************************************
 * Copyright (c) Roman Ivantsov
 * This source code is subject to terms and conditions of the MIT License
 * for CLIrony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using sones.Lib.Frameworks.CLIrony.Compiler;

namespace sones.Lib.Frameworks.CLIrony {
  using Compiler.Lalr;

  public class EscapeTable : Dictionary<char, char> { }
  public static class TextUtils {
    public const string AllLatinLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    public const string DecimalDigits = "1234567890";
    public const string OctalDigits = "12345670";
    public const string HexDigits = "1234567890aAbBcCdDeEfF";
    public const string BinaryDigits = "01";

    public static EscapeTable GetDefaultEscapes() {
      EscapeTable escapes = new EscapeTable();
      escapes.Add('a', '\u0007');
      escapes.Add('b', '\b');
      escapes.Add('t', '\t');
      escapes.Add('n', '\n');
      escapes.Add('v', '\v');
      escapes.Add('f', '\f');
      escapes.Add('r', '\r');
      escapes.Add('"', '"');
      escapes.Add('\'', '\'');
      escapes.Add('\\', '\\');
      escapes.Add(' ', ' ');
      escapes.Add('\n', '\n'); //this is a special escape of the linebreak itself, 
      // when string ends with "\" char and continues on the next line
      return escapes;
    }

    public static string TerminalsToText(TerminalList terminals) {
      StringBuilder sb = new StringBuilder();
      foreach (Terminal term in terminals) {
        sb.Append(term.ToString());
        sb.AppendLine();
      }
      return sb.ToString();
    }

    public static string NonTerminalsToText(NonTerminalList nonTerminals) {
      StringBuilder sb = new StringBuilder();
      foreach (NonTerminal nt in nonTerminals) {
        sb.Append(nt.Name);
        sb.Append(nt.Nullable ? "  (Nullable) " : "");
        sb.AppendLine();
        foreach (Production pr in nt.Productions) {
          sb.Append("   ");
          sb.AppendLine(pr.ToString());
        }
        sb.Append("  FIRSTS: ");
        string firsts = TextUtils.Cleanup(nt.Firsts.ToString(" "));
        sb.AppendLine(firsts);
        sb.AppendLine();
      }//foreachc nt
      return sb.ToString();
    }
    public static string StateListToText(ParserStateList states) {
      StringBuilder sb = new StringBuilder();
      foreach (ParserState state in states) {
        sb.Append("State ");
        sb.AppendLine(state.Name);
        //LRItems
        foreach (LRItem item in state.Items) {
          sb.Append("    ");
          sb.AppendLine(item.ToString());
        }
        //Transitions
        sb.Append("      TRANSITIONS: ");
        foreach (String key in state.Actions.Keys) {
          ActionRecord action = state.Actions[key];
          if (action.NewState == null) continue; //may be null in malformed grammars
          string displayKey = key.EndsWith("\b") ? key.Substring(0, key.Length - 1) : key;
          sb.Append(displayKey);
          sb.Append("->");
          sb.Append(action.NewState.Name);
          sb.Append("; ");
        }
        sb.AppendLine();
        sb.AppendLine();
      }//foreach
      return sb.ToString();
    }

    //Utility method used by Production and LR0Item
    public static string ProductionToString(Production production, int dotPosition) {
      char dotChar = '\u00B7'; //dot in the middle of the line
      StringBuilder bld = new StringBuilder();
      bld.Append(production.LValue.Name);
      bld.Append(" -> ");
      for (int i = 0; i < production.RValues.Count; i++) {
        if (i == dotPosition)
          bld.Append(dotChar);
        bld.Append(production.RValues[i].Name);
        bld.Append(" ");
      }//for i
      if (dotPosition == production.RValues.Count)
        bld.Append(dotChar);
      return bld.ToString();
    }


    public static string JoinStrings( string separator, IEnumerable<string> values) {
      StringList list = new StringList();
      list.AddRange(values);
      string[] arr = new string[list.Count];
      list.CopyTo(arr, 0);
      return string.Join(separator, arr);
    }

    //removes \b characters
    public static string Cleanup(String value) {
      if (String.IsNullOrEmpty(value)) return value;
      return value.Replace("\b", " ");
    }

  }//class


}
