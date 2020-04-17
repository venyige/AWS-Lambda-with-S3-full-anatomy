using System;
using System.Collections.Generic;
using System.IO;

namespace ArabToRomanConverter
{
    struct RomanDigit
    {
        public bool pT; //isPrimaryType;//is it ten exponential that is in the set of {I, X, C, M}
        public string d; //digit;
        public int n;
    }
    class Program
    {
        public static List<RomanDigit> romanDigits = new List<RomanDigit>() {
         new RomanDigit{pT=true,  n=1000, d="M" },
         new RomanDigit{pT=false, n=500,  d="D" },
         new RomanDigit{pT=true,  n=100,  d="C" },
         new RomanDigit{pT=false, n=50,   d="L" },
         new RomanDigit{pT=true,  n=10,   d="X" },
         new RomanDigit{pT=false, n=5,    d="V" },
         new RomanDigit{pT=true,  n=1,    d="I" }};

        public static string ArabToRoman(int arabNum)
        {
            var retStr = string.Empty;
            if (arabNum > 0 && arabNum <= 4000)
            {
                while (arabNum > 0)
                {
                    int iiPri = romanDigits.FindIndex(x => x.n <= arabNum); /// Primary index
                    if (iiPri > 0)
                    {
                        int iiNeg = iiPri + (romanDigits[iiPri].pT ? 0 : 1); /// Index of the first element of the reduced number
                        int neg = romanDigits[iiNeg].n;/// The first element of the reduced number
                        int iiRed = romanDigits.FindIndex(x => x.n - neg <= arabNum);/// Index of the largest reduced number 
                        if (iiRed < iiPri)/// If the reduced number is more significant
                        {
                            iiPri = iiRed;
                            arabNum -= (romanDigits[iiPri].n - neg);
                            retStr += (romanDigits[iiNeg].d + romanDigits[iiPri].d);
                        }
                        else
                        {
                            arabNum -= romanDigits[iiPri].n;
                            retStr += romanDigits[iiPri].d;
                        }

                    }
                    else /// If 1000
                    {
                        arabNum -= romanDigits[iiPri].n;
                        retStr += romanDigits[iiPri].d;
                    }

                }
            }
            else
            {
                /// With the actual Main() function, it is never processed
                retStr = "ERROR: The number must be between 0 and 4000";

            }
            return retStr;
        }
        enum FileRes{no_file=0, arab_in, cmd_arg };
        static void Main(string[] args)
        {
            string inFile = "ARAB.IN";
            string outFile = string.Empty;

            FileRes fileRes = FileRes.no_file;
            List<int> arabNums=new List<int> { };
            if (args.Length == 1)
            {
                if (File.Exists(args[0]))
                {
                    inFile = args[0];
                    fileRes = FileRes.cmd_arg;
                }
                else
                {
                    if (File.Exists(inFile))
                    {
                        Console.WriteLine("The file given in parameter not valid, reading \"ARAB.IN\" instead.");
                        fileRes = FileRes.arab_in;
                    }
                }
            }
            else if(args.Length==0)
            {
                if (File.Exists(inFile))
                {
                    fileRes = FileRes.arab_in;
                }

            }
            if (fileRes == FileRes.no_file)
            {
                Console.WriteLine("The program accepts only one parameter,");
                Console.WriteLine("the path to the file containing the arab numbers to convert.");
                Console.WriteLine("File format:\nEach line contains one number of value between 0-4000");
                Console.WriteLine("If no file given at parameter, the current directory is looked up for");
                Console.WriteLine("a file with the specific name \"ARAB.IN\"");
            }
            else
            {
                inFile = Path.GetFullPath(inFile);
                outFile = new FileInfo(inFile).DirectoryName+Path.DirectorySeparatorChar+"ROMAI.OUT";
                try
                {
                    using (StreamReader sr = new StreamReader(inFile))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.Length == 0)
                                continue;
                            try
                            {
                                int numToAdd = Int32.Parse(line);
                                if (numToAdd > 0 && numToAdd < 4000)
                                {
                                    arabNums.Add(numToAdd);
                                }
                                else
                                {
                                    Console.WriteLine("The input file contains number(s) out of range 0 - 4000");

                                }

                            }
                            catch (FormatException e)
                            {
                                Console.WriteLine("The input file contains non integer-convertible line(s)" + e.Message);
                                return;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The input file could not be read:");
                    Console.WriteLine(e.Message);
                }
                try
                {
                    using (StreamWriter sw = new StreamWriter(outFile))
                    {
                        foreach (int iii in arabNums)
                        {
                            sw.WriteLine(ArabToRoman(iii));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The output file could not be written:");
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
