using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kbg.NppPluginNET.PluginInfrastructure;

namespace COBOLInsights.SNClasses
{
    class SimpleSnippet : ISnippet
    {
        private string SnippetBody;

        public string SnippetMenuCommandName;
        private string Placeholder = "{*}";

        public SimpleSnippet(string name, string snippet)
        {
            SnippetBody = snippet;
            SnippetMenuCommandName = name;
        }
        public void InsertSnippet()
        {
            var editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
            editor.AddText(SnippetBody.Length, SnippetBody);

            using (TextToFind textToFind = new TextToFind(editor.GetCurrentPos().Value - SnippetBody.Length, editor.GetCurrentPos().Value, Placeholder))
            {
                Position placeholderPosition = editor.FindText(0, textToFind);
                editor.SetSelection(placeholderPosition.Value + Placeholder.Length, placeholderPosition.Value);
            }
        }

        public string GetCommandName()
        {
            return SnippetMenuCommandName;
        }
    }
}
