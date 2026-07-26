using AutoMapper;
using HRestaurant.DTOS.Menu;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Extentions;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class MenuService : IMenuService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _web;
    private readonly IHttpContextAccessor _accessor;

    public MenuService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IWebHostEnvironment web,
        IHttpContextAccessor accessor)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(web);
        ArgumentNullException.ThrowIfNull(accessor);

        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _web = web;
        _accessor = accessor;
    }

    public async Task<ApiResponse> CreateAsync(
        MenuCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var imageName = await dto.Image.CreateFileAsync(
            cancellationToken,
            _web.WebRootPath,
            "images",
            "menus");

        var menuItem = _mapper.Map<Menu>(dto);
        menuItem.Image = imageName;
        menuItem.ImageURL = BuildImageUrl(imageName);

        await _unitOfWork.MenuItems.AddAsync(
            menuItem,
            cancellationToken);

        var saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? new ApiResponse
            {
                StatusCode = 201,
                Message = "Created successfully!"
            }
            : new ApiResponse
            {
                StatusCode = 500,
                Message = "Save failed!"
            };
    }

    public async Task<ApiResponse> GetAllAsync(
        ViewType type,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.MenuItems.GetQueryable();

        query = type switch
        {
            ViewType.deleted => query.Where(entity => entity.IsDeleted),
            ViewType.notdeleted => query.Where(entity => !entity.IsDeleted),
            _ => query
        };

        var menuItems = await query.ToListAsync(cancellationToken);
        var dtos = _mapper.Map<List<MenuGetDTO>>(menuItems);

        return new ApiResponse
        {
            StatusCode = 200,
            Data = dtos,
            Message = $"Total: {dtos.Count}"
        };
    }

    public async Task<ApiResponse> GetByID(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var menuItem = await _unitOfWork.MenuItems
            .GetQueryable()
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ID == id,
                cancellationToken);

        if (menuItem is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Menu not found!"
            };
        }

        return new ApiResponse
        {
            StatusCode = 200,
            Data = _mapper.Map<MenuGetDTO>(menuItem)
        };
    }

    public async Task<ApiResponse> RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var menuItem = await _unitOfWork.MenuItems.GetByIdAsync(
            id,
            cancellationToken);

        if (menuItem is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Menu not found!"
            };
        }

        _unitOfWork.MenuItems.Delete(menuItem);
        var saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveCount <= 0)
        {
            return new ApiResponse
            {
                StatusCode = 500,
                Message = "Save failed!"
            };
        }

        menuItem.Image.DeleteFile(_web.WebRootPath, "images", "menus");

        return new ApiResponse
        {
            StatusCode = 204,
            Message = "Deleted successfully!"
        };
    }

    public async Task<ApiResponse> ToggleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var menuItem = await _unitOfWork.MenuItems.GetByIdAsync(
            id,
            cancellationToken);

        if (menuItem is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Menu not found!"
            };
        }

        menuItem.IsDeleted = !menuItem.IsDeleted;
        menuItem.DeletedAt = menuItem.IsDeleted ? DateTime.UtcNow : null;
        menuItem.UpdateAt = DateTime.UtcNow;

        _unitOfWork.MenuItems.Update(menuItem);
        var saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? menuItem.IsDeleted
                ? new ApiResponse
                {
                    StatusCode = 204,
                    Message = "Deleted temporarily!"
                }
                : new ApiResponse
                {
                    StatusCode = 200,
                    Message = "Restored successfully!"
                }
            : new ApiResponse
            {
                StatusCode = 500,
                Message = "Save failed!"
            };
    }

    public async Task<ApiResponse> UpdateAsync(
        Guid id,
        MenuUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var menuItem = await _unitOfWork.MenuItems.GetByIdAsync(
            id,
            cancellationToken);

        if (menuItem is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Menu tapılmadı"
            };
        }

        string? previousImage = null;

        _mapper.Map(dto, menuItem);

        if (dto.Image is not null)
        {
            previousImage = menuItem.Image;
            menuItem.Image = await dto.Image.CreateFileAsync(
                cancellationToken,
                _web.WebRootPath,
                "images",
                "menus");
            menuItem.ImageURL = BuildImageUrl(menuItem.Image);
        }
        menuItem.UpdateAt = DateTime.UtcNow;

        _unitOfWork.MenuItems.Update(menuItem);
        var saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveCount <= 0)
        {
            return new ApiResponse
            {
                StatusCode = 500,
                Message = "Save uğursuz oldu!"
            };
        }

        previousImage?.DeleteFile(_web.WebRootPath, "images", "menus");

        return new ApiResponse
        {
            StatusCode = 200,
            Message = "Uğurla yeniləndi!"
        };
    }

    private string BuildImageUrl(string imageName)
    {
        var request = _accessor.HttpContext?.Request
            ?? throw new InvalidOperationException(
                "The current HTTP request is not available.");

        return $"{request.Scheme}://{request.Host}/images/menus/{imageName}";
    }

}
