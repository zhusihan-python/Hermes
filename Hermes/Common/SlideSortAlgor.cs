using System;
using System.Collections.Generic;
using SlideSort;
using static SlideSort.SlideSorter;
//using System.ComponentModel.DataAnnotations;
//using Microsoft.FSharp.Core;
using System.Collections;
using System.Threading.Tasks;

namespace Hermes.Common.SlideSortCSharp;

// 定义C#版本的移动结果
public class SortResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public (int From, int To)[] Moves { get; set; }
}

// 定义排序键枚举
public enum SortKey
{
    ProgramId,
    PathologyId,
    SlideId,
    DoctorId,
    DoctorName,
    PatientName
}

// 主要的C#接口类
public static class SlideManager
{
    // 将C#的Slide转换为F#的Slide
    public static SlideSorter.Slide ToFSharpSlide(Hermes.Models.Slide slide)
    {
        return new SlideSorter.Slide(
            slide.Id,
            slide.ProgramId ?? string.Empty,
            slide.PathologyId.ToString(),
            slide.SlideId.ToString(),
            slide.DoctorId,
            slide.Doctor!.Name ?? string.Empty,
            slide.PatientName ?? string.Empty
        );
    }

    // 将C#的SortKey转换为F#的SortKeyType
    private static SlideSorter.SortKeyType ToFSharpSortKeyType(SortKey key)
    {
        switch (key)
        {
            case SortKey.ProgramId:
                return SlideSorter.SortKeyType.ByProgramId;
            case SortKey.PathologyId:
                return SlideSorter.SortKeyType.ByPathologyId;
            case SortKey.SlideId:
                return SlideSorter.SortKeyType.BySlideId;
            case SortKey.DoctorId:
                return SlideSorter.SortKeyType.ByDoctorId;
            case SortKey.DoctorName:
                return SlideSorter.SortKeyType.ByDoctorName;
            case SortKey.PatientName:
                return SlideSorter.SortKeyType.ByPatientName;
            default:
                throw new ArgumentOutOfRangeException(nameof(key));
        }
    }

    public static Task<SortResultDto> SortSlidesAsync(
        Tuple<SlideSorter.Slide, int>[] slides,
        SortKey sortKey,
        int[] availablePositions)
    {
        return Task.Run(() =>
        {
            // 在后台线程上执行F#排序函数
            return SortSlides(slides, sortKey, availablePositions);
        });
    }

    // 主排序函数
    public static SortResultDto SortSlides(
        Tuple<SlideSorter.Slide, int>[] slides,
        SortKey sortKey,
        int[] availablePositions)
    {
        // 调用F#排序函数
        var fsharpSortKey = ToFSharpSortKeyType(sortKey);
        var result = SlideSorter.sortSlidesByKeyForCSharp(slides, fsharpSortKey, availablePositions);

        if (result.IsSuccess)
        {
            var moves = result.Moves.Value;
            foreach (var (from, to) in moves)
            {
                Console.WriteLine($"从 {from} 移动到 {to}");
            }
        }
        else
        {
            Console.WriteLine($"排序失败: {result.ErrorMessage.Value}");
        }
        return result;
    }

    // 生成可用位置数组的辅助方法
    public static int[] GenerateAvailablePositions()
    {
        return new int[1500];
    }

    public static int[] GenerateStatusArray(BitArray slideBoxInPlace, BitArray slideInPlace)
    {
        // 验证输入的 BitArray 长度
        if (slideBoxInPlace == null || slideBoxInPlace.Length != 75)
        {
            throw new ArgumentException("slideBoxInPlace 必须是长度为 75 的 BitArray。", nameof(slideBoxInPlace));
        }
        if (slideInPlace == null || slideInPlace.Length != 1500)
        {
            throw new ArgumentException("slideInPlace 必须是长度为 1500 的 BitArray。", nameof(slideInPlace));
        }

        int totalPositions = 1500;
        int slotsPerBox = 20; // 每个玻片盒有20个位置 (1500 / 75 = 20)
        int[] statusByteArray = new int[totalPositions];

        // 遍历所有 1500 个位置
        for (int i = 0; i < totalPositions; i++)
        {
            // 计算当前位置 i 所在的玻片盒索引
            int boxIndex = i / slotsPerBox; // 使用整数除法

            // 获取该玻片盒的在位状态
            bool isBoxInPlace = slideBoxInPlace[boxIndex];

            // 根据规则确定位置状态
            if (!isBoxInPlace)
            {
                // 如果玻片盒不在位，则该盒内的所有位置都标记为 0 (不可用)
                statusByteArray[i] = 0;
            }
            else
            {
                // 如果玻片盒在位，则检查该位置是否有玻片
                bool isSlideAtPosition = slideInPlace[i];

                if (isSlideAtPosition)
                {
                    // 如果位置有玻片，则标记为 1 (已占用)
                    statusByteArray[i] = 1;
                }
                else
                {
                    // 如果位置没有玻片，则标记为 2 (可用/空闲)
                    statusByteArray[i] = 2;
                }
            }
        }

        return statusByteArray;
    }
}

//class Program
//{
//    static void Main(string[] args)
//    {
//        var slides = new Tuple<SlideSorter.Slide, int>[]
//        {
//            Tuple.Create(SlideManager.ToFSharpSlide(new Hermes.Models.Slide
//            {
//                Id = 1,
//                ProgramId = "0001",
//                PathologyId = 20250001,
//                SlideId = 24051369,
//                DoctorId = 1,
//                Doctor = new Models.Doctor(),
//                PatientName = "王患者"
//            }), 20),

//            Tuple.Create(SlideManager.ToFSharpSlide(new Hermes.Models.Slide
//            {
//                Id = 2,
//                ProgramId = "0001",
//                PathologyId = 20250002,
//                SlideId = 24051555,
//                DoctorId = 2,
//                Doctor = new Models.Doctor(),
//                PatientName = "赵患者"
//            }), 21),

//            Tuple.Create(SlideManager.ToFSharpSlide(new Hermes.Models.Slide
//            {
//                Id = 3,
//                ProgramId = "0002",
//                PathologyId = 20250001,
//                SlideId = 24051596,
//                DoctorId = 1,
//                Doctor = new Models.Doctor(),
//                PatientName = "王患者"
//            }), 22),

//            Tuple.Create(SlideManager.ToFSharpSlide(new Hermes.Models.Slide
//            {
//                Id = 4,
//                ProgramId = "0002",
//                PathologyId = 20250003,
//                SlideId = 24051597,
//                DoctorId = 3,
//                Doctor = new Models.Doctor(),
//                PatientName = "钱患者"
//            }), 23)
//        };

//        // 生成可用位置数组
//        var availablePositions = SlideManager.GenerateAvailablePositions();

//        // 按病理ID排序
//        Console.WriteLine("按病理ID排序：");
//        var resultByPathologyId = SlideManager.SortSlides(slides, SortKey.PathologyId, availablePositions);
//        PrintResult(resultByPathologyId);

//        // 按医生姓名排序
//        Console.WriteLine("\n按医生姓名排序：");
//        var resultByDoctorName = SlideManager.SortSlides(slides, SortKey.DoctorName, availablePositions);
//        PrintResult(resultByDoctorName);

//        // 按病人姓名排序
//        Console.WriteLine("\n按病人姓名排序：");
//        var resultByPatientName = SlideManager.SortSlides(slides, SortKey.PatientName, availablePositions);
//        PrintResult(resultByPatientName);
//    }

//    static void PrintResult(SortResultDto result)
//    {
//        if (result.IsSuccess)
//        {
//            Console.WriteLine("排序成功，移动序列：");

//            if (FSharpOption<Tuple<int, int>[]>.get_IsSome(result.Moves))
//            {
//                foreach (var (from, to) in result.Moves.Value)
//                {
//                    Console.WriteLine($"从位置 {from} 移动到位置 {to}");
//                }
//            }
//            else
//            {
//                Console.WriteLine("没有可用的移动数据。");
//            }
//        }
//        else
//        {
//            Console.WriteLine($"排序失败：{result.ErrorMessage.Value}");
//        }
//    }
//}
