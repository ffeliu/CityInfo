using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private ICityInfoRepository _cityInfoRepository;
        private IMapper _mapper;

        public CitiesController(ICityInfoRepository cityInfoRepository,
            IMapper mapper)
        {
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public IActionResult GetCities()
        {
            var cityEntities = _cityInfoRepository.GetCities();

            //var results = new List<CityWithoutPointsOfInterestDTO>();

            //foreach (var item in cityEntities)
            //{
            //    results.Add(new CityWithoutPointsOfInterestDTO
            //    {
            //        Id = item.Id,
            //        Name = item.Name,
            //        Description = item.Description
            //    });
            //    ;
            //}

            var results = _mapper.Map<IEnumerable<CityWithoutPointsOfInterestDTO>>(cityEntities);

            return Ok(results);
        }

        [HttpGet("{id}")]
        public IActionResult GetCity(int id, bool includePointsOfInterest = false)
        {
            var city = _cityInfoRepository.GetCity(id, includePointsOfInterest);

            if (city == null)
                return NotFound();

            if (includePointsOfInterest)
            {
                //var cityResult = new CityDTO()
                //{
                //    Id = city.Id,
                //    Name = city.Name,
                //    Description = city.Description
                //};


                //foreach (var poi in city.PointsOfInterest)
                //{
                //    cityResult.PointsOfInterest.Add(
                //        new PointsOfInterestDTO()
                //        {
                //            Id = poi.Id,
                //            Name = poi.Name
                //        });
                //}

                //return Ok(cityResult);

                return Ok(_mapper.Map<CityDTO>(city));
            }

            //var cityWithoutPointsOfInterest = new CityWithoutPointsOfInterestDTO()
            //{
            //    Id = city.Id,
            //    Name = city.Name,
            //    Description = city.Description
            //};

            return Ok(_mapper.Map<CityWithoutPointsOfInterestDTO>(city));
        }
    }
}
