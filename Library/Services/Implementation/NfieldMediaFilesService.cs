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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nfield.Extensions;
using Nfield.Infrastructure;

namespace Nfield.Services.Implementation
{
    internal class NfieldMediaFilesService : INfieldMediaFilesService, INfieldConnectionClientObject
    {

        #region INfieldMediaFilesService Members

        public async Task<IQueryable<string>> QueryAsync(string surveyId)
        {
            if (string.IsNullOrEmpty(surveyId))
            {
                throw new ArgumentNullException("surveyId");
            }

            var resposneMesage = await Client.GetAsync(MediaFilesApi(surveyId, null).AbsoluteUri);
            var responseAsString = await resposneMesage.Content.ReadAsStringAsync();
            var mediaFilesList =
                await JsonConvert.DeserializeObjectAsync<List<string>>(responseAsString).FlattenExceptions();
            return mediaFilesList.AsQueryable();
        }

        public async Task RemoveAsync(string surveyId, string fileName)
        {
            if (string.IsNullOrEmpty(surveyId))
            {
                throw new ArgumentNullException("surveyId");
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            await Client.DeleteAsync(MediaFilesApi(surveyId, fileName).AbsoluteUri).FlattenExceptions();
        }

        public async Task AddOrUpdateAsync(string surveyId, string fileName, byte[] content)
        {
            if (string.IsNullOrEmpty(surveyId))
            {
                throw new ArgumentNullException("surveyId");
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            var postContent = new ByteArrayContent(content);
            postContent.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");
            await Client.PutAsync(MediaFilesApi(surveyId, fileName).AbsoluteUri, postContent).FlattenExceptions();
        }

        #endregion

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

        private Uri MediaFilesApi(string surveyId, string fileName)
        {
            StringBuilder uriText = new StringBuilder(ConnectionClient.NfieldServerUri.AbsoluteUri);
            uriText.AppendFormat("Surveys/{0}/MediaFiles", surveyId);
            if (!string.IsNullOrEmpty(fileName))
                uriText.AppendFormat("/{0}", Uri.EscapeUriString(fileName));
            return new Uri(uriText.ToString());
        }
    }
}
