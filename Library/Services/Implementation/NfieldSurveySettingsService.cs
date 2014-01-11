//    This file is part of Nfield.SDK.
//
//    Nfield.SDK is free software: you can redistribute it and/or modify
//    it under the terms of the GNU Lesser General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Nfield.SDK is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public License
//    along with Nfield.SDK.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nfield.Extensions;
using Nfield.Infrastructure;
using Nfield.Models;

namespace Nfield.Services.Implementation
{
    /// <summary>
    /// Implementation of <see cref="INfieldSurveySettingsService"/>
    /// </summary>
    internal class NfieldSurveySettingsService : INfieldSurveySettingsService, INfieldConnectionClientObject
    {
        #region Implementation of INfieldLanguagesService

        /// <summary>
        /// See <see cref="INfieldSurveySettingsService.QueryAsync"/>
        /// </summary>
        public async Task<IQueryable<SurveySetting>> QueryAsync(string surveyId)
        {
            CheckSurveyId(surveyId);

            var resposneMesage = await Client.GetAsync(SettingsApi(surveyId, null).AbsoluteUri);
            var responseAsString = await resposneMesage.Content.ReadAsStringAsync();
            var surveySettingsList =
                await JsonConvert.DeserializeObjectAsync<List<SurveySetting>>(responseAsString).FlattenExceptions();
            return surveySettingsList.AsQueryable();
        }

        /// <summary>
        /// See <see cref="INfieldSurveySettingsService.AddOrUpdateAsync"/>
        /// </summary>
        public async Task<SurveySetting> AddOrUpdateAsync(string surveyId, SurveySetting setting)
        {
            CheckSurveyId(surveyId);

            if (setting == null)
            {
                throw new ArgumentNullException("setting");
            }

            var resposneMesage = await Client.PostAsJsonAsync(SettingsApi(surveyId, null).AbsoluteUri, setting);
            var responseAsString = await resposneMesage.Content.ReadAsStringAsync();
            return await JsonConvert.DeserializeObjectAsync<SurveySetting>(responseAsString).FlattenExceptions();
        }

        #endregion

        #region Implementation of INfieldConnectionClientObject

        public INfieldConnectionClient ConnectionClient { get; internal set; }

        public void InitializeNfieldConnection(INfieldConnectionClient connection)
        {
            ConnectionClient = connection;
        }

        #endregion

        private static void CheckSurveyId(string surveyId)
        {
            if (surveyId == null)
                throw new ArgumentNullException("surveyId");
            if (surveyId.Trim().Length == 0)
                throw new ArgumentException("surveyId cannot be empty");
        }

        private INfieldHttpClient Client
        {
            get { return ConnectionClient.Client; }
        }

        private Uri SettingsApi(string surveyId, string id)
        {
            StringBuilder uriText = new StringBuilder(ConnectionClient.NfieldServerUri.AbsoluteUri);
            uriText.AppendFormat("Surveys/{0}/Settings", surveyId);
            if (!string.IsNullOrEmpty(id))
                uriText.AppendFormat("/{0}", id);
            return new Uri(uriText.ToString());
        }
    }
}