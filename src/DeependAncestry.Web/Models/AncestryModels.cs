using System.Collections.Generic;

namespace DeependAncestry.Web.Models
{

    public class Place
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Person
    {
        public int id { get; set; }
        public string name { get; set; }
        public string gender { get; set; }
        public int? father_id { get; set; }
        public int? mother_id { get; set; }
        public int place_id { get; set; }
        public int level { get; set; }
    }

    public class PeopleLocations
    {
        public List<Place> places { get; set; }
        public List<Person> people { get; set; }
    }

    public class Results
    {
        public int id { get; set; }
        public string name { get; set; }
        public string gender { get; set; }
        public string birthplace { get; set; }
        public int level { get; set; }
    }

    public class PersonBirthPlace
    {
        public int id { get; set; }
        public string name { get; set; }
        public string gender { get; set; }
        public int? father_id { get; set; }
        public int? mother_id { get; set; }
        public string birthplace { get; set; }
        public int level { get; set; }
        public List<PersonBirthPlace> children { get; set; }
    }
}