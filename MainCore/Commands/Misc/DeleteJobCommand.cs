﻿using MainCore.Commands.Abstract;

namespace MainCore.Commands.Misc
{
    public class DeleteJobCommand(IDbContextFactory<AppDbContext> contextFactory = null) : QueryBase(contextFactory)
    {
        public void ByJobId(JobId jobId)
        {
            using var context = _contextFactory.CreateDbContext();

            var job = context.Jobs
                .Where(x => x.Id == jobId.Value)
                .Select(x => new
                {
                    x.VillageId,
                    x.Position
                })
                .FirstOrDefault();

            if (job is null) return;

            context.Jobs
                .Where(x => x.Id == jobId.Value)
                .ExecuteDelete();

            context.Jobs
                .Where(x => x.VillageId == job.VillageId)
                .Where(x => x.Position > job.Position)
                .ExecuteUpdate(x => x.SetProperty(x => x.Position, x => x.Position - 1));
        }
    }
}