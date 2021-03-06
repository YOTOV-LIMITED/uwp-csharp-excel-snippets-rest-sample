﻿/*
 *  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 *  See LICENSE in the source repository root for complete license information.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using Office365Service;

namespace Microsoft.ExcelServices
{
    public class SessionInfo
    {
        public static string LastId = string.Empty;

        #region Properties
        private string id;
        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                LastId = id;
            }
        }
        public bool? PersistChanges { get; set; }
        #endregion

        #region Methods
        public static SessionInfo MapFromJson(JObject json)
        {
            var sessionInfo = new SessionInfo();
            sessionInfo.Id = RestApi.MapStringFromJson(json, "id");
            sessionInfo.PersistChanges = RestApi.MapBooleanFromJson(json, "persistChanges");
            return sessionInfo;
        }
        #endregion
    }
}
