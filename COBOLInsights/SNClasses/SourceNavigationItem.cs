using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COBOLInsights.SNClasses
{
    class SourceNavigationItem
    {
        string _name;
        int _lineNumber;
        int _charNumber;
        public string Name { get => _name; set => _name = value; }
        public int LineNumber { get => _lineNumber; set => _lineNumber = value; }
        public int CharNumber { get => _charNumber; set => _charNumber = value; }
    }
}
