using System;
using System.Collections.Generic;
using System.IO;

namespace AutoMailerWebUI.Models
{
    public class ResponseModel
    {
        public string cancelReason {get; set;}
        public Guid validationGuid {get; set;}
        public string rsvpResponse {get; set;}
        public bool valid {get; set;}
        public string memberName {get; set;}
        public string memberEmail {get; set;}
        public string showNoShow {get; set;}

        public ResponseModel(Guid _validationGuid, string _responsePath)
        {
            string[] masterAttendanceLogs = File.ReadAllLines(_responsePath);
            valid = false;
            cancelReason = "";
            rsvpResponse = "";
            validationGuid = new Guid();

            for (int i = 0; i < masterAttendanceLogs.Length; i++)
            {
                string[] logDetails = masterAttendanceLogs[i].Split('|');

                if (logDetails[1] == _validationGuid.ToString())
                {
                    valid = true;
                    validationGuid = _validationGuid;
                    return;
                }
            }
        }

        public ResponseModel()
        {
            cancelReason = "";
            validationGuid = new Guid();
            rsvpResponse = "";
            valid = false;
        }
    }

    public class ResponseList
    {
        public ResponseModel[] responses;
        public DateTime sessionDate;
        public Guid adminKey {get;set;}

        public ResponseList()
        {
            
        }

        public ResponseList(string _schedule, string _attendance) : this(null, null, _schedule, _attendance) {}

        /// <summary>
        /// DateTime? _dateFrom, Guid? _sessionID, optional filters. Specify one or the other or both.
        /// </summary>
        /// <param name="_dateFrom"></param>
        /// <param name="_sessionID"></param>
        public ResponseList(DateTime? _dateFrom, Guid? _sessionID, string _schedule, string _attendance)
        {
            string[] masterSchedule = File.ReadAllLines(_schedule);
            string[] masterAttendanceLogs = File.ReadAllLines(_attendance);
            List<ResponseModel> topSessionResponses = new List<ResponseModel>();
            
            KeyValuePair<Guid, DateTime> topScheduleID = new KeyValuePair<Guid, DateTime>(Guid.Empty, DateTime.Now);

            for (int i = 0; i < masterSchedule.Length; i++)
            {
                string[] entryDetails = masterSchedule[i].Split('|');

                if (topScheduleID.Key == Guid.Empty
                    && entryDetails[2] == "a" && entryDetails[5] == "y"
                    && (_dateFrom == null || (Convert.ToDateTime(entryDetails[1]) >= _dateFrom))
                    && (_sessionID == null || _sessionID.Equals(Guid.Parse(entryDetails[0])))
                )
                {
                    topScheduleID = new KeyValuePair<Guid, DateTime>(Guid.Parse(entryDetails[0]), Convert.ToDateTime(entryDetails[1]));
                }
            }

            sessionDate = topScheduleID.Value;

            for (int i = 0; i < masterAttendanceLogs.Length; i++)
            {
                string[] entryDetails = masterAttendanceLogs[i].Split('|');

                if (Guid.Parse(entryDetails[0]) == topScheduleID.Key)
                {
                    ResponseModel newResponse = new ResponseModel()
                        {
                            cancelReason = entryDetails[5]
                            , memberEmail = entryDetails[2]
                            , memberName = entryDetails[6]
                            , rsvpResponse = entryDetails[3]
                            , validationGuid = Guid.Parse(entryDetails[1])
                            , showNoShow = entryDetails[4]
                        };
                    
                    if (newResponse.cancelReason == "X")
                    {
                        newResponse.cancelReason = "";
                    }

                    if (newResponse.rsvpResponse == "NR")
                    {
                        newResponse.rsvpResponse = "Not Responded";
                    }

                    if (newResponse.rsvpResponse == "G")
                    {
                        newResponse.rsvpResponse = "Going";
                    }

                    if (newResponse.rsvpResponse == "C")
                    {
                        newResponse.rsvpResponse = "Not Going";
                    }

                    if (newResponse.showNoShow == "S")
                    {
                        newResponse.showNoShow = "Show";
                    }

                    if (newResponse.showNoShow == "N")
                    {
                        newResponse.showNoShow = "No Show";
                    }

                    if (newResponse.showNoShow == "X")
                    {
                        newResponse.showNoShow = "";
                    }
                    
                    topSessionResponses.Add(newResponse);
                }
            }

            responses = topSessionResponses.ToArray();
        }
    }

    public static class RSVPSubmissionGate
    {
        public static bool Submit(string _rsvpResponse, string _cancelReason, Guid _rsvpGuid, string _attendance)
        {
            return Submit(_rsvpResponse, _cancelReason, _rsvpGuid, _attendance, null);
        }

        public static bool Submit(Guid _rsvpGuid, string _attendance, string _showNoShow)
        {
            return Submit(null, null, _rsvpGuid, _attendance, _showNoShow);
        }

        private static bool Submit(string _rsvpResponse, string _cancelReason, Guid _rsvpGuid, string _attendance, string _showNoShow)
        {
            bool success = true;

            try
            {
                string[] masterAttendanceLogs = File.ReadAllLines(_attendance);
                List<string> updatedAttendanceLogs = new List<string>();

                for (int i = 0; i < masterAttendanceLogs.Length; i++)
                {
                    string[] entryDetails = masterAttendanceLogs[i].Split('|');

                    if (Guid.Parse(entryDetails[1]) == _rsvpGuid)
                    {
                        string entryBuilderString = "";

                        entryBuilderString += entryDetails[0]
                            + "|" + entryDetails[1]
                            + "|" + entryDetails[2];
                        
                        if (_rsvpResponse != null && _rsvpResponse != "")
                        {
                            entryBuilderString += "|" + _rsvpResponse;
                        }

                        else
                        {
                            entryBuilderString += "|" + entryDetails[3];
                        }

                        if (_showNoShow != null && _showNoShow != "")
                        {
                            entryBuilderString += "|" + _showNoShow;
                        }

                        else
                        {
                            entryBuilderString += "|" + entryDetails[4];
                        }

                        if (_cancelReason != null && _cancelReason != "")
                        {
                            entryBuilderString += "|" + _cancelReason;
                        }

                        else
                        {
                            entryBuilderString += "|" + entryDetails[5];
                        }

                        entryBuilderString += "|" + entryDetails[6];

                        updatedAttendanceLogs.Add(entryBuilderString);
                    }

                    else
                    {
                        updatedAttendanceLogs.Add(masterAttendanceLogs[i]);
                    }
                }

                File.WriteAllLines(_attendance, updatedAttendanceLogs);
            }

            catch
            {
                success = false;
            }

            return success;
        }
    }

    public class SessionList
    {
        public Session[] sessions {get; set;}

        public SessionList(string _scheduleList, DateTime? _dateFrom)
        {
            string[] masterSchedule = File.ReadAllLines(_scheduleList);
            List<Session> tempSchedule = new List<Session>();

            for(int i = 0; i < masterSchedule.Length; i++)
            {
                string[] entryDetails = masterSchedule[i].Split('|');

                if (_dateFrom == null || Convert.ToDateTime(entryDetails[1]) >= _dateFrom)
                {
                    string[] emails = entryDetails[4].Split(',');
                    string[] names = entryDetails[3].Split(',');
                    string activeCanceled = "Active";

                    if (entryDetails[2] == "c")
                    {
                        activeCanceled = "Canceled";
                    }

                    if (entryDetails[2] == "f")
                    {
                        activeCanceled = "Finished";
                    }

                    tempSchedule.Add(new Session(){
                        memberEmails = emails
                        , memberNames = names
                        , sessionDate = Convert.ToDateTime(entryDetails[1])
                        , remindersSent = (entryDetails[5] == "y")
                        , ac = activeCanceled
                        , id = Guid.Parse(entryDetails[0])
                        });
                }
            }

            sessions = tempSchedule.ToArray();
        }
    }

    public class Session
    {
        public string[] memberNames {get; set;}
        public string[] memberEmails {get; set;}
        public DateTime sessionDate {get; set;}
        public bool remindersSent {get; set;}
        public string ac {get;set;}
        public Guid id {get; set;}
    }

    public class FilePathsConfig
    {
        public string Attendance {get; set;}
        public string Schedule {get; set;}
    }

    public class NewSession
    {
        string newDate {get;set;}
        string newAmPm {get;set;}
        int newHh {get;set;}
        int newMm {get;set;}
        string newEmails {get;set;}
        string newMembers {get;set;}

        public Session BuildSession()
        {
            return new Session();
        }
    }
}
