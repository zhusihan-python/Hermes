using Hermes.Common.Parsers;
using Hermes.Models;
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

    private string _serialNumber = "1A62TESTSERIALNUMBER";
    private string _fileName = "fileName";
    private bool _isPass = true;

    private readonly FileService _fileService;
    private readonly Settings _settings;
    private readonly Random _random = new();
    private readonly ParserPrototype _parserPrototype;


    public UnitUnderTestBuilder(
        Settings settings,
        FileService fileService,
        ParserPrototype parserPrototype)
    {
        this._fileService = fileService;
        this._settings = settings;
        this._fileName += settings.InputFileExtension;
        this._parserPrototype = parserPrototype;
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

    private string GetContent()
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
        };
    }

    private bool HasValidExtension(string fileName)
    {
        return _settings.InputFileExtension == "*.*" ||
               _settings.InputFileExtension.Contains(Path.GetExtension(fileName), StringComparison.OrdinalIgnoreCase);
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

    public UnitUnderTestBuilder InputFileExtension(string value)
    {
        this._settings.InputFileExtension = value;
        return this;
    }

    public UnitUnderTestBuilder AddRandomDefect()
    {
        var rnd = this._random.Next();
        this.Defects.Add(new Defect()
        {
            ErrorFlag = ErrorFlag.Good,
            Location = $"L{rnd}",
            ErrorCode = $"EC{rnd}"
        });
        return this;
    }
}