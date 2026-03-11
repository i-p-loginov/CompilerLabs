using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerLabs.Core.Semantic
{
    public class SemanticEnvironment
    {
        private readonly SemanticEnvironment? _parent;
        private readonly Dictionary<string, bool> _variables;

        public SemanticEnvironment(SemanticEnvironment? parent = null)
        {
            _parent = parent;
            _variables = new Dictionary<string, bool>();
        }

        public bool DefineVariable(string name)
        {
            if (_variables.ContainsKey(name))
            {
                return false;
            }

            _variables[name] = true;
            return true;
        }

        public bool IsVariableDefined(string name)
        {
            if (_variables.ContainsKey(name))
            {
                return true;
            }
            
            return _parent?.IsVariableDefined(name) ?? false;
        }
    }
}
