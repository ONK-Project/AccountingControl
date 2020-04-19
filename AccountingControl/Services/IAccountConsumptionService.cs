﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
namespace AccountingControl.Services
{
    public interface IAccountConsumptionService
    {
        Task<AccountConsumption> GetAccountConsumption(long accountId, int days, string type);
    }
}