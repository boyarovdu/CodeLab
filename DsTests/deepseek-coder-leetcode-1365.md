```
Solve coding problem for me using C#: Given the array nums, for each nums[i] find out how many numbers in the array
... are smaller than it. That is, for each nums[i] you have to count the number of valid j's such that j != i and nums[j
... ] < nums[i].
...
... Return the answer in an array.
...
...
...
... Example 1:
...
... Input: nums = [8,1,2,2,3]
... Output: [4,0,1,1,3]
... Explanation:
... For nums[0]=8 there exist four smaller numbers than it (1, 2, 2 and 3).
... For nums[1]=1 does not exist any smaller number than it.
... For nums[2]=2 there exist one smaller number than it (1).
... For nums[3]=2 there exist one smaller number than it (1).
... For nums[4]=3 there exist three smaller numbers than it (1, 2 and 2).
... Example 2:
...
... Input: nums = [6,5,4,8]
... Output: [2,1,0,3]
... Example 3:
...
... Input: nums = [7,7,7,7]
... Output: [0,0,0,0]
...
...
... Constraints:
...
... 2 <= nums.length <= 500
... 0 <= nums[i] <= 100
```

Here is a C# solution for the problem using brute force approach to solve it in O(n^2) time complexity where n is
the length of the array. It iterates over each element, and then again over the whole array to count how many
numbers are less than it.

```Csharp
public int[] SmallerNumbersThanCurrent(int[] nums) {
    int[] result = new int[nums.Length];

    for (int i = 0; i < nums.Length; i++){
        foreach(var item in nums) {
            if(item != nums[i] && item < nums[i])
                result[i] += 1;
        }
    }

    return result;
}
```
This solution can be optimized to O(n) time complexity by using a sorting algorithm and then finding the position
of each element in the sorted array. Here is the optimized version:

```Csharp
public int[] SmallerNumbersThanCurrentOptimized(int[] nums) {
    var sorted = new int[nums.Length];
    Array.Copy(nums, 0, sorted, 0, nums.Length);
    Array.Sort(sorted);

    Dictionary<int, int> dict = new Dictionary<int, int>();
    for (var i = 0; i < sorted.Length; i++) {
        if (!dict.ContainsKey(sorted[i]))
            dict[sorted[i]] = i; // Record the smallest index which is not zero when duplicates exist
    }

    int[] result = new int[nums.Length];
    for (int i = 0; i < nums.Length; i++) {
        if(dict.ContainsKey(nums[i])){
            // The dict value represents the smallest index of that number in sorted array not including zero when
duplicates exist
            result[i] = dict[nums[i]];
        } else {
             // If num does not exists in dictionary, it must be a unique number and its position would be the
length of the array. So assign this as count to ensure none are smaller than it.
              result[i] = 0;
         }
    }

    return result;
}
```
Note: If you have multiple duplicates in your nums, both solutions will provide different results for those cases,
the first one considers all numbers as unique and gives count including itself while second one only counts once
considering it to be smallest number among its duplicates.