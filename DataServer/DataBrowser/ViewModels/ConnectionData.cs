using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBrowser.ViewModels
{
    public class ConnectionData
    {
        public string ServerName { get; set; }
        public AuthenticationType AuthType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        internal string GetConnectionString()
        {
            if (AuthType == AuthenticationType.SqlServer)
            {
                return $"Server={ServerName};Database=master;User Id={Username}; Password = {Password};";
            }
            else
            {
                return $"Server={ServerName};Database=master;Trusted_Connection=True;";
            }
        }
    }
}
