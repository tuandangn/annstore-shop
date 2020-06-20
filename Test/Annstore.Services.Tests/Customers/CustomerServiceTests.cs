using Annstore.Core.Entities.Customers;
using Annstore.Data;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using TestHelper;
using Annstore.Services.Customers;
using System;
using System.Linq;

namespace Annstore.Services.Tests.Customers
{
    public class CustomerServiceTests
    {
        #region GetCustomersAsync
        [Fact]
        public async Task GetCustomersAsync_ReturnAllCustomers()
        {
            var availableCustomers = new List<Customer> { Customer.CreateWithId(1) };
            var customerRepositoryMock = new Mock<IRepository<Customer>>();
            customerRepositoryMock.Setup(c => c.Table)
                .Returns(availableCustomers.ToAsync())
                .Verifiable();
            var customerService = new CustomerService(customerRepositoryMock.Object);

            var result = await customerService.GetCustomersAsync();

            Assert.Equal(availableCustomers.Count, result.Count);
            Assert.Equal(availableCustomers[0].Id, result[0].Id);
            customerRepositoryMock.Verify();
        }

        [Fact]
        public async Task GetCustomersAsync_ExcludeDeletedCustomer()
        {
            var deletedCustomer = Customer.CreateWithId(1);
            deletedCustomer.IsDeleted(true);
            var availableCustomers = new List<Customer> { deletedCustomer };
            var customerRepositoryMock = new Mock<IRepository<Customer>>();
            customerRepositoryMock.Setup(c => c.Table)
                .Returns(availableCustomers.ToAsync())
                .Verifiable();
            var customerService = new CustomerService(customerRepositoryMock.Object);

            var result = await customerService.GetCustomersAsync();

            Assert.Empty(result);
            customerRepositoryMock.Verify();
        }

        #endregion

        #region CreateCustomerAsync
        [Fact]
        public async Task CreateCustomerAsync_CustomerIsNull_ThrowArgumentNullException()
        {
            var customerService = new CustomerService(Mock.Of<IRepository<Customer>>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateCustomerAsync(null));
        }

        [Fact]
        public async Task CreateCustomerAsync_CustomerIsNotNull_InsertCustomer()
        {
            var customer = Customer.CreateWithId(1);
            customer.FullName = "full name";
            var customerRepositoryMock = new Mock<IRepository<Customer>>();
            customerRepositoryMock.Setup(c => c.InsertAsync(customer))
                .ReturnsAsync(customer)
                .Verifiable();
            var customerService = new CustomerService(customerRepositoryMock.Object);

            var result = await customerService.CreateCustomerAsync(customer);

            Assert.Equal(customer, result);
            customerRepositoryMock.Verify();
        }

        #endregion

        #region UpdateCustomerAsync
        [Fact]
        public async Task UpdateCustomerAsync_CustomerIsNull_ThrowArgumentNullException()
        {
            var customerService = new CustomerService(Mock.Of<IRepository<Customer>>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.UpdateCustomerAsync(null));
        }

        [Fact]
        public async Task UpdateCustomerAsync_CustomerIsNotNull_UpdateCustomer()
        {
            var customer = Customer.CreateWithId(1);
            customer.FullName = "full name";
            var customerRepositoryMock = new Mock<IRepository<Customer>>();
            customerRepositoryMock.Setup(c => c.UpdateAsync(customer))
                .ReturnsAsync(customer)
                .Verifiable();
            var customerService = new CustomerService(customerRepositoryMock.Object);

            var result = await customerService.UpdateCustomerAsync(customer);

            Assert.Equal(customer, result);
            customerRepositoryMock.Verify();
        }

        #endregion

        #region DeleteCustomerAsync
        [Fact]
        public async Task DeleteCustomerAsync_CustomerIsNull_ThrowArgumentNullException()
        {
            var customerService = new CustomerService(Mock.Of<IRepository<Customer>>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.DeleteCustomerAsync(null));
        }

        [Fact]
        public async Task DeleteCustomerAsync_CustomerIsNotNull_DeleteCustomer()
        {
            var customer = Customer.CreateWithId(1);
            customer.FullName = "full name";
            var customerRepositoryMock = new Mock<IRepository<Customer>>();
            customerRepositoryMock.Setup(c => c.UpdateAsync(It.Is<Customer>(cust => cust.Deleted)))
                .ReturnsAsync(customer)
                .Verifiable();
            var customerService = new CustomerService(customerRepositoryMock.Object);

            await customerService.DeleteCustomerAsync(customer);

            customerRepositoryMock.Verify();
        }

        #endregion

        #region GetPagedCustomersAsync
        [Fact]
        public async Task GetPagedCustomersAsync_PageNumberLessThanOne_ThrowArgumentException()
        {
            var pageNumber = 0;
            var customerService = new CustomerService(Mock.Of<IRepository<Customer>>());

            await Assert.ThrowsAsync<ArgumentException>(() => customerService.GetPagedCustomersAsync(pageNumber, int.MaxValue));
        }

        [Fact]
        public async Task GetPagedCustomersAsync_PageSizeLessThanOrEqualZero_ThrowArgumentException()
        {
            var pageSize = 0;
            var customerService = new CustomerService(Mock.Of<IRepository<Customer>>());

            await Assert.ThrowsAsync<ArgumentException>(() => customerService.GetPagedCustomersAsync(1, pageSize));
        }

        [Fact]
        public async Task GetPagedCustomersAsync_ReturnValidResult()
        {
            var pageNumber = 2;
            var pageSize = 2;
            var availableCustomers = new List<Customer>
            {
                Customer.CreateWithId(1), Customer.CreateWithId(2),
                Customer.CreateWithId(3), Customer.CreateWithId(4),
                Customer.CreateWithId(5), Customer.CreateWithId(6)
            };
            var customerRepositoryMock = new Mock<IRepository<Customer>>();
            customerRepositoryMock.Setup(c => c.Table)
                .Returns(availableCustomers.ToAsync())
                .Verifiable();
            var customerService = new CustomerService(customerRepositoryMock.Object);

            var result = await customerService.GetPagedCustomersAsync(pageNumber, pageSize);

            Assert.Equal(3, result.TotalPages);
            Assert.Equal(availableCustomers.Count, result.TotalItems);
            //order by id desc
            Assert.Equal(4, result.Items.ElementAt(0).Id);
            customerRepositoryMock.Verify();
        }

        #endregion

        #region GetCustomerByIdAsync
        [Fact]
        public async Task GetCustomerByIdAsync_ReturnCustomer()
        {
            var id = 1;
            var customer = Customer.CreateWithId(id);
            var customerRepositoryMock = new Mock<IRepository<Customer>>();
            customerRepositoryMock.Setup(c => c.FindByIdAsync(id))
                .ReturnsAsync(customer)
                .Verifiable();
            var customerService = new CustomerService(customerRepositoryMock.Object);

            var result = await customerService.GetCustomerByIdAsync(id);

            Assert.Equal(customer, result);
            customerRepositoryMock.Verify();
        }

        #endregion

        #region HasCustomersAsync
        [Fact]
        public async Task HasCustomersAsync()
        {
            var availableCustomer = new List<Customer>();
            var customerRepositoryMock = new Mock<IRepository<Customer>>();
            customerRepositoryMock.Setup(c => c.Table)
                .Returns(availableCustomer.ToAsync())
                .Verifiable();
            var customerService = new CustomerService(customerRepositoryMock.Object);

            var result = await customerService.HasCustomersAsync();

            Assert.Equal(availableCustomer.Count > 0, result);
            customerRepositoryMock.Verify();
        }

        [Fact]
        public async Task HasCustomersAsync_ExcludeDeletedCustomer()
        {
            var deletedCustomer = Customer.CreateWithId(1);
            deletedCustomer.IsDeleted(true);
            var availableCustomer = new List<Customer> { deletedCustomer };
            var customerRepositoryMock = new Mock<IRepository<Customer>>();
            customerRepositoryMock.Setup(c => c.Table)
                .Returns(availableCustomer.ToAsync())
                .Verifiable();
            var customerService = new CustomerService(customerRepositoryMock.Object);

            var result = await customerService.HasCustomersAsync();

            Assert.False(result);
            customerRepositoryMock.Verify();
        }

        #endregion
    }
}
