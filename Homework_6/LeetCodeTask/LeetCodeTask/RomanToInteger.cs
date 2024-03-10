// Задача с LeetCode. Написал для неё тесты.
// Нужно создать класс, который преобразует римское число в арабское.

namespace LeetCodeTask
{
    public static class RomanToInteger
    {
        private static List<char> romanAlphabet = new(){ 'I', 'V', 'X', 'L', 'C', 'D', 'M' };
        public static int Calculate(string? romanNumber)
        {
            if(romanNumber is null)
            {
                throw new ArgumentNullException("Аргумент метода - null");
            }
            if(romanNumber == string.Empty)
            {
                throw new ArgumentException("Вы ввели пустую строку!");
            }
            foreach(var letter in romanNumber)
            {
                if(!romanAlphabet.Contains(letter))
                {
                    throw new ArgumentException("Некорректное римское число! Проверьте аргумент функции");
                }
            }
            char[] romanDigits = romanNumber.ToCharArray();
            int result = 0;
            for (int i = romanDigits.Length - 1; i >= 0; --i)
            {
                char currDigit;
                char lastDigit;
                if (i == 0)
                {
                    lastDigit = ' ';
                }
                else
                {
                    lastDigit = romanDigits[i - 1];
                }
                currDigit = romanDigits[i];
                int tempNumber = 0;
                switch (currDigit)
                {
                    case 'I': { tempNumber = 1; break; }
                    case 'V': { tempNumber = 5; break; }
                    case 'X': { tempNumber = 10; break; }
                    case 'L': { tempNumber = 50; break; }
                    case 'C': { tempNumber = 100; break; }
                    case 'D': { tempNumber = 500; break; }
                    case 'M': { tempNumber = 1000; break; }
                    default: { break; }
                }
                if (lastDigit == 'I' && (currDigit == 'V' || currDigit == 'X'))
                {
                    tempNumber -= 1;
                    --i;
                }
                if (lastDigit == 'X' && (currDigit == 'L' || currDigit == 'C'))
                {
                    tempNumber -= 10;
                    --i;
                }
                if (lastDigit == 'C' && (currDigit == 'D' || currDigit == 'M'))
                {
                    tempNumber -= 100;
                    --i;
                }
                result += tempNumber;
            }
            return result;
        }
    }
}
