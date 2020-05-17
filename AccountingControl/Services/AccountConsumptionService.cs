using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountingControl.Data;
using Models;
using MongoDB.Driver;

namespace AccountingControl.Services
{
    public class AccountConsumptionService : IAccountConsumptionService
    {
        private readonly IMongoCollection<AccountConsumption> _accountConsumptions;
        public AccountConsumptionService(IAccountControlDBSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _accountConsumptions = database.GetCollection<AccountConsumption>(settings.SubmissionCollectionName);
        }

        public async Task<List<AccountConsumption>> GetAccountConsumption(long accountId)
        {
            var filter = new ExpressionFilterDefinition<AccountConsumption>(x =>
                    accountId == x.Account.AccountId);

            var ac =  await _accountConsumptions.FindAsync<AccountConsumption>(filter);
            return ac.ToList();
        }

        public AccountConsumption getLastInsertedAccountConsumption()
        {
            var filter = new ExpressionFilterDefinition<AccountConsumption>(x =>
                    true == true);

            var accountConsumptions = _accountConsumptions.Find(filter).SortBy(x => x.EventSequence).ToList();

            if (!accountConsumptions.Any())
                return null;

            var ac = accountConsumptions.First();
            return ac;
        }

        public Task PostAccountConsumption(AccountConsumption accountConsumption)
        {
            return _accountConsumptions.InsertOneAsync(accountConsumption);
        }
    }
}
