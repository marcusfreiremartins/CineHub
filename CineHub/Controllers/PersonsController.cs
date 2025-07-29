using Microsoft.AspNetCore.Mvc;
using CineHub.Models;
using CineHub.Services;
using CineHub.Models.ViewModels.Persons;
using CineHub.Configuration;
using Microsoft.Extensions.Options;

namespace CineHub.Controllers
{
    public class PersonsController : Controller
    {
        private readonly PersonService _personService;
        private readonly ImageSettings _imageSettings;

        public PersonsController(PersonService personService, IOptions<ImageSettings> imageSettings)
        {
            _personService = personService;
            _imageSettings = imageSettings.Value;
        }

        public async Task<IActionResult> Details(int id)
        {
            var person = await _personService.GetPersonByIdAsync(id);
            if (person == null)
            {
                return NotFound();
            }

            var movies = await _personService.GetPersonMoviesAsync(id);

            var viewModel = new PersonDetailsViewModel
            {
                Person = person,
                Movies = movies,
                ImageBaseUrl = _imageSettings.BaseUrl
            };

            ViewData["Title"] = person.Name;

            return View(viewModel);
        }
    }
}