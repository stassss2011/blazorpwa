using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLibrary.Ciphers
{
    public class VigenereCipher : ICipher
    {
        private readonly Dictionary<char, string> _keyAlphabets;

        public VigenereCipher(string key, string alphabet)
        {
            Key = key.ToUpperInvariant();
            Alphabet = alphabet;
            _keyAlphabets = new Dictionary<char, string>();
            foreach (var chr in Key)
            {
                var newAlphabet = new string(Alphabet.Select(symbol =>
                        !Alphabet.Contains(symbol)
                            ? symbol
                            : Alphabet[(Alphabet.IndexOf(symbol) + chr - 'A') % Alphabet.Length])
                    .ToArray()
                );
                if (!_keyAlphabets.ContainsKey(chr))
                    _keyAlphabets.Add(chr, newAlphabet);
            }
        }

        public string Key { get; set; }
        public string Alphabet { get; set; }

        public string Encrypt(string message)
        {
            return Code(message.ToUpperInvariant(), true);
        }

        public string Decrypt(string message)
        {
            return Code(message.ToUpperInvariant(), false);
        }

        private string Code(string message, bool encrypt)
        {
            var sign = encrypt ? 1 : -1;

            var keyI = 0;
            var encryptedSb = new StringBuilder();
            foreach (var msg in message)
            {
                var key = Key[keyI % Key.Length];
                if (!_keyAlphabets[key].Contains(msg))
                {
                    encryptedSb.Append(' ');
                }
                else
                {
                    var signedIndex = (_keyAlphabets[key].IndexOf(msg) + sign * (key - 'A')) %
                                      _keyAlphabets[key].Length;
                    var index = signedIndex >= 0 ? signedIndex : _keyAlphabets[key].Length + signedIndex;
                    encryptedSb.Append(_keyAlphabets[key][index]);
                    keyI++;
                }
            }

            return encryptedSb.ToString();
        }
    }
}