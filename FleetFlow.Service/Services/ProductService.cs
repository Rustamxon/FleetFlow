﻿using AutoMapper;
using FleetFlow.DAL.IRepositories;
using FleetFlow.Domain.Entities;
using FleetFlow.Service.DTOs;
using FleetFlow.Service.Exceptions;
using FleetFlow.Service.Interfaces;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace FleetFlow.Service.Services
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<Product> CreatAsync(ProductCreationDto creationDto)
        {
            var product = await this._unitOfWork.Products.SelectAsync(srn => srn.Serial == creationDto.Serial);
            if (product is not null)
                throw new FleetFlowException(409, "Product Already exists");

            try
            {
                var mapped = this._mapper.Map<Product>(product);
                mapped.CreatedAt = DateTime.UtcNow;
                var restored = await this._unitOfWork.Products.InsertAsync(mapped);
                await this._unitOfWork.SaveChangesAsync();

                return restored;
            }
            catch
            {
                throw new FleetFlowException();
            }
        }

        public async Task<bool> DeleteAsync(Expression<Func<Product, bool>> expression)
        {
            Product product = await this._unitOfWork.Products.SelectAsync(expression);
            if (product is null)
                throw new FleetFlowException(404, "Couldn't find product for this given Id");

            bool isDeleted = await this._unitOfWork.Products.DeleteAsync(expression);
            await this._unitOfWork.SaveChangesAsync();

            return isDeleted;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            var products = this._unitOfWork.Products.SelectAll();

            if (products is null)
                throw new FleetFlowException(404, "Product not found");

            return products;

        }

        public async Task<Product> GetByIdAsync(Expression<Func<Product, bool>> expression)
        {
            Product product = await this._unitOfWork.Products.SelectAsync(expression);

            if (product is null)
                throw new FleetFlowException(404, "Product Not Found");

            return product;
        }

        public async Task<Product> UpdateAsync(long id, ProductCreationDto productDto)
        {
            Product product =
                await this._unitOfWork.Products.SelectAsync((p) => p.Id == id);

            if (product is null)
                throw new FleetFlowException(404, "Couldn't found product for given Id");

            var mapped = this._mapper.Map<Product>(productDto);

            var updated  = await this._unitOfWork.Products.UpdateAsync(mapped);
            await this._unitOfWork.SaveChangesAsync();

            return updated;
            
        }
    }
}
