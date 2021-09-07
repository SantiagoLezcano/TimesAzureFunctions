using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TimesAzureFunctions.Functions.Entities
{
    public class ConsolidateBiometricEntity : TableEntity
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public int AcumMinutes { get; set; }

        
    }
}
