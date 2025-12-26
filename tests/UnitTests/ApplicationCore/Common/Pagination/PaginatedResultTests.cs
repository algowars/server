using ApplicationCore.Common.Pagination;

namespace UnitTests.ApplicationCore.Common.Pagination;

[TestFixture]
public sealed class PaginatedResultTests
{
    [Test]
    public void HasPrevious_is_false_on_first_page()
    {
        var result = new PaginatedResult<int>
        {
            Results = [1, 2],
            Total = 10,
            Page = 1,
            Size = 2,
        };

        Assert.That(result.HasPrevious, Is.False);
    }

    [Test]
    public void HasPrevious_is_true_when_page_greater_than_one()
    {
        var result = new PaginatedResult<int>
        {
            Results = [3, 4],
            Total = 10,
            Page = 2,
            Size = 2,
        };

        Assert.That(result.HasPrevious, Is.True);
    }

    [Test]
    public void HasNext_is_true_when_not_on_last_page()
    {
        var result = new PaginatedResult<int>
        {
            Results = [1, 2],
            Total = 10,
            Page = 1,
            Size = 2,
        };

        Assert.That(result.HasNext, Is.True);
    }

    [Test]
    public void HasNext_is_false_on_last_page()
    {
        var result = new PaginatedResult<int>
        {
            Results = [9, 10],
            Total = 10,
            Page = 5,
            Size = 2,
        };

        Assert.That(result.HasNext, Is.False);
    }

    [Test]
    public void Offset_is_calculated_correctly()
    {
        var result = new PaginatedResult<int>
        {
            Results = [5, 6],
            Total = 10,
            Page = 3,
            Size = 2,
        };

        Assert.That(result.Offset, Is.EqualTo(4));
    }

    [Test]
    public void HasNext_is_false_when_size_is_zero()
    {
        var result = new PaginatedResult<int>
        {
            Results = [],
            Total = 10,
            Page = 1,
            Size = 0,
        };

        Assert.Multiple(() =>
        {
            Assert.That(result.HasNext, Is.False);
            Assert.That(result.Offset, Is.EqualTo(0));
        });
    }

    [Test]
    public void HasNext_is_false_when_total_is_zero()
    {
        var result = new PaginatedResult<int>
        {
            Results = [],
            Total = 0,
            Page = 1,
            Size = 10,
        };

        Assert.Multiple(() =>
        {
            Assert.That(result.HasNext, Is.False);
            Assert.That(result.HasPrevious, Is.False);
        });
    }
}
