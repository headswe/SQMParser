using ArmaSQMParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQMParserTest
{
    class Program
    {
        static void Main(string[] args)
        {
            SQMParser parser = new SQMParser(@"C:\Users\Sebastian\Documents\Arma 3 - Other Profiles\Head\MPMissions\ffff.Altis\mission.sqm");
            parser.Process();
            parser.Print();
            Console.Read();
        }
    }
}
