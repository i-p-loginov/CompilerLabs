using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerLabs.Core.Semantic
{
    public class SymbolInfo
    {
        public string Name { get; set; } = string.Empty;
        public bool IsInitialized { get; set; }
        public bool IsUsed { get; set; }
        public DataType Type { get; set; } = DataType.Unknown;
        public int? Arity { get; set; } // Для функций - количество параметров
    }
}
