using CompilerLabs.Core.Parser.Ast;

namespace CompilerLabs.Core.Interpreter
{
    public class RuntimeEnvironment
    {
        private readonly RuntimeEnvironment? _parent;

        private readonly Dictionary<string, object?> _values;

        private readonly Dictionary<string, FunctionStatement> _functions = new();

        public RuntimeEnvironment(RuntimeEnvironment? parent = null)
        {
            _parent = parent;
            _values = new Dictionary<string, object?>();
        }

        public void Define(string name, object? value)
        {
            _values[name] = value;
        }

        public void Assign(string name, object? value)
        {
            if (_values.ContainsKey(name))
            {
                _values[name] = value;
                return;
            }

            if (_parent != null)
            {
                _parent.Assign(name, value);
                return;
            }

            throw new Exception($"[Runtime Error] Неизвестная переменная '{name}'.");
        }

        public object? Get(string name)
        {
            if (_values.TryGetValue(name, out var value))
            {
                return value;
            }

            if (_parent != null)
            {
                return _parent.Get(name);
            }

            throw new Exception($"[Runtime Error] Неизвестная переменная '{name}'.");
        }

        public void DefineFunction(string name, FunctionStatement function)
        {
            _functions[name] = function;
        }

        public FunctionStatement GetFunction(string name)
        {
            if (_functions.TryGetValue(name, out var function))
            {
                return function;
            }
            if (_parent != null)
            {
                return _parent.GetFunction(name);
            }
            throw new Exception($"[Runtime Error] Неизвестная функция '{name}'.");
        }
    }
}
