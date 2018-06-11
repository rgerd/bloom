using System.Threading.Tasks;
using System;

namespace Bloom
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Bloom Filter");
            BloomFilter filter = new BloomFilter(50);

            filter.Put("first key");
            filter.Put("ok next key");
            filter.Put("first ke");
            filter.Put("first key");

            Task<bool> resultTask1 = filter.MightContain("first ke");
            resultTask1.Wait();
            bool result1 = resultTask1.Result;
            Console.WriteLine("Result 1: " + result1);

            Task<bool> resultTask2 = filter.MightContain("first kes");
            resultTask2.Wait();
            bool result2 = resultTask2.Result;
            Console.WriteLine("Result 2: " + result2);

            Console.WriteLine("Expected FPR: " + filter.ExpectedFdr());

            filter.Put("first keys");

            Task<bool> resultTask3 = filter.MightContain("first kes");
            resultTask3.Wait();
            bool result3 = resultTask3.Result;
            Console.WriteLine("Result 3: " + result3);

            Console.WriteLine("Expected FPR: " + filter.ExpectedFdr());
            
        }
    }
}
