﻿using NextSolution.Core.Entities;
using NextSolution.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Repositories
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<bool> IsUserExistsAsync(long userId, CancellationToken cancellationToken = default);
    }
}