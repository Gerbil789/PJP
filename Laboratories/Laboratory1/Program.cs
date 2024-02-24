// Laboratory 1 : Interpreter of Arithmetic Expressions
// RUB0031
// 24.2.2024

class Program
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());

        for (int i = 0; i < N; i++)
        {
            try
            {
                string expression = Console.ReadLine();
                int result = EvaluateExpression(expression);
                Console.WriteLine(result);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR");
                //Console.WriteLine(e.Message.ToString());
            }
        }
    }

    static int EvaluateExpression(string expression)
    {
        expression = expression.Replace(" ", ""); // Remove whitespace
        Queue<char> tokens = new Queue<char>(expression); // Convert to queue
        return EvaluateAdditionSubtraction(tokens);
    }

    static int EvaluateAdditionSubtraction(Queue<char> tokens)
    {
        int result = EvaluateMultiplicationDivision(tokens);
        while (tokens.Count > 0)
        {
            char op = tokens.Peek();
            if (op == '+' || op == '-')
            {
                tokens.Dequeue();
                int nextOperand = EvaluateMultiplicationDivision(tokens);
                if (op == '+')
                    result += nextOperand;
                else
                    result -= nextOperand;
            }
            else
            {
                break;
            }
        }
        return result;
    }

    static int EvaluateMultiplicationDivision(Queue<char> tokens)
    {
        int result = EvaluateOperand(tokens);
        while (tokens.Count > 0)
        {
            char op = tokens.Peek();
            if (op == '*' || op == '/')
            {
                tokens.Dequeue();
                int nextOperand = EvaluateOperand(tokens);
                if (op == '*')
                    result *= nextOperand;
                else
                    result /= nextOperand;
            }
            else
            {
                break;
            }
        }
        return result;
    }

    static int EvaluateOperand(Queue<char> tokens)
    {
        if (tokens.Count == 0)
            throw new ArgumentException("Expression ended unexpectedly.");

        char c = tokens.Dequeue();
        if (c == '(')
        {
            int result = EvaluateAdditionSubtraction(tokens);
            if (tokens.Count == 0 || tokens.Dequeue() != ')')
                throw new ArgumentException("Closing parenthesis expected.");
            return result;
        }
        else if (char.IsDigit(c))
        {
            int value = c - '0';
            while (tokens.Count > 0 && char.IsDigit(tokens.Peek()))
            {
                c = tokens.Dequeue();
                value = value * 10 + (c - '0');
            }
            return value;
        }
        else
        {
            throw new ArgumentException("Invalid character in expression.");
        }
    }
}
