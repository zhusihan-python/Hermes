using AutoMapper;
using Hermes.Common;
using Hermes.Models;

namespace Hermes.Repositories;

public class GeneralSettingsRepository : SettingsRepository<GeneralSettings>
{
    private readonly Mapper _mapper;

    public GeneralSettingsRepository(AesEncryptor aesEncryptor) : base(aesEncryptor)
    {
        this.FileName = "GeneralSettings.json";
        var config = new MapperConfiguration(cfg => cfg
            .CreateMap<GeneralSettings, GeneralSettings>()
            .ReverseMap());
        this._mapper = new Mapper(config);
    }
}