﻿using Moq;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using AutoMapper;
using System;
using Annstore.Core.Common;
using Annstore.Application.Services.Categories;
using Annstore.Application.Models.Admin.Common;
using Annstore.Application.Infrastructure;
using Annstore.Services.Customers;
using Annstore.Core.Entities.Customers;
using Annstore.Application.Models.Admin.Customers;
using Microsoft.AspNetCore.Identity;
using Annstore.Auth.Entities;
using TestHelper;

namespace Annstore.Application.Tests
{
    public class AdminCustomerServiceTests
    {
        #region Helpers
        private Mock<UserManager<Account>> GetDefaultUserManager()
        {
            var accountManagerStub = new Mock<UserManager<Account>>(Mock.Of<IUserStore<Account>>(), null, null, null, null, null, null, null, null);
            return accountManagerStub;
        }

        #endregion

        #region GetCustomerModelAsync
        [Fact]
        public async Task GetCustomerModelAsync_CustomerNotFound_ReturnNullCustomerModel()
        {
            var notFoundCustomerId = 0;
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.GetCustomerByIdAsync(notFoundCustomerId))
                .ReturnsAsync((Customer)null)
                .Verifiable();
            var adminCustomerService = new AdminCustomerService(customerServiceMock.Object, GetDefaultUserManager().Object, Mock.Of<IMapper>());

            var nullCustomerModel = await adminCustomerService.GetCustomerModelAsync(notFoundCustomerId);

            Assert.IsType<NullCustomerModel>(nullCustomerModel);
            customerServiceMock.Verify();
        }

        [Fact]
        public async Task GetCustomerModelAsync_CustomerDeleted_ReturnNullCustomerModel()
        {
            var deletedCustomerId = 1;
            var deletedCustomer = Customer.CreateWithId(deletedCustomerId);
            deletedCustomer.IsDeleted(true);
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.GetCustomerByIdAsync(deletedCustomerId))
                .ReturnsAsync(deletedCustomer)
                .Verifiable();
            var adminCustomerService = new AdminCustomerService(customerServiceMock.Object, GetDefaultUserManager().Object, Mock.Of<IMapper>());

            var nullCustomerModel = await adminCustomerService.GetCustomerModelAsync(deletedCustomerId);

            Assert.IsType<NullCustomerModel>(nullCustomerModel);
            customerServiceMock.Verify();
        }

        [Fact]
        public async Task GetCustomerModelAsync_CustomerNotNull_MapToCustomerModel()
        {
            var id = 1;
            var customer = Customer.CreateWithId(id);
            var model = new CustomerModel { Id = id };
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<CustomerModel>(customer))
                .Returns(model)
                .Verifiable();
            var customerServiceStub = new Mock<ICustomerService>();
            customerServiceStub.Setup(c => c.GetCustomerByIdAsync(id))
                .ReturnsAsync(customer);
            customerServiceStub.Setup(c => c.GetCustomersAsync())
                .ReturnsAsync(new List<Customer>());
            var adminCustomerService = new AdminCustomerService(customerServiceStub.Object, GetDefaultUserManager().Object, mapperMock.Object);

            var result = await adminCustomerService.GetCustomerModelAsync(id);

            Assert.Equal(model, result);
            mapperMock.Verify();
        }
        #endregion

        #region GetCustomerListModelAsync
        [Fact]
        public async Task GetCustomerListModelAsync_ReturnMappedAllCategoriesListModel()
        {
            var customer = Customer.CreateWithId(1);
            var mappedModel = new CustomerSimpleModel { Id = customer.Id };
            var allCategories = new List<Customer> { customer };
            var customerListOptions = new CustomerListOptions { PageNumber = 1, PageSize = int.MaxValue };
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.GetPagedCustomersAsync(customerListOptions.PageNumber, customerListOptions.PageSize))
                .ReturnsAsync(allCategories.ToPagedList(customerListOptions.PageSize, customerListOptions.PageNumber, 12))
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<CustomerSimpleModel>(customer))
                .Returns(mappedModel)
                .Verifiable();
            var adminCustomerService = new AdminCustomerService(customerServiceMock.Object, GetDefaultUserManager().Object, mapperMock.Object);

            var customerListModel = await adminCustomerService.GetCustomerListModelAsync(customerListOptions);

            Assert.Single(customerListModel.Customers);
            Assert.Equal(mappedModel, customerListModel.Customers[0]);
            customerServiceMock.Verify();
            mapperMock.Verify();
        }

        [Fact]
        public async Task GetCustomerListModelAsync_Pagination_ReturnPagedCategories()
        {
            var customer = Customer.CreateWithId(2);
            var mappedModel = new CustomerSimpleModel { Id = customer.Id };
            var allCategories = new List<Customer> { customer };
            var totalItems = 3;
            var customerListOptions = new CustomerListOptions { PageNumber = 2, PageSize = 1 };
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.GetPagedCustomersAsync(customerListOptions.PageNumber, customerListOptions.PageSize))
                .ReturnsAsync(allCategories.ToPagedList(customerListOptions.PageSize, customerListOptions.PageNumber, totalItems))
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<CustomerSimpleModel>(customer))
                .Returns(mappedModel);
            var adminCustomerService = new AdminCustomerService(customerServiceMock.Object, GetDefaultUserManager().Object, mapperStub.Object);

            var customerListModel = await adminCustomerService.GetCustomerListModelAsync(customerListOptions);

            Assert.Equal(3, customerListModel.TotalPages);
            Assert.Equal(3, customerListModel.TotalItems);
            Assert.Equal(2, customerListModel.Customers[0].Id);
            customerServiceMock.Verify();
        }
        #endregion

        #region CreateCustomerAsync
        [Fact]
        public async Task CreateCustomerAsync_RequestIsNull_ThrowArgumentNullException()
        {
            var adminCustomerService = new AdminCustomerService(Mock.Of<ICustomerService>(), GetDefaultUserManager().Object, Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => adminCustomerService.CreateCustomerAsync(null));
        }

        [Fact]
        public async Task CreateCustomerAsync_RequestNotNull_InsertCustomer()
        {
            var customerModel = new CustomerModel();
            var customer = new Customer();
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.CreateCustomerAsync(customer))
                .ReturnsAsync(customer)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<Customer>(customerModel))
                .Returns(customer)
                .Verifiable();
            var adminCustomerService = new AdminCustomerService(customerServiceMock.Object, GetDefaultUserManager().Object, mapperMock.Object);
            var appRequest = new AppRequest<CustomerModel>(customerModel);

            var appResponse = await adminCustomerService.CreateCustomerAsync(appRequest);

            Assert.True(appResponse.Success);
            Assert.Equal(customer, appResponse.Result);
            customerServiceMock.Verify();
            mapperMock.Verify();
        }

        [Fact]
        public async Task CreateCustomerAsync_InsertCustomerError_ReturnErrorResponse()
        {
            var customerModel = new CustomerModel();
            var customer = new Customer();
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.CreateCustomerAsync(customer))
                .Throws<Exception>()
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<Customer>(customerModel))
                .Returns(customer);
            var adminCustomerService = new AdminCustomerService(customerServiceMock.Object, GetDefaultUserManager().Object, mapperStub.Object);
            var appRequest = new AppRequest<CustomerModel>(customerModel);

            var appResponse = await adminCustomerService.CreateCustomerAsync(appRequest);

            Assert.False(appResponse.Success);
            Assert.NotNull(appResponse.Message);
            customerServiceMock.Verify();
        }

        #endregion

        #region UpdateCustomerAsync
        [Fact]
        public async Task UpdateCustomerAsync_RequestIsNull_ThrowArgumentNullException()
        {
            var adminCustomerService = new AdminCustomerService(Mock.Of<ICustomerService>(), GetDefaultUserManager().Object, Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => adminCustomerService.UpdateCustomerAsync(null));
        }

        [Fact]
        public async Task UpdateCustomerAsync_CustomerIsFound_UpdateCustomer()
        {
            var customerModel = new CustomerModel { Id = 1 };
            var customer = Customer.CreateWithId(customerModel.Id);
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.GetCustomerByIdAsync(customerModel.Id))
                .ReturnsAsync(customer);
            customerServiceMock.Setup(c => c.UpdateCustomerAsync(customer))
                .ReturnsAsync(customer)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map(customerModel, customer))
                .Returns(customer)
                .Verifiable();
            var adminCustomerService = new AdminCustomerService(customerServiceMock.Object, GetDefaultUserManager().Object, mapperMock.Object);
            var appRequest = new AppRequest<CustomerModel>(customerModel);

            var appResponse = await adminCustomerService.UpdateCustomerAsync(appRequest);

            Assert.True(appResponse.Success);
            Assert.Equal(customer, appResponse.Result);
            customerServiceMock.Verify();
            mapperMock.Verify();
        }

        [Fact]
        public async Task UpdateCustomerAsync_CustomerNotFound_ReturnModelInvalidResponse()
        {
            var notFoundCustomerId = 0;
            var customerModel = new CustomerModel { Id = notFoundCustomerId };
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.GetCustomerByIdAsync(customerModel.Id))
                .ReturnsAsync((Customer)null)
                .Verifiable();
            var adminCustomerService = new AdminCustomerService(customerServiceMock.Object, GetDefaultUserManager().Object, Mock.Of<IMapper>());
            var appRequest = new AppRequest<CustomerModel>(customerModel);

            var appResponse = await adminCustomerService.UpdateCustomerAsync(appRequest);

            Assert.False(appResponse.Success);
            Assert.True(appResponse.ModelIsInvalid);
            customerServiceMock.Verify();
        }

        [Fact]
        public async Task UpdateCustomerAsync_CustomerIsDeleted_ReturnModelInvalidResponse()
        {
            var deletedCustomerId = 1;
            var deletedCustomer = Customer.CreateWithId(deletedCustomerId);
            deletedCustomer.IsDeleted(true);
            var customerModel = new CustomerModel { Id = deletedCustomerId };
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.GetCustomerByIdAsync(customerModel.Id))
                .ReturnsAsync(deletedCustomer)
                .Verifiable();
            var adminCustomerService = new AdminCustomerService(customerServiceMock.Object, GetDefaultUserManager().Object, Mock.Of<IMapper>());
            var appRequest = new AppRequest<CustomerModel>(customerModel);

            var appResponse = await adminCustomerService.UpdateCustomerAsync(appRequest);

            Assert.False(appResponse.Success);
            Assert.True(appResponse.ModelIsInvalid);
            customerServiceMock.Verify();
        }

        [Fact]
        public async Task UpdateCustomerAsync_UpdateCustomerError_ReturnErrorResponse()
        {
            var customerModel = new CustomerModel { Id = 2 };
            var customer = Customer.CreateWithId(customerModel.Id);
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.GetCustomerByIdAsync(customerModel.Id))
                .ReturnsAsync(customer);
            customerServiceMock.Setup(c => c.UpdateCustomerAsync(customer))
                .Throws<Exception>()
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map(customerModel, customer))
                .Returns(customer);
            var adminCustomerService = new AdminCustomerService(customerServiceMock.Object, GetDefaultUserManager().Object, mapperStub.Object);
            var appRequest = new AppRequest<CustomerModel>(customerModel);

            var appResponse = await adminCustomerService.UpdateCustomerAsync(appRequest);

            Assert.False(appResponse.Success);
            Assert.NotNull(appResponse.Message);
            customerServiceMock.Verify();
        }

        #endregion

        #region DeleteCustomerAsync

        [Fact]
        public async Task DeleteCustomerAsync_RequestIsNull_ThrowArgumentNullException()
        {
            var adminCustomerService = new AdminCustomerService(Mock.Of<ICustomerService>(), GetDefaultUserManager().Object, Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => adminCustomerService.DeleteCustomerAsync(null));
        }

        [Fact]
        public async Task DeleteCustomerAsync_CustomerIsFound_DeleteCustomerAndAccounts()
        {
            var id = 1;
            var customerModel = new CustomerModel { Id = id };
            var customer = Customer.CreateWithId(customerModel.Id);
            var availableAccounts = new List<Account> { new Account { CustomerId = id } };
            var deletedAccountResult = IdentityResult.Success;
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.GetCustomerByIdAsync(customerModel.Id))
                .ReturnsAsync(customer);
            customerServiceMock.Setup(c => c.DeleteCustomerAsync(customer))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(u => u.Users)
                .Returns(availableAccounts.ToAsync())
                .Verifiable();
            userManagerMock.Setup(u => u.DeleteAsync(availableAccounts[0]))
                .ReturnsAsync(deletedAccountResult)
                .Verifiable();
            var adminCustomerService = new AdminCustomerService(customerServiceMock.Object, userManagerMock.Object, Mock.Of<IMapper>());
            var appRequest = new AppRequest<int>(id);

            var appResponse = await adminCustomerService.DeleteCustomerAsync(appRequest);

            Assert.True(appResponse.Success);
            customerServiceMock.Verify();
            userManagerMock.Verify();
        }

        [Fact]
        public async Task DeleteCustomerAsync_CustomerIsDeleted_SkipToSuccess()
        {
            var deletedCustomerId = 1;
            var customerModel = new CustomerModel { Id = deletedCustomerId };
            var deletedCustomer = Customer.CreateWithId(deletedCustomerId);
            deletedCustomer.IsDeleted(true);
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.GetCustomerByIdAsync(customerModel.Id))
                .ReturnsAsync(deletedCustomer);
            customerServiceMock.Setup(c => c.DeleteCustomerAsync(deletedCustomer))
                .Returns(Task.CompletedTask);
            var adminCustomerService = new AdminCustomerService(customerServiceMock.Object, GetDefaultUserManager().Object, Mock.Of<IMapper>());
            var appRequest = new AppRequest<int>(deletedCustomerId);

            var appResponse = await adminCustomerService.DeleteCustomerAsync(appRequest);

            Assert.True(appResponse.Success);
            customerServiceMock.Verify(c => c.DeleteCustomerAsync(deletedCustomer), Times.Never);
        }

        [Fact]
        public async Task DeleteCustomerAsync_CustomerNotFound_ReturnModelInvalidResponse()
        {
            var notFoundCustomerId = 0;
            var customerModel = new CustomerModel { Id = notFoundCustomerId };
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.GetCustomerByIdAsync(customerModel.Id))
                .ReturnsAsync((Customer)null)
                .Verifiable();
            var adminCustomerService = new AdminCustomerService(customerServiceMock.Object, GetDefaultUserManager().Object, Mock.Of<IMapper>());
            var appRequest = new AppRequest<int>(notFoundCustomerId);

            var appResponse = await adminCustomerService.DeleteCustomerAsync(appRequest);

            Assert.False(appResponse.Success);
            Assert.True(appResponse.ModelIsInvalid);
            customerServiceMock.Verify();
        }

        [Fact]
        public async Task DeleteCustomerAsync_DeleteCustomerError_ReturnErrorResponse()
        {
            var id = 2;
            var customerModel = new CustomerModel { Id = id };
            var customer = Customer.CreateWithId(customerModel.Id);
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.GetCustomerByIdAsync(customerModel.Id))
                .ReturnsAsync(customer);
            customerServiceMock.Setup(c => c.DeleteCustomerAsync(customer))
                .Throws<Exception>()
                .Verifiable();
            var adminCustomerService = new AdminCustomerService(customerServiceMock.Object, GetDefaultUserManager().Object, Mock.Of<IMapper>());
            var appRequest = new AppRequest<int>(id);

            var appResponse = await adminCustomerService.DeleteCustomerAsync(appRequest);

            Assert.False(appResponse.Success);
            Assert.NotNull(appResponse.Message);
            customerServiceMock.Verify();
        }

        #endregion
    }
}
