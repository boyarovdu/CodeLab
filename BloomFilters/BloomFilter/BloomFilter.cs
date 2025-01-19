﻿using System.Collections;
using System.Text;

namespace BloomFilter;

public class BloomFilter
{
    private readonly BitArray _bitsArrea;
    private readonly int _bitsPerItem;

    public BloomFilter(int capcity, double errorRate)
    {
        var totalNumberOfBits = (int)Math.Ceiling(
            BloomFilterCalculator.CalculateTotalBitsNumber(
                numberOfItems: capcity, 
                errorFraction: errorRate, 
                zeroBitsFraction: 0.5));
        
        _bitsPerItem = (int)Math.Round(
            BloomFilterCalculator.CalculateBitsNumberPerItem(
                numberOfItems: capcity, 
                zeroBitsFraction: 0.5, 
                totalBitsNumber: totalNumberOfBits)) / 2;
        
        _bitsArrea = new BitArray(totalNumberOfBits);
    }
    
    private int DoubleHash(int primaryHash, int secondaryHash, int iter)
    {
        return Math.Abs((primaryHash + iter * secondaryHash) % _bitsArrea.Length) ;
    }
    
    public void Add(string item)
    {
        var primaryHash = item.GetHashCode();
        var secondaryHash = JenkinsHash(item);
        
        for (var i = 0; i < _bitsPerItem; i++)
        {
            var hash = DoubleHash(primaryHash, secondaryHash, i);
            _bitsArrea[hash] = true;
        }
    }

    public bool Contains(string item)
    {
        var primaryHash = item.GetHashCode();
        var secondaryHash = JenkinsHash(item);
        
        for (var i = 0; i < _bitsPerItem; i++)
        {
            var hash = DoubleHash(primaryHash, secondaryHash, i);
            if (_bitsArrea[hash] == false)
            {
                return false;
            }
        }

        return true;
    }

    /*--------------------------------------------------------------------
      --- The following code represents implementation of Bob Jenkins' ---
      --- One-at-a-Time Hashing algorithm generated by famous LLM AI -----
      --------------------------------------------------------------------*/
    
    private int JenkinsHash(string input)
    {
        uint hash = 0;

        foreach (char c in input)
        {
            hash += c;
            hash += (hash << 10); // Left-shift hash by 10 and add
            hash ^= (hash >> 6);  // XOR hash with the right-shifted value by 6
        }

        hash += (hash << 3);  // Left-shift hash by 3 and add
        hash ^= (hash >> 11); // XOR hash with the right-shifted value by 11
        hash += (hash << 15); // Left-shift hash by 15 and add

        // Final step: cast to int and ensure it's non-negative
        return (int)(hash & 0x7FFFFFFF);
    }

    /*----------------------------------------------------------------------*/
}