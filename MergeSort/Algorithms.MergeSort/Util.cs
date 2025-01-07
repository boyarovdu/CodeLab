namespace Algorithms.MergeSort;

internal static class Util
{
    internal static int ToNearestLowerPowerOfTwo(int number)
    {
        if ((number & (number - 1)) == 0) 
            return number;

        number |= (number >> 1);
        number |= (number >> 2);
        number |= (number >> 4);
        number |= (number >> 8);
        number |= (number >> 16);

        return number - (number >> 1);
    }
}