using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AirService.Services.Contracts
{
    public interface ICascadeEntity
    {
        bool CascadeDelete(int id, bool confirmCascadeEligible, out string message);
    }
}