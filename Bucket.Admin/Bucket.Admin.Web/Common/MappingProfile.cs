
using AutoMapper;
using Bucket.Admin.Dto.Api;
using Bucket.Admin.Dto.Config;
using Bucket.Admin.Dto.Menu;
using Bucket.Admin.Dto.Platform;
using Bucket.Admin.Dto.Project;
using Bucket.Admin.Dto.Role;
using Bucket.Admin.Dto.User;
using Bucket.Admin.Model.Config;
using Bucket.Admin.Model.Setting;
using Bucket.Admin.Model.User;

namespace Pinzhi.Platform.WebApi.Common
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SetPlatformInput, PlatformModel>();
            CreateMap<SetMenuInput, MenuModel>();
            CreateMap<SetRoleInput, RoleModel>();
            CreateMap<SetUserInput, UserModel>();
            CreateMap<SetApiInput, ApiModel>();
            CreateMap<SetAppInfoInput, AppModel>();
            CreateMap<SetAppProjectInfoInput, AppNamespaceModel>();
            CreateMap<SetAppConfigInfoInput, AppConfigModel>()
                .ForMember(destination => destination.CreateTime, option => option.Ignore())
                .ForMember(destination => destination.LastTime, option => option.Ignore())
                .ForMember(destination => destination.Version, option => option.Ignore());
            CreateMap<SetProjectInput, ProjectModel>();
        }
    }
}
