using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;


namespace AppIt.Data.Enums
{
    public enum FeatureId
    {
        [Description("Reservations")]
        Reservations = 1,
        [Description("Sales")]
        Sales = 2,
        [Description("Cashier")]
        Cashier = 3,
        [Description("Accounts")]
        Accounts = 4,
        [Description("Statistics")]
        Statistics =5 ,
        [Description("Operations")]
        Operations = 6,
        [Description("Setup")]
        Setup =7,
        [Description("Administration")]
        Administration =8 
    }
}
