using Hermes.Models;
using Hermes.Types;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Hermes.Models.Parsers;
using Hermes.Services;

namespace Hermes.Builders;

public class UnitUnderTestBuilder
{
    public string Content { get; private set; } = "";
    public List<Defect> Defects { get; private set; } = [];

    private string _serialNumber = "1A62TESTSERIAL";
    private string _fileName = "fileName";
    private bool _isPass = true;
    private readonly Random _random = new();
    private readonly FileService _fileService;

    public UnitUnderTestBuilder(FileService fileService)
    {
        this._fileService = fileService;
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

    public UnitUnderTestBuilder Defect(Defect defect)
    {
        this.Defects.Add(defect);
        return this;
    }

    public UnitUnderTest Build()
    {
        this.Content = this.GetContent();
        return Build(this._fileName, this.Content);
    }

    public async Task<UnitUnderTest> BuildAsync(string fileFullPath)
    {
        var fileName = this._fileService.FileName(fileFullPath);
        var content = await this._fileService.TryReadAllTextAsync(fileFullPath);
        return Build(fileName, content);
    }

    private static UnitUnderTest Build(string fileName, string content)
    {
        return Settings.Instance.LogfileType switch
        {
            LogfileType.TriDefault => new UnitUnderTest(fileName, content, new UnitUnderTestParser()),
            _ => new UnitUnderTest(fileName, content)
        };
    }

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
            content += this.ToString(defect);
        }

        return content;
    }

    private string ToString(Defect defect)
    {
        return
            $"\n{(defect.ErrorFlag.ToString().ToUpper())};1A626AY00-600-G+A0-TOP;NA;{defect.Location};{defect.ErrorCode};1";
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