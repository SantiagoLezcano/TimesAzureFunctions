using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TimesAzureFunctions.Test.Helpers
{
    public class MockCloudTableConsolidatedBiomet : CloudTable
    {
        public MockCloudTableConsolidatedBiomet(Uri tableAddress) : base(tableAddress)
        {
        }

        public MockCloudTableConsolidatedBiomet(Uri tableAbsoluteUri, StorageCredentials credentials) : base(tableAbsoluteUri, credentials)
        {
        }

        public MockCloudTableConsolidatedBiomet(StorageUri tableAddress, StorageCredentials credentials) : base(tableAddress, credentials)
        {
        }
        public override async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            return await Task.FromResult(new TableResult
            {
                HttpStatusCode = 200,
                Result = TestFactory.GetBiometricEntity()
            });
        }
        public override async Task<TableQuerySegment<ConsolidateBiometricEntity>> ExecuteQuerySegmentedAsync<ConsolidateBiometricEntity>(TableQuery<ConsolidateBiometricEntity> query, TableContinuationToken token)
        {
            ConstructorInfo constructor = typeof(TableQuerySegment<ConsolidateBiometricEntity>)
                   .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                   .FirstOrDefault(c => c.GetParameters().Count() == 1);

            return await Task.FromResult(constructor.Invoke(new object[] { TestFactory.GetBiometricConsolidatedEntities()}) as TableQuerySegment<ConsolidateBiometricEntity>);
        }
    }
}
