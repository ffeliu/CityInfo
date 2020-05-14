using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityid}/pointsofinterest")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetPointsOfInterest(int cityId) 
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

            if (city == null)
                return NotFound();

            return Ok(city.PointsOfInterest);
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
            validateModel(pointsOfInterestForCreationDTO);

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
                new { cityId, id = finalPointOfInterest.Id},
                finalPointOfInterest);
        }

        private void validateModel(PointsOfInterestForCreationDTO pointsOfInterestForCreationDTO)
        {
            if (String.IsNullOrEmpty(pointsOfInterestForCreationDTO.Name))
            {
                ModelState.AddModelError(
                    "Nome",
                    "Nome inválido");

                return;
            }

            if (String.IsNullOrEmpty(pointsOfInterestForCreationDTO.Description))
            {
                ModelState.AddModelError(
                    "Descrição",
                    "Descrição inválida");

                return;
            }

            if (pointsOfInterestForCreationDTO.Name.Length > 50)
            {
                ModelState.AddModelError(
                    "Tamanho do campo nome",
                    "O campo nome não pode ser maior que 50 caracteres");
            }

            if (pointsOfInterestForCreationDTO.Description.Length > 255)
            {
                ModelState.AddModelError(
                    "Tamanho do campo descrição",
                    "O campo descrição não pode ser maior que 255 caracteres");
            }
        }
    }
}