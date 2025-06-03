using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hermes.Common.Helpers;
using Hermes.Models;
using Hermes.Types;

namespace Hermes.Repositories;

public class RecordRepository(IDbContextFactory<HermesLocalContext> context)
    : BaseRepository<Record, HermesLocalContext>(context)
{
    private readonly IDbContextFactory<HermesLocalContext> _context = context;

    public async Task<IEnumerable<Record>> FindAll()
    {
        await using var ctx = await _context.CreateDbContextAsync();
        return await GetRecordQuery(ctx).ToListAsync();
    }

    public async Task<List<Record>> GetRecords(
        RecordType? recordType = null,
        RecordStatusType? recordStatus = null,
        TimeSpanType? timeSpanType = null)
    {
        var ctx = await _context.CreateDbContextAsync();
        var query = GetRecordQuery(ctx);

        if (recordType != null)
        {
            query = query.Where(x => x.RecordType == recordType);
        }

        if (recordStatus != null)
        {
            query = query.Where(x => x.RecordStatus == recordStatus);
        }

        if (timeSpanType != null)
        {
            query = query.Where(x => x.StartTime >= DateTime.Now.AddHours(-(double)timeSpanType));
        }

        return await query.ToListAsync();
    }

    private IQueryable<Record> GetRecordQuery(HermesLocalContext ctx)
    {
        return ctx.Set<Record>()
            .OrderByDescending(x => x.StartTime);
    }

    public async Task Add(Record record)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        await using var ctx = await _context.CreateDbContextAsync();
        ctx.Set<Record>().Add(record);
        await ctx.SaveChangesAsync();
    }

    public Record GenRecord(RecordType recordType, RecordStatusType recordStatusType)
    {
        return new Record
        {
            RecordId = RecordIdGenerator.GenerateUniqueTaskIdRecordId(),
            RecordType = recordType,
            RecordStatus = recordStatusType,
            StartTime = DateTime.UtcNow,
        };
    }
}
