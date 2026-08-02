using AutoMapper;
using HRestaurant.Mappings.Restaurants;
using Microsoft.Extensions.Logging.Abstractions;

namespace HRestaurant.Tests.Mapping;

public sealed class ManagementMappingTests
{
    [Fact]
    public void ApplicationProfiles_AreValid()
    {
        var configuration = new MapperConfiguration(
            expression => expression.AddMaps(
                typeof(RestaurantProfile).Assembly),
            NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }
}
