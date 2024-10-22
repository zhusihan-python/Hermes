using System;
using Hermes.Models;

namespace Hermes.Repositories;

public interface ISettingsRepository
{
    event Action<Settings>? SettingsChanged;
    string Path { get; init; }
    string FileName { get; init; }
    void Save(Settings settings);
    Settings Load();
    Settings Read();
}