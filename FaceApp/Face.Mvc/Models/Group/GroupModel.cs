using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Face.Mvc.Models.Group
{
    public class GroupViewModel
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string UserData { get; set; }
    }

    public class Group
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string UserData { get; set; }
        public string Status { get; set; }
        public string StatusMessage { get; set; }
    }

    public class GroupListViewModel
    {
        public List<Group> Groups { get; set; }
    }
}