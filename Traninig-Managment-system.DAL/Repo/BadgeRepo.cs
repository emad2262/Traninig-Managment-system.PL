using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.DAL.Repo
{
    public class BadgeRepo : Repo<Badge> , IBadgeRepo
    {
        private readonly ApplicationDbContext _Context;

        public BadgeRepo(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
            _Context = applicationDbContext;
        }
    }
}
