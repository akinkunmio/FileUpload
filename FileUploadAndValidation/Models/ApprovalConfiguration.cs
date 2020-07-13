using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class ApprovalConfiguration
    {
    }

    public class ApprovalConfigError
    {
        public string responseCode { get; set; }
        public string responseDescription { get; set; }
    }

    public class ApprovalConfigResponseList
    {
        public string responseCode { get; set; }
        public List<ApprovalConfig> responseData { get; set; }
    }

    public class ApprovalConfig
    {
        public long Id { get; set; }
        public int NoOfLevels { get; set; }
        public string Status { get; set; }
        public string TenantId { get; set; }
        public string ApprovalName { get; set; }
        public List<ApprovalLevelConfig> ApprovalLevelConfigs { get; set; }
    }
    public class ApprovalLevelConfigRequest
    {
        public string LevelId { get; set; }
        public string OnRejection { get; set; }
        public List<ApprovalLevelCategoryWithUserIds> ApprovalLevelCategories { get; set; }
        public int MinimumLevelOfApproval { get; set; }
    }

    public class ApprovalLevelCategoryWithUserIds
    {
        public string Name { get; set; }
        public long Limit { get; set; }
        public IEnumerable<long> UserIds { get; set; }
    }

    public class ApprovalLevelConfig
    {
        public string levelId { get; set; }
        public string onRejection { get; set; }
        public List<ApprovalLevelCategoryWithRoleIds> approvalLevelCategories { get; set; }
        public int minimumLevelOfApproval { get; set; }
    }

    public class ApprovalLevelCategoryWithRoleIds
    {
        public string name { get; set; }
        public long limit { get; set; }
        public IEnumerable<long> roleIds { get; set; }
    }
}
