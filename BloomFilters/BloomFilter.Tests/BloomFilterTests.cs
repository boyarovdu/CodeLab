namespace BloomFilter.Tests;

[TestFixture]
public class BloomFilterTests
{
    private const int Capacity = 1_000_000;
    private const double ErrorRate = 0.001;

    [Test]
    public void False_Positives_are_not_Allowed_for_Existing_Items()
    {
        var filter = new BloomFilter(Capacity, ErrorRate);

        var insertedItems = new List<string>();
        for (var i = 0; i < Capacity; i++)
        {
            var item = $"item{i}";
            filter.Add(item);
            insertedItems.Add(item);
        }

        foreach (var item in insertedItems)
        {
            Assert.That(filter.Contains(item), Is.True, $"False negative detected for item: {item}");
        }
    }

    [Test]
    public void Error_Rate_Stays_Within_Acceptable_Range()
    {
        var filter = new BloomFilter(Capacity, ErrorRate);

        for (var i = 0; i < Capacity; i++)
        {
            var item = $"item{i}";
            filter.Add(item);
        }

        var falsePositiveCount = 0;
        var attempts = Capacity;
        for (var i = 0; i < attempts; i++)
        {
            var item = $"missing_item{i}";
            if (filter.Contains(item))
            {
                falsePositiveCount++;
            }
        }

        var measuredFalsePositiveRate = (double)falsePositiveCount / attempts;

        const double tolerance = 0.1;
        Assert.That(measuredFalsePositiveRate, Is.InRange(ErrorRate * (1 - tolerance), ErrorRate * (1 + tolerance)),
            $"Measured false positive rate {measuredFalsePositiveRate} is outside expected range.");
    }
}