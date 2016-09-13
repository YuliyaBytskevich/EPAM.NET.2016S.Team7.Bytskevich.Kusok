using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ToDoClient.Infrastructure
{
    public static class CallsSwitcher
    {
        public static bool IsFirstCallToGet { get; set; } = true;
    }
}