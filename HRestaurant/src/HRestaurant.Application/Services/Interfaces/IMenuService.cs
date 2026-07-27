using HRestaurant.DTOS.Menu;

namespace HRestaurant.Services.Interfaces;

public interface IMenuService :
    ICrudService<MenuCreateDTO, MenuUpdateDTO, MenuGetDTO>;
