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

        public SimpleSnippet(string snippet)
        {
            SnippetBody = snippet;
        }
        public void InsertSnippet(IScintillaGateway scintilla)
        {
            scintilla.AddText(SnippetBody.Length, SnippetBody);
            throw new NotImplementedException();
        }
    }
}
