using System.Text;

namespace Annstore.Core.Common
{
    public sealed class StringHelper : IStringHelper
    {
        private static readonly string[] _vietnameseChars = new string[]
        {
            "aAeEoOuUiIdDyY",
            "áàạảãâấầậẩẫăắằặẳẵ",
            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
            "éèẹẻẽêếềệểễ",
            "ÉÈẸẺẼÊẾỀỆỂỄ",
            "óòọỏõôốồộổỗơớờợởỡ",
            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
            "úùụủũưứừựửữ",
            "ÚÙỤỦŨƯỨỪỰỬỮ",
            "íìịỉĩ",
            "ÍÌỊỈĨ",
            "đ",
            "Đ",
            "ýỳỵỷỹ",
            "ÝỲỴỶỸ"
        };

        public string TransformVietnameseToAscii(string source)
        {
            if (string.IsNullOrEmpty(source))
                return source;

            var builder = new StringBuilder(source);
            var targetChars = _vietnameseChars[0];
            for (int charBlockIndex = 1; charBlockIndex < _vietnameseChars.Length; charBlockIndex++)
            {
                for (int charIndex = 0; charIndex < _vietnameseChars[charBlockIndex].Length; charIndex++)
                {
                    var vnChar = _vietnameseChars[charBlockIndex][charIndex];
                    if (source.IndexOf(vnChar) != -1)
                    {
                        builder.Replace(vnChar, targetChars[charBlockIndex - 1]);
                    }
                }
            }
            return builder.ToString();
        }
    }
}
