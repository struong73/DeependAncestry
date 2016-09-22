using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DeependAncestry.Web.Models;
using System.IO;
using System.Web.Hosting;
using Newtonsoft.Json;
using DeependAncestry.Web.Helper;
using System.Text.RegularExpressions;
using DeependAncestry.Web.Interface;
using System.Data;

namespace DeependAncestry.Web.Search
{
    public class AncestrySearch : IAncestrySearch
    {
        private static readonly int CacheTime = 60;
        const string cachePeople = "People";
        const string cachePlaces = "Place";
        const string cachePeopleGrouped = "PeopleGrouped";
        private readonly ICacheHelper _cacheHelper;
        private readonly ILogger _logger;

        public AncestrySearch(ICacheHelper cacheHelper, ILogger logger)
        {
            _cacheHelper = cacheHelper;
            _logger = logger;
        }

        public List<Results> Search(string name, string gender, string direction)
        {
            List<Results> resultList = new List<Results>();

            if (_cacheHelper.Get<List<Person>>(cachePeople) == null && _cacheHelper.Get<List<Place>>(cachePlaces) == null)
            {
                // fetch database
                FetchDBandCache();
            }

            List<Person> peoplelist = _cacheHelper.Get<List<Person>>(cachePeople);
            List<Place> placesList = _cacheHelper.Get<List<Place>>(cachePlaces);

            if (peoplelist == null && placesList == null)
                return resultList; //return empty result

            // let map all the people with their birthplace
            var resultsWithPlace = PeopleWithBirthPlace(peoplelist, placesList);

            // let make a copy so we can use for later query
            var resultsWithPlaceCopy = resultsWithPlace;

            var predicate = PredicateBuilder.True<PersonBirthPlace>();

            // if simple search name must contains
            if (string.IsNullOrWhiteSpace(direction))
                predicate = predicate.And(i => i.name.ToLower().Contains(name.ToLower()));
            else // advanced search the name must match case-insensitive
                predicate = predicate.And(i => i.name.ToLower().Equals(name.ToLower()));

            if (!string.IsNullOrEmpty(gender))
            {
                string[] genderList = gender.ToLower().Split(',');

                predicate = predicate.And(i => genderList.Contains(i.gender.ToLower()));

            }

            IQueryable<PersonBirthPlace> resultMatches = resultsWithPlace.AsQueryable().Where<PersonBirthPlace>(predicate);

            if (resultMatches.Count() > 0)
            {
                // if simple search
                if (string.IsNullOrWhiteSpace(direction))
                {
                    foreach (var result in resultMatches.ToList())
                    {
                        Results person = CreateNewPerson(result);
                        resultList.Add(person);
                    }
                }
                else // advanced search
                {
                    // we only want the first record
                    var firstRecord = resultMatches.FirstOrDefault();
                    var motherId = firstRecord.mother_id;
                    var fatherId = firstRecord.father_id;
                    var id = firstRecord.id;

                    // search for ancestors
                    if (direction == "A")
                    {
                        int i = 0;
                        do
                        {
                            var predicateDirection = PredicateBuilder.True<PersonBirthPlace>();
                            predicateDirection = predicateDirection.And(x => x.id.Equals(motherId) || x.id.Equals(fatherId));

                            IQueryable<PersonBirthPlace> resultMatchesDirection = resultsWithPlaceCopy.AsQueryable().Where<PersonBirthPlace>(predicateDirection);

                            if (resultMatchesDirection != null && resultMatchesDirection.Any())
                            {
                                foreach (var result in resultMatchesDirection.ToList())
                                {
                                    Results person = CreateNewPerson(result);

                                    // let set the motherId and fatherId for next search
                                    if (!string.IsNullOrWhiteSpace(result.gender) && result.gender.ToUpper().Equals("F"))
                                    {
                                       motherId = result.mother_id;
                                       fatherId = result.father_id;
                                    }

                                    resultList.Add(person);
                                    i++;
                                }
                            }
                            else
                            {
                                i = 11;
                                break;
                            }
                        } while (i < 11); // we only want 10 records
                    }
                    else if(direction == "D") // search for Descendants  
                    {
                        var predicateDirection = PredicateBuilder.True<PersonBirthPlace>();
                        predicateDirection = predicateDirection.And(x => x.mother_id.Equals(id) || x.father_id.Equals(id));

                        IQueryable<PersonBirthPlace> resultMatchesDirection = resultsWithPlaceCopy.AsQueryable().Where<PersonBirthPlace>(predicateDirection);

                        //var resultMatchesDirection = resultsWithPlaceCopy.Where(ch => ch.mother_id == id || ch.father_id == id).ToList();

                        if (resultMatchesDirection != null)
                        {
                            var childrenList = GetChildrenList(resultMatchesDirection, resultsWithPlaceCopy);

                            resultList = childrenList.ToList();
                        }
                    }
                }
            }

            return resultList;
        }

        private void FetchDBandCache()
        {
            PeopleLocations listOfPeopleLocation = new PeopleLocations();

            try
            {
                using (StreamReader r = new StreamReader(HostingEnvironment.MapPath("~/data/data_large.json")))
                {
                    // get a list of people and places
                    string json = r.ReadToEnd();
                    listOfPeopleLocation = JsonConvert.DeserializeObject<PeopleLocations>(json);
                }

                if (listOfPeopleLocation != null)
                {
                    // check for people and cache for an hour
                    if (listOfPeopleLocation.people != null)
                    {
                        _cacheHelper.Add(cachePeople, listOfPeopleLocation.people, CacheTime);
                    }

                    // check for people and cache for an hour
                    if (listOfPeopleLocation.places != null)
                    {
                        _cacheHelper.Add(cachePlaces, listOfPeopleLocation.places, CacheTime);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Unable to fetch ancestry database due to: " + ex.Message, ex);
            }
        }

        private List<PersonBirthPlace> PeopleWithBirthPlace(List<Person> peoplelist, List<Place> placesList)
        {
            // let map all the people with their birthplace
            var resultsWithPlace = from person in peoplelist
                                   join place in placesList on person.place_id equals place.id into gj
                                   from subplace in gj.DefaultIfEmpty()
                                   select new PersonBirthPlace{ id = person.id, name = person.name, gender = person.gender, father_id = person.father_id, mother_id = person.mother_id, birthplace = (subplace == null ? String.Empty : subplace.name), level = person.level };

            return resultsWithPlace.ToList();

        }

        private IEnumerable<Results> GetChildrenList(IQueryable<PersonBirthPlace> peopleWithBirthPlace, List<PersonBirthPlace> peopleWithBirthPlaceList)
        {
            if (peopleWithBirthPlace == null)
                return null;

            List<Results> personList = new List<Results>();
            var level = 0;

            foreach (var result in peopleWithBirthPlace.ToList())
            {
                Results person = CreateNewPerson(result);

                personList.Add(person);

                // get the origin level
                level = result.level;

                // let get children
                var getChildren = peopleWithBirthPlaceList.Where(c => c.father_id == result.id || c.mother_id == result.id).ToList();

                if (getChildren != null && getChildren.Any())
                {
                    GetChildrenForPerson(getChildren, peopleWithBirthPlaceList, personList, level, 0);
                }
            }

            return personList.ToList().OrderBy(x => x.level).Take(10);
        }

        private static void GetChildrenForPerson(List<PersonBirthPlace> childrenList, List<PersonBirthPlace> peopleWithBirthPlaceList, List<Results> personList, int originalLevel, int level)
        {
            var childlevel = 0;
            var maxLevel = originalLevel + 3; // allow 3 more level down as we only need 10 records

            foreach (var result in childrenList.ToList())
            {
                Results person = CreateNewPerson(result);

                personList.Add(person);
                childlevel = result.level;

                // let get children
                var motherId = result.id;

                if (level < maxLevel)
                {
                    var getChildren = peopleWithBirthPlaceList.Where(c => c.father_id == result.id || c.mother_id == result.id).ToList();

                    if (getChildren != null && getChildren.Any())
                    {
                        GetChildrenForPerson(getChildren, peopleWithBirthPlaceList, personList, originalLevel, childlevel);
                    }
                }
            }
        }

        private static Results CreateNewPerson(PersonBirthPlace personData)
        {

            Results person = new Results();
            person.id = personData.id;
            person.name = personData.name;

            if (!string.IsNullOrWhiteSpace(personData.gender) && personData.gender.ToUpper().Equals("F"))
            {
                person.gender = "Female";
            }
            else
            {
                person.gender = "Male";
            }

            // regex to match any digit and replace them with empty string
            var regex = new Regex("\\d");
            // replace all matches in input with empty string
            var birthplaceMinusNumber = regex.Replace(personData.birthplace, String.Empty);

            person.birthplace = birthplaceMinusNumber;
            person.level = personData.level;

            return person;
        }
    }
}