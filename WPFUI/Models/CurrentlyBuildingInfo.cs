﻿using System;

namespace WPFUI.Models
{
    public class CurrentlyBuildingInfo : Building
    {
        public int Level { get; set; }
        public DateTime Complete { get; set; }
    }
}