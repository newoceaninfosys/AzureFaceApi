using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Face.Mvc.Models.Group;

namespace Face.Mvc.Models.Person
{
    public class PersonListViewModel
    {
        public string GroupId { get; set; }

        public List<GroupViewModel> Groups { get; set; }

        public List<PersonViewModel> Persons { get; set; }
    }

    public class PersonViewModel
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string UserData { get; set; }

        public List<string> PersistedFaceIds { get; set; }

        public List<string> PersistedFaceUrls { get; set; }

        public GroupViewModel Group { get; set; }
    }
}
