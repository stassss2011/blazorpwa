using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CommonLibrary.Ciphers;

namespace CommonLibrary.Attacks
{
    public class BabbageAttack : IAttack
    {
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private static readonly Dictionary<char, double> EnglishFrequency = new()
        {
            { 'A', 0.08167 },
            { 'B', 0.01492 },
            { 'C', 0.02782 },
            { 'D', 0.04253 },
            { 'E', 0.12702 },
            { 'F', 0.02228 },
            { 'G', 0.02015 },
            { 'H', 0.06094 },
            { 'I', 0.06966 },
            { 'J', 0.00153 },
            { 'K', 0.00772 },
            { 'L', 0.04025 },
            { 'M', 0.02406 },
            { 'N', 0.06749 },
            { 'O', 0.07507 },
            { 'P', 0.01929 },
            { 'Q', 0.00095 },
            { 'R', 0.05987 },
            { 'S', 0.06327 },
            { 'T', 0.09056 },
            { 'U', 0.02758 },
            { 'V', 0.00978 },
            { 'W', 0.02360 },
            { 'X', 0.00150 },
            { 'Y', 0.01974 },
            { 'Z', 0.00074 }
        };

        private readonly int _maxKeyLenght;

        public BabbageAttack(int maxKeyLenght)
        {
            _maxKeyLenght = maxKeyLenght;
        }

        public string ForceDecrypt(string message)
        {
            var onlyLettersMessage = new string(message.Where(chr => char.IsLetter(chr) && !char.IsSeparator(chr))
                .Select(char.ToUpper)
                .ToArray());

            var allRepeatingSubstrings = Enumerable.Range(0, onlyLettersMessage.Length)
                .SelectMany(chrI1 =>
                    Enumerable.Range(0, onlyLettersMessage.Length - chrI1 + 1)
                        .Select(chrI2 => (chrI1, chrI2)))
                .Where(tpl => tpl.chrI2 > 0)
                .Select(tpl => onlyLettersMessage.Substring(tpl.chrI1, tpl.chrI2))
                .GroupBy(x => x)
                .Where(g => g.Count() > 1 && g.Key.Length > 2)
                .Select(x => x.Key);


            var differencesDict = new Dictionary<string, List<int>>();

            foreach (var repeatingSubstring in allRepeatingSubstrings)
            {
                var allMatches = Regex.Matches(onlyLettersMessage, repeatingSubstring)
                    .Select(x => x.Index)
                    .ToList();

                foreach (var (match, i) in allMatches.Select((value, i) => (value, i)))
                {
                    if (i == 0)
                        continue;
                    var diff = allMatches[i] - allMatches[i - 1];
                    if (!differencesDict.TryAdd(repeatingSubstring, new List<int> { diff }))
                        differencesDict[repeatingSubstring].Add(diff);
                }
            }


            var allPossibleKeyLen = differencesDict
                .Values
                .SelectMany(
                    x => x
                        .SelectMany(y => y.GetDivisors())
                        .Where(y => y > 1)
                )
                .Where(x => x < _maxKeyLenght)
                .GroupBy(x => x)
                .Select(x => x.Key)
                .OrderBy(x => x)
                .ToList();

            var output = allPossibleKeyLen.Select(possibleKeyLen =>
            {
                var keyBestFitPos = AnalyseFreq(possibleKeyLen, onlyLettersMessage);
                return Bruteforce(onlyLettersMessage, keyBestFitPos);
            });

            return string.Join("\n\n", output
                .SelectMany(x => x)
                .OrderBy(x => x.Item1)
                .Select(x => $"Score: {x.Item1}\nKey: {x.Item2}\nMessage: {x.Item3}"));
        }

        private static List<(int, string)> AnalyseFreq(int tryKeyLen, string encryptedMessage)
        {
            var keyPosToString = new Dictionary<int, string>();

            for (var i = 0; i < tryKeyLen; i++)
            {
                var nths = new string(encryptedMessage
                    .Substring(i, encryptedMessage.Length - i)
                    .Where((c, cI) => cI % tryKeyLen == 0)
                    .ToArray());

                var bestFit = FindBestFit(nths);

                keyPosToString.Add(i, bestFit);
            }

            return keyPosToString.OrderBy(x => x.Key)
                .Select(x => (x.Key, x.Value))
                .ToList();
        }

        private static string FindBestFit(string nths)
        {
            var characterScoreMap = new Dictionary<char, double>();

            foreach (var chr in Alphabet)
            {
                var cipher = new VigenereCipher(chr.ToString(), Alphabet);
                var encryptedBySubKey = cipher.Decrypt(nths);

                var score = EnglishFrequencyScore(encryptedBySubKey);
                characterScoreMap.Add(chr, score);
            }

            return new string(characterScoreMap.OrderBy(x => x.Value)
                .Take(1)
                .Select(x => x.Key)
                .ToArray());
        }

        private static double EnglishFrequencyScore(string encryptedBySubKey)
        {
            var frequencyMap = encryptedBySubKey
                .GroupBy(c => c)
                .ToDictionary(x => x.Key, x => x.Count());

            return Alphabet.Select(chr =>
                {
                    if (!frequencyMap.TryGetValue(chr, out var observed))
                        observed = 0;
                    var expected = encryptedBySubKey.Length * EnglishFrequency[chr];
                    var difference = observed - expected;
                    return difference * difference / expected;
                })
                .Sum();
        }

        private static List<(double, string, string)> Bruteforce(string encryptedMessage,
            IReadOnlyCollection<(int, string)> keyBestFitPos)
        {
            var possibleKeys = OrderedPermutations(keyBestFitPos);

            return possibleKeys.Select(key =>
            {
                var cipher = new VigenereCipher(key, Alphabet);
                var currentdecrypted = cipher.Decrypt(encryptedMessage);
                return (EnglishFrequencyScore(currentdecrypted), key, currentdecrypted);
            }).ToList();
        }

        private static List<string> OrderedPermutations(IReadOnlyCollection<(int, string)> keyBestFitPos)
        {
            var keys = new List<string>();

            if (keyBestFitPos.Any()) return new List<string>();

            var root = keyBestFitPos.First();
            var possibleRootChars = root.Item2;
            foreach (var rootChar in possibleRootChars)
            {
                var combinations = OrderedPermutations(keyBestFitPos
                    .Skip(1)
                    .ToList());

                if (combinations.Any())
                    keys.Add(rootChar.ToString());
                keys.AddRange(combinations.Select(x => rootChar + x));
            }

            return keys;
        }
    }
}