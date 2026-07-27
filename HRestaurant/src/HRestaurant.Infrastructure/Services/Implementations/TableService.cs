using AutoMapper;
using HRestaurant.DTOS.Table;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;

namespace HRestaurant.Services.Implementations;

public sealed class TableService :
    CrudServiceBase<
        Table,
        TableCreateDTO,
        TableUpdateDTO,
        TableGetDTO>,
    ITableService
{
    public TableService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(unitOfWork, mapper, "Table")
    {
    }
}
