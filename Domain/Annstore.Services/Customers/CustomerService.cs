using Annstore.Core.Entities.Customers;
using Annstore.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using Annstore.Core.Common;

namespace Annstore.Services.Customers
{
    public sealed class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer> _customerRepository;

        public CustomerService(IRepository<Customer> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var result = await _customerRepository.InsertAsync(customer)
                .ConfigureAwait(false);

            return result;
        }

        public async Task DeleteCustomerAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            customer.Deleted = true;
            await _customerRepository.UpdateAsync(customer)
                .ConfigureAwait(false);
        }

        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            var customer = await _customerRepository.FindByIdAsync(id);

            return customer;
        }

        public async Task<List<Customer>> GetCustomersAsync()
        {
            var query = from customer in _customerRepository.Table
                        where !customer.Deleted
                        orderby customer.Id descending
                        select customer;

            return await query.ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<IPagedList<Customer>> GetPagedCustomersAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Page number must greater than or equal 1");
            if (pageSize <= 0)
                throw new ArgumentException("Page size must greater than 0");

            var query = from customer in _customerRepository.Table
                        where !customer.Deleted
                        orderby customer.Id descending
                        select customer;
            //*TODO*
            var allCustomers = await query.ToListAsync()
                .ConfigureAwait(false);
            var customers = allCustomers.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            var result = customers.ToPagedList(pageSize, pageNumber, allCustomers.Count);

            return result;
        }

        public async Task<bool> HasCustomersAsync()
        {
            var query = from customer in _customerRepository.Table
                        where !customer.Deleted
                        select customer;
            //*TODO*
            var allCustomers = await query.ToListAsync()
                .ConfigureAwait(false);

            return allCustomers.Count > 0;
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var result = await _customerRepository.UpdateAsync(customer)
                .ConfigureAwait(false);

            return result;
        }
    }
}
