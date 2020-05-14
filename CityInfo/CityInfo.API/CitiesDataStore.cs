using CityInfo.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API
{
    public class CitiesDataStore
    {
        public static CitiesDataStore Current { get; } = new CitiesDataStore();

        public List<CityDTO> Cities { get; set; }

        public CitiesDataStore()
        {
            Cities = new List<CityDTO>()
            {
               new CityDTO()
               {
                   Id = 1,
                   Name = "Brasilia",
                   Description = "bla bla bla",
                   PointsOfInterest = new List<PointsOfInterestDTO>()
                   {
                       new PointsOfInterestDTO()
                       {
                           Id = 1,
                           Name = "Ponto 1"
                       },
                        new PointsOfInterestDTO()
                       {
                           Id = 2,
                           Name = "Ponto 2"
                       }
                   }
               },
               new CityDTO()
               {
                   Id = 2,
                   Name = "São Paulo",
                   Description = "ble ble ble",
                   PointsOfInterest = new List<PointsOfInterestDTO>()
                   {
                       new PointsOfInterestDTO()
                       {
                           Id = 3,
                           Name = "Ponto 3"
                       },
                        new PointsOfInterestDTO()
                       {
                           Id = 4,
                           Name = "Ponto 4"
                       }
                   }
               },
               new CityDTO()
               {
                   Id = 3,
                   Name = "Porto",
                   Description = "bli bli bli",
                   PointsOfInterest = new List<PointsOfInterestDTO>()
                   {
                       new PointsOfInterestDTO()
                       {
                           Id = 5,
                           Name = "Ponto 5"
                       },
                        new PointsOfInterestDTO()
                       {
                           Id = 6,
                           Name = "Ponto 6"
                       }
                   }
               }
            };
        }


    }
}
