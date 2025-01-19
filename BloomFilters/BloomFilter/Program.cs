namespace BloomFilter;

class Program
{
    static void Main(string[] args)
    {
        const int numberOfItems = 1_000_000; // number of elements in the set
        const double errorFraction = 0.01; // expected fraction of errors
        const double zeroBitsFraction = 0.5; // represent the expected proportion of bits in the hash area of N" bits still set to 0 after n messages have been hash stored
        
        var totalBitsNumber = BloomFilterCalculator.CalculateTotalBitsNumber(numberOfItems, errorFraction, zeroBitsFraction);
        Console.WriteLine($"Total bits number = {totalBitsNumber / 8}");
        
        var bitsPerItem = BloomFilterCalculator.CalculateBitsNumberPerItem(numberOfItems, zeroBitsFraction, totalBitsNumber); // represents number of bits set to 1 in the hash area of N" bits" for each new message
        Console.WriteLine($"Bits number per item = {bitsPerItem}");
        
        var memSample1= GC.GetTotalMemory(true);
        _ = new BloomFilter(numberOfItems, (float)errorFraction);
        var memSample2 = GC.GetTotalMemory(true);

        Console.WriteLine($"Memory acllocated: {memSample2 - memSample1} bytes");
    }
}