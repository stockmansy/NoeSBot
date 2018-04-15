using System;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Helpers
{
    public static class DiceHelper
    {
        private static Random _random = new Random();

        public static int GetDiceResult(string input)
        {
            var fixedInput = input.Replace('x', '*');
            var stf = new StringToFormula();
            var db = stf.Eval(fixedInput);
            var number = (int)Math.Round(db);

            return _random.Next(1, number);
        }

        public static int GetDndResult(string input)
        {
            var chars = new char[] { '+', '-', '*', 'x', '/' };

            string[] splits = null;
            foreach (var c in chars)
            {
                splits = input.Split(c);
                if (splits.Length > 2)
                    throw new Exception("Too big of an expression for now...");

                if (splits.Length == 2)
                {
                    var parsedStart = GetIntValue(splits[0]);
                    var parsedEnd = GetIntValue(splits[1]);
                    var rnd = _random.Next(1, parsedStart);

                    switch (c)
                    {
                        case '+':
                            return rnd + parsedEnd;
                        case '-':
                            return rnd - parsedEnd;
                        case 'x':
                        case '*':
                            return rnd * parsedEnd;
                        case '/':
                            return rnd / parsedEnd;
                    }
                }
            }

            return _random.Next(1, GetIntValue(splits[0]));
        }

        #region Private

        private static int GetIntValue(string input)
        {
            if (!int.TryParse(input.Replace(" ", ""), out int result))
                throw new Exception("Invalid input");

            return result;
        }

        private class StringToFormula
        {
            private string[] _operators = { "-", "+", "/", "*", "^" };
            private Func<double, double, double>[] _operations = {
                (a1, a2) => a1 - a2,
                (a1, a2) => a1 + a2,
                (a1, a2) => a1 / a2,
                (a1, a2) => a1 * a2,
                (a1, a2) => Math.Pow(a1, a2)
            };

            public double Eval(string expression)
            {
                List<string> tokens = getTokens(expression);
                Stack<double> operandStack = new Stack<double>();
                Stack<string> operatorStack = new Stack<string>();
                int tokenIndex = 0;

                while (tokenIndex < tokens.Count)
                {
                    string token = tokens[tokenIndex];
                    if (token == "(")
                    {
                        string subExpr = getSubExpression(tokens, ref tokenIndex);
                        operandStack.Push(Eval(subExpr));
                        continue;
                    }
                    if (token == ")")
                    {
                        throw new ArgumentException("Mis-matched parentheses in expression");
                    }
                    //If this is an operator  
                    if (Array.IndexOf(_operators, token) >= 0)
                    {
                        while (operatorStack.Count > 0 && Array.IndexOf(_operators, token) < Array.IndexOf(_operators, operatorStack.Peek()))
                        {
                            string op = operatorStack.Pop();
                            double arg2 = operandStack.Pop();
                            double arg1 = operandStack.Pop();
                            operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
                        }
                        operatorStack.Push(token);
                    }
                    else
                    {
                        operandStack.Push(double.Parse(token));
                    }
                    tokenIndex += 1;
                }

                while (operatorStack.Count > 0)
                {
                    string op = operatorStack.Pop();
                    double arg2 = operandStack.Pop();
                    double arg1 = operandStack.Pop();
                    operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
                }
                return operandStack.Pop();
            }

            #region Private

            private string getSubExpression(List<string> tokens, ref int index)
            {
                StringBuilder subExpr = new StringBuilder();
                int parenlevels = 1;
                index += 1;
                while (index < tokens.Count && parenlevels > 0)
                {
                    string token = tokens[index];
                    if (tokens[index] == "(")
                    {
                        parenlevels += 1;
                    }

                    if (tokens[index] == ")")
                    {
                        parenlevels -= 1;
                    }

                    if (parenlevels > 0)
                    {
                        subExpr.Append(token);
                    }

                    index += 1;
                }

                if ((parenlevels > 0))
                {
                    throw new ArgumentException("Mis-matched parentheses in expression");
                }
                return subExpr.ToString();
            }

            private List<string> getTokens(string expression)
            {
                string operators = "()^*/+-";
                List<string> tokens = new List<string>();
                StringBuilder sb = new StringBuilder();

                foreach (char c in expression.Replace(" ", string.Empty))
                {
                    if (operators.IndexOf(c) >= 0)
                    {
                        if ((sb.Length > 0))
                        {
                            tokens.Add(sb.ToString());
                            sb.Length = 0;
                        }
                        tokens.Add(c.ToString());
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }

                if ((sb.Length > 0))
                {
                    tokens.Add(sb.ToString());
                }
                return tokens;
            }

            #endregion
        }

        #endregion 
    }
}
