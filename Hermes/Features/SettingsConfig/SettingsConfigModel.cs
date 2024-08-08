using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core.Components;
using ConfigFactory.Core.Models;
using ConfigFactory.Core;
using Hermes.Common;
using Hermes.Language;
using Hermes.Repositories;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System;
using System.Threading.Tasks;

namespace Hermes.Features.SettingsConfig;

public abstract class SettingsConfigModel<TConfigModel, TSettingsModel> : ObservableObject, IConfigModule
{
    public string LocalPath { get; }
    public IConfigModule Shared { get; }
    public IValidationInterface? ValidationInterface { get; set; }
    public ConfigValidatorCollection Validators { get; } = new();
    public ConfigPropertyCollection Properties { get; }

    private readonly ILogger _logger;
    private readonly SettingsRepository<TSettingsModel> _settingsRepository;
    private readonly Mapper _mapper;

    public SettingsConfigModel(ILogger logger, SettingsRepository<TSettingsModel> settingsRepository)
    {
        this._logger = logger;
        this._settingsRepository = settingsRepository;
        this.LocalPath = this._settingsRepository.Path;
        this.Properties = ConfigPropertyCollection.Generate<TConfigModel>();
        this.Shared = this;
        var config = new MapperConfiguration(cfg => cfg
            .CreateMap<TConfigModel, TSettingsModel>()
            .ReverseMap());
        this._mapper = new Mapper(config);
        this.Load();
    }

    public bool Validate(out string? message, [MaybeNullWhen(true)] out ConfigProperty target)
    {
        message = null;
        target = null;
        return true;
    }

    public string Translate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        return Resources.ResourceManager?.GetString(input, Resources.Culture) ?? input;
    }

    public void Reset()
    {
        this.Load();
    }

    public void Load()
    {
        this._logger.Debug($"Loading data from {LocalPath}");
        try
        {
            if (!File.Exists(LocalPath)) return;
            var generalSettings = this._settingsRepository.Read();
            this._mapper.Map(generalSettings, this);
        }
        catch (Exception ex)
        {
            var msg = $"Error loading general settings: {ex.Message}";
            _logger.Error(msg);
            throw new Exception(msg, ex);
        }
    }

    public void Load(ref IConfigModule module)
    {
        this.Load();
    }

    public async void Save()
    {
        try
        {
            var generalSettings = _mapper.Map<TSettingsModel>(this);
            this._settingsRepository.Save(generalSettings);
            await Task.Delay(2000);
        }
        catch (Exception ex)
        {
            var msg = $"Error saving general settings: {ex.Message}";
            _logger.Error(msg);
            throw new Exception(msg, ex);
        }
    }
}