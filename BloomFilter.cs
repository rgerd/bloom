using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System.Text;
using System.Linq;
using System;

namespace Bloom
{
    class BloomFilter
    {
        private BitArray bits;
        private int capacity;
        private int size = 0;
        private static readonly int K = 5;

        /// <summary>
        /// Bloom Filter constructor
        /// </summary>
        /// <param name="capacity">The size of the bloom filter</param>
        public BloomFilter(int capacity)
        {
            this.capacity = capacity;
            bits = new BitArray(capacity);
        }

        /// <summary>
        /// Puts a string in the bloom filter
        /// Note that the size of the bloom filter is only incremented when the bits are actually changed
        /// </summary>
        /// <param name="key">The string to be added</param>
        /// <returns>void</returns>
        public async void Put(string key)
        {
            int[] hashIndices = await GetHashIndices(key);
            bool changed = false;
            lock (bits)
            {
                foreach (int hashIndex in hashIndices)
                {
                    if (!bits[hashIndex])
                        changed = true;
                    bits[hashIndex] = true;
                }
            }
            if (changed)
                size++;
        }

        /// <summary>
        /// Queries the filter for whether a string might be contained
        /// If the result is false, then the string is certainly not contained in the bloom filter
        /// If the result is true, then there is a chance the result is really not contained in the bloom filter
        /// See ExpectedFdr() method for more on false positives
        /// </summary>
        /// <param name="key">The string to check</param>
        /// <returns>A boolean, whether the filter has a chance of containing the given string</returns>
        public async Task<bool> MightContain(string key)
        {
            int[] hashIndices = await GetHashIndices(key);
            foreach (int hashIndex in hashIndices)
                lock (bits)
                    if (!bits[hashIndex])
                        return false;
            return true;
        }

        /// <summary>
        /// Calculates the expected false drop rate for the bloom filter
        /// </summary>
        /// <returns>The likely proportion of false positives returned by the filter</returns>
        public double ExpectedFdr()
        {
            return Math.Pow(1 - Math.Pow(1 - 1.0 / capacity, 1.0 * K * size), 1.0 * K);
        }

        private async Task<int[]> GetHashIndices(string key)
        {
            int[] hashIndices = new int[K];
            for (int k = 0; k < K; k++)
            {
                hashIndices[k] = await Hash(key, k) % capacity;
            }
            return hashIndices;
        }

        private async Task<int> Hash(string key, int hashFuncIndex)
        {
            // Hashing keys has been made asynchronous because it is an expensive operation
            byte[] hash = await Task.Run((() => SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key))));
            int hashNum = Math.Abs(BitConverter.ToInt32(hash, hashFuncIndex));
            return hashNum;
        }
    }
}
