using System.Linq;

namespace CommonLibrary.Ciphers
{
    public class CaesarCipher : ICipher
    {
        public CaesarCipher(int shift, string alphabet)
        {
            Shift = shift;
            Alphabet = alphabet;
        }

        public int Shift { get; }
        public string Alphabet { get; }

        public string Encrypt(string message)
        {
            return new string(message.Select(symbol =>
                    !Alphabet.Contains(symbol)
                        ? symbol
                        : Alphabet[(Alphabet.IndexOf(symbol) + Shift) % Alphabet.Length])
                .ToArray()
            );
        }

        public string Decrypt(string message)
        {
            return new string(message.Select(symbol =>
                    !Alphabet.Contains(symbol)
                        ? symbol
                        : Alphabet[(Alphabet.IndexOf(symbol) - Shift) % Alphabet.Length])
                .ToArray()
            );
        }
    }
}