using AutoMapper;
using Hermes.Common;
using Hermes.Models;

namespace Hermes.Repositories;

public class GeneralSettingsRepository : SettingsRepository<GeneralSettings>
{
    private readonly GeneralSettings _settings;
    private readonly Mapper _mapper;

    public GeneralSettingsRepository(GeneralSettings settings, AesEncryptor aesEncryptor) : base(aesEncryptor)
    {
        this._settings = settings;
        this.FileName = "GeneralSettings.json";
        var config = new MapperConfiguration(cfg => cfg
            .CreateMap<GeneralSettings, GeneralSettings>()
            .ReverseMap());
        this._mapper = new Mapper(config);
    }
    
    public override void Save(GeneralSettings settings)
    {
        base.Save(_settings);
        _mapper.Map(settings, _settings);
    }

    public void Load()
    {
        var settings = this.Read();
        if (settings != null)
        {
            _mapper.Map(settings, _settings);
        }
    }
}