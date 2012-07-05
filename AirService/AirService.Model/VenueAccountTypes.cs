namespace AirService.Model
{
    public enum VenueAccountTypes
    {
        AccountTypeFull = 1, 
        // due to requirement changes evaluation means subscription that wasn't paid. 
        // in the future may require different account types. 
        AccountTypeEvaluation = 4
    }
}