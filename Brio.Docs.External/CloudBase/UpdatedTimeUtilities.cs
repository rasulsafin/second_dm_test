using System;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.External
{
    public static class UpdatedTimeUtilities
    {
        public static void UpdateTime(ProjectExternalDto project, DateTime time = default)
        {
            var now = GetCorrectTime(time);
            project.UpdatedAt = now;

            if (project.Items != null)
            {
                foreach (var item in project.Items)
                    UpdateTime(item, now);
            }
        }

        public static void UpdateTime(ObjectiveExternalDto objective, DateTime time = default)
        {
            var now = GetCorrectTime(time);
            objective.UpdatedAt = now;

            if (objective.Items != null)
            {
                foreach (var item in objective.Items)
                    UpdateTime(item, now);
            }

            if (objective.DynamicFields != null)
            {
                foreach (var dynamicField in objective.DynamicFields)
                    UpdateTime(dynamicField, now);
            }

            if (objective.Location?.Item != null)
                UpdateTime(objective.Location.Item, now);
        }

        private static void UpdateTime(ItemExternalDto item, DateTime time)
            => item.UpdatedAt = time;

        private static void UpdateTime(DynamicFieldExternalDto dynamicField, DateTime time)
            => dynamicField.UpdatedAt = time;

        private static DateTime GetCorrectTime(DateTime time)
            => time == default ? DateTime.UtcNow : time;
    }
}
