using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPSiteGenerator
{
    public enum MathOp
    {
        Expotent,
        Multiply,
        Divide,
        Int_Div,
        Add,
        Subtract,
        Modulo,
        Equal,
        NotEqual,
    }

    public class TextFormulaParser
    {
        private enum PState
        {
            // Whitespace or at the start
            none,
            // hitting underscore or letters, then until whitespace
            read_name,
            after_name,
            // hitting digit (either after whitespace or a single minus sign)
            read_int,
            // hit a . either after whitespace or read_int
            read_float,
            after_const,
            // reading an operator
            read_op,
            after_op,
            // Right after opening brackets "("
            paren_open,
            // Right after closing brackets ")"
            paren_closed
        }

        public static IReadOnlyDictionary<string, MathOp> Ops = new Dictionary<string, MathOp>
        {
            {"+", MathOp.Add },
            {"-", MathOp.Subtract },
            {"*", MathOp.Multiply },
            {"/", MathOp.Divide },
            {"^", MathOp.Expotent },
            {"%", MathOp.Modulo },
            {"//", MathOp.Int_Div },
            {"==", MathOp.Equal },
            {"!=", MathOp.NotEqual }
        };

        public static IReadOnlyDictionary<MathOp, int> OpOrder = new Dictionary<MathOp, int>
        {
            { MathOp.Expotent, 100 },
            { MathOp.Multiply, 50 },
            { MathOp.Divide, 50 },
            { MathOp.Int_Div, 50 },
            { MathOp.Add, 20 },
            { MathOp.Subtract, 20 },
            { MathOp.Modulo, 10 },
            { MathOp.Equal, 5 },
            { MathOp.NotEqual, 5 },
         };

        private struct Range
        {
            public int start, end;
        }

        public static ITextFormula Parse(string text)
        {
            PState state = PState.none;
            Range workingText;
            workingText.start = 0;

            ITextFormula topLevelFormula = null;
            ITextFormula workingFormula = null;

            string workingString() =>
                text.Substring(workingText.start, workingText.end - workingText.start + 1);

            bool idStartChar(char c) => char.IsLetter(c) || c == '_';
            bool idCharacter(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '-';

            bool isOp(char c) =>
                c == '+' || c == '%' || c == '*' || c == '-' || c == '^' || c == '/' || c == '!' || c == '=';

            void applyValueFormula(ITextFormula f)
            {
                if (workingFormula is null)
                {
                    workingFormula = f;
                }
                else if (workingFormula is BinMathFormula bin)
                {
                    if (bin.Right != null)
                    {
                        throw new Exception(
                            string.Format("Unexpected variable after formula {0}\n{1}",
                                bin, text));
                    }
                    bin.Right = f;
                }
                else
                {
                    throw new Exception(
                        string.Format("Unexpected VariableFormula following {0}\n{1}",
                            workingFormula, text));
                }
                if(topLevelFormula is null)
                {
                    topLevelFormula = workingFormula;
                }
            }

            for (int i = 0; i < text.Length; i++)
            {
                workingText.end = i;
                char c = text[i];
                char next = i+1 >= text.Length ? '\0' : text[i+1]; ;

                //start reading a token
                if (state == PState.none)
                {
                    workingText.start = i;
                    if (idStartChar(c))
                    {
                        state = PState.read_name;
                    }
                    else if (char.IsDigit(c))
                    {
                        state = PState.read_int;
                    }
                    else if (c == '.')
                    {
                        state = PState.read_float;
                    }
                    else if (!char.IsWhiteSpace(c))
                    {
                        state = PState.read_op;
                    }
                }

                // Finished reading a variable
                if (state == PState.read_name && !idCharacter(next))
                {
                    var v = new VariableFormula(workingString());
                    applyValueFormula(v);
                    state = PState.none;
                }
                // finished reading a number
                else if((state == PState.read_int || state == PState.read_float) && !char.IsDigit(next))
                {
                    var con = new ConstFormula(workingString());
                    applyValueFormula(con);
                    state = PState.none;
                }
                // finished reading an operator
                else if(state == PState.read_op && !isOp(next))
                {
                    var bin = new BinMathFormula(workingString(), null, null);
                    if(topLevelFormula is null)
                    {
                        throw new Exception(string.Format("Binary operator before any values!: {0}\n{1}", workingString(), text));
                    }
                    if(topLevelFormula is BinMathFormula prev)
                    {
                        if(prev.Right is null)
                        {
                            throw new Exception(string.Format("Incomplete formula before operator {0}: {1}\n{2}", workingString(), prev, text));
                        }
                        if(OpOrder[prev.Op] >= OpOrder[bin.Op])
                        {
                            bin.Left = prev;
                            topLevelFormula = bin;
                        }
                        else
                        {
                            bin.Left = prev.Right;
                            prev.Right = bin;
                        }
                    }
                    else
                    {
                        bin.Left = topLevelFormula;
                        topLevelFormula = bin;
                    }
                    workingFormula = topLevelFormula;
                    state = PState.none;
                }
                // switch from read_int to read_float
                else if (state == PState.read_int && next == '.')
                {
                    state = PState.read_float;
                }
            }
            return topLevelFormula;
        }
    }

    public interface ITextFormula
    {
        bool CanCompute(IDictionary<string, ITemplateValue> values);
        string Compute(IDictionary<string, ITemplateValue> values);
    }

    public class VariableFormula : ITextFormula
    {
        public string Name
        {
            get;
            private set;
        }

        public VariableFormula(string name)
        {
            Name = name;
        }

        public bool CanCompute(IDictionary<string, ITemplateValue> values)
        {
            return values.ContainsKey(Name);
        }

        public string Compute(IDictionary<string, ITemplateValue> values)
        {
            return values[Name].ToString();
        }

        public override string ToString() => Name;
    }

    public class ConstFormula : ITextFormula
    {
        public string Value
        {
            get;
            set;
        }

        public ConstFormula(string value)
        {
            Value = value;
        }

        public bool CanCompute(IDictionary<string, ITemplateValue> values) => true;
        public string Compute(IDictionary<string, ITemplateValue> values) => Value;

        public override string ToString() => Value;
    }

    public class BinMathFormula : ITextFormula
    {
        public ITextFormula Left
        {
            get;
            set;
        }

        public ITextFormula Right
        {
            get;
            set;
        }

        public MathOp Op
        {
            get;
            set;
        }

        public BinMathFormula(string op, ITextFormula left, ITextFormula right)
        {
            if (!TextFormulaParser.Ops.ContainsKey(op))
            {
                throw new NotImplementedException(string.Format("No math operation: '{0}'", op));
            }
            Op = TextFormulaParser.Ops[op];

            left = Left;
            right = Right;
        }

        public bool CanCompute(IDictionary<string, ITemplateValue> values)
        {
            return Left.CanCompute(values) && Right.CanCompute(values);
        }

        public string Compute(IDictionary<string, ITemplateValue> values)
        {
            string ls = Left.Compute(values);
            string rs = Right.Compute(values);
            try
            {
                double lhs = double.Parse(Left.Compute(values));
                double rhs = double.Parse(Right.Compute(values));

                double res = double.NaN;
                switch (Op)
                {
                    case MathOp.Add:
                        res = lhs + rhs;
                        break;
                    case MathOp.Subtract:
                        res = lhs - rhs;
                        break;
                    case MathOp.Multiply:
                        res = lhs * rhs;
                        break;
                    case MathOp.Divide:
                        res = lhs / rhs;
                        break;
                    case MathOp.Int_Div:
                        res = Math.Floor(lhs / rhs);
                        break;
                    case MathOp.Expotent:
                        res = Math.Pow(lhs, rhs);
                        break;
                    case MathOp.Modulo:
                        res = lhs % rhs;
                        break;
                    case MathOp.Equal:
                        return (lhs == rhs).ToString();
                    case MathOp.NotEqual:
                        return (lhs != rhs).ToString();
                    default:
                        throw new NotImplementedException(string.Format("No math operation: {0}", Op));
                }

                return res.ToString();
            }
            catch(Exception e)
            {
                throw new Exception(string.Format("Failed to compute {0}({1}, {2})", Op, ls, rs), e);
            }
        }

        public override string ToString() => string.Format("{0}({1}, {2})", Op, Left, Right);
    }
}
