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
        private readonly IMongoCollection<Submission> _submissions;
        private readonly ISubmissionService _submissionService;
        public AccountConsumptionService(IAccountControlDBSettings settings, ISubmissionService submissionService)
        {
            _submissionService = submissionService;
        }

        public async Task<AccountConsumption> GetAccountConsumption(long accountId, int days, string type)
        {
            var accountConsumption = new AccountConsumption();
            accountConsumption.Price = new SubmissionPrice();

            accountConsumption.Type = type;
            accountConsumption.From = DateTime.UtcNow.AddDays(-days);
            accountConsumption.To = DateTime.UtcNow;
            var test = _submissionService.GetSubmissions(new ExpressionFilterDefinition<Submission>(x => true == true)).Result.ToList();

            var submissions = _submissionService.GetSubmissions(new ExpressionFilterDefinition<Submission>(x =>
                    DateTime.UtcNow.AddDays(-days) <= x.DateTime &&
                    accountId == x.MeteringUnit.AccountId &&
                    type == x.MeteringUnit.Type))
                .Result
                .ToList();

            foreach (var submission in submissions)
            {
                accountConsumption.RessourceUsage += submission.RessourceUsage;
                accountConsumption.Price.TotalCost += submission.SubmissionPrice.TotalCost;
            }

            accountConsumption.UnitOfMeassure = submissions.First()?.UnitOfMeassure;
            accountConsumption.Price.Currency = submissions.First()?.SubmissionPrice?.Currency;

            return accountConsumption;
        }
    }
}
