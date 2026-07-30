using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.DAL.Repo
{
    public class CategoryRepo:Repo<Category>, ICategoryRepo
    {
        private readonly ApplicationDbContext _dbContext;

        public CategoryRepo(ApplicationDbContext dbContext):base(dbContext)
        {
            _dbContext = dbContext;
        }

        
    }
}
