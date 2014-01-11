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
    /// Implementation of <see cref="INfieldSurveyDataService"/>
    /// </summary>
    internal class NfieldSurveyDataService : INfieldSurveyDataService, INfieldConnectionClientObject
    {
        /// <summary>
        /// See <see cref="INfieldSurveyDataService.PostAsync"/>
        /// </summary>
        public async Task<BackgroundTask> PostAsync(SurveyDownloadDataRequest surveyDownloadDataRequest)
        {
            var resposneMesage = await Client.PostAsJsonAsync(SurveyDataApi.AbsoluteUri, surveyDownloadDataRequest);
            var responseAsString = await resposneMesage.Content.ReadAsStringAsync();
            return await JsonConvert.DeserializeObjectAsync<BackgroundTask>(responseAsString).FlattenExceptions();
        }

        #region Implementation of INfieldConnectionClientObject

        public INfieldConnectionClient ConnectionClient { get; internal set; }

        public void InitializeNfieldConnection(INfieldConnectionClient connection)
        {
            ConnectionClient = connection;
        }

        #endregion

        private INfieldHttpClient Client
        {
            get { return ConnectionClient.Client; }
        }

        private Uri SurveyDataApi
        {
            get { return new Uri(ConnectionClient.NfieldServerUri.AbsoluteUri + "SurveyData/"); }
        }

    }
}