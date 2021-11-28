using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalDatabase_Server.Server
{
    public class Incrementer
    {
        private int portNumber = 25100;

        public void incrementPort()
        {
            if (portNumber < 25200)
                portNumber++;
            else
                portNumber = 25100;
        }
        public int getPort()
        {
            Console.WriteLine(portNumber);
            return portNumber;
        }
    }
}
