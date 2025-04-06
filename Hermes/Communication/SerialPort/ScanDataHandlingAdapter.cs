using Hermes.Communication.Protocol;
using TouchSocket.Core;

namespace Hermes.Communication.SerialPort;

public class ScanDataHandlingAdapter : CustomDataHandlingAdapter<ScanRequestInfo>
{
    // 数据为空的包 04 D0 00 00 FF 2C
    public int HeaderLength = 6;
    public override bool CanSendRequestInfo => true;

    /// <summary>
    /// 筛选解析数据。实例化的TRequest会一直保存，直至解析成功，或手动清除。
    /// <para>当不满足解析条件时，请返回<see cref="FilterResult.Cache"/>，此时会保存<see cref="ByteBlock.CanReadLen"/>的数据</para>
    /// <para>当数据部分异常时，请移动<see cref="ByteBlock.Pos"/>到指定位置，然后返回<see cref="FilterResult.GoOn"/></para>
    /// <para>当完全满足解析条件时，请返回<see cref="FilterResult.Success"/>最后将<see cref="ByteBlock.Pos"/>移至指定位置。</para>
    /// </summary>
    /// <param name="byteBlock">字节块</param>
    /// <param name="beCached">是否为上次遗留对象，当该参数为True时，request也将是上次实例化的对象。</param>
    /// <param name="request">对象。</param>
    /// <param name="tempCapacity">缓存容量指导，指示当需要缓存时，应该申请多大的内存。</param>
    /// <returns></returns>
    protected override FilterResult Filter<TByteBlock>(ref TByteBlock byteBlock, bool beCached, ref ScanRequestInfo request, ref int tempCapacity)
    {
        if (this.HeaderLength > byteBlock.CanReadLength)
        {
            this.SurLength = this.HeaderLength - byteBlock.CanReadLength;
            return FilterResult.Cache;
        }

        var span = byteBlock.Span.Slice(byteBlock.Position, byteBlock.CanReadLength);
        // 先找包尾，有可能有多个包尾，取第一个
        var tailIndex = span.IndexOfFirst(0, span.Length, Scan.FullTail);
        if (tailIndex > 0)
        {
            // 从包尾往前找包头
            var headIndexes = span.IndexOfInclude(0, tailIndex - Scan.FullTail.Length, Scan.FullHead);
            // 找到取第一个
            if (headIndexes.Count > 0)
            {
                // 取距离包尾最近的包头
                var lastHeadIndex = headIndexes[^1];
                //var pos = byteBlock.Position;//记录初始游标位置，防止本次无法解析时，回退游标。
                // 去掉包头包尾
                var package = span.Slice(lastHeadIndex + 1, tailIndex + 1 - Scan.FullTail.Length - Scan.FullHead.Length).ToArray();
                // 清理之后包长度，不含包头包尾最少是14
                if (package.Length > 0)
                {
                    var requestInfo = new ScanRequestInfo();
                    requestInfo.dataFrame = package;
                    request = requestInfo;
                    byteBlock.Position += tailIndex + 1;
                    return FilterResult.Success;
                }
                else
                {
                    // 数据部分长度为0，移动Position到该包尾的位置
                    byteBlock.Position += tailIndex;
                    return FilterResult.GoOn;
                }
            }
            else
            {
                // 找不到则移动Position到该包尾的位置
                byteBlock.Position += tailIndex;
                return FilterResult.GoOn;
            }
        }
        else
        {
            var firstHeadIndex = span.IndexOfFirst(0, span.Length, Scan.FullHead);
            return FilterResult.Cache;
            //if (firstHeadIndex > 0 && span.Length == Scan.FullHead.Length)
            //{
            //    // 只有包头的包
            //    var head = span.Slice(firstHeadIndex + 1, Svt.FullHead.Length).ToArray();
            //    var emptyRequestInfo = new ScanRequestInfo();
            //    emptyRequestInfo.header = head;
            //    return FilterResult.Success;
            //}
            //else
            //{
            //    return FilterResult.Cache;
            //}
        }
    }
}
