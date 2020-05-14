using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Models
{
    public class PointsOfInterestForCreationDTO
    {
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
