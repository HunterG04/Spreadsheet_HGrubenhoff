using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CptS322;
using SpreadsheetNamespace;

namespace CptS322
{
    public class ExpTree
    {
        //varName and varValue
        private Dictionary<string, double> Variables = new Dictionary<string, double>();
        private Node mRoot;
        private SpreadsheetCell OccupiedCell;
        
        public ExpTree ()
        {
            mRoot = null;
        }

        public ExpTree(string expression, SpreadsheetCell cell)
        {
            OccupiedCell = cell;
            //Constructor (Will construct tree based on expression)
            if (expression != null)
            {
                mRoot = Compile(expression); // create tree
            }
        }

        public string[] getVarNames()
        {
            if (Variables.Count != 0)
            {
                return Variables.Keys.ToArray();
            }
            else
                return null;
        }

        public void SetVar(string varName, double varValue)
        {
            if (Variables.ContainsKey(varName)) // update key
            {
                Variables[varName] = varValue;
            }
            else // add key
            {
                Variables.Add(varName, varValue);
            }
        }

        public double Eval()
        {
            double result = 0;
            
            result = calcValue(mRoot);

            return result;
        }

        public Node MakeSimple (string s) // creates var or num node
        {
            double num = 0;

            if (double.TryParse(s, out num)) // number?
            {
                return new NumericNode(num);
            }

            return new VariableNode(s); // variable?
        }

        public Node Compile (string exp)
        {
            //int index = GetOp(exp);
            string postfixExp = infixToPostfix(exp);
            return PostfixToTree(postfixExp);
        }

        ////Source: http://www.codeproject.com/Tips/370486/Converting-InFix-to-PostFix-using-Csharp-VB-NET
        public string infixToPostfix(string infix)
        {
            StringBuilder postfix = new StringBuilder();
            Stack<string> s = new Stack<string>();
            string ch;

            //will create postfix expression with spaces between each value
            for (int i = 0; i < infix.Length; i++)
            {
                if (Char.IsNumber(infix[i])) // first part is number so its num
                {
                    while (i < infix.Length && Char.IsNumber(infix[i])) // adds nums that arent only one digit. Ex: 55 690
                    {
                        postfix.Append(infix[i]);
                        i++;
                    }

                    postfix.Append(" "); //add space after number in postfix
                    i--;
                }
                else if (Char.IsLetter(infix[i])) // variable
                {
                    int l = i;
                    string var = null;

                    postfix.Append(infix[i]); // append letter of var
                    var = infix[i].ToString();
                    i++;

                    while (i < infix.Length && Char.IsNumber(infix[i])) // go to end of variable
                    {
                        postfix.Append(infix[i]); //add variable to postfix
                        var = var + infix[i]; // used to add to variables
                        i++;
                    }
                    Variables.Add(var, 0); // adds cell to variable list. Value set later
                    postfix.Append(" ");
                    i--;
                }
                else if (infix[i] == '(')
                {
                    s.Push(infix[i].ToString());
                }
                else if (infix[i] == ')')
                {
                    ch = s.Pop();

                    while (ch != "(")
                    {
                        postfix.Append(ch + " ");
                        if (s.Count != 0)
                        {
                            ch = s.Pop();
                        }
                        else
                        { break; }
                    }
                }
                else
                {
                    if (s.Count != 0 && Precedence(s.Peek(), infix[i].ToString()))
                    {
                        ch = s.Pop();

                        while (Precedence(ch, infix[i].ToString()))
                        {
                            postfix.Append(ch + " ");

                            if (s.Count == 0)
                            {
                                break;
                            }

                            ch = s.Pop();
                        }

                        s.Push(infix[i].ToString());
                    }
                    else
                    {
                        s.Push(infix[i].ToString());
                    }
                }
            }
            while (s.Count > 0)
            {
                ch = s.Pop();
                postfix.Append(ch + " ");
            }
            postfix = postfix.Remove((postfix.Length - 1), 1); // remove space at the end of the postfix expression
            return postfix.ToString();
        }

        private static bool Precedence(string firstOp, string secondOp)
        {
            string opString = "(+-*/";

            int firstPoint, secondPoint;

            int[] precedence = { 0, 12, 12, 13, 13 };// "(" has less prececence

            firstPoint = opString.IndexOf(firstOp);
            secondPoint = opString.IndexOf(secondOp);

            return (precedence[firstPoint] >= precedence[secondPoint]) ? true : false;
        }

        public Node PostfixToTree(string postfix)
        {
            if (string.IsNullOrEmpty(postfix) == false)
            {
                string[] noSpacePost = postfix.Split(' '); // will hold operators or digits or varibles in each cell
                Stack<Node> s = new Stack<Node>();
         
                foreach (string item in noSpacePost)
                {
                    if (isOperator(item) == false)
                    {
                        s.Push(MakeSimple(item));
                    }
                    else // it is an operator
                    {
                        Node rightC = s.Pop();
                        Node leftC = s.Pop();

                        Node tempRoot = new OperatorNode(item); // temp root for subtree
                        ((OperatorNode)tempRoot).RightNode = rightC; // sets right child of temp root
                        ((OperatorNode)tempRoot).LeftNode = leftC; // sets left child of temp root
                        s.Push(tempRoot); // push subtree root back into stack
                    }
                }

                return s.Pop(); // return root of tree
            }

            return null;
        }

        public bool isOperator (string s)
        {
            if (s == "+" || s == "-" || s == "*" || s == "/")
            {
                return true;
            }

            return false;
        }

        public double calcValue(Node root) // will calculate value of two children
        {
            double result = 0;
            double leftVal = 0;
            double rightVal = 0;

            if (root is NumericNode) // check if has number
            {
                return ((NumericNode)root).mValue; // just return value
            }
            if (root is VariableNode) // check if variable
            {
                if (Variables.ContainsKey(((VariableNode)root).VarName)) // check if variable is initialized
                {
                    return Variables[((VariableNode)root).VarName]; //returns variable value
                }
                else // variable wasnt initialized, display message and "set" to 0
                {
                    Console.Write("Unitialized Variable " + ((VariableNode)root).VarName + " Set as 0 Temporarily\n");
                    return 0;
                }
            }

            
            leftVal = calcValue(((OperatorNode)root).LeftNode); // left branch
            
            rightVal = calcValue(((OperatorNode)root).RightNode); // right branch

            switch (((OperatorNode)root).Value) // get operation
            {
                case "+":
                    result = leftVal + rightVal;
                    break;
                case "-":
                    result = leftVal - rightVal;
                    break;
                case "*":
                    result = leftVal * rightVal;
                    break;
                case "/":
                    result = leftVal / rightVal;
                    break;
            }

            return result;
        }
    }
}
