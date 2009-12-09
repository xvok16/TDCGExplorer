using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TDCG.Extensions;

namespace TDCG
{
    /// <summary>
    /// TMOFileを書き出すメソッド群
    /// </summary>
    public class TMOWriter
    {
        /// <summary>
        /// 指定ライタ に 'TMO1' を書き出します。
        /// </summary>
        /// <param name="bw">ライタ</param>
        public static void WriteMagic(BinaryWriter bw)
        {
            bw.Write(0x314F4D54);
        }

        /// <summary>
        /// 指定ライタにbyte配列を書き出します。
        /// </summary>
        /// <param name="bw">ライタ</param>
        /// <param name="bytes">byte配列</param>
        public static void Write(BinaryWriter bw, byte[] bytes)
        {
            bw.Write(bytes);
        }

        /// <summary>
        /// 指定ライタにnode配列を書き出します。
        /// </summary>
        /// <param name="bw">ライタ</param>
        /// <param name="items">node配列</param>
        public static void Write(BinaryWriter bw, TMONode[] items)
        {
            bw.Write(items.Length);

            foreach (TMONode i in items)
                Write(bw, i);
        }

        /// <summary>
        /// 指定ライタにnodeを書き出します。
        /// </summary>
        /// <param name="bw">ライタ</param>
        /// <param name="item">node</param>
        public static void Write(BinaryWriter bw, TMONode item)
        {
            bw.WriteCString(item.Path);
        }

        /// <summary>
        /// 指定ライタにframe配列を書き出します。
        /// </summary>
        /// <param name="bw">ライタ</param>
        /// <param name="items">frame配列</param>
        public static void Write(BinaryWriter bw, TMOFrame[] items)
        {
            bw.Write(items.Length);

            foreach (TMOFrame i in items)
            {
                Write(bw, i);
            }
        }

        /// <summary>
        /// 指定ライタにframeを書き出します。
        /// </summary>
        /// <param name="bw">ライタ</param>
        /// <param name="item">frame</param>
        public static void Write(BinaryWriter bw, TMOFrame item)
        {
            Write(bw, item.matrices);
        }

        /// <summary>
        /// 指定ライタに行列の配列を書き出します。
        /// </summary>
        /// <param name="bw">ライタ</param>
        /// <param name="items">行列の配列</param>
        public static void Write(BinaryWriter bw, TMOMat[] items)
        {
            bw.Write(items.Length);

            foreach (TMOMat i in items)
                Write(bw, i);
        }

        /// <summary>
        /// 指定ライタに行列を書き出します。
        /// </summary>
        /// <param name="bw">ライタ</param>
        /// <param name="item">行列</param>
        public static void Write(BinaryWriter bw, TMOMat item)
        {
            Matrix m = item.m;
            bw.Write(ref m);
        }
    }
}
