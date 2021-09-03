
using System;
using System.Collections.Generic;
using System.Text;

namespace TimesAzureFunctions.common.Model
{
    public class Times
    {
        public int Id { get; set; }
        public DateTime dateCreate { get; set; }
        public int type { get; set; }
        public bool consolidate { get; set; }
    }
}