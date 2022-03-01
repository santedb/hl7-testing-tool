using System;
using System.Collections.Generic;
using System.Text;

namespace HL7TestingTool.Interop
{
    public interface IMllpMessageSender
    { 
        string SendAndReceive(string message);
    }
}
