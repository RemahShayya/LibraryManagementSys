using AutoMapper;
using LibraryManagementSystem.API.DTO;
using LibraryManagementSystem.Data.DTO.Requests;
using LibraryManagementSystem.Data.Entities.ImageEntities;
using LibraryManagmentSystem.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Data.DTO.AutoMapper
{
    public class LibraryAutoMapperProfile : Profile
    {
        public LibraryAutoMapperProfile()
        {
            CreateMap<Book, BookDTO>();
            CreateMap<CreatedBookRequest, Book>();
            CreateMap<Author, AuthorDTO>();
            CreateMap<CreateAuthorRequest, Author>();
            CreateMap<CreateCategoryRequest, Category>();
            CreateMap<Publisher, PublisherDTO>();
            CreateMap<Category, CategoryDTO>();
            CreateMap<CreatePublisherRequest, Publisher>();
            CreateMap<BookImages, ImageBookDTO>();
            CreateMap<AuthorImage, ImageAuthorDTO>();
        }
    }
}
