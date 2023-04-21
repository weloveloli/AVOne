// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.
#nullable disable

namespace AVOne.Models.Job
{
    public class JobStatusArgs
    {
        public virtual string Status { get; set; }

        public virtual double Progress { get; set; }
    }
}
