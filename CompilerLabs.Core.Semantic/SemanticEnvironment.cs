using System.Collections.Generic;

namespace CompilerLabs.Core.Semantic
{
    public class SemanticEnvironment
    {
        private readonly SemanticEnvironment? _parent;

        private readonly Dictionary<string, SymbolInfo> _variables;

        public SemanticEnvironment(SemanticEnvironment? parent = null)
        {
            _parent = parent;
            _variables = new Dictionary<string, SymbolInfo>();
        }

        public bool DefineVariable(string name, bool isInitialized)
        {
            if (_variables.ContainsKey(name))
            {
                return false;
            }

            _variables[name] = new SymbolInfo { Name = name, IsInitialized = isInitialized };
            return true;
        }

        public bool IsVariableDefined(string name)
        {
            if (_variables.ContainsKey(name)) return true;
            return _parent?.IsVariableDefined(name) ?? false;
        }

        public SymbolInfo? GetVariable(string name)
        {
            if (_variables.TryGetValue(name, out var symbol)) return symbol;
            return _parent?.GetVariable(name);
        }

        public void SetInitialized(string name)
        {
            var symbol = GetVariable(name);
            if (symbol != null)
                symbol.IsInitialized = true;
        }

        public IEnumerable<SymbolInfo> GetLocalVariables()
        {
            return _variables.Values;
        }
    }
}