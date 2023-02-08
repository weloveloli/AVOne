// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

#nullable disable

namespace AVOne.Models.Result
{
    using AVOne.Models.Info;
    using AVOne.Helper;

    public class MetadataResult<T>
    {
        public bool HasMetadata { get; set; }

        public T Item { get; set; }

        public string Provider { get; set; }

        public bool QueriedById { get; set; }

        public List<PersonInfo> People { get; set; }

        public void AddPerson(PersonInfo p)
        {
            People ??= new List<PersonInfo>();

            PeopleHelper.AddPerson(People, p);
        }

        /// <summary>
        /// Not only does this clear, but initializes the list so that services can differentiate between a null list and zero people.
        /// </summary>
        public void ResetPeople()
        {
            if (People == null)
            {
                People = new List<PersonInfo>();
            }
            else
            {
                People.Clear();
            }
        }
    }
}
