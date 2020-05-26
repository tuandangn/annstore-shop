using System.Threading.Tasks;
using Annstore.Application.Infrastructure;
using Annstore.Application.Infrastructure.Settings;
using Annstore.Application.Models.Admin.Common;
using Annstore.Application.Models.Admin.Customers;
using Annstore.Application.Services.Customers;
using Annstore.Core.Entities.Customers;
using Annstore.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Annstore.Web.Tests.Admin.Controllers
{
    public class CustomerControllerTests
    {
        #region Index
        [Fact]
        public void Index_RedirectToList()
        {
            var customerController = new CustomerController(Mock.Of<IAdminCustomerService>(), Mock.Of<IOptionsSnapshot<CustomerSettings>>());

            var redirectResult = customerController.Index();

            var listRedirectResult = Assert.IsType<RedirectToActionResult>(redirectResult);
            Assert.Equal(nameof(CustomerController.List), listRedirectResult.ActionName);
        }
        #endregion

        #region List
        [Fact]
        public async Task List_ReturnValidModel()
        {
            var customerListModel = new CustomerListModel();
            var customerSettings = new CustomerSettings();
            var adminCustomerServiceMock = new Mock<IAdminCustomerService>();
            adminCustomerServiceMock.Setup(c => c.GetCustomerListModelAsync(It.IsAny<CustomerListOptions>()))
                .ReturnsAsync(customerListModel)
                .Verifiable();
            var customerSettingsSnapshopStub = new Mock<IOptionsSnapshot<CustomerSettings>>();
            customerSettingsSnapshopStub.Setup(csn => csn.Value)
                .Returns(customerSettings)
                .Verifiable();
            var customerController = new CustomerController(adminCustomerServiceMock.Object, customerSettingsSnapshopStub.Object);

            var result = await customerController.List();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(customerListModel, viewResult.Model);
            adminCustomerServiceMock.Verify();
        }

        [Fact]
        public async Task List_PaginationInfoInvalid_UseDefaultValues()
        {
            var page = 0;
            var size = -1;
            var customerListModel = new CustomerListModel();
            var customerSettings = new CustomerSettings { Admin = new CustomerSettings.AdminCustomerSettings { DefaultPageSize = 12 } };
            var adminCustomerServiceStub = new Mock<IAdminCustomerService>();
            adminCustomerServiceStub.Setup(c =>
            c.GetCustomerListModelAsync(It.Is<CustomerListOptions>(opts =>
                opts.PageSize == 12 && opts.PageNumber == 1
            ))).ReturnsAsync(customerListModel);
            var customerSettingsSnapshopStub = new Mock<IOptionsSnapshot<CustomerSettings>>();
            customerSettingsSnapshopStub.Setup(csn => csn.Value)
                .Returns(customerSettings)
                .Verifiable();
            var customerController = new CustomerController(adminCustomerServiceStub.Object, customerSettingsSnapshopStub.Object);

            var result = await customerController.List(page, size);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(customerListModel, viewResult.Model);
        }
        #endregion

        #region Edit

        [Fact]
        public async Task Edit_ModelIsNull_RedirectToList()
        {
            var notFoundCustomerId = 0;
            var adminCustomerServiceMock = new Mock<IAdminCustomerService>();
            adminCustomerServiceMock.Setup(s => s.GetCustomerModelAsync(notFoundCustomerId))
                .ReturnsAsync((CustomerModel)null)
                .Verifiable();
            var customerController = new CustomerController(adminCustomerServiceMock.Object, _GetDefaultCustomerSettingsSnapshot());

            var result = await customerController.Edit(0);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CustomerController.List), redirectToActionResult.ActionName);
            adminCustomerServiceMock.Verify();
        }

        [Fact]
        public async Task Edit_CustomerFound_PrepareValidViewModel()
        {
            var id = 1;
            var model = new CustomerModel();
            var adminCustomerServiceMock = new Mock<IAdminCustomerService>();
            adminCustomerServiceMock.Setup(s => s.GetCustomerModelAsync(id))
                .ReturnsAsync(model)
                .Verifiable();
            var customerController = new CustomerController(adminCustomerServiceMock.Object, _GetDefaultCustomerSettingsSnapshot());

            var result = await customerController.Edit(id);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            adminCustomerServiceMock.Verify();
        }
        #endregion

        #region Edit Post
        [Fact]
        public async Task EditPost_ModelStateIsInvalid_PrepareValidModel()
        {
            var model = new CustomerModel();
            var customerController = new CustomerController(Mock.Of<IAdminCustomerService>(), _GetDefaultCustomerSettingsSnapshot());
            customerController.ModelState.AddModelError("error", "error");

            var result = await customerController.Edit(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task EditPost_InvalidModelResponse_RedisplayView()
        {
            var notFoundCustomerId = 1;
            var model = new CustomerModel { Id = notFoundCustomerId };
            var invalidModelResponse = AppResponse.InvalidModelResult<Customer>(string.Empty);
            var adminCustomerServiceMock = new Mock<IAdminCustomerService>();
            adminCustomerServiceMock.Setup(c => c.UpdateCustomerAsync(It.Is<AppRequest<CustomerModel>>(req => req.Data == model)))
                .ReturnsAsync(invalidModelResponse)
                .Verifiable();
            var customerController = new CustomerController(adminCustomerServiceMock.Object, Mock.Of<IOptionsSnapshot<CustomerSettings>>());
            customerController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await customerController.Edit(model);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CustomerController.List), redirectToActionResult.ActionName);
            adminCustomerServiceMock.Verify();
        }

        [Fact]
        public async Task EditPost_ErrorResponse_RedisplayViewWithValidModel()
        {
            var customerId = 1;
            var customer = new Customer { Id = customerId };
            var model = new CustomerModel { Id = customerId };
            var showHidden = true;
            var errorResponse = AppResponse.ErrorResult<Customer>(string.Empty);
            var adminCustomerServiceMock = new Mock<IAdminCustomerService>();
            adminCustomerServiceMock.Setup(c => c.UpdateCustomerAsync(It.Is<AppRequest<CustomerModel>>(req => req.Data == model)))
                .ReturnsAsync(errorResponse)
                .Verifiable();
            var customerController = new CustomerController(adminCustomerServiceMock.Object, _GetDefaultCustomerSettingsSnapshot());
            customerController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await customerController.Edit(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            adminCustomerServiceMock.Verify();
        }

        [Fact]
        public async Task EditPost_SuccessResponse_RedirectToList()
        {
            var customerId = 1;
            var customer = new Customer { Id = customerId };
            var model = new CustomerModel { Id = customerId };
            var successResponse = AppResponse.SuccessResult<Customer>(customer);
            var adminCustomerServiceMock = new Mock<IAdminCustomerService>();
            adminCustomerServiceMock.Setup(c => c.UpdateCustomerAsync(It.Is<AppRequest<CustomerModel>>(req => req.Data == model)))
                .ReturnsAsync(successResponse)
                .Verifiable();
            var customerController = new CustomerController(adminCustomerServiceMock.Object, Mock.Of<IOptionsSnapshot<CustomerSettings>>());
            customerController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await customerController.Edit(model);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CustomerController.List), redirectToActionResult.ActionName);
            adminCustomerServiceMock.Verify();
        }
        #endregion

        #region Create
        [Fact]
        public void Create_PrepareValidModel()
        {
            var customerController = new CustomerController(Mock.Of<IAdminCustomerService>(), _GetDefaultCustomerSettingsSnapshot());

            var result = customerController.Create();

            Assert.IsType<ViewResult>(result);
        }

        #endregion

        #region Create Post

        [Fact]
        public async Task CreatePost_ModelStateIsInvalid_RedisplayViewWithValidModel()
        {
            var model = new CustomerModel();
            var customerController = new CustomerController(Mock.Of<IAdminCustomerService>(), _GetDefaultCustomerSettingsSnapshot());
            customerController.ModelState.AddModelError("error", "error");

            var result = await customerController.Create(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task CreatePost_ErrorResponse_RedisplayViewWithValidModel()
        {
            var model = new CustomerModel();
            var adminCustomerServiceMock = new Mock<IAdminCustomerService>();
            var errorResponse = AppResponse.ErrorResult<Customer>(string.Empty);
            adminCustomerServiceMock.Setup(cf => cf.CreateCustomerAsync(It.Is<AppRequest<CustomerModel>>(req => req.Data == model)))
                .ReturnsAsync(errorResponse)
                .Verifiable();
            var customerController = new CustomerController(adminCustomerServiceMock.Object, _GetDefaultCustomerSettingsSnapshot());
            customerController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await customerController.Create(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
            adminCustomerServiceMock.Verify();
        }

        [Fact]
        public async Task CreatePost_InvalidModelResponse_RedirectToList()
        {
            var model = new CustomerModel();
            var customer = new Customer();
            var invalidModelResponse = AppResponse.InvalidModelResult<Customer>(string.Empty);
            var adminCustomerServiceMock = new Mock<IAdminCustomerService>();
            adminCustomerServiceMock.Setup(c => c.CreateCustomerAsync(It.Is<AppRequest<CustomerModel>>(req => req.Data == model)))
                .ReturnsAsync(invalidModelResponse)
                .Verifiable();
            var customerController = new CustomerController(adminCustomerServiceMock.Object, Mock.Of<IOptionsSnapshot<CustomerSettings>>());
            customerController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await customerController.Create(model);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CustomerController.List), redirectToActionResult.ActionName);
            adminCustomerServiceMock.Verify();
        }

        [Fact]
        public async Task CreatePost_SuccessResponse_RedirectToList()
        {
            var model = new CustomerModel();
            var customer = new Customer();
            var successResponse = AppResponse.SuccessResult<Customer>(customer);
            var adminCustomerServiceMock = new Mock<IAdminCustomerService>();
            adminCustomerServiceMock.Setup(c => c.CreateCustomerAsync(It.Is<AppRequest<CustomerModel>>(req => req.Data == model)))
                .ReturnsAsync(successResponse)
                .Verifiable();
            var customerController = new CustomerController(adminCustomerServiceMock.Object, Mock.Of<IOptionsSnapshot<CustomerSettings>>());
            customerController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await customerController.Create(model);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CustomerController.List), redirectToActionResult.ActionName);
            adminCustomerServiceMock.Verify();
        }
        #endregion

        #region Delete

        [Fact]
        public async Task Delete_InvalidModelResponse_RedirectToList()
        {
            var notFoundCustomerId = 1;
            var adminCustomerServiceMock = new Mock<IAdminCustomerService>();
            var invalidModelResponse = AppResponse.InvalidModelResult<Customer>(string.Empty);
            adminCustomerServiceMock.Setup(c => c.DeleteCustomerAsync(It.Is<AppRequest<int>>(req => req.Data == notFoundCustomerId)))
                .ReturnsAsync(invalidModelResponse)
                .Verifiable();
            var customerController = new CustomerController(adminCustomerServiceMock.Object, Mock.Of<IOptionsSnapshot<CustomerSettings>>());
            customerController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await customerController.Delete(notFoundCustomerId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CustomerController.List), redirectToActionResult.ActionName);
            adminCustomerServiceMock.Verify();
        }

        [Fact]
        public async Task Delete_ErrorResponse_RedirectToList()
        {
            var customerId = 1;
            var customer = new Customer { Id = customerId };
            var errorResponse = AppResponse.ErrorResult<Customer>(string.Empty);
            var adminCustomerServiceMock = new Mock<IAdminCustomerService>();
            adminCustomerServiceMock.Setup(c => c.DeleteCustomerAsync(It.Is<AppRequest<int>>(req => req.Data == customerId)))
                .ReturnsAsync(errorResponse)
                .Verifiable();
            var customerController = new CustomerController(adminCustomerServiceMock.Object, Mock.Of<IOptionsSnapshot<CustomerSettings>>());
            customerController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await customerController.Delete(customerId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CustomerController.List), redirectToActionResult.ActionName);
            adminCustomerServiceMock.Verify();
        }

        [Fact]
        public async Task Delete_SuccessResponse_RedirectToList()
        {
            var customerId = 1;
            var customer = new Customer { Id = customerId };
            var successResponse = AppResponse.SuccessResult<Customer>(customer);
            var adminCustomerServiceMock = new Mock<IAdminCustomerService>();
            adminCustomerServiceMock.Setup(c => c.DeleteCustomerAsync(It.Is<AppRequest<int>>(req => req.Data == customerId)))
                .ReturnsAsync(successResponse)
                .Verifiable();
            var customerController = new CustomerController(adminCustomerServiceMock.Object, Mock.Of<IOptionsSnapshot<CustomerSettings>>());
            customerController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await customerController.Delete(customerId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CustomerController.List), redirectToActionResult.ActionName);
            adminCustomerServiceMock.Verify();
        }

        #endregion

        #region Helpers
        private IOptionsSnapshot<CustomerSettings> _GetDefaultCustomerSettingsSnapshot()
        {
            var customerSettingsSnapshotStub = new Mock<IOptionsSnapshot<CustomerSettings>>();
            customerSettingsSnapshotStub.Setup(c => c.Value)
                .Returns(new CustomerSettings());

            return customerSettingsSnapshotStub.Object; ;
        }
        #endregion
    }
}
