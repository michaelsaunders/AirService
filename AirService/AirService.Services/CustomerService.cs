using System.Linq;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;

namespace AirService.Services
{
    public class CustomerService : SimpleService<Customer>, ICustomerService
    {
        public CustomerService(IRepository<Customer> customerRepository)
        {
            this.Repository = customerRepository;
        }

        #region ICustomerService Members

        void ICustomerService.Update(Customer customerToUpdate)
        {
            this.InsertOrUpdate(customerToUpdate);
            this.Save(); 
        }

        Customer ICustomerService.Insert(Customer customerToUpdate)
        {
            if (string.IsNullOrWhiteSpace(customerToUpdate.Udid) || customerToUpdate.Udid.Length != 40)
            {
                throw new ServiceFaultException(1001,
                                                Resources.Err1001UdidRequired);
            }

            if (string.IsNullOrWhiteSpace(customerToUpdate.FirstName) ||
                string.IsNullOrWhiteSpace(customerToUpdate.LastName))
            {
                throw new ServiceFaultException(1002,
                                                Resources.Err1002FirstNameAndLastNameAreRequired);
            }

            var existingCustomer = this.FindByUdid(customerToUpdate.Udid);
            if (existingCustomer != null)
            {
                existingCustomer.FirstName = customerToUpdate.FirstName;
                existingCustomer.LastName = customerToUpdate.LastName;
                existingCustomer.FacebookId = customerToUpdate.FacebookId;
                existingCustomer.Mobile = customerToUpdate.Mobile;
                existingCustomer.Email = customerToUpdate.Email;
                existingCustomer.ReceiveEmailNotification = customerToUpdate.ReceiveEmailNotification;
                existingCustomer.ReceiveSpecialOffers = customerToUpdate.ReceiveSpecialOffers;
                customerToUpdate = existingCustomer;
            }

            customerToUpdate.Status = SimpleModel.StatusActive;
            this.InsertOrUpdate(customerToUpdate); 
            this.Save();
            return customerToUpdate;
        }

        public Customer FindByUdid(string uuid)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation, false);
            var customer = (from c in this.FindAll()
                            where c.Udid == uuid && c.Status == SimpleModel.StatusActive
                            orderby c.UpdateDate descending
                            select c).FirstOrDefault();
            return customer;
        } 

        #endregion
    }
}