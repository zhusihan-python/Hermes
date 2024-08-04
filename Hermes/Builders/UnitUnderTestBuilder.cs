using Hermes.Common.Parsers;
using Hermes.Models;
using Hermes.Services;
using Hermes.Types;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
using Hermes.Common.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Hermes.Builders;

public class UnitUnderTestBuilder
{
    public string Content { get; private set; } = "";
    public List<Defect> Defects { get; } = [];

    private string _serialNumber = "1A62TESTSERIALNUMBER";
    private string _fileName = "fileName";
    private bool _isPass = true;
    private DateTime _createdAt = DateTime.Now;

    private readonly FileService _fileService;
    private readonly Settings _settings;
    private readonly Random _random = new();
    private readonly ParserPrototype _parserPrototype;
    private readonly SfcResponseBuilder _sfcResponseBuilder;


    public UnitUnderTestBuilder(
        Settings settings,
        FileService fileService,
        ParserPrototype parserPrototype,
        SfcResponseBuilder sfcResponseBuilder)
    {
        this._fileService = fileService;
        this._settings = settings;
        this._fileName += settings.InputFileExtension.GetDescription();
        this._parserPrototype = parserPrototype;
        this._sfcResponseBuilder = sfcResponseBuilder;
        sfcResponseBuilder.SetOkContent();
    }

    public async Task<UnitUnderTest> BuildAsync(string fileFullPath)
    {
        var fileName = this._fileService.FileName(fileFullPath);
        var content = await this._fileService.TryReadAllTextAsync(fileFullPath);
        return Build(fileName, content);
    }

    public UnitUnderTest Build()
    {
        this.Content = this.GetContent();
        return Build(this._fileName, this.Content);
    }

    // TODO: Agregar botones para crear archivos Pass, Fail, Critical, Modo supervisor activado
    public string GetContent()
    {
        var content = $"""
                       {this._serialNumber}
                       1105
                       110733
                       1A62FMM00-600-G+30-TOP
                       {(this._isPass ? "PASS" : "FAIL")}
                       210323220021
                       220323002539
                       NA
                       9
                       0
                       Error flag,Recipe name,Paste ID,CAD link Gerber,Error code,Multi Number 
                       """;
        foreach (var defect in this.Defects)
        {
            content +=
                $"\n{(defect.ErrorFlag.ToString().ToUpper())};1A626AY00-600-G+A0-TOP;NA;{defect.Location};{defect.ErrorCode};1";
        }

        return content;
    }

    private UnitUnderTest Build(string fileName, string content)
    {
        var parser = _parserPrototype.GetUnderTestParser(_settings.LogfileType);
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
            SfcResponse = _sfcResponseBuilder
                .SerialNumber(_serialNumber)
                .Build()
        };
    }

    private bool HasValidExtension(string fileName)
    {
        return _settings.InputFileExtension.GetDescription() == "*.*" ||
               _settings.InputFileExtension.GetDescription()
                   .Contains(Path.GetExtension(fileName), StringComparison.OrdinalIgnoreCase);
    }

    public UnitUnderTestBuilder FileName(string fileName)
    {
        this._fileName = fileName;
        return this;
    }

    public UnitUnderTestBuilder SerialNumber(string serialNumber)
    {
        this._serialNumber = serialNumber;
        return this;
    }

    public UnitUnderTestBuilder IsPass(bool isPass)
    {
        this._isPass = isPass;
        return this;
    }

    public UnitUnderTestBuilder InputFileExtension(FileExtension value)
    {
        this._settings.InputFileExtension = value;
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
            this._settings,
            this._fileService,
            this._parserPrototype,
            this._sfcResponseBuilder);
    }

    public UnitUnderTestBuilder CreatedAt(DateTime createdAt)
    {
        this._createdAt = createdAt;
        return this;
    }

    public UnitUnderTestBuilder IsSfcFail(bool isFail)
    {
        if (isFail)
        {
            this._sfcResponseBuilder.SetFailContent();
        }
        else
        {
            this._sfcResponseBuilder.SetOkContent();
        }

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
}