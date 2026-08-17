using System.Text;

namespace Pos.Helpers
{
    public static class SoundexHelper
    {
        public static string GenerateSoundex(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            text = text.ToUpperInvariant();
            var result = new StringBuilder();
            var previousCode = '0';

            result.Append(text[0]);

            var soundexMap = new Dictionary<char, char>
            {
                {'B', '1'}, {'F', '1'}, {'P', '1'}, {'V', '1'},
                {'C', '2'}, {'G', '2'}, {'J', '2'}, {'K', '2'},
                {'Q', '2'}, {'S', '2'}, {'X', '2'}, {'Z', '2'},
                {'D', '3'}, {'T', '3'},
                {'L', '4'},
                {'M', '5'}, {'N', '5'},
                {'R', '6'}
            };

            for (int i = 1; i < text.Length && result.Length < 4; i++)
            {
                var currentChar = text[i];
                if (soundexMap.TryGetValue(currentChar, out var code))
                {
                    if (code != previousCode)
                    {
                        result.Append(code);
                        previousCode = code;
                    }
                }
                else
                {
                    previousCode = '0';
                }
            }

            while (result.Length < 4)
            {
                result.Append('0');
            }

            return result.ToString();
        }

        public static bool IsSimilarPassword(string newPassword, string oldPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(oldPassword))
                return false;

            var newSoundex = GenerateSoundex(newPassword);
            var oldSoundex = GenerateSoundex(oldPassword);

            return newSoundex == oldSoundex;
        }
    }
}