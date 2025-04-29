using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Common
{
    public enum TransactionAction
    {// Enumerations to determine the task or action of a transaction.
        // For more details see the example of the UserMaintenance form of the user interface layer.
        Add=1,
        Edit=2, 
        Remove=3,
        View = 4,
        Special = 5//You can use this action for special cases, in this case it was used to edit 
                   //the user profile of the connected user, see the UserMaintenance form of the user interface layer.
    }
}
