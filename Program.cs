using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using project1;

namespace project1
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1) 
            {
                Console.WriteLine("Supply one input file as an argument");
                return;
            }
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("File not found");
                return;
            }

            using (var file = File.OpenRead(args[0]))
            {

                try
                {
                    Parser p = new Parser(file);
                    p.Prog();
                    Console.WriteLine("Parse successful");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Parse ended with exception: " + e);
                }
                

            }
            Console.ReadKey(); //  keep the console window open
        }
    }
}
