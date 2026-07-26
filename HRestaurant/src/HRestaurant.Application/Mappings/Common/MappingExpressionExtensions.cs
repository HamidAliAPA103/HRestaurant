using AutoMapper;
using HRestaurant.Models.BaseModels;

namespace HRestaurant.Mappings.Common;

internal static class MappingExpressionExtensions
{
    public static IMappingExpression<TSource, TDestination>
        IgnoreBaseEntityMembers<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> mapping)
        where TDestination : BaseEntity
    {
        mapping
            .ForMember(destination => destination.ID, options => options.Ignore())
            .ForMember(destination => destination.CreatAt, options => options.Ignore())
            .ForMember(destination => destination.UpdateAt, options => options.Ignore())
            .ForMember(destination => destination.DeletedAt, options => options.Ignore())
            .ForMember(destination => destination.IsDeleted, options => options.Ignore());

        return mapping;
    }

    public static IMappingExpression<TSource, TDestination>
        IgnoreNullSourceMembers<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> mapping)
    {
        mapping.ForAllMembers(options =>
            options.Condition((_, _, sourceMember) => sourceMember is not null));

        return mapping;
    }
}
