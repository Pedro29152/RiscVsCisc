using System;

namespace RiscVsCisc
{
    public class Risc
    {
        public static double MakeOperation(double n1, double n2, Operations op)
        {
            switch (op)
            {
                case Operations.Sum:
                    return MathOperations.Sum(n1, n2);
                case Operations.Subtract:
                    return MathOperations.Subtract(n1, n2);
                case Operations.Divide:
                    return MathOperations.Divide(n1, n2);
                case Operations.Multiply:
                    return MathOperations.Multiply(n1, n2);

                default:
                    throw new ArgumentException("Unknown operation");
            }
        }

        private class MathOperations
        {
            public static double Sum(double n1, double n2)
            {
                return n1 + n2;
            }
            public static double Subtract(double n1, double n2)
            {
                return n1 - n2;
            }
            public static double Divide(double n1, double n2)
            {
                return n1 / n2;
            }
            public static double Multiply(double n1, double n2)
            {
                return n1 * n2;
            }
        }
    }
}
