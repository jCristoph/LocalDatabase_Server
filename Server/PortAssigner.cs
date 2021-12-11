using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalDatabase_Server.Server
{
    public class PortAssigner
    {
        private int portNumber = 25100;

        public void AssignPort()
        {
            if (portNumber < 25200)
                portNumber++;
            else
                portNumber = 25100;
        }
        public int GetPort()
        {
            return portNumber;
        }
    }
}
