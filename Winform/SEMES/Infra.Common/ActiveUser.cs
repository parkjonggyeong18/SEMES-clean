using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Common
{
    public struct ActiveUser
    {
        /// <summary>
        /// Responsible for storing the ID and position of the user who logged in, which allows you to perform user permissions in any layer.
        /// This is optional, it is generally not necessary to perform user permissions in the domain and data access layer.
        /// </summary>
        /// 
        public static int Id {get;set;}
        public static string Position {get;set;}
        //This is a simple and basic example, there are many ways to do it.
    }
}
