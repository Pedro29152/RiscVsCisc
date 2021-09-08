using System;

namespace RiscVsCisc
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Cisc.MakeOperation(90, 90, Operations.Multiply));
            Console.WriteLine(Risc.MakeOperation(90, 90, Operations.Multiply));
            Console.ReadKey();
        }
    }
}
