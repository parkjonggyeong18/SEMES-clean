using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Common
{
    public struct Positions
    {
        /// <summary>
        /// This structure stores the user charges, essential to carry out the conditions of user permissions (For the project example).
        /// In the same way this is optional, instead you can use a table of Positions in the database, you can do it through enumerations
        /// and the ID of the positions.
        /// </summary>

        public const string GeneralManager = "General manager";
        public const string Accountant = "Accountant";
        public const string MarketingGuru = "Marketing guru";
        public const string AdministrativeAssistant = "Administrative assistant";
        public const string HMR = "Human resource manager";
        public const string Receptionist = "Receptionist";
        public const string SystemAdministrator = "System administrator";

        public static IEnumerable<string> GetPositions()
        {//Method for listing charges. It is used to set the data source of the ComboBox in the user form of the user interface layer.
            var positions = new List<string>();
            positions.Add(GeneralManager);
            positions.Add(Accountant);
            positions.Add(MarketingGuru);
            positions.Add(AdministrativeAssistant);
            positions.Add(HMR);
            positions.Add(Receptionist);
            positions.Add(SystemAdministrator);
            positions.Sort();
            return positions;
        }
    }
}
