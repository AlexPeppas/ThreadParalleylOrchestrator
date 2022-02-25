using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelegatesOrchestrator.Types
{
    public class ErrorResponse
    {
        public object data { get; set; }

        public string exception { get; set; }

        public string stackTrace { get; set; }

    }
}
