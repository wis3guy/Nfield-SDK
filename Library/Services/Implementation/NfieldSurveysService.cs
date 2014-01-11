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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nfield.Extensions;
using Nfield.Infrastructure;
using Nfield.Models;

namespace Nfield.Services.Implementation
{
    /// <summary>
    /// Implementation of <see cref="INfieldSurveysService"/>
    /// </summary>
    internal class NfieldSurveysService : INfieldSurveysService, INfieldConnectionClientObject
    {
        #region Implementation of INfieldSurveysService

        /// <summary>
        /// See <see cref="INfieldSurveysService.QueryAsync"/>
        /// </summary>
        public async Task<IQueryable<Survey>> QueryAsync()
        {
            var resposneMesage = await Client.GetAsync(SurveysApi.AbsoluteUri);
            var responseAsString = await resposneMesage.Content.ReadAsStringAsync();
            var surveysList =
                await JsonConvert.DeserializeObjectAsync<List<Survey>>(responseAsString).FlattenExceptions();
            return surveysList.AsQueryable();
        }

        /// <summary>
        /// See <see cref="INfieldSurveysService.AddAsync"/>
        /// </summary>
        public async Task<Survey> AddAsync(Survey survey)
        {
            if (survey == null)
            {
                throw new ArgumentNullException("survey");
            }

            var resposneMesage = await Client.PostAsJsonAsync(SurveysApi.AbsoluteUri, survey);
            var responseAsString = await resposneMesage.Content.ReadAsStringAsync();
            return await JsonConvert.DeserializeObjectAsync<Survey>(responseAsString).FlattenExceptions();
        }

        /// <summary>
        /// See <see cref="INfieldSurveysService.RemoveAsync"/>
        /// </summary>
        public async Task RemoveAsync(Survey survey)
        {
            if (survey == null)
            {
                throw new ArgumentNullException("survey");
            }

            await Client.DeleteAsync(SurveysApi.AbsoluteUri + survey.SurveyId).FlattenExceptions();
        }

        /// <summary>
        /// See <see cref="INfieldSurveysService.UpdateAsync"/>
        /// </summary>
        public async Task<Survey> UpdateAsync(Survey survey)
        {
            if (survey == null)
            {
                throw new ArgumentNullException("survey");
            }

            var updatedSurvey = new UpdateSurvey
            {
                ClientName = survey.ClientName,
                Description = survey.Description,
                SurveyName = survey.SurveyName,
                InterviewerInstruction = survey.InterviewerInstruction
            };

            var resposneMesage = await Client.PatchAsJsonAsync(SurveysApi + survey.SurveyId, updatedSurvey);
            var responseAsString = await resposneMesage.Content.ReadAsStringAsync();
            return await JsonConvert.DeserializeObjectAsync<Survey>(responseAsString).FlattenExceptions();
        }

        /// <summary>
        /// <see cref="INfieldSurveysService.UploadInterviewerFileInstructionsAsync(string,string)"/>
        /// </summary>
        public async Task UploadInterviewerFileInstructionsAsync(string filePath, string surveyId)
        {
            var fileName = Path.GetFileName(filePath);

            if(!File.Exists(filePath))
                throw new FileNotFoundException(fileName);

            var uri = GetInterviewerInstructionUri(surveyId, fileName);
            
            var byteArrayContent = new ByteArrayContent(File.ReadAllBytes(filePath));
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            await Client.PostAsync(uri, byteArrayContent).FlattenExceptions();
        }

        /// <summary>
        /// <see cref="INfieldSurveysService.UploadInterviewerFileInstructionsAsync(byte[], string ,string)"/>
        /// </summary>
        public async Task UploadInterviewerFileInstructionsAsync(byte[] fileContent, string fileName, string surveyId)
        {
            var uri = GetInterviewerInstructionUri(surveyId, fileName);
            
            var byteArrayContent = new ByteArrayContent(fileContent);
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            await Client.PostAsync(uri, byteArrayContent).FlattenExceptions();
        }

        /// <summary>
        /// See <see cref="INfieldSurveysService.QuotaQueryAsync"/>
        /// </summary>
        /// <returns></returns>
        public async Task<QuotaLevel> QuotaQueryAsync(string surveyId)
        {
            var uri = string.Format(@"{0}{1}/{2}", SurveysApi.AbsoluteUri, surveyId, QuotaControllerName);

            var resposneMesage = await Client.GetAsync(uri);
            var responseAsString = await resposneMesage.Content.ReadAsStringAsync();
            return await JsonConvert.DeserializeObjectAsync<QuotaLevel>(responseAsString).FlattenExceptions();
        }

        /// <summary>
        /// See <see cref="INfieldSurveysService.SamplingPointsQueryAsync"/>
        /// </summary>
        public async Task<IQueryable<SamplingPoint>> SamplingPointsQueryAsync(string surveyId)
        {
            var uri = string.Format(@"{0}{1}/{2}", SurveysApi.AbsoluteUri, surveyId, SamplingPointsControllerName);

            var resposneMesage = await Client.GetAsync(uri);
            var responseAsString = await resposneMesage.Content.ReadAsStringAsync();
            var samplingPointsList =
                await JsonConvert.DeserializeObjectAsync<List<SamplingPoint>>(responseAsString).FlattenExceptions();
            return samplingPointsList.AsQueryable();
        }

        /// <summary>
        /// See <see cref="INfieldSurveysService.SamplingPointQueryAsync"/>
        /// </summary>
        public async Task<SamplingPoint> SamplingPointQueryAsync(string surveyId, string samplingPointId)
        {
            var uri = string.Format(@"{0}{1}/{2}/{3}", SurveysApi.AbsoluteUri, surveyId, SamplingPointsControllerName,
                samplingPointId);

            var resposneMesage = await Client.GetAsync(uri);
            var responseAsString = await resposneMesage.Content.ReadAsStringAsync();
            return await JsonConvert.DeserializeObjectAsync<SamplingPoint>(responseAsString).FlattenExceptions();
        }

        /// <summary>
        /// See <see cref="INfieldSurveysService.SamplingPointUpdateAsync"/>
        /// </summary>
        public async Task<SamplingPoint> SamplingPointUpdateAsync(string surveyId, SamplingPoint samplingPoint)
        {
            if (samplingPoint == null)
            {
                throw new ArgumentNullException("samplingPoint");
            }

            var updatedSamplingPoint = new UpdateSamplingPoint
            {
                Name = samplingPoint.Name,
                Description = samplingPoint.Description,
                FieldworkOfficeId = samplingPoint.FieldworkOfficeId,
                GroupId = samplingPoint.GroupId
            };

            var uri = string.Format(@"{0}{1}/{2}/{3}", SurveysApi.AbsoluteUri, surveyId, SamplingPointsControllerName,
                samplingPoint.SamplingPointId);

            var resposneMesage = await Client.PatchAsJsonAsync(uri, updatedSamplingPoint);
            var responseAsString = await resposneMesage.Content.ReadAsStringAsync();
            return await JsonConvert.DeserializeObjectAsync<SamplingPoint>(responseAsString).FlattenExceptions();
        }

        /// <summary>
        /// See <see cref="INfieldSurveysService.SamplingPointAddAsync"/>
        /// </summary>
        public async Task<SamplingPoint> SamplingPointAddAsync(string surveyId, SamplingPoint samplingPoint)
        {
            var uri = string.Format(@"{0}{1}/{2}", SurveysApi.AbsoluteUri, surveyId, SamplingPointsControllerName);

            var resposneMesage = await Client.PostAsJsonAsync(uri, samplingPoint);
            var responseAsString = await resposneMesage.Content.ReadAsStringAsync();
            return await JsonConvert.DeserializeObjectAsync<SamplingPoint>(responseAsString).FlattenExceptions(); 
        }

        /// <summary>
        /// See <see cref="INfieldSurveysService.SamplingPointDeleteAsync"/>
        /// </summary>
        public async Task SamplingPointDeleteAsync(string surveyId, SamplingPoint samplingPoint)
        {
            if (samplingPoint == null)
            {
                throw new ArgumentNullException("samplingPoint");
            }
            var uri = string.Format(@"{0}{1}/{2}/{3}", SurveysApi.AbsoluteUri, surveyId, SamplingPointsControllerName,
                samplingPoint.SamplingPointId);
            await Client.DeleteAsync(uri).FlattenExceptions();
        }

        /// <summary>
        /// See <see cref="INfieldSurveysService.SamplingPointQuotaTargetsQueryAsync"/>
        /// </summary>
        public async Task<IQueryable<SamplingPointQuotaTarget>> SamplingPointQuotaTargetsQueryAsync(string surveyId, string samplingPointId)
        {
            var uri = string.Format(@"{0}{1}/{2}/{3}/{4}", SurveysApi.AbsoluteUri, surveyId,
                SamplingPointsControllerName, samplingPointId, SamplingPointsQuotaControllerName);

            var resposneMesage = await Client.GetAsync(uri);
            var responseAsString = await resposneMesage.Content.ReadAsStringAsync();
            var samplingPointQuotaTargetsList =
                await JsonConvert.DeserializeObjectAsync<List<SamplingPointQuotaTarget>>(responseAsString).FlattenExceptions();
            return samplingPointQuotaTargetsList.AsQueryable();
        }

        /// <summary>
        /// See <see cref="INfieldSurveysService.SamplingPointQuotaTargetQueryAsync"/>
        /// </summary>
        public async Task<SamplingPointQuotaTarget> SamplingPointQuotaTargetQueryAsync(string surveyId,
            string samplingPointId, string levelId)
        {
            var uri = string.Format(@"{0}{1}/{2}/{3}/{4}/{5}", SurveysApi.AbsoluteUri, surveyId,
                SamplingPointsControllerName, samplingPointId, SamplingPointsQuotaControllerName, levelId);

            var resposneMesage = await Client.GetAsync(uri);
            var responseAsString = await resposneMesage.Content.ReadAsStringAsync();
            return
                await JsonConvert.DeserializeObjectAsync<SamplingPointQuotaTarget>(responseAsString).FlattenExceptions();
        }

        /// <summary>
        /// See <see cref="INfieldSurveysService.SamplingPointQuotaTargetUpdateAsync"/>
        /// </summary>
        public async Task<SamplingPointQuotaTarget> SamplingPointQuotaTargetUpdateAsync(string surveyId, string samplingPointId, SamplingPointQuotaTarget samplingPointQuotaTarget)
        {
            if (samplingPointQuotaTarget == null)
            {
                throw new ArgumentNullException("samplingPointQuotaTarget");
            }

            var updatedSamplingPointQuotaTarget = new UpdateSamplingPointQuotaTarget
            {
                Target = samplingPointQuotaTarget.Target
            };

            var uri = string.Format(@"{0}{1}/{2}/{3}/{4}/{5}", SurveysApi.AbsoluteUri, surveyId,
                SamplingPointsControllerName, samplingPointId, SamplingPointsQuotaControllerName,
                samplingPointQuotaTarget.LevelId);

            var resposneMesage = await Client.PatchAsJsonAsync(uri, updatedSamplingPointQuotaTarget);
            var responseAsString = await resposneMesage.Content.ReadAsStringAsync();
            return
                await JsonConvert.DeserializeObjectAsync<SamplingPointQuotaTarget>(responseAsString).FlattenExceptions();
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

        private static string SamplingPointsControllerName
        {
            get { return "samplingpoints";  }
        }

        private static string QuotaControllerName
        {
            get { return "quota"; }
        }
        private static string SamplingPointsQuotaControllerName
        {
            get { return "quotatargets"; }
        }

        private Uri SurveysApi
        {
            get { return new Uri(ConnectionClient.NfieldServerUri.AbsoluteUri + "surveys/"); }
        }

        private static string SurveyInterviewerInstructionsControllerName
        {
            get { return "SurveyInterviewerInstructions"; }
        }

        /// <summary>
        /// Returns the URI to upload the interviewer instructions 
        /// based on the provided <paramref name="surveyId"/> and <paramref name="fileName"/>
        /// </summary>
        private string GetInterviewerInstructionUri(string surveyId, string fileName)
        {
            return string.Format(@"{0}{1}/{2}/?fileName={3}", ConnectionClient.NfieldServerUri.AbsoluteUri,
                SurveyInterviewerInstructionsControllerName, surveyId, fileName);
        }

    }

    /// <summary>
    /// Update model for a sampling point
    /// Instruction is not allowed to be updated (because this is a link to a pdf in blob storage)
    /// </summary>
    internal class UpdateSamplingPoint
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string FieldworkOfficeId { get; set; }
        public string GroupId { get; set; }
    }

    /// <summary>
    /// Update model for a sampling point's qouta target
    /// </summary>
    internal class UpdateSamplingPointQuotaTarget
    {
        public int? Target { get; set; }
    }

    /// <summary>
    /// Update model for a survey
    /// </summary>
    internal class UpdateSurvey
    {
        /// <summary>
        /// Name of the survey
        /// </summary>
        public string SurveyName { get; set; }

        /// <summary>
        /// Name of the survey client
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// The description of the survey
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The default interviewer instruction of a survey
        /// </summary>
        public string InterviewerInstruction { get; set; }
    }

}