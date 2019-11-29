using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kbg.NppPluginNET.PluginInfrastructure;

namespace COBOLInsights.SNClasses
{
    interface ISnippet
    {
        void InsertSnippet();
        string GetCommandName();
    }
}
