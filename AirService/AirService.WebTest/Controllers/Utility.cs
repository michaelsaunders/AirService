using System;
using System.Text;

namespace AirService.WebTest.Controllers
{
    public static class Utility
    {
        static Utility()
        {
            Random = new Random(DateTime.Now.Millisecond);
        }

        public static Random Random
        {
            get;
            private set;
        }

        public static string GetRandomString(int size, int? maxSize = null)
        {
            var value = new StringBuilder();
            if(maxSize.HasValue && maxSize.Value > size)
            {
                size = Random.Next(size, maxSize.Value);
            }

            for (int i = 0; i < size; i++)
            {
                value.Append((char) Random.Next('A',
                                                'z'));
            }

            return value.ToString();
        }

        public static string GetRandomEmail()
        {
            return string.Format("{0}@{1}.com.au", GetRandomString(10), GetRandomString(10));
        }
    }
}