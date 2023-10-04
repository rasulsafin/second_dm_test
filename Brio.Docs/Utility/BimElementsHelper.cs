using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility
{
    public class BimElementsHelper
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly ILogger<BimElementsHelper> logger;

        public BimElementsHelper(
            DMContext context,
            IMapper mapper,
            ILogger<BimElementsHelper> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
            logger.LogTrace("BimElementsHelper created");
        }

        internal async Task<Objective> AddBimElementsAsync(IEnumerable<BimElementDto> bimElementsDto, Objective objective)
        {
            objective.BimElements = new List<BimElementObjective>();
            foreach (var bim in bimElementsDto ?? Enumerable.Empty<BimElementDto>())
            {
                logger.LogTrace("Bim element: {@Bim}", bim);
                var dbBim = await context.BimElements
                    .Where(x => x.ParentName == bim.ParentName)
                    .Where(x => x.GlobalID == bim.GlobalID)
                    .FirstOrDefaultAsync();
                logger.LogDebug("Found BIM element: {@DBBim}", dbBim);

                if (dbBim == null)
                {
                    dbBim = mapper.Map<BimElement>(bim);
                    await context.BimElements.AddAsync(dbBim);
                    await context.SaveChangesAsync();
                }

                objective.BimElements.Add(new BimElementObjective
                {
                    ObjectiveID = objective.ID,
                    BimElementID = dbBim.ID,
                });
            }

            await context.SaveChangesAsync();
            return objective;
        }

        internal async Task UpdateBimElementsAsync(ICollection<BimElementDto> bimElementDtos, int objectiveId)
        {
            var newBimElements = bimElementDtos ?? Enumerable.Empty<BimElementDto>();
            var currentBimElements = await context.BimElementObjectives.Where(x => x.ObjectiveID == objectiveId).Select(x => x.BimElement).ToListAsync();
            var bimElementsToRemove = currentBimElements
                .Where(x => !newBimElements.Any(e =>
                    e.ParentName == x.ParentName
                    && e.GlobalID == x.GlobalID))
                .ToList();
            logger.LogDebug(
                "Objective's ({ID}) BIM elements links to remove: {@LinksToRemove}",
                objectiveId,
                bimElementsToRemove);
            context.BimElements.RemoveRange(bimElementsToRemove);

            foreach (var bimElement in newBimElements)
            {
                // See if objective already had this bim element referenced
                var linkedBimElementFromDb = currentBimElements.SingleOrDefault(x => x.ParentName == bimElement.ParentName && x.GlobalID == bimElement.GlobalID);
                logger.LogDebug("Found bimElementFromDb: {@DBBim}", linkedBimElementFromDb);
                if (linkedBimElementFromDb == null)
                {
                    // Bim element was not referenced. Does it exist?
                    var bimElementFromDb = await context.BimElements.FirstOrDefaultAsync(x => x.ParentName == bimElement.ParentName && x.GlobalID == bimElement.GlobalID);
                    if (bimElementFromDb == null)
                    {
                        // Bim element does not exist at all - should be created
                        bimElementFromDb = mapper.Map<BimElement>(bimElement);
                        logger.LogDebug("Adding BIM element: {@BimElement}", bimElementFromDb);
                        await context.BimElements.AddAsync(bimElementFromDb);
                        await context.SaveChangesAsync();
                    }

                    // Add link between bim element and objective
                    var link = new BimElementObjective { BimElementID = bimElementFromDb.ID, ObjectiveID = objectiveId };
                    await context.BimElementObjectives.AddAsync(link);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
