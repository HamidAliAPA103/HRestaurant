using AutoMapper;
using HRestaurant.Mappings.Branches;
using HRestaurant.Mappings.Restaurants;
using Microsoft.Extensions.Logging.Abstractions;

namespace HRestaurant.Tests.Mapping;

public sealed class ManagementMappingTests
{
    [Fact]
    public void RestaurantAndBranchProfiles_AreValid()
    {
        var configuration = new MapperConfiguration(
            expression =>
            {
                expression.AddProfile<RestaurantProfile>();
                expression.AddProfile<BranchProfile>();
            },
            NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }
}
