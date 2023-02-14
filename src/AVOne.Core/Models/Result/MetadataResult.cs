// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

#nullable disable

namespace AVOne.Models.Result
{
    using AVOne.Enum;
    using AVOne.Helper;
    using AVOne.Models.Info;

    public class MetadataResult<T>
    {
        // Images aren't always used so the allocation is a waste a lot of the time
        private List<LocalImageInfo> _images;
        private List<(string Url, ImageType Type)> _remoteImages;

        public MetadataResult()
        {
            ResultLanguage = "en";
        }

        public List<LocalImageInfo> Images
        {
            get => _images ??= new List<LocalImageInfo>();
            set => _images = value;
        }

        public List<(string Url, ImageType Type)> RemoteImages
        {
            get => _remoteImages ??= new List<(string Url, ImageType Type)>();
            set => _remoteImages = value;
        }
        public string ResultLanguage { get; set; }

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
