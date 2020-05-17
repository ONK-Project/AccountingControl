using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using MongoDB.Driver;

namespace AccountingControl.Services
{
    public interface IAccountConsumptionService
    {
        Task<List<AccountConsumption>> GetAccountConsumption(long accountId);
        Task PostAccountConsumption(AccountConsumption accountConsumption);
        AccountConsumption getLastInsertedAccountConsumption();
    }
}
