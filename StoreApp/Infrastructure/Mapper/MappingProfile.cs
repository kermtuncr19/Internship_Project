using AutoMapper;
using Entities.Dto;
using Entities.Models;
using Microsoft.AspNetCore.Identity;

namespace StoreApp.Infrastructure.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ProductDtoForInsertion, Product>();
            CreateMap<ProductDtoForUpdate, Product>().ReverseMap(); // tersinden de eşleme yapabilmek için reverse map kullandık.
            CreateMap<UserDtoForCreation, IdentityUser>();
            CreateMap<UserDtoForUpdate, IdentityUser>().ReverseMap();
            
            
        }
    }
}