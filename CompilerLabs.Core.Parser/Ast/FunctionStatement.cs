using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerLabs.Core.Parser.Ast
{
    public class FunctionStatement : Statement
    {
        public string Name { get; }
        public List<string> Parameters { get; }
        public BlockStatement Body { get; }

        public FunctionStatement(string name, List<string> parameters, BlockStatement body, int line, int col)
            : base(line, col)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }
    }
}
