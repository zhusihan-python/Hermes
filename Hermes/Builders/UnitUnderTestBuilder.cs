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
    public static string DummySerialNumber = "Dummy";

    public string Content { get; private set; } = "";
    public List<Defect> Defects { get; } = [];

    private string _serialNumber = "SCAN_ERROR";
    private string _responseFailMessage = "";
    private string _fileNameWithoutExtension = "fileName";
    private bool _isPass = true;
    private bool _isScanError;
    private bool _isSfcResponseOk;
    private string _message = "";
    private DateTime _createdAt = DateTime.Now;

    private readonly FileService _fileService;
    private readonly Random _random = new();
    private readonly ParserPrototype _parserPrototype;
    private readonly SfcResponseBuilder _sfcResponseBuilder;
    private readonly Session _session;

    public UnitUnderTestBuilder(
        FileService fileService,
        ParserPrototype parserPrototype,
        Session session,
        SfcResponseBuilder sfcResponseBuilder)
    {
        this._fileService = fileService;
        this._session = session;
        this._parserPrototype = parserPrototype;
        this._sfcResponseBuilder = sfcResponseBuilder;
        sfcResponseBuilder.SetOkSfcResponse();
    }

    public UnitUnderTest Build()
    {
        this.Content = this.GetTestContent();
        return Build(
            this._fileNameWithoutExtension +
            _session.Settings.InputFileExtension.GetDescription()
            , this.Content);
    }

    public string GetTestContent()
    {
        var parser = _parserPrototype.GetUnitUnderTestParser(_session.Settings.LogfileType);
        if (parser == null)
        {
            return "";
        }

        return parser.GetTestContent(this._serialNumber, this._isPass, this.Defects);
    }

    public UnitUnderTest Build(TextDocument textDocument)
    {
        return Build(textDocument.FullPath, textDocument.Content);
    }

    private UnitUnderTest Build(string fullPath, string content)
    {
        var parser = _parserPrototype.GetUnitUnderTestParser(_session.Settings.LogfileType);
        if (!HasValidExtension(fullPath) || parser == null)
        {
            return UnitUnderTest.Null;
        }


        return new UnitUnderTest(fullPath, content)
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
            .SerialNumber(_serialNumber);

        if (_isSfcResponseOk)
        {
            sfcResponseBuilder.SetOkSfcResponse();
        }
        else
        {
            sfcResponseBuilder.SetFailContent(_responseFailMessage);
        }

        if (_isScanError)
        {
            sfcResponseBuilder.SetScanError();
        }

        return sfcResponseBuilder.Build();
    }

    private bool HasValidExtension(string fileName)
    {
        return _session.Settings.InputFileExtension.GetDescription() == "*.*" ||
               _session.Settings.InputFileExtension.GetDescription()
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
        this._session.Settings.InputFileExtension = value;
        return this;
    }

    public UnitUnderTestBuilder LogfileType(LogfileType value)
    {
        this._session.Settings.LogfileType = value;
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
            this._session,
            this._sfcResponseBuilder)
        {
            _isPass = this._isPass,
            _isScanError = this._isScanError,
            _isSfcResponseOk = this._isSfcResponseOk,
            _message = this._message,
            _createdAt = this._createdAt
        };
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
        _isSfcResponseOk = !isFail;
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
            this._sfcResponseBuilder.SetOkSfcResponse();
        }

        return this;
    }

    public UnitUnderTestBuilder Message(string message)
    {
        this._message = message;
        return this;
    }

    public UnitUnderTestBuilder SetOkResponse()
    {
        this._isSfcResponseOk = true;
        return this;
    }
}