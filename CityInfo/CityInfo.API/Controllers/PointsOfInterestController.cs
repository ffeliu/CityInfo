using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger,
            IMailService mailLocalService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailLocalService = mailLocalService ?? throw new ArgumentNullException(nameof(mailLocalService));
        }

        [HttpGet]
        public IActionResult GetPointsOfInterest(int cityId)
        {
            try
            {
                var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

                if (city == null)
                    return NotFound();

                return Ok(city.PointsOfInterest);
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
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

            if (city == null)
                return NotFound();

            var point = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);

            if (point == null)
                return NotFound();

            return Ok(point);
        }

        [HttpPost]
        public IActionResult CreatePointOfInterest(int cityId,
            [FromBody] PointsOfInterestForCreationDTO pointsOfInterestForCreationDTO)
        {
            validateModelForCreation(pointsOfInterestForCreationDTO);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
                return NotFound("City not Found");

            var maxPointOfInterestedID = CitiesDataStore.Current.Cities.SelectMany(
                c => c.PointsOfInterest).Max(p => p.Id);

            var finalPointOfInterest = new PointsOfInterestDTO()
            {
                Id = ++maxPointOfInterestedID,
                Name = pointsOfInterestForCreationDTO.Name,
                Description = pointsOfInterestForCreationDTO.Description
            };

            city.PointsOfInterest.Add(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest",
                new { cityId, id = finalPointOfInterest.Id },
                finalPointOfInterest);
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