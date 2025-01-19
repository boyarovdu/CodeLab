namespace BloomFilter;

using System;

// For more information check paper called "Space/Time Trade-offs in Hash Coding with Allowable Errors" by BURTON H. BLOOM 
static class BloomFilterCalculator
{
    private static double Log2(double x) => Math.Log(x) / Math.Log(2.0);

    public static double CalculateTotalBitsNumber(int numberOfItems, double errorFraction, double zeroBitsFraction) =>
        numberOfItems * (-1 * Log2(errorFraction)) * (Log2(Math.E) / Log2(zeroBitsFraction) * Log2(1.0 - zeroBitsFraction));
    
    public static double CalculateBitsNumberPerItem(int numberOfItems, double zeroBitsFraction, double totalBitsNumber) =>
        Log2(zeroBitsFraction) *  totalBitsNumber / -numberOfItems * Log2(Math.E);
}