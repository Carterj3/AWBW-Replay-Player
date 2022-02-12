﻿using System;
using osu.Framework.IO.Network;

namespace AWBWApp.Game.API
{
    /// <summary>
    /// Submit a request to get the username of a player. This requires downloading a html page as AWBW does not have an API.
    /// </summary>
    public class UsernameWebRequest : WebRequest
    {
        public long UserID { get; private set; }

        public string Username { get; private set; }

        private const string username_index = "Username:";

        public UsernameWebRequest(long userID)
            : base($"https://awbw.amarriner.com/profile.php?users_id= {userID}")
        {
            UserID = userID;
        }

        protected override void ProcessResponse()
        {
            base.ProcessResponse();

            var htmlPage = GetResponseString();
            var idx = htmlPage.IndexOf(username_index);

            if (idx < 0)
                throw new Exception("Unable to find username from profile page.");

            var usernameStartItalicsMarker = htmlPage.IndexOf("<i>", idx);
            if (usernameStartItalicsMarker < 0)
                throw new Exception("Unable to find username from profile page.");

            usernameStartItalicsMarker += 3;

            var usernameEndItalicsMarker = htmlPage.IndexOf("</i>", usernameStartItalicsMarker);
            if (usernameEndItalicsMarker < 0)
                throw new Exception("Unable to find username from profile page.");

            Username = htmlPage[usernameStartItalicsMarker..usernameEndItalicsMarker];
        }
    }
}
