using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountingControl.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;

namespace AccountingControl.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AccountConsumptionController : Controller
    {
        private readonly ILogger<AccountConsumptionController> Logger;
        private readonly IAccountConsumptionService AccountConsumptionService;

        public AccountConsumptionController(ILogger<AccountConsumptionController> logger,
            IAccountConsumptionService accountConsumptionService)
        {
            Logger = logger;
            AccountConsumptionService = accountConsumptionService;
        }

        [HttpGet]
        public async Task<AccountConsumption> PostAccountConsumption(int days, long accountId, string type)
        {
            var accountConsumption = await AccountConsumptionService.GetAccountConsumption(accountId, days, type);
            return accountConsumption;
        }
    }
}