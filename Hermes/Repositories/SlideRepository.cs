using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hermes.Models;

namespace Hermes.Repositories;

public class SlideRepository(IDbContextFactory<HermesLocalContext> context)
    : BaseRepository<Slide, HermesLocalContext>(context)
{
    private readonly IDbContextFactory<HermesLocalContext> _context = context;

    public async Task<IEnumerable<Slide>> FindAll()
    {
        await using var ctx = await _context.CreateDbContextAsync();
        return await FindAllSlideQuery(ctx).ToListAsync();
    }

    public async Task<Slide> FindById(string searchSlideId)
    {
        await using var ctx = await _context.CreateDbContextAsync();
        return await ctx.Slides.FirstOrDefaultAsync(x => x.Id.ToString() == searchSlideId) ?? Slide.Null;
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
                    EntryDate = string.Empty
                }).ToList();

        return result;
    }

    private IQueryable<Slide> FindAllSlideQuery(HermesLocalContext ctx)
    {
        return ctx.Slides;
    }
}
