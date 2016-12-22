using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMailerWebUI.Models;
using WebApplication.Models.ManageViewModels;
using Microsoft.Extensions.Options;

namespace WebApplication.Controllers
{
    public class SessionsController : Controller
    {
        FilePathsConfig paths;
        public SessionsController(IOptions<FilePathsConfig> _filePathsConfig)
        {
            paths = _filePathsConfig.Value;
        }

        [HttpGet]
        public IActionResult SessionIndex()
        {
            SessionList sl;

            try
            {
                sl = new SessionList(paths.Schedule, DateTime.Now.AddDays(-1));
            }

            catch (System.IO.IOException)
            {
                return View("~/Views/Errors/GenericError.cshtml", "Something happened while trying to read the session list. Please try again.");
            }

            return View(sl);
        }

        [HttpGet]
        public IActionResult AdminSessionIndex()
        {
            SessionList sl;

            try
            {
                sl = new SessionList(paths.Schedule, DateTime.Now.AddDays(-1));
            }

            catch (System.IO.IOException)
            {
                return View("~/Views/Errors/GenericError.cshtml", "Something happened while trying to read the session list. Please try again.");
            }

            return View(sl);
        }

        [HttpPost]
        public IActionResult AddSession([FromForm]NewSession _newSession)
        {
            Session _session = _newSession.BuildSession();

            return RedirectToAction("AdminSessionIndex");
        }
    }
}