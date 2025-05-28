using Hermes.Models;
using Hermes.Types;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hermes.Repositories;

public class SlideRepository(IDbContextFactory<HermesLocalContext> context)
    : BaseRepository<Slide, HermesLocalContext>(context)
{
    private readonly IDbContextFactory<HermesLocalContext> _context = context;

    public async Task<IEnumerable<Slide>> FindAll()
    {
        await using var ctx = await _context.CreateDbContextAsync();
        return await GetSlidesQuery(ctx).ToListAsync();
    }

    public async Task<Slide> FindById(string searchSlideId)
    {
        await using var ctx = await _context.CreateDbContextAsync();
        return await ctx.Slides
            .Include(slide => slide.Doctor)
            .FirstOrDefaultAsync(x => x.SlideId.ToString() == searchSlideId) ?? Slide.Null;
    }

    public async Task<List<Slide>> FindBySlideIds(List<string> ids)
    {
        await using var ctx = await _context.CreateDbContextAsync();

        // 获取数据库中匹配的 Slide
        var slidesFromDb = await ctx.Slides
            .Include(slide => slide.Doctor) // 加载导航属性 Doctor
            .Where(slide => ids.Contains(slide.SlideId.ToString()))
            .ToDictionaryAsync(slide => slide.SlideId.ToString());

        // 构造结果列表：存在的使用数据库数据，不存在的生成空 Slide
        var result = ids.Select(slideId =>
            slidesFromDb.TryGetValue(slideId, out var slide)
                ? slide
                : new Slide
                {
                    Id = default,
                    ProgramId = string.Empty,
                    PathologyId = default,
                    SlideId = default,
                    PatientName = string.Empty,
                    DoctorId = default,
                    EntryDate = DateTime.Now
                }).ToList();

        return result;
    }

    private IQueryable<Slide> FindAllSlideQuery(HermesLocalContext ctx)
    {
        return ctx.Slides;
    }

    public async Task<List<Slide>> GetSlides(
        string? pathologyId = null,
        string? slideId = null,
        Doctor? doctor = null,
        SealStateType? sealStateType = null,
        SortStateType? sortStateType = null,
        TimeSpanType? timeSpanType = null)
    {
        var ctx = await _context.CreateDbContextAsync();
        var query = GetSlidesQuery(ctx);
        if (pathologyId != null && pathologyId.Length > 0)
        {
            query = query.Where(x => x.PathologyId.ToString().Contains(pathologyId));
        }

        if (slideId != null && slideId.Length > 0)
        {
            query = query.Where(x => x.SlideId.ToString().Contains(slideId));
        }

        if (doctor != null)
        {
            query = query.Where(x => x.DoctorId == doctor.Id);
        }

        if (sealStateType != null)
        {
            query = query.Where(x => x.SealState == (byte)sealStateType);
        }

        if (sortStateType != null)
        {
            query = query.Where(x => x.SortState == (byte)sortStateType);
        }

        if (timeSpanType != null)
        {
            query = query.Where(x => x.EntryDate >= DateTime.Now.AddHours(-(int)timeSpanType));
        }

        return await query.ToListAsync();
    }

    //public async Task<List<Slide>> GetAllLast24HrsUnits()
    //{
    //    await using var ctx = await _context.CreateDbContextAsync();
    //    return await GetAllLast24HrsUnitsQuery(ctx)
    //        .ToListAsync();
    //}

    private IQueryable<Slide> GetSlidesQuery(HermesLocalContext ctx)
    {
        return ctx.Set<Slide>()
            .Include(x => x.Doctor)
            .OrderByDescending(x => x.EntryDate);
    }
}
