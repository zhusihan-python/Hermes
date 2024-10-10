using Hermes.Common.Extensions;
using Hermes.Common.Parsers;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using Hermes.Types;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;

namespace Hermes.Builders;

public class UnitUnderTestBuilder
{
    public string Content { get; private set; } = "";
    public List<Defect> Defects { get; } = [];

    private string _serialNumber = "SCAN_ERROR";
    private string _responseFailMessage = "";
    private string _fileNameWithoutExtension = "fileName";
    private bool _isPass = true;
    private bool _isScanError = true;
    private string _message = "";
    private DateTime _createdAt = DateTime.Now;

    private readonly FileService _fileService;
    private readonly Random _random = new();
    private readonly ISettingsRepository _settingsRepository;
    private readonly ParserPrototype _parserPrototype;
    private readonly SfcResponseBuilder _sfcResponseBuilder;

    public UnitUnderTestBuilder(
        FileService fileService,
        ParserPrototype parserPrototype,
        ISettingsRepository settingsRepositoryRepository,
        SfcResponseBuilder sfcResponseBuilder)
    {
        this._fileService = fileService;
        this._settingsRepository = settingsRepositoryRepository;
        this._parserPrototype = parserPrototype;
        this._sfcResponseBuilder = sfcResponseBuilder;
        sfcResponseBuilder.SetOkContent();
    }

    public async Task<UnitUnderTest> BuildAsync(string fileFullPath)
    {
        var fileName = this._fileService.FileName(fileFullPath);
        var content = await this._fileService.TryReadAllTextAsync(fileFullPath);
        var parser = _parserPrototype.GetUnitUnderTestParser(_settingsRepository.Settings.LogfileType);
        if (parser != null)
        {
            content = await parser.GetContentAsync(content);
        }

        return Build(fileName, content);
    }

    public UnitUnderTest Build()
    {
        this.Content = this.GetTestContent();
        return Build(
            this._fileNameWithoutExtension +
            _settingsRepository.Settings.InputFileExtension.GetDescription()
            , this.Content);
    }

    public string GetTestContent()
    {
        var parser = _parserPrototype.GetUnitUnderTestParser(_settingsRepository.Settings.LogfileType);
        if (parser == null)
        {
            return "";
        }

        return parser.GetTestContent(this._serialNumber, this._isPass, this.Defects);
    }

    private UnitUnderTest Build(string fileName, string content)
    {
        var parser = _parserPrototype.GetUnitUnderTestParser(_settingsRepository.Settings.LogfileType);
        if (!HasValidExtension(fileName) || parser == null)
        {
            return UnitUnderTest.Null;
        }


        return new UnitUnderTest(fileName, content)
        {
            IsFail = parser.ParseIsFail(content),
            SerialNumber = parser.ParseSerialNumber(content),
            Defects = parser.ParseDefects(content),
            CreatedAt = this._createdAt,
            Message = this._message,
            SfcResponse = GetSfcResponse()
        };
    }

    private SfcResponse GetSfcResponse()
    {
        var sfcResponseBuilder = _sfcResponseBuilder
            .Clone()
            .SerialNumber(_serialNumber)
            .SetFailContent(_responseFailMessage);

        if (_isScanError)
        {
            sfcResponseBuilder.ScanError();
        }

        return sfcResponseBuilder.Build();
    }

    private bool HasValidExtension(string fileName)
    {
        return _settingsRepository.Settings.InputFileExtension.GetDescription() == "*.*" ||
               _settingsRepository.Settings.InputFileExtension.GetDescription()
                   .Contains(Path.GetExtension(fileName), StringComparison.OrdinalIgnoreCase);
    }

    public UnitUnderTestBuilder FileNameWithoutExtension(string fileName)
    {
        this._fileNameWithoutExtension = fileName;
        return this;
    }

    public UnitUnderTestBuilder SerialNumber(string serialNumber)
    {
        this._serialNumber = serialNumber;
        return this;
    }

    public UnitUnderTestBuilder ResponseFailMessage(string message)
    {
        this._responseFailMessage = message;
        return this;
    }

    public UnitUnderTestBuilder IsPass(bool isPass)
    {
        this._isPass = isPass;
        return this;
    }

    public UnitUnderTestBuilder InputFileExtension(FileExtension value)
    {
        this._settingsRepository.Settings.InputFileExtension = value;
        return this;
    }

    public UnitUnderTestBuilder LogfileType(LogfileType value)
    {
        this._settingsRepository.Settings.LogfileType = value;
        return this;
    }

    public UnitUnderTestBuilder AddRandomDefect(string? location = null, bool isBad = false)
    {
        var rnd = this._random.Next();
        this.Defects.Add(new Defect()
        {
            ErrorFlag = isBad ? ErrorFlag.Bad : ErrorFlag.Good,
            Location = location ?? $"L{rnd}",
            ErrorCode = $"EC{rnd}"
        });
        return this;
    }

    public UnitUnderTestBuilder AddDefect(Defect defect)
    {
        this.Defects.Add(defect);
        return this;
    }

    public UnitUnderTestBuilder Clone()
    {
        return new UnitUnderTestBuilder(
            this._fileService,
            this._parserPrototype,
            this._settingsRepository,
            this._sfcResponseBuilder);
    }

    public UnitUnderTestBuilder CreatedAt(DateTime createdAt)
    {
        this._createdAt = createdAt;
        return this;
    }

    public UnitUnderTestBuilder ScanError(bool scanError)
    {
        this._isScanError = scanError;
        return this;
    }

    public UnitUnderTestBuilder IsSfcFail(bool isFail)
    {
        return this;
    }

    public UnitUnderTestBuilder IsSfcTimeout(bool isTimeout)
    {
        if (isTimeout)
        {
            this._sfcResponseBuilder.SetTimeoutContent();
        }
        else
        {
            this._sfcResponseBuilder.SetOkContent();
        }

        return this;
    }

    public UnitUnderTestBuilder Message(string message)
    {
        this._message = message;
        return this;
    }
}