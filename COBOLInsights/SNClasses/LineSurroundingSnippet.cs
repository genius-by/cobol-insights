using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kbg.NppPluginNET.PluginInfrastructure;

namespace COBOLInsights.SNClasses
{
    class LineSurroundingSnippet : ISnippet
    {
        private string SnippetBodyBefore;
        private string SnippetBodyAfter;

        private string SnippetMenuCommandName;
        private bool KeepIndent;
        private int IndentSelection;
        private string Placeholder = "{*}";

        public LineSurroundingSnippet(string name, string snippetBodyBefore, string snippetBodyAfter, bool keepIndent = false, int indentSelection = 0)
        {
            SnippetMenuCommandName = name;
            SnippetBodyBefore = snippetBodyBefore;
            SnippetBodyAfter = snippetBodyAfter;
            KeepIndent = keepIndent;
            IndentSelection = indentSelection;
        }

        public string GetCommandName()
        {
            return SnippetMenuCommandName;
        }

        public void InsertSnippet()
        {
            ScintillaGateway editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
            editor.BeginUndoAction();

            Position end = editor.GetLineEndPosition(editor.LineFromPosition(editor.GetSelectionEnd()));
            Position start = editor.PositionFromLine(editor.LineFromPosition(editor.GetSelectionStart()));
            int indent = KeepIndent ? editor.GetLineIndentation(editor.LineFromPosition(start)) : 0;

            string insertText = IndentSnippetBody(SnippetBodyBefore, indent)
                              + IndentSelectedStrings(editor, start, end)
                              + IndentSnippetBody(SnippetBodyAfter, indent);

            editor.DeleteRange(start, end.Value - start.Value + 1);
            editor.InsertText(start, insertText);
            using (TextToFind textToFind = new TextToFind(start.Value, start.Value + insertText.Length, Placeholder))
            {
                Position placeholderPosition = editor.FindText(0, textToFind);
                editor.SetSelection(placeholderPosition.Value + Placeholder.Length, placeholderPosition.Value);
            }
            editor.EndUndoAction();
        }

        private string IndentSelectedStrings(IScintillaGateway editor, Position start, Position end)
        {
            int firstLine = editor.LineFromPosition(start);
            int lastLine = editor.LineFromPosition(end);
            string output = "";
            for (int i = firstLine; i <= lastLine; i++)
            {
                string lineText = editor.GetLine(i);
                output += lineText.StartsWith("*") ? lineText : new string(' ', IndentSelection) + lineText;
            }
            return output;
        }

        private string IndentSnippetBody(string text, int indent)
        {
            var lines = text.Split('\n');
            string result = "";
            foreach (var line in lines)
            {
                string indentSpaces = "";
                for (int i = 0; i < indent; i++)
                {
                    indentSpaces += " ";
                }
                result += indentSpaces + line + "\n";
            }
            return result;
        }
    }
}
