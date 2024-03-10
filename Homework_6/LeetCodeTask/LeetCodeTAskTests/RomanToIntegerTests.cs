using LeetCodeTask;

namespace LeetCodeTaskTests
{
    [TestClass]
    public class RomanToIntegerTests
    {
        [TestMethod]
        public void TestCorrectness()
        {
            string romanNumber = "LVIII";

            int expected = 58;

            int actual = RomanToInteger.Calculate(romanNumber);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Вы ввели пустую строку!")]
        public void TestEmpty()
        {
            string romanNumber = string.Empty;
            
            RomanToInteger.Calculate(romanNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Некорректное римское число! Проверьте аргумент функции")]
        public void TestInvalidRomanNumber()
        {
            string romanNumber = "АБВГД";

            RomanToInteger.Calculate(romanNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Аргумент метода - null")]
        public void TestNullArgument()
        {
            string romanNumber = null!;

            RomanToInteger.Calculate(romanNumber);
        }
    }
}