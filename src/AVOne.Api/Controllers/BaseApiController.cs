// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Api.Controllers
{
    using System.Net.Mime;
    using AVOne.Impl.Json;
    using Microsoft.AspNetCore.Mvc;
    /// <summary>
    /// Base api controller for the API setting a default route.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Produces(
        MediaTypeNames.Application.Json,
        JsonDefaults.CamelCaseMediaType,
        JsonDefaults.PascalCaseMediaType)]
    public class BaseApiController : ControllerBase
    {
    }
}
