using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMailerWebUI.Models;
using WebApplication.Models.ManageViewModels;

namespace WebApplication.Controllers
{
    public class ErrorsController : Controller
    {
        public IActionResult GenericError()
        {
            return View("GenericError");
        }
    }
}