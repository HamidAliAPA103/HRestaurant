using AutoMapper;
using HRestaurant.DTOS.Menu;
using HRestaurant.DTOS.Responses;
using HRestaurant.Extentions;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class MenuService :
    CrudServiceBase<
        Menu,
        MenuCreateDTO,
        MenuUpdateDTO,
        MenuGetDTO>,
    IMenuService
{
    private readonly IWebHostEnvironment _web;
    private readonly IHttpContextAccessor _accessor;

    public MenuService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IWebHostEnvironment web,
        IHttpContextAccessor accessor)
        : base(unitOfWork, mapper, "Menu item")
    {
        ArgumentNullException.ThrowIfNull(web);
        ArgumentNullException.ThrowIfNull(accessor);

        _web = web;
        _accessor = accessor;
    }

    public override async Task<ApiResponse<Guid>> CreateAsync(
        MenuCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(dto.Image);

        var imageName = await dto.Image.CreateFileAsync(
            cancellationToken,
            _web.WebRootPath,
            "images",
            "menus");

        try
        {
            var menuItem = Mapper.Map<Menu>(dto);
            menuItem.Image = imageName;
            menuItem.ImageURL = BuildImageUrl(imageName);

            await Repository.AddAsync(menuItem, cancellationToken);
            var saveCount = await UnitOfWork.SaveChangesAsync(
                cancellationToken);

            if (saveCount <= 0)
            {
                imageName.DeleteFile(
                    _web.WebRootPath,
                    "images",
                    "menus");

                return ApiResponse.PersistenceFailure<Guid>();
            }

            return ApiResponse.Created(
                menuItem.ID,
                "Menu item created successfully.");
        }
        catch
        {
            imageName.DeleteFile(_web.WebRootPath, "images", "menus");
            throw;
        }
    }

    public override async Task<ApiResponse<object?>> RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var menuItem = await Repository.GetByIdAsync(
            id,
            cancellationToken);

        if (menuItem is null)
        {
            return ApiResponse.NotFound<object?>(ResourceName);
        }

        Repository.Delete(menuItem);
        var saveCount = await UnitOfWork.SaveChangesAsync(cancellationToken);

        if (saveCount <= 0)
        {
            return ApiResponse.PersistenceFailure<object?>();
        }

        menuItem.Image.DeleteFile(
            _web.WebRootPath,
            "images",
            "menus");

        return ApiResponse.NoContent("Menu item deleted successfully.");
    }

    public override async Task<ApiResponse<object?>> UpdateAsync(
        Guid id,
        MenuUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var menuItem = await Repository
            .GetQueryable()
            .FirstOrDefaultAsync(
                item => !item.IsDeleted && item.ID == id,
                cancellationToken);

        if (menuItem is null)
        {
            return ApiResponse.NotFound<object?>(ResourceName);
        }

        var previousImage = menuItem.Image;
        string? newImage = null;

        Mapper.Map(dto, menuItem);

        try
        {
            if (dto.Image is not null)
            {
                newImage = await dto.Image.CreateFileAsync(
                    cancellationToken,
                    _web.WebRootPath,
                    "images",
                    "menus");
                menuItem.Image = newImage;
                menuItem.ImageURL = BuildImageUrl(newImage);
            }

            menuItem.UpdateAt = DateTime.UtcNow;

            Repository.Update(menuItem);
            var saveCount = await UnitOfWork.SaveChangesAsync(
                cancellationToken);

            if (saveCount <= 0)
            {
                newImage?.DeleteFile(
                    _web.WebRootPath,
                    "images",
                    "menus");

                return ApiResponse.PersistenceFailure<object?>();
            }
        }
        catch
        {
            newImage?.DeleteFile(
                _web.WebRootPath,
                "images",
                "menus");
            throw;
        }

        if (newImage is not null)
        {
            previousImage.DeleteFile(
                _web.WebRootPath,
                "images",
                "menus");
        }

        return ApiResponse.Success("Menu item updated successfully.");
    }

    private string BuildImageUrl(string imageName)
    {
        var request = _accessor.HttpContext?.Request
            ?? throw new InvalidOperationException(
                "The current HTTP request is not available.");

        return $"{request.Scheme}://{request.Host}/images/menus/{imageName}";
    }
}
