using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using NLog;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityid}/pointsofinterest")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {
        private ILogger<PointsOfInterestController> _logger { get; set; }
        private IMailService _mailLocalService { get; set; }

        private ICityInfoRepository _cityInfoRepository;
        private IMapper _mapper;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger,
            IMailService mailLocalService,
            ICityInfoRepository cityInfoRepository,
            IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailLocalService = mailLocalService ?? throw new ArgumentNullException(nameof(mailLocalService));
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public IActionResult GetPointsOfInterest(int cityId)
        {
            try
            {
                if(!_cityInfoRepository.CityExists(cityId))
                {
                    _logger.LogInformation($"City with id {cityId} wasn't found when acessing points of interest");
                    return NotFound();
                }

                var poiForCity = _cityInfoRepository.GetPointsOfInterestForCity(cityId);

                //var poiResults = new List<PointsOfInterestDTO>();
                //foreach (var item in poiForCity)
                //{
                //    poiResults.Add(new PointsOfInterestDTO()
                //    {
                //        Id = item.Id,
                //        Name = item.Name
                //    });
                //}


                return Ok(_mapper.Map<IEnumerable<PointsOfInterestDTO>>(poiForCity));
            }
            catch (Exception)
            {
                _logger.LogInformation("Testando o NLOG");
                return StatusCode(500, "Testando o NLOG");
            }
        }


        [HttpGet("{id}", Name = "GetPointOfInterest")]
        public IActionResult GetPointOfInterest(int cityId, int id)
        {
            if (!_cityInfoRepository.CityExists(cityId))
            {
                _logger.LogInformation($"City with id {cityId} wasn't found when acessing points of interest");
                return NotFound();
            }

            var poi = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);

            if (poi == null)
                return NotFound();

            //var poiResults = new PointsOfInterestDTO()
            //{
            //    Id = poi.Id,
            //    Name = poi.Name
            //};

            return Ok(_mapper.Map<PointsOfInterestDTO>(poi));
        }

        [HttpPost]
        public IActionResult CreatePointOfInterest(int cityId,
            [FromBody] PointsOfInterestForCreationDTO pointsOfInterestForCreationDTO)
        {
            validateModelForCreation(pointsOfInterestForCreationDTO);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_cityInfoRepository.CityExists(cityId))
                return NotFound("City not Found");


            var finalPointOfInterest = _mapper.Map<Entities.PointOfInterest>(pointsOfInterestForCreationDTO);

            _cityInfoRepository.AddPointOfInterestForCity(cityId, finalPointOfInterest);

            _cityInfoRepository.Save();

            var cretedPointOfInterest = _mapper.Map<Models.PointsOfInterestDTO>(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest",
                new { cityId, id = cretedPointOfInterest.Id },
                cretedPointOfInterest);
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePointOfInterest(int cityId, int id,
            [FromBody] PointsOfInterestForUpdateDTO pointsOfInterestForUpdateDTO)
        {
            validateModelForUpdate(pointsOfInterestForUpdateDTO);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
                return NotFound("City not Found");

            var poi = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);
            if (poi == null)
                return NotFound("Point of Interest not Found");

            poi.Name = pointsOfInterestForUpdateDTO.Name;
            poi.Description = pointsOfInterestForUpdateDTO.Description;

            return NoContent();
        }

        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdatePointOfInterest(int cityId, int id,
            [FromBody] JsonPatchDocument<PointsOfInterestForUpdateDTO> patchDocument)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
                return NotFound("City not Found");

            var poi = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);
            if (poi == null)
                return NotFound("Point of Interest not Found");

            var pointOfInterestToPath = new PointsOfInterestForUpdateDTO()
            {
                Name = poi.Name,
                Description = poi.Description
            };

            patchDocument.ApplyTo(pointOfInterestToPath);

            validateModelForUpdate(pointOfInterestToPath);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            poi.Name = pointOfInterestToPath.Name;
            poi.Description = pointOfInterestToPath.Description;

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePointOfInterest(int cityId, int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
                return NotFound("City not Found");

            var poi = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);
            if (poi == null)
                return NotFound("Point of Interest not Found");

            city.PointsOfInterest.Remove(poi);

            _mailLocalService.Send("POI deletado",
                $"O POI de nome {poi.Name} e id {poi.Id} foi excluído");

            return NoContent();
        }

        private void validateModelForCreation(PointsOfInterestForCreationDTO pointOfInterest)
        {
            if (String.IsNullOrEmpty(pointOfInterest.Name))
            {
                ModelState.AddModelError(
                    "Nome",
                    "Nome inválido");

                return;
            }

            if (pointOfInterest.Name.Length > 50)
            {
                ModelState.AddModelError(
                    "Tamanho do campo nome",
                    "O campo nome não pode ser maior que 50 caracteres");
            }

            if (!String.IsNullOrEmpty(pointOfInterest.Description) && pointOfInterest.Description.Length > 255)
            {
                ModelState.AddModelError(
                    "Tamanho do campo descrição",
                    "O campo descrição não pode ser maior que 255 caracteres");
            }
        }

        private void validateModelForUpdate(PointsOfInterestForUpdateDTO pointOfInterest)
        {
            if (String.IsNullOrEmpty(pointOfInterest.Name))
            {
                ModelState.AddModelError(
                    "Nome",
                    "Nome inválido");

                return;
            }

            if (pointOfInterest.Name.Length > 50)
            {
                ModelState.AddModelError(
                    "Tamanho do campo nome",
                    "O campo nome não pode ser maior que 50 caracteres");
            }

            if (!String.IsNullOrEmpty(pointOfInterest.Description) && pointOfInterest.Description.Length > 255)
            {
                ModelState.AddModelError(
                    "Tamanho do campo descrição",
                    "O campo descrição não pode ser maior que 255 caracteres");
            }
        }
    }
}