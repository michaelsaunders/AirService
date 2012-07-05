namespace AirService.Web.ViewModels
{

    public class ContactUsInput
    { 
        public string FormType { get; set; }
         
        public string Name { get; set; }
         
        public string Email { get; set; }
         
        public string Subject { get; set; }
         
        public string Type { get; set; }
         
        public string Message { get; set; }
         
        public bool ReceiveSpecialOffers { get; set; }
    }

}