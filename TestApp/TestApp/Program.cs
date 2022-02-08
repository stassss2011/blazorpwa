using System;
using System.Text;
using CommonLibrary.Attacks;
using CommonLibrary.Ciphers;

namespace TestApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var key = Encoding.ASCII.GetBytes("bab");
            var a = new DES(key);
            var input =
                "In polyalphabetic substitution ciphers where the substitution alphabets are chosen by the use of a keyword, the Kasiski examination allows a cryptanalyst to deduce the length of the keyword.";

            var enc = a.Encrypt(Encoding.ASCII.GetBytes(input));
            Console.WriteLine(Encoding.ASCII.GetString(enc));
            a = new DES(key);
            var dec = a.Decrypt(enc);
            Console.WriteLine(Encoding.ASCII.GetString(dec));
        }
    }
}