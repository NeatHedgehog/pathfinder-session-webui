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
    public class RSVPController : Controller
    {
        FilePathsConfig paths;

        public RSVPController(IOptions<FilePathsConfig> _filePathsConfig)
        {
            paths = _filePathsConfig.Value;
        }

        [HttpGet]
        public IActionResult RSVPIndex(Guid validationGuid)
        {
            ResponseModel rm;

            try
            {
                rm = new ResponseModel(validationGuid, paths.Attendance);
            }

            catch (System.IO.IOException)
            {
                return View("~/Views/Errors/GenericError.cshtml", "Something happened while trying to load your RSVP page. Please try again.");
            }

            return View(rm);
        }

        [HttpPost]
        public IActionResult SubmitRSVP([FromForm]ResponseModel _rm)
        {
            try
            {
                RSVPSubmissionGate.Submit(_rm.rsvpResponse, _rm.cancelReason, _rm.validationGuid, paths.Attendance);
            }

            catch (System.IO.IOException)
            {
                return View("~/Views/Errors/GenericError.cshtml", "Something happened while trying to submit your RSVP. Please try again.");
            }

            ResponseList rl;

            try
            {
                rl = new ResponseList(paths.Schedule, paths.Attendance);
            }

            catch (System.IO.IOException)
            {
                return View("~/Views/Errors/GenericError.cshtml", "Something happened while trying to load the list of RSVP responses. Please try again.");
            }

            return RedirectToAction("RSVPList");
            //return View("RSVPList", new ResponseList(paths.Schedule, paths.Attendance));
        }

        public IActionResult RSVPList()
        {
            List<ResponseList> rl;
            SessionList sl;

            try
            {
                rl = new List<ResponseList>();
                sl = new SessionList(paths.Schedule, DateTime.Now.AddDays(-1));

                for (int i = 0; i < sl.sessions.Length; i++)
                {
                    if (sl.sessions[i].ac == "Active")
                    {
                        rl.Add(new ResponseList(DateTime.Now.AddDays(-1), sl.sessions[i].id, paths.Schedule, paths.Attendance));
                    }
                }

                rl = rl.OrderBy(r => r.sessionDate).ToList();
            }

            catch (System.IO.IOException)
            {
                return View("~/Views/Errors/GenericError.cshtml", "Something happened while trying to load the list of RSVP responses. Please try again.");
            }

            return View(rl);
        }

        public IActionResult UpdateShowNoShow(string _show, Guid _rsvpGuid, Guid _passthroughkey)
        {
            if(_passthroughkey.ToString() != "5ac59a93-b1fb-4153-af95-b37e3636197d")
            {
                return View("~/Views/Errors/GenericError.cshtml", "uh-uh-uh, you didn't say the magic word");
            }

            RSVPSubmissionGate.Submit(_rsvpGuid, paths.Attendance, _show);

            return RedirectToAction("RSVPHistoryAdmin", new { key = _passthroughkey });
        }

        public IActionResult RSVPHistoryAdmin(Guid key)
        {
            if(key.ToString() != "5ac59a93-b1fb-4153-af95-b37e3636197d")
            {
                return View("~/Views/Errors/GenericError.cshtml", "uh-uh-uh, you didn't say the magic word");
            }

            List<ResponseList> rl;
            SessionList sl;

            try
            {
                rl = new List<ResponseList>();
                sl = new SessionList(paths.Schedule, null);

                for (int i = 0; i < sl.sessions.Length; i++)
                {
                        rl.Add(new ResponseList(null, sl.sessions[i].id, paths.Schedule, paths.Attendance) { adminKey = key});
                }

                rl = rl.OrderByDescending(r => r.sessionDate).ToList();
            }

            catch (System.IO.IOException)
            {
                return View("~/Views/Errors/GenericError.cshtml", "Something happened while trying to load the list of RSVP responses. Please try again.");
            }

            return View(rl);
        }

        public IActionResult RSVPHistory()
        {
            List<ResponseList> rl;
            SessionList sl;

            try
            {
                rl = new List<ResponseList>();
                sl = new SessionList(paths.Schedule, null);

                for (int i = 0; i < sl.sessions.Length; i++)
                {
                        rl.Add(new ResponseList(null, sl.sessions[i].id, paths.Schedule, paths.Attendance));
                }

                rl = rl.OrderByDescending(r => r.sessionDate).ToList();
            }

            catch (System.IO.IOException)
            {
                return View("~/Views/Errors/GenericError.cshtml", "Something happened while trying to load the list of RSVP responses. Please try again.");
            }

            return View(rl);
        }
    }
}
