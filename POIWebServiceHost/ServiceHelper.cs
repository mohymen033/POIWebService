using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POIWebServiceHost
{
    static class ServiceHelper
    {
        static public int GetParsedId(string id)
        {
            int parsedId = -1;
            int.TryParse(id, out parsedId);
            return parsedId;
        }
    }
}
