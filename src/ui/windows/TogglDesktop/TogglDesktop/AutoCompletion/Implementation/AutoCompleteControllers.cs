using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TogglDesktop.AutoCompletion.Implementation
{
    static class AutoCompleteControllers
    {
        public static AutoCompleteController ForStrings(IEnumerable<string> items, Func<string, bool> ignoreTag)
        {
            var list = items.Select(i => new StringItem(i, ignoreTag)).Cast<IAutoCompleteListItem>().ToList();

            return new AutoCompleteController(list);
        }

        public static AutoCompleteController ForProjects(
            List<Toggl.AutocompleteItem> projects, List<Toggl.Model> clients, List<Toggl.Model> workspaces)
        {
            //var list = items.Select(i => new ProjectItem(i)).Cast<IAutoCompleteListItem>().ToList();
            
            var workspaceLookup = workspaces.ToDictionary(w => w.ID);
            var clientLookup = clients.ToDictionary(w => w.Name);

            // TODO: make sure this really is the default workspace
            var defaultWorkspaceID = workspaces[0].ID;

            // categorise by workspace and client
            var list = projects.Select(p =>
            {
                Toggl.Model client = default(Toggl.Model);
                ulong workspaceID = defaultWorkspaceID;
                if (p.ClientLabel != null && clientLookup.TryGetValue(p.ClientLabel, out client))
                {
                    workspaceID = client.WID;
                }
                return new
                {
                    Project = p,
                    Client = client,
                    WorkspaceID = workspaceID
                };
            }).GroupBy(p => p.WorkspaceID)
                .Select(ps => new WorkspaceCategory(
                    workspaceLookup[ps.Key].Name,
                    ps.GroupBy(p => p.Client.ID)
                        .Select(c =>
                        {
                            var projectItems = c.Select(p2 => new ProjectItem(p2.Project));
                            if (c.Key == 0)
                                return projectItems;
                            var clientName = c.First().Client.Name;
                            return new ClientCategory(clientName, projectItems.Cast<IAutoCompleteListItem>().ToList()).yield<IAutoCompleteListItem>();
                        })
                        .SelectMany(i => i).ToList()
                    ))
                .Cast<IAutoCompleteListItem>().ToList();

            return new AutoCompleteController(list);
        }

        private static IEnumerable<T> yield<T>(this T subject)
        {
            yield return subject;
        }
 

        public static AutoCompleteController ForDescriptions(List<Toggl.AutocompleteItem> items)
        {
            var list = items.Select(i => new DescriptionItem(i)).Cast<IAutoCompleteListItem>().ToList();

            // TODO: categorize by workspace/client/project?

            return new AutoCompleteController(list);
        }

        public static AutoCompleteController ForClients(
            List<Toggl.Model> clients, List<Toggl.Model> workspaces)
        {
            var workspaceLookup = workspaces.ToDictionary(w => w.ID);

            // categorise by workspace
            var list = clients.GroupBy(c => c.WID).Select(
                cs =>
                    new WorkspaceCategory(workspaceLookup[cs.Key].Name, cs.Select(
                        c => new ModelItem(c)).Cast<IAutoCompleteListItem>().ToList()
                        )
                ).Cast<IAutoCompleteListItem>().ToList();

            return new AutoCompleteController(list);
        }

        public static AutoCompleteController ForWorkspaces(List<Toggl.Model> list)
        {
            var items = list.Select(m => new ModelItem(m))
                .Cast<IAutoCompleteListItem>().ToList();

            return new AutoCompleteController(items);
        }
    }
}