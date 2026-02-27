using CompilerLabs.Core.Parser.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab02.ParserDemo
{
    public class AstPrinter
    {
        // Главный метод для вывода всей программы
        public void Print(List<Statement> statements)
        {
            Console.WriteLine("Root (Program)");
            for (int i = 0; i < statements.Count; i++)
            {
                PrintNode(statements[i], "", i == statements.Count - 1);
            }
        }

        // Рекурсивный метод отрисовки
        private void PrintNode(object node, string indent, bool isLast)
        {
            if (node == null) return;

            // Рисуем веточку
            string marker = isLast ? "└── " : "├── ";
            Console.Write(indent + marker);

            // Подготавливаем отступ для дочерних элементов
            string childIndent = indent + (isLast ? "    " : "│   ");

            switch (node)
            {
                case VarStatement v:
                    Console.WriteLine($"VarStatement: {v.Name}");

                    if (v.Initializer != null) 
                        PrintNode(v.Initializer, childIndent, true);

                    break;

                case PrintStatement p:
                    Console.WriteLine("PrintStatement");
                    PrintNode(p.Expression, childIndent, true);
                    break;

                case IfStatement i:
                    Console.WriteLine("IfStatement");
                    PrintNode(i.Condition, childIndent, false);
                    PrintNode(i.ThenBranch, childIndent, i.ElseBranch == null);
                
                    if (i.ElseBranch != null) 
                        PrintNode(i.ElseBranch, childIndent, true);
                    
                    break;

                case WhileStatement w:
                    Console.WriteLine("WhileStatement");
                    PrintNode(w.Condition, childIndent, false);
                    PrintNode(w.Body, childIndent, true);
                    break;

                case BlockStatement b:
                    Console.WriteLine("BlockStatement");
                    for (int j = 0; j < b.Statements.Count; j++)
                    {
                        PrintNode(b.Statements[j], childIndent, j == b.Statements.Count - 1);
                    }
                    break;

                case ExpressionStatement e:
                    Console.WriteLine("ExpressionStatement");
                    PrintNode(e.Expression, childIndent, true);
                    break;

                case BinaryExpression bin:
                    Console.WriteLine($"BinaryExpression: {bin.Operator}");
                    PrintNode(bin.Left, childIndent, false);
                    PrintNode(bin.Right, childIndent, true);
                    break;

                case UnaryExpression un:
                    Console.WriteLine($"UnaryExpression: {un.Operator}");
                    PrintNode(un.Right, childIndent, true);
                    break;

                case AssignExpression assign:
                    Console.WriteLine($"AssignExpression: {assign.Name} =");
                    PrintNode(assign.Value, childIndent, true);
                    break;

                case NumberExpression num:
                    Console.WriteLine($"Number: {num.Value}");
                    break;

                case VariableExpression varExpr:
                    Console.WriteLine($"Variable: {varExpr.Name}");
                    break;

                default:
                    Console.WriteLine($"Unknown Node: {node.GetType().Name}");
                    break;
            }
        }
    }
}
