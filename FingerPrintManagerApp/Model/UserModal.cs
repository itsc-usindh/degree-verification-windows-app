using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FingerPrintManagerApp.Model
{
    public class UserResponseModel
    {
        [JsonProperty("success")]
        public UserModel Success { get; set; }
    }
    public class UserModel
    {

        [JsonProperty("profile")]
        public ProfileModel Profile { get; set; }
        [JsonProperty("role")]
        public List<RoleModel> Roles { get; set; }
    }
    public class ProfileModel
    {
        [JsonProperty("USER_ID")]
        public string UserId { get; set; }

        [JsonProperty("FIRST_NAME")]
        public string FirstName { get; set; }

        [JsonProperty("LAST_NAME")]
        public string LastName { get; set; }

        [JsonProperty("EMAIL")]
        public string Email { get; set; }
    }
    public class RoleModel
    {
        [JsonProperty("ROLE_ID")]
        public string RoleId { get; set; }

        [JsonProperty("ROLE_NAME")]
        public string RoleName { get; set; }

        [JsonProperty("REMARKS")]
        public string Remarks { get; set; }

        [JsonProperty("ACTIVE")]
        public string Active { get; set; }

        [JsonProperty("KEYWORD")]
        public string Keyword { get; set; }

        [JsonProperty("DEPT_ID")]
        public string DeptId { get; set; }

        [JsonProperty("R_R_ID")]
        public string RRId { get; set; }

        [JsonProperty("USER_ID")]
        public string UserId { get; set; }
    }
}
