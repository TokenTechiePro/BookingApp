﻿using System;

namespace BookingApp.DTOs
{
    /// <summary>
    /// Helper object, lists all update properties.
    /// </summary>
    public class ResourceUpdateSubsetDto : ResourceDetailedDto
    {
        public DateTime UpdatedTime { get; set; }
        public string UpdatedUserId { get; set; }
    }
}
