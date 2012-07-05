using AirService.Model;

namespace AirService.Services.Security
{
    public class AirServiceCustomerIdentity : AirServiceIdentity
    {
        private readonly Customer _customer;

        public AirServiceCustomerIdentity(string name, Customer customer) : base(name)
        {
            this._customer = customer;
        }

        public Customer Customer
        {
            get { return this._customer; }
        }
    }
}
