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
        [ExpectedException(typeof(ArgumentException), "�� ����� ������ ������!")]
        public void TestEmpty()
        {
            string romanNumber = string.Empty;
            
            RomanToInteger.Calculate(romanNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "������������ ������� �����! ��������� �������� �������")]
        public void TestInvalidRomanNumber()
        {
            string romanNumber = "�����";

            RomanToInteger.Calculate(romanNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "�������� ������ - null")]
        public void TestNullArgument()
        {
            string romanNumber = null!;

            RomanToInteger.Calculate(romanNumber);
        }
    }
}