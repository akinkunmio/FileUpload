using Dapper;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.UploadServices;
using FilleUploadCore.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadAndValidation.Utils
{
    public class ApprovalUtil
    {
        private readonly IHttpService _httpService;
        private readonly ILogger<ApprovalUtil> _logger;
        public ApprovalUtil(IHttpService httpService, ILogger<ApprovalUtil> logger)
        {
            _httpService = httpService;
            _logger = logger;
        }


        private static IEnumerable<ApprovalLevelConfigRequest> TransformApprovalLevelFromResponse(
            List<ApprovalLevelConfig> levelConfigs)
        {
            var approvalLevelConfigList = new List<ApprovalLevelConfigRequest>();

            foreach (var item in levelConfigs)
            {
                var configLevelObj = new ApprovalLevelConfigRequest
                {
                    LevelId = item.levelId,
                    OnRejection = item.onRejection ?? "null",
                    ApprovalLevelCategories = ModifyRoleIdsToUserIds(item.approvalLevelCategories.AsList()),
                    MinimumLevelOfApproval = item.minimumLevelOfApproval
                };
                approvalLevelConfigList.Add(configLevelObj);
            }

            return approvalLevelConfigList;
        }

        private static List<ApprovalLevelCategoryWithUserIds> ModifyRoleIdsToUserIds(List<ApprovalLevelCategoryWithRoleIds> categoryWithRoleIds)
        {
            var approvalLevelConfigList = new List<ApprovalLevelCategoryWithUserIds>();

            if (categoryWithRoleIds == null || categoryWithRoleIds.Count < 1)
                return approvalLevelConfigList;
            foreach (var item in categoryWithRoleIds)
            {
                var categoryObj = new ApprovalLevelCategoryWithUserIds
                {
                    Limit = item.limit,
                    Name = item.name,
                    UserIds = item.roleIds
                };
                approvalLevelConfigList.Add(categoryObj);
            }
            return approvalLevelConfigList;
        }

        public async Task<bool> GetApprovalConfiguration(long? businessId, long? userId, long amount)
        {
            try
            {
                var config = await _httpService.GetApprovalConfiguration(businessId);
                var configLevelList = TransformApprovalLevelFromResponse(config.responseData[0].ApprovalLevelConfigs);
                bool isValidInitiator = false;
                for (int i = 0; i< configLevelList.FirstOrDefault().ApprovalLevelCategories.Count; i++)
                {
                    var users = configLevelList.FirstOrDefault().ApprovalLevelCategories[i].UserIds.ToList();
                    var limit = (configLevelList.FirstOrDefault().ApprovalLevelCategories[i].Limit / 100);
                    for (int j = 0; j < users.Count; j++)
                    {
                        if (!isValidInitiator)
                        {
                            isValidInitiator = userId == users[j] && amount <= limit;
                        }
                    }                
                }

                return isValidInitiator;

            }
            catch (AppException ex)
            {
                _logger.LogError($"Error while fetching approval configuration {ex.Message} | {ex.StackTrace}");
                throw new AppException(ex.Message, ex.StatusCode); ;
            }
            catch (Exception ex)
            {
                _logger.LogError($"unable to fetch approval configuration {ex.Message} | {ex.StackTrace}");
                throw new AppException("An unexpected error occured.Please try again later.", 400);
            }
        }
    }
}
