using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace TimesAzureFunctions.Function.Entities
{
    public class TimeEntity : TableEntity
    {
        public int Id { get; set; }
        public DateTime dateCreate { get; set; }
        public int type { get; set; }
        public bool consolidate { get; set; }
    }
}