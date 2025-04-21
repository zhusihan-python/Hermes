using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core.Components;
using ConfigFactory.Core.Models;
using ConfigFactory.Core;
using Hermes.Common;
using Hermes.Language;
using Hermes.Models;
using Hermes.Repositories;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System;
using R3;

namespace Hermes.Features.Bender;

public abstract class ParamBaseConfigModel<TConfigModel> : ObservableObject, IConfigModule
{
    public string LocalPath { get; }
    public IConfigModule Shared { get; }
    public IValidationInterface? ValidationInterface { get; set; }
    public ConfigValidatorCollection Validators { get; } = new();
    public ConfigPropertyCollection Properties { get; }
    public ReactiveProperty<Settings> SettingsSaved { get; } = new();

    private readonly ILogger _logger;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IMapper _mapper;

    public ParamBaseConfigModel(
        ILogger logger,
        ISettingsRepository settingsRepository)
    {
        this._logger = logger;
        this._settingsRepository = settingsRepository;
        this.LocalPath = this._settingsRepository.Path;
        this.Properties = ConfigPropertyCollection.Generate<TConfigModel>();
        this.Shared = this;
        this._mapper = new MapperConfiguration(cfg => cfg
                .CreateMap<TConfigModel, Settings>()
                .ReverseMap())
            .CreateMapper();
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
        var result = Resources.ResourceManager.GetString(input) ?? input;
        return Resources.ResourceManager.GetString(input) ?? input;
    }

    public void Reset()
    {
        this.Load();
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(LocalPath)) return;
            var settings = this._settingsRepository.Load();
            this._mapper.Map(settings, this);
            this._logger.Debug($"Data loaded from {LocalPath}");
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

    public void Save()
    {
        try
        {
            var settings = _mapper.Map<Settings>(this);
            this._settingsRepository.Save(settings);
            this.SettingsSaved.Value = settings;
            this._logger.Debug($"Data saved from {LocalPath}");
        }
        catch (Exception ex)
        {
            var msg = $"Error saving general settings: {ex.Message}";
            _logger.Error(msg);
            throw new Exception(msg, ex);
        }
    }
}