namespace UserAccounts.Service.Mappers
{
    using AutoMapper;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UserAccounts.Domain.Models;
    using UserAccounts.Service.Models;

    public class UserAccountProfile : Profile
    {
        public UserAccountProfile()
        {
            CreateMap<User, UserModel>()
                .ReverseMap();
        }
    }
}
