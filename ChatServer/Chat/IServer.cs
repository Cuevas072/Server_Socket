using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
namespace ChatServer

{
    public interface IServer
    {
        void notify(object o);
    }
}
