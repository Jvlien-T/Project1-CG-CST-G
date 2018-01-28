using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] ParametersList = new string[] { "1", "2", "100 100", "300 300", "10 10 0 0 0 0", "1000 1000 0 0 0 0", "2010 2010 0 0 0 0", "3010 3010 0 0 0 0" };
            CodersStrikeBackGOLD.CSBProgram.Main(ParametersList);
            Console.ReadLine();
        }
    }
}
