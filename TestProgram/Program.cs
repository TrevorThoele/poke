using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace HelloWorld
{
    class Program
    {
        static string zecksString = "zecks-string";

        static void Main(string[] args)
        {
            zecksString = "different";
            var trevorsInt = 456;
            var output = $"Hello, World!";
            Console.WriteLine(output);
        }
    }
}