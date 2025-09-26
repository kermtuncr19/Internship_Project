using AutoMapper;
using Entities.Dto;
using Entities.Models;

namespace StoreApp.Infrastructure.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ProductDtoForInsertion, Product>();
            CreateMap<ProductDtoForUpdate, Product>().ReverseMap(); // tersinden de eşleme yapabilmek için reverse map kullandık.
            
        }
    }
}