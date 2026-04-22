using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerLabs.Core.Parser.Ast
{
    public class CallExpression : Expression
    {
        public string CalleeName { get; }
        public List<Expression> Arguments { get; }

        public CallExpression(string calleeName, List<Expression> arguments, int line, int col)
            : base(line, col)
        {
            CalleeName = calleeName;
            Arguments = arguments;
        }
    }
}
