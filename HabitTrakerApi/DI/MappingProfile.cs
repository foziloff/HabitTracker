using AutoMapper;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Models.DTO;

namespace HabitTrakerApi.DI;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Habit, HabitDto>();
    }

}