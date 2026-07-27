using HRestaurant.DTOS.Table;

namespace HRestaurant.Services.Interfaces;

public interface ITableService :
    ICrudService<TableCreateDTO, TableUpdateDTO, TableGetDTO>;
